using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tasvir.Modules.Studio.Tests;

public sealed class StudioApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-studio-tests-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Studio", $"Data Source={Path.Combine(_dir, "studio.db")};Pooling=False");
        builder.UseSetting("Identity:RateLimit:PermitPerMinute", "10000");
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

public sealed class StudioApiTests(StudioApiFactory factory) : IClassFixture<StudioApiFactory>
{
    private const string Url = "/api/studio/materials";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken);
    private sealed record Item(string? Heading, string? Text, List<string>? Cells);
    private sealed record Content(string? Accent, string? Note, List<string>? Columns, List<Item>? Items);
    private sealed record Material(Guid Id, string TemplateKey, string Title, string? Subject, int? Grade, Content Content);
    private sealed record Problem(string Code);

    private async Task<HttpClient> SignedInClient()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Test O‘qituvchi",
            email = $"oqituvchi-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
        });
        var session = (await response.Content.ReadFromJsonAsync<Session>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private static object Process(string title = "Fotosintez", int items = 3) => new
    {
        templateKey = "process",
        title,
        subject = "Biologiya",
        grade = 7,
        content = new
        {
            accent = "sun",
            note = "Dars uchun",
            items = Enumerable.Range(1, items).Select(i => new { heading = $"{i}-bosqich", text = "Izoh" }).ToArray(),
        },
    };

    private static async Task<T> Read<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    private static async Task<Material> Create(HttpClient client, object body)
    {
        var response = await client.PostAsJsonAsync(Url, body);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Read<Material>(response);
    }

    [Fact]
    public async Task Requires_sign_in()
    {
        var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Url)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(Url, Process())).StatusCode);
    }

    [Fact]
    public async Task Create_get_and_list()
    {
        var client = await SignedInClient();

        var created = await Create(client, Process());
        var loaded = await Read<Material>(await client.GetAsync($"{Url}/{created.Id}"));
        var list = await Read<List<Material>>(await client.GetAsync(Url));

        Assert.Equal("process", loaded.TemplateKey);
        Assert.Equal("Fotosintez", loaded.Title);
        Assert.Equal("Biologiya", loaded.Subject);
        Assert.Equal(7, loaded.Grade);
        Assert.Equal(3, loaded.Content.Items!.Count);
        Assert.Single(list);
    }

    [Fact]
    public async Task Update_saves_changes_but_keeps_template()
    {
        var client = await SignedInClient();
        var created = await Create(client, Process());

        var response = await client.PutAsJsonAsync($"{Url}/{created.Id}", new
        {
            templateKey = "timeline",
            title = "Yangi sarlavha",
            grade = (int?)null,
            content = new { items = new[] { new { heading = "Bir" }, new { heading = "Ikki" } } },
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await Read<Material>(response);
        Assert.Equal("Yangi sarlavha", updated.Title);
        Assert.Equal("process", updated.TemplateKey);
        Assert.Null(updated.Subject);
        Assert.Equal(2, updated.Content.Items!.Count);
    }

    [Fact]
    public async Task Materials_of_other_users_are_invisible()
    {
        var owner = await SignedInClient();
        var other = await SignedInClient();
        var created = await Create(owner, Process());

        Assert.Equal(HttpStatusCode.NotFound, (await other.GetAsync($"{Url}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PutAsJsonAsync($"{Url}/{created.Id}", Process("Buzildi"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.DeleteAsync($"{Url}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await other.PostAsync($"{Url}/{created.Id}/duplicate", null)).StatusCode);
        Assert.Empty(await Read<List<Material>>(await other.GetAsync(Url)));

        var untouched = await Read<Material>(await owner.GetAsync($"{Url}/{created.Id}"));
        Assert.Equal("Fotosintez", untouched.Title);
    }

    [Fact]
    public async Task Duplicate_creates_independent_copy()
    {
        var client = await SignedInClient();
        var created = await Create(client, Process());

        var response = await client.PostAsync($"{Url}/{created.Id}/duplicate", null);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var copy = await Read<Material>(response);

        Assert.NotEqual(created.Id, copy.Id);
        Assert.Equal("Fotosintez (nusxa)", copy.Title);
        Assert.Equal(2, (await Read<List<Material>>(await client.GetAsync(Url))).Count);
    }

    [Fact]
    public async Task Delete_removes_material()
    {
        var client = await SignedInClient();
        var created = await Create(client, Process());

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"{Url}/{created.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Url}/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Comparison_requires_cells_matching_columns()
    {
        var client = await SignedInClient();

        var valid = new
        {
            templateKey = "comparison",
            title = "Taqqoslash",
            content = new
            {
                columns = new[] { "A", "B" },
                items = new[] { new { heading = "Mezon", cells = new[] { "1", "2" } } },
            },
        };
        await Create(client, valid);

        var invalid = new
        {
            templateKey = "comparison",
            title = "Taqqoslash",
            content = new
            {
                columns = new[] { "A", "B" },
                items = new[] { new { heading = "Mezon", cells = new[] { "1" } } },
            },
        };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Url, invalid)).StatusCode);
    }

    public static TheoryData<object> InvalidBodies => new()
    {
        new { templateKey = "yoq", title = "T", content = new { items = new[] { new { heading = "A" }, new { heading = "B" } } } },
        new { templateKey = "process", title = "  ", content = new { items = new[] { new { heading = "A" }, new { heading = "B" } } } },
        new { templateKey = "process", title = "T", content = new { items = new[] { new { heading = "A" } } } },
        new { templateKey = "process", title = "T", content = new { items = new[] { new { heading = "A" }, new { heading = "  " } } } },
        new { templateKey = "process", title = "T", content = new { items = new[] { new { heading = new string('a', 81) }, new { heading = "B" } } } },
        new { templateKey = "process", title = "T", grade = 12, content = new { items = new[] { new { heading = "A" }, new { heading = "B" } } } },
        new { templateKey = "process", title = "T", content = new { accent = "qizil", items = new[] { new { heading = "A" }, new { heading = "B" } } } },
        new { templateKey = "process", title = "T" },
    };

    [Theory]
    [MemberData(nameof(InvalidBodies))]
    public async Task Invalid_material_is_rejected(object body)
    {
        var client = await SignedInClient();

        var response = await client.PostAsync(Url, JsonContent.Create(body, body.GetType()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_material", (await Read<Problem>(response)).Code);
    }

    [Fact]
    public async Task Too_many_items_are_rejected()
    {
        var client = await SignedInClient();

        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(Url, Process(items: 9))).StatusCode);
    }
}
