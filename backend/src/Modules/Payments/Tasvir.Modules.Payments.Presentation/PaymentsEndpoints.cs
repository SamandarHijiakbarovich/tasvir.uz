using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Payments.Application;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Payments.Presentation;

public static class PaymentsEndpoints
{
    /// <summary>Payme va Click so‘rovi tanasi bundan katta bo‘lmaydi.</summary>
    private const int MaxCallbackBytes = 64 * 1024;

    /// <summary>Sozlamalar: "Payments" bo‘limi (tariflar, Payme va Click kalitlari).</summary>
    public static IServiceCollection AddPaymentsPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(PaymentOptions.Section).Get<PaymentOptions>() ?? new PaymentOptions();
        services.AddSingleton(options);
        return services;
    }

    /// <summary>
    /// /api/payments/*: tariflar, buyurtmalar va to‘lov tizimlari chaqiradigan manzillar (callback).
    /// Callbacklar anonim: Payme Basic sarlavha bilan, Click md5 imzo bilan tekshiriladi.
    /// </summary>
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/payments").WithTags("Payments");

        api.MapGet("/plans", (PaymentService payments) => Results.Ok(payments.GetCatalog()))
            .WithSummary("Tariflar va ulangan tölov tizimlari");

        api.MapPost("/orders", async (CreateOrderRequest request, ClaimsPrincipal principal, PaymentService payments, CancellationToken ct) =>
            ToResponse(await payments.CreateOrderAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .RequireAuthorization()
            .WithSummary("Buyurtma yaratiş va tölov sahifasi havolasini oliş");

        api.MapGet("/orders/{id:guid}", async (Guid id, ClaimsPrincipal principal, PaymentService payments, CancellationToken ct) =>
            ToResponse(await payments.GetOrderAsync(UserId(principal), id, ct)))
            .RequireAuthorization()
            .WithSummary("Buyurtma holati (faqat egasi)");

        api.MapGet("/me", async (ClaimsPrincipal principal, PaymentService payments, CancellationToken ct) =>
            ToResponse(await payments.GetMineAsync(UserId(principal), ct)))
            .RequireAuthorization()
            .WithSummary("Mening obunam va tölovlarim");

        api.MapPost("/payme", async (HttpContext context, PaymeMerchantService payme, PaymentOptions options, CancellationToken ct) =>
        {
            if (!IsAllowed(context, options.Payme.AllowedIps))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            using var reader = new StreamReader(context.Request.Body);
            var buffer = new char[MaxCallbackBytes + 1];
            var length = await reader.ReadBlockAsync(buffer, ct);
            var body = length > MaxCallbackBytes ? string.Empty : new string(buffer, 0, length);
            return Results.Json(await payme.HandleAsync(body, context.Request.Headers.Authorization, ct));
        }).AllowAnonymous().WithSummary("Payme Merchant API (JSON-RPC)");

        api.MapPost("/click/prepare", async (HttpContext context, ClickMerchantService click, PaymentOptions options, CancellationToken ct) =>
            await HandleClickAsync(context, options, request => click.PrepareAsync(request, ct)))
            .AllowAnonymous().WithSummary("Click SHOP API: Prepare");

        api.MapPost("/click/complete", async (HttpContext context, ClickMerchantService click, PaymentOptions options, CancellationToken ct) =>
            await HandleClickAsync(context, options, request => click.CompleteAsync(request, ct)))
            .AllowAnonymous().WithSummary("Click SHOP API: Complete");

        return app;
    }

    private static async Task<IResult> HandleClickAsync(
        HttpContext context, PaymentOptions options, Func<ClickRequest, Task<Dictionary<string, object?>>> handle)
    {
        if (!IsAllowed(context, options.Click.AllowedIps))
        {
            return Results.StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!context.Request.HasFormContentType || context.Request.ContentLength > MaxCallbackBytes)
        {
            return Results.Json(new Dictionary<string, object?> { ["error"] = ClickMerchantService.Codes.BadRequest, ["error_note"] = "Error in request from click" });
        }

        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        string? Field(string key) => form.TryGetValue(key, out var value) ? value.ToString() : null;
        var request = new ClickRequest(
            Field("click_trans_id"), Field("service_id"), Field("click_paydoc_id"), Field("merchant_trans_id"),
            Field("merchant_prepare_id"), Field("amount"), Field("action"), Field("error"), Field("error_note"),
            Field("sign_time"), Field("sign_string"));
        return Results.Json(await handle(request));
    }

    /// <summary>Ro‘yxat bo‘sh bo‘lsa, hamma manzil ruxsat etiladi (imzo baribir tekshiriladi).</summary>
    private static bool IsAllowed(HttpContext context, IReadOnlyCollection<string> allowed)
    {
        if (allowed.Count == 0)
        {
            return true;
        }

        var remote = context.Connection.RemoteIpAddress;
        if (remote is null)
        {
            return false;
        }

        if (remote.IsIPv4MappedToIPv6)
        {
            remote = remote.MapToIPv4();
        }

        return allowed.Any(ip => IPAddress.TryParse(ip, out var address) && address.Equals(remote));
    }

    private static Guid UserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    private static IResult ToResponse<T>(Result<T> result, int successStatus = StatusCodes.Status200OK)
    {
        if (result.Error is { } error)
        {
            return Results.Problem(
                title: error.Message,
                statusCode: error.Kind switch
                {
                    ErrorKind.Validation => StatusCodes.Status400BadRequest,
                    ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
                    ErrorKind.NotFound => StatusCodes.Status404NotFound,
                    ErrorKind.Conflict => StatusCodes.Status409Conflict,
                    ErrorKind.TooManyRequests => StatusCodes.Status429TooManyRequests,
                    _ => StatusCodes.Status500InternalServerError,
                },
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = error.Code,
                    ["errors"] = error.Fields,
                });
        }

        return Results.Json(result.Value, statusCode: successStatus);
    }
}
