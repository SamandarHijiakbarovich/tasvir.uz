using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Tasvir.Modules.Academy.Application;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Academy.Presentation;

public static class AcademyEndpoints
{
    /// <summary>Academy moduli HTTP marshrutlari: /api/academy/*. Hammasi kirgan foydalanuvchi uchun.</summary>
    public static IEndpointRouteBuilder MapAcademyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/academy").WithTags("Academy").RequireAuthorization();

        group.MapGet("/courses", async (ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.GetCoursesAsync(UserId(principal), ct)))
            .WithSummary("Barça kurslar va foydalanuvçi progressi");

        group.MapGet("/courses/{slug}", async (string slug, ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.GetCourseAsync(UserId(principal), slug, ct)))
            .WithSummary("Kurs, darslar röyxati va foydalanuvçi progressi");

        group.MapGet("/my-work", async (ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.GetMyWorkAsync(UserId(principal), ct)))
            .WithSummary("Foydalanuvçi amaliy topşiriqlarda yaratgan işlar");

        group.MapGet("/lessons/{slug}", async (string slug, ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.GetLessonAsync(UserId(principal), slug, ct)))
            .WithSummary("Dars bosqiçlari (test javoblarisiz)");

        group.MapPost("/lessons/{slug}/start", async (string slug, ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.StartLessonAsync(UserId(principal), slug, ct)))
            .WithSummary("Darsni boşlaş");

        group.MapPost("/lessons/{slug}/steps/{stepId:guid}/complete", async (
                string slug, Guid stepId, CompleteStepRequest request, ClaimsPrincipal principal, AcademyService academy, CancellationToken ct) =>
            ToResponse(await academy.CompleteStepAsync(UserId(principal), slug, stepId, request, ct)))
            .WithSummary("Bosqiçni yakunlaş: test javobini tekşiradi, topşiriqni saqlaydi");

        return app;
    }

    private static Guid UserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out var id) ? id : Guid.Empty;

    private static IResult ToResponse<T>(Result<T> result)
    {
        if (result.Error is { } error)
        {
            return Results.Problem(
                title: error.Message,
                statusCode: error.Kind switch
                {
                    ErrorKind.Validation => StatusCodes.Status400BadRequest,
                    ErrorKind.NotFound => StatusCodes.Status404NotFound,
                    ErrorKind.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError,
                },
                extensions: new Dictionary<string, object?> { ["code"] = error.Code });
        }

        return Results.Ok(result.Value);
    }
}
