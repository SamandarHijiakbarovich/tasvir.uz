using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Tasvir.Modules.Content.Application.Landing;
using Tasvir.Modules.Content.Application.Tools;

namespace Tasvir.Modules.Content.Presentation;

public static class ContentEndpoints
{
    /// <summary>Content moduli HTTP marshrutlari: /api/content/*</summary>
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content").WithTags("Content");

        group.MapGet("/landing", async (GetLandingContentHandler handler, HttpContext http, CancellationToken ct) =>
            {
                // Landing ma’lumotlari kam o‘zgaradi — qisqa muddatga keshlash mumkin.
                http.Response.Headers.CacheControl = "public, max-age=60";
                return TypedResults.Ok(await handler.HandleAsync(ct));
            })
            .WithName("GetLandingContent")
            .WithSummary("Landing sahifa uchun dinamik ma’lumotlar (narxlar, FAQ, sharhlar, natijalar, hamkorlar)");

        group.MapGet("/tools", (HttpContext http) =>
            {
                http.Response.Headers.CacheControl = "public, max-age=3600";
                return TypedResults.Ok(ToolCatalog.Value);
            })
            .WithName("GetToolCatalog")
            .WithSummary("Vizuallashtirish dasturlari katalogi: toifalar, shakllar, fanlar va vositalar");

        return app;
    }
}
