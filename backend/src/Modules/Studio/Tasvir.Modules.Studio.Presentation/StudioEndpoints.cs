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
            .WithSummary("Mening materiallarim (oxirgi özgartirilgani birinçi)");

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.GetAsync(UserId(principal), id, ct)))
            .WithSummary("Bitta material");

        group.MapPost("", async (SaveMaterialRequest request, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.CreateAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .WithSummary("Yangi material");

        group.MapPut("/{id:guid}", async (Guid id, SaveMaterialRequest request, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.UpdateAsync(UserId(principal), id, request, ct)))
            .WithSummary("Materialni saqlaş");

        group.MapPost("/{id:guid}/duplicate", async (Guid id, ClaimsPrincipal principal, StudioService studio, CancellationToken ct) =>
            ToResponse(await studio.DuplicateAsync(UserId(principal), id, ct), StatusCodes.Status201Created))
            .WithSummary("Nusxa oliş");

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal principal, StudioService studio, LibraryService library, CancellationToken ct) =>
        {
            var result = await studio.DeleteAsync(UserId(principal), id, ct);
            if (result.IsSuccess)
            {
                await library.RemoveMaterialShareAsync(id, ct);
            }

            return ToResponse(result);
        })
            .WithSummary("Öçiriş");

        MapLibrary(app);
        return app;
    }

    /// <summary>
    /// Kabinetdagi "Mening materiallarim": yuklangan fayllar va havolalar (/api/studio/library), ochiq ulashish havolalari
    /// (/api/studio/shares) va ular orqali kirmasdan ko‘rish (/api/studio/shared/{token}).
    /// </summary>
    private static void MapLibrary(IEndpointRouteBuilder app)
    {
        var library = app.MapGroup("/api/studio/library").WithTags("Studio").RequireAuthorization();

        library.MapGet("", async (ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.ListAsync(UserId(principal), ct)))
            .WithSummary("Saqlangan fayl va havolalar");

        library.MapPost("/links", async (SaveLinkRequest request, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.AddLinkAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .WithSummary("Havola saqlaş (Google Slides, Canva va h.k.)");

        library.MapPost("/files", async (UploadFileRequest request, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.AddFileAsync(UserId(principal), request, ct), StatusCodes.Status201Created))
            .WithSummary("Fayl yuklaş (PDF, PPTX, PPT, PNG, JPG, WEBP)");

        library.MapPut("/{id:guid}", async (Guid id, UpdateLibraryItemRequest request, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.UpdateAsync(UserId(principal), id, request, ct)))
            .WithSummary("Nomi, fani, mavzusi yoki havolasini özgartiriş");

        library.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.DeleteAsync(UserId(principal), id, ct)))
            .WithSummary("Öçiriş");

        library.MapGet("/{id:guid}/file", async (Guid id, ClaimsPrincipal principal, LibraryService service, HttpContext http, CancellationToken ct) =>
            ToFile(await service.GetFileAsync(UserId(principal), id, ct), http))
            .WithSummary("Fayl mazmuni");

        var shares = app.MapGroup("/api/studio/shares").WithTags("Studio").RequireAuthorization();

        shares.MapGet("", async (ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.ListSharesAsync(UserId(principal), ct)))
            .WithSummary("Mening ochiq havolalarim");

        shares.MapPost("", async (ShareRequest request, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.ShareAsync(UserId(principal), request, ct)))
            .WithSummary("Ulaşiş havolasini yaratiş (bor bölsa, öşani qaytaradi)");

        shares.MapDelete("/{kind}/{id:guid}", async (string kind, Guid id, ClaimsPrincipal principal, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.UnshareAsync(UserId(principal), kind, id, ct)))
            .WithSummary("Ulaşişni töxtatiş: eski havola işlamay qoladi");

        var shared = app.MapGroup("/api/studio/shared").WithTags("Studio").AllowAnonymous();

        shared.MapGet("/{token}", async (string token, LibraryService service, CancellationToken ct) =>
            ToResponse(await service.GetSharedAsync(token, ct)))
            .WithSummary("Ulaşilgan material (kirmasdan)");

        shared.MapGet("/{token}/file", async (string token, LibraryService service, HttpContext http, CancellationToken ct) =>
            ToFile(await service.GetSharedFileAsync(token, ct), http))
            .WithSummary("Ulaşilgan fayl (kirmasdan)");
    }

    /// <summary>
    /// Fayl brauzerda ochiladi (inline): PDF va rasm sahifada ko‘rinadi. Turi faqat ruxsat etilganlardan biri,
    /// nosniff bilan brauzer uni boshqa tur (masalan, HTML) deb taxmin qilmaydi.
    /// </summary>
    private static IResult ToFile(Result<FileContent> result, HttpContext http)
    {
        if (result.Error is not null || result.Value is null)
        {
            return ToResponse(result);
        }

        var file = result.Value;
        var disposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("inline");
        disposition.SetHttpFileName(file.FileName);
        http.Response.Headers.ContentDisposition = disposition.ToString();
        http.Response.Headers.XContentTypeOptions = "nosniff";
        http.Response.Headers.CacheControl = "private, no-store";
        return Results.Bytes(file.Data, file.ContentType);
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
