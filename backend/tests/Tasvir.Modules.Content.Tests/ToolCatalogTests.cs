using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Tasvir.Modules.Content.Application.Tools;

namespace Tasvir.Modules.Content.Tests;

public sealed class ToolCatalogTests
{
    private static readonly ToolCatalogDto Catalog = ToolCatalog.Value;

    [Fact]
    public void Keys_are_unique()
    {
        Assert.Equal(Catalog.Tools.Count, Catalog.Tools.Select(t => t.Key).Distinct().Count());
        Assert.Equal(Catalog.Categories.Count, Catalog.Categories.Select(c => c.Key).Distinct().Count());
        Assert.Equal(Catalog.Shapes.Count, Catalog.Shapes.Select(s => s.Key).Distinct().Count());
        Assert.Equal(Catalog.Subjects.Count, Catalog.Subjects.Select(s => s.Key).Distinct().Count());
    }

    [Fact]
    public void Every_tool_refers_to_known_values()
    {
        var categories = Catalog.Categories.Select(c => c.Key).ToHashSet();
        var shapes = Catalog.Shapes.Select(s => s.Key).ToHashSet();
        var subjects = Catalog.Subjects.Select(s => s.Key).ToHashSet();

        foreach (var tool in Catalog.Tools)
        {
            Assert.Contains(tool.Category, categories);
            Assert.NotEmpty(tool.Shapes);
            Assert.All(tool.Shapes, s => Assert.Contains(s, shapes));
            Assert.All(tool.Subjects, s => Assert.Contains(s, subjects));
            Assert.Contains(tool.Access, new[] { "free", "freemium", "paid" });
            Assert.Contains(tool.Level, new[] { "easy", "medium", "coding" });
            Assert.True(Uri.TryCreate(tool.Url, UriKind.Absolute, out var url) && url.Scheme.StartsWith("http"), tool.Key);
            Assert.False(string.IsNullOrWhiteSpace(tool.Summary), tool.Key);
            Assert.False(string.IsNullOrWhiteSpace(tool.ClassroomUse), tool.Key);
        }
    }

    // Har bir toifa, shakl va fan uchun kamida bitta vosita bo‘lsin: aks holda sahifada bo‘sh bo‘lim chiqadi
    [Fact]
    public void Nothing_is_left_empty()
    {
        Assert.All(Catalog.Categories, c => Assert.Contains(Catalog.Tools, t => t.Category == c.Key));
        Assert.All(Catalog.Shapes, s => Assert.Contains(Catalog.Tools, t => t.Shapes.Contains(s.Key)));
        Assert.All(Catalog.Subjects, s => Assert.Contains(Catalog.Tools, t => t.Subjects.Contains(s.Key)));
    }

    [Fact]
    public void Studio_shapes_match_studio_templates()
    {
        // frontend/src/features/studio/lib/templates.ts dagi shablon kalitlari
        string[] templates = ["process", "timeline", "mindmap", "comparison", "infographic"];
        Assert.Equal(templates.Order(), Catalog.Shapes.Where(s => s.InStudio).Select(s => s.Key).Order());
    }
}

public sealed class ContentApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-content-tests-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Studio", $"Data Source={Path.Combine(_dir, "studio.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Classroom", $"Data Source={Path.Combine(_dir, "classroom.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Payments", $"Data Source={Path.Combine(_dir, "payments.db")};Pooling=False");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            Directory.Delete(_dir, recursive: true);
        }
        catch (IOException)
        {
            // Vaqtinchalik papka; Windows faylni band qilgan bo‘lsa, muhim emas
        }
    }
}

public sealed class ContentApiTests(ContentApiFactory factory) : IClassFixture<ContentApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Tool_catalog_is_public()
    {
        var response = await factory.CreateClient().GetAsync("/api/content/tools");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var catalog = await response.Content.ReadFromJsonAsync<ToolCatalogDto>(Json);
        Assert.Equal(ToolCatalog.Value.Tools.Count, catalog!.Tools.Count);
        Assert.Contains(catalog.Tools, t => t.Name == "Canva" && t.Url == "https://www.canva.com");
    }
}
