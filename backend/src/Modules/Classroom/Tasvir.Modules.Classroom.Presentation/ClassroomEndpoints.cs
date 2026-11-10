using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Classroom.Application;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Classroom.Presentation;

public static class ClassroomEndpoints
{
    private const string JoinRateLimit = "classroom-join";

    /// <summary>Guruh kodini terib ko‘rishga qarshi cheklov. Sozlama: "Classroom:RateLimit:PermitPerMinute".</summary>
    public static IServiceCollection AddClassroomPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        var permitPerMinute = configuration.GetValue("Classroom:RateLimit:PermitPerMinute", 10);

        services.AddRateLimiter(options => options.AddPolicy(JoinRateLimit, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress ?? IPAddress.None,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permitPerMinute, Window = TimeSpan.FromMinutes(1) })));

        return services;
    }

    /// <summary>Classroom moduli HTTP marshrutlari: /api/classroom/*. Rol guruhga bog‘liq va har so‘rovda tekshiriladi.</summary>
    public static IEndpointRouteBuilder MapClassroomEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/classroom").WithTags("Classroom").RequireAuthorization();

        api.MapGet("/groups", async (ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.GetMyGroupsAsync(UserId(principal), ct)))
            .WithSummary("Men öqitayotgan va öqiyotgan guruhlar");

        api.MapPost("/groups", async (CreateGroupRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.CreateGroupAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .WithSummary("Yangi guruh (yaratgan kişi öqituvçi böladi)");

        api.MapPost("/groups/join", async (JoinGroupRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.JoinAsync(UserId(principal), request, ct)))
            .RequireRateLimiting(JoinRateLimit)
            .WithSummary("Kod bilan guruhga qöşiliş");

        api.MapDelete("/groups/{id:guid}", async (Guid id, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.DeleteGroupAsync(UserId(principal), id, ct)))
            .WithSummary("Guruhni öçiriş (öqituvçi)");

        api.MapGet("/groups/{id:guid}/dashboard", async (Guid id, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.GetDashboardAsync(UserId(principal), id, ct)))
            .WithSummary("Öqituvçi paneli: progress, işlar, test tahlili, sörovnoma");

        api.MapGet("/groups/{id:guid}/export/{kind}", async (Guid id, string kind, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
        {
            var result = await classroom.ExportAsync(UserId(principal), id, kind, ct);
            if (result.Error is not null)
            {
                return ToResponse(result);
            }

            // UTF-8 belgisi (BOM) bo‘lmasa, Excel o‘zbekcha harflarni buzadi
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(result.Value!.Csv)).ToArray();
            return Results.File(bytes, "text/csv; charset=utf-8", result.Value.FileName);
        }).WithSummary("Natijalarni Excel uçun CSV körinişida yuklab oliş (öqituvçi)");

        api.MapGet("/groups/{id:guid}/me", async (Guid id, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.GetStudentViewAsync(UserId(principal), id, ct)))
            .WithSummary("Talaba körinişi: mening işlarim va sörovnoma");

        api.MapDelete("/groups/{id:guid}/members/{userId:guid}", async (Guid id, Guid userId, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.RemoveMemberAsync(UserId(principal), id, userId, ct)))
            .WithSummary("Guruhdan çiqiş yoki talabani çiqariş");

        api.MapPost("/groups/{id:guid}/submissions", async (Guid id, SubmitWorkRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.SubmitAsync(UserId(principal), id, request, ct), StatusCodes.Status201Created))
            .WithSummary("Materialni öqituvçiga topşiriş");

        api.MapPut("/groups/{id:guid}/survey", async (Guid id, SaveSurveyRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.SaveSurveyAsync(UserId(principal), id, request, ct)))
            .WithSummary("Sörovnomaga javob");

        api.MapGet("/groups/{id:guid}/diagnostics/{stage}", async (Guid id, string stage, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.GetDiagnosticTestAsync(UserId(principal), id, stage, ct)))
            .WithSummary("Diagnostika testi savollari (talaba; test oçiq va hali topşirilmagan bölsa)");

        api.MapPost("/groups/{id:guid}/diagnostics/{stage}", async (Guid id, string stage, SubmitDiagnosticRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.SubmitDiagnosticAsync(UserId(principal), id, stage, request, ct), StatusCodes.Status201Created))
            .WithSummary("Diagnostika testini topşiriş (bir marta)");

        api.MapPut("/groups/{id:guid}/diagnostics/{stage}/state", async (Guid id, string stage, SetDiagnosticStateRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.SetDiagnosticStateAsync(UserId(principal), id, stage, request, ct)))
            .WithSummary("Diagnostika testini oçiş yoki yopiş (öqituvçi)");

        api.MapGet("/groups/{id:guid}/compare/{otherId:guid}", async (Guid id, Guid otherId, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.CompareAsync(UserId(principal), id, otherId, ct)))
            .WithSummary("Ikki guruh diagnostika natijalarini soliştiriş (öqituvçi)");

        api.MapGet("/submissions/{id:guid}", async (Guid id, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.GetSubmissionAsync(UserId(principal), id, ct)))
            .WithSummary("Topşirilgan iş (öqituvçi yoki iş egasi)");

        api.MapPut("/submissions/{id:guid}/review", async (Guid id, SaveReviewRequest request, ClaimsPrincipal principal, ClassroomService classroom, CancellationToken ct) =>
            ToResponse(await classroom.SaveReviewAsync(UserId(principal), id, request, ct)))
            .WithSummary("Rubrika böyiça baholaş (öqituvçi)");

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
                    _ => StatusCodes.Status500InternalServerError,
                },
                extensions: new Dictionary<string, object?> { ["code"] = error.Code, ["errors"] = error.Fields });
        }

        return result.Value is Unit ? Results.NoContent() : Results.Json(result.Value, statusCode: successStatus);
    }
}
