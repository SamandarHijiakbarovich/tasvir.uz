using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Tasvir.Modules.Studio.Tests;

/// <summary>Kabinetdagi "Mening materiallarim": fayl va havolalar, ochiq ulashish havolalari.</summary>
public sealed class LibraryApiTests(StudioApiFactory factory) : IClassFixture<StudioApiFactory>
{
    private const string Url = "/api/studio/library";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken);
    private sealed record Item(Guid Id, string Kind, string Title, string? Subject, string? Topic, string? Url, string? FileName, string? ContentType, long SizeBytes, string? ShareToken);
    private sealed record Library(List<Item> Items, long UsedBytes, long QuotaBytes, long MaxFileBytes);
    private sealed record Share(string Kind, Guid Id, string Token);
    private sealed record SharedMaterial(Guid Id, string Title);
    private sealed record Shared(string Kind, SharedMaterial? Material, Item? Item);
    private sealed record Problem(string Code);

    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj << >> endobj\ntrailer << >>\n%%EOF");

    private async Task<HttpClient> SignedInClient()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Test Öqituvçi",
            email = $"kutubxona-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
            acceptPrivacy = true,
        });
        var session = (await response.Content.ReadFromJsonAsync<Session>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private static object File(string fileName, byte[] data, string title = "Fotosintez taqdimoti") => new
    {
        title,
        subject = "Biologiya",
        topic = "Fotosintez",
        fileName,
        dataBase64 = Convert.ToBase64String(data),
    };

    private static async Task<T> Read<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    [Fact]
    public async Task Requires_sign_in()
    {
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(Url)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/studio/shares", new { kind = "item", id = Guid.NewGuid() })).StatusCode);
    }

    [Fact]
    public async Task File_is_saved_listed_and_returned_inline()
    {
        var client = await SignedInClient();

        var created = await client.PostAsJsonAsync($"{Url}/files", File("dars.pdf", Pdf));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var item = await Read<Item>(created);
        Assert.Equal("file", item.Kind);
        Assert.Equal("application/pdf", item.ContentType);
        Assert.Equal(Pdf.Length, item.SizeBytes);

        var library = await Read<Library>(await client.GetAsync(Url));
        Assert.Equal(item.Id, Assert.Single(library.Items).Id);
        Assert.Equal(Pdf.Length, library.UsedBytes);

        var file = await client.GetAsync($"{Url}/{item.Id}/file");
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        Assert.Equal("application/pdf", file.Content.Headers.ContentType!.MediaType);
        Assert.Equal("inline", file.Content.Headers.ContentDisposition!.DispositionType);
        Assert.Equal(Pdf, await file.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("sahifa.html")]
    [InlineData("rasm.svg")]
    [InlineData("dastur.exe")]
    public async Task Unsafe_file_types_are_rejected(string fileName)
    {
        var client = await SignedInClient();
        var response = await client.PostAsJsonAsync($"{Url}/files", File(fileName, Encoding.UTF8.GetBytes("<script>alert(1)</script>")));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Content_must_match_extension()
    {
        var client = await SignedInClient();
        // .pdf deb nomlangan HTML
        var response = await client.PostAsJsonAsync($"{Url}/files", File("dars.pdf", Encoding.UTF8.GetBytes("<html><script>alert(1)</script></html>")));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Too_large_file_is_rejected()
    {
        var client = await SignedInClient();
        var big = new byte[4 * 1024 * 1024 + 1];
        Pdf.CopyTo(big, 0);
        var response = await client.PostAsJsonAsync($"{Url}/files", File("katta.pdf", big));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Link_is_saved_and_updated()
    {
        var client = await SignedInClient();
        var created = await Read<Item>(await client.PostAsJsonAsync($"{Url}/links", new
        {
            title = "Suv aylanişi",
            subject = "Geografiya",
            topic = "Gidrosfera",
            url = "https://docs.google.com/presentation/d/abc/edit",
        }));
        Assert.Equal("link", created.Kind);

        var updated = await client.PutAsJsonAsync($"{Url}/{created.Id}", new
        {
            title = "Suvning tabiatda aylanişi",
            subject = "Geografiya",
            topic = "Gidrosfera",
            url = "https://www.canva.com/design/xyz/view",
        });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var item = await Read<Item>(updated);
        Assert.Equal("Suvning tabiatda aylanişi", item.Title);
        Assert.Equal("https://www.canva.com/design/xyz/view", item.Url);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/a")]
    [InlineData("example.com")]
    public async Task Unsafe_links_are_rejected(string url)
    {
        var client = await SignedInClient();
        var response = await client.PostAsJsonAsync($"{Url}/links", new { title = "Havola", url });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Items_of_other_users_are_invisible()
    {
        var owner = await SignedInClient();
        var item = await Read<Item>(await owner.PostAsJsonAsync($"{Url}/files", File("dars.pdf", Pdf)));

        var stranger = await SignedInClient();
        Assert.Empty((await Read<Library>(await stranger.GetAsync(Url))).Items);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.GetAsync($"{Url}/{item.Id}/file")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.DeleteAsync($"{Url}/{item.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.PostAsJsonAsync("/api/studio/shares", new { kind = "item", id = item.Id })).StatusCode);
    }

    [Fact]
    public async Task Shared_file_opens_without_sign_in_until_unshared()
    {
        var owner = await SignedInClient();
        var item = await Read<Item>(await owner.PostAsJsonAsync($"{Url}/files", File("dars.pdf", Pdf)));

        var share = await Read<Share>(await owner.PostAsJsonAsync("/api/studio/shares", new { kind = "item", id = item.Id }));
        Assert.Equal(22, share.Token.Length);
        // Qayta bosilsa, o‘sha havola qaytadi
        Assert.Equal(share.Token, (await Read<Share>(await owner.PostAsJsonAsync("/api/studio/shares", new { kind = "item", id = item.Id }))).Token);
        Assert.Equal(share.Token, Assert.Single((await Read<Library>(await owner.GetAsync(Url))).Items).ShareToken);

        var anonymous = factory.CreateClient();
        var shared = await Read<Shared>(await anonymous.GetAsync($"/api/studio/shared/{share.Token}"));
        Assert.Equal("item", shared.Kind);
        Assert.Equal(item.Id, shared.Item!.Id);
        Assert.Equal(Pdf, await anonymous.GetByteArrayAsync($"/api/studio/shared/{share.Token}/file"));

        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"/api/studio/shares/item/{item.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/studio/shared/{share.Token}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/studio/shared/{share.Token}/file")).StatusCode);
    }

    [Fact]
    public async Task Studio_material_can_be_shared_and_share_is_removed_with_it()
    {
        var owner = await SignedInClient();
        var material = await Read<SharedMaterial>(await owner.PostAsJsonAsync("/api/studio/materials", new
        {
            templateKey = "process",
            title = "Fotosintez",
            content = new { items = new[] { new { heading = "Yoruğlik" }, new { heading = "Glyukoza" } } },
        }));

        var share = await Read<Share>(await owner.PostAsJsonAsync("/api/studio/shares", new { kind = "material", id = material.Id }));
        var anonymous = factory.CreateClient();
        var shared = await Read<Shared>(await anonymous.GetAsync($"/api/studio/shared/{share.Token}"));
        Assert.Equal("material", shared.Kind);
        Assert.Equal("Fotosintez", shared.Material!.Title);
        // Studiya materialida fayl yo‘q
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/studio/shared/{share.Token}/file")).StatusCode);

        await owner.DeleteAsync($"/api/studio/materials/{material.Id}");
        Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/studio/shared/{share.Token}")).StatusCode);
    }

    [Theory]
    [InlineData("qisqa")]
    [InlineData("AAAAAAAAAAAAAAAAAAAAAA")]
    public async Task Unknown_token_is_not_found(string token)
    {
        var anonymous = factory.CreateClient();
        var response = await anonymous.GetAsync($"/api/studio/shared/{token}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("share_not_found", (await Read<Problem>(response)).Code);
    }

    [Fact]
    public async Task Delete_removes_file_and_share()
    {
        var owner = await SignedInClient();
        var item = await Read<Item>(await owner.PostAsJsonAsync($"{Url}/files", File("dars.pdf", Pdf)));
        var share = await Read<Share>(await owner.PostAsJsonAsync("/api/studio/shares", new { kind = "item", id = item.Id }));

        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"{Url}/{item.Id}")).StatusCode);
        Assert.Empty((await Read<Library>(await owner.GetAsync(Url))).Items);
        Assert.Equal(HttpStatusCode.NotFound, (await factory.CreateClient().GetAsync($"/api/studio/shared/{share.Token}")).StatusCode);
    }
}
