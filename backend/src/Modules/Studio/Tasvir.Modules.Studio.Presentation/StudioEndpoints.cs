using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Tasvir.Modules.Studio.Application;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Studio.Presentation;

public static class StudioEndpoints
{
    /// <summary>Studio moduli HTTP marshrutlari: /api/studio/*. Har kim faqat o‘z materiallari bilan ishlaydi.</summary>
    public static IEndpointRouteBuilder MapStudioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/studio/materials").WithTags("Studio").RequireAuthorization();

        group.MapGet("", async (ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.ListAsync(UserId(principal), ct)))
            .WithSummary("Mening materiallarim (oxirgi o‘zgartirilgani birinchi)");

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.GetAsync(UserId(principal), id, ct)))
            .WithSummary("Bitta material");

        group.MapPost("", async (SaveMaterialRequest request, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.CreateAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .WithSummary("Yangi material");

        group.MapPut("/{id:guid}", async (Guid id, SaveMaterialRequest request, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.UpdateAsync(UserId(principal), id, request, ct)))
            .WithSummary("Materialni saqlash");

        group.MapPost("/{id:guid}/duplicate", async (Guid id, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.DuplicateAsync(UserId(principal), id, ct), StatusCodes.Status201Created))
            .WithSummary("Nusxa olish");

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.DeleteAsync(UserId(principal), id, ct)))
            .WithSummary("O‘chirish");

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
                    ErrorKind.NotFound => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status500InternalServerError,
                },
                extensions: new Dictionary<string, object?> { ["code"] = error.Code });
        }

        return result.Value is Unit ? Results.NoContent() : Results.Json(result.Value, statusCode: successStatus);
    }
}
