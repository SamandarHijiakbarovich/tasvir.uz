using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Identity.Application;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Identity.Presentation;

public static class IdentityEndpoints
{
    private const string AuthRateLimit = "identity-auth";

    /// <summary>
    /// Parol va havola bilan ishlovchi marshrutlar uchun IP bo‘yicha cheklov (parol terib ko‘rishga qarshi).
    /// Sozlama: "Identity:RateLimit:PermitPerMinute".
    /// </summary>
    public static IServiceCollection AddIdentityPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var permitPerMinute = configuration.GetValue("Identity:RateLimit:PermitPerMinute", 20);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, ct) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "So‘rovlar juda ko‘p. Bir daqiqadan so‘ng qayta urining.",
                    code = "rate_limited",
                }, ct));
            };
            options.AddPolicy(AuthRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress ?? IPAddress.None,
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permitPerMinute, Window = TimeSpan.FromMinutes(1) }));
        });

        return services;
    }

    /// <summary>Identity moduli HTTP marshrutlari: /api/identity/*</summary>
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity").WithTags("Identity");

        // Javoblarda token bor — hech qayerda keshlanmasin
        group.AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            return await next(context);
        });

        var limited = group.MapGroup("").RequireRateLimiting(AuthRateLimit);

        limited.MapPost("/register", async (RegisterRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.RegisterAsync(request, ct), StatusCodes.Status201Created))
            .WithSummary("Ro‘yxatdan o‘tish va sessiya ochish");

        limited.MapPost("/login", async (LoginRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.LoginAsync(request, ct)))
            .WithSummary("Email va parol bilan kirish");

        limited.MapPost("/password/forgot", async (ForgotPasswordRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.ForgotPasswordAsync(request, ct), StatusCodes.Status202Accepted))
            .WithSummary("Parolni tiklash havolasini yuborish");

        limited.MapPost("/password/reset", async (ResetPasswordRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.ResetPasswordAsync(request, ct)))
            .WithSummary("Havola orqali yangi parol o‘rnatish");

        limited.MapPost("/email/confirm", async (ConfirmEmailRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.ConfirmEmailAsync(request, ct)))
            .WithSummary("Emailni tasdiqlash");

        // Sahifa ochilganda avtomatik chaqiriladi, shuning uchun IP cheklovi yo‘q: token 256 bitli va terib topilmaydi
        group.MapPost("/refresh", async (RefreshRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.RefreshAsync(request, ct)))
            .WithSummary("Access tokenni yangilash (refresh token almashadi)");

        group.MapPost("/logout", async (RefreshRequest request, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.LogoutAsync(request, ct)))
            .WithSummary("Joriy sessiyani yopish");

        group.MapGet("/me", async (ClaimsPrincipal principal, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.GetCurrentUserAsync(UserId(principal), ct)))
            .RequireAuthorization()
            .WithSummary("Joriy foydalanuvchi");

        group.MapPost("/password/change", async (ChangePasswordRequest request, ClaimsPrincipal principal, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.ChangePasswordAsync(UserId(principal), request, ct)))
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimit)
            .WithSummary("Parolni o‘zgartirish (boshqa qurilmalardagi sessiyalar yopiladi)");

        group.MapPost("/email/resend", async (ClaimsPrincipal principal, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.ResendConfirmationAsync(UserId(principal), ct), StatusCodes.Status202Accepted))
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimit)
            .WithSummary("Tasdiqlash xatini qayta yuborish");

        group.MapPost("/privacy/accept", async (ClaimsPrincipal principal, AuthService auth, CancellationToken ct) =>
            ToResponse(await auth.AcceptPrivacyAsync(UserId(principal), ct)))
            .RequireAuthorization()
            .WithSummary("Maxfiylik siyosatiga rozilik (eski hisoblar uchun)");

        return app;
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
                extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["errors"] = error.Fields });
        }

        return result.Value is Unit
            ? Results.StatusCode(successStatus == StatusCodes.Status200OK ? StatusCodes.Status204NoContent : successStatus)
            : Results.Json(result.Value, statusCode: successStatus);
    }
}
