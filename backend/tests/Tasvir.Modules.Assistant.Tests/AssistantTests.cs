using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Tasvir.Modules.Assistant.Application;

namespace Tasvir.Modules.Assistant.Tests;

public sealed class KnowledgeBaseTests
{
    private readonly KnowledgeBaseAnswerProvider _provider = new();

    [Theory]
    [InlineData("Parolimni unutdim", "Parolni tiklash")]
    [InlineData("parolni qanday tiklasam boladi?", "Parolni tiklash")]
    [InlineData("PAROL ESIMDAN CHIQDI", "Parolni tiklash")]
    [InlineData("Narxi qancha?", "bepul")]
    [InlineData("to‘lov qilish kerakmi", "bepul")]
    [InlineData("to'lov qilish kerakmi", "bepul")]
    [InlineData("tolov qilish kerakmi", "bepul")]
    [InlineData("Yangi alifboda tölov", "bepul")]
    [InlineData("Qaysi shaklni tanlayman?", "bog‘liqlik")]
    [InlineData("Infografika nima?", "bitta asosiy g‘oyani")]
    [InlineData("aqliy xarita nima", "markazdagi")]
    [InlineData("tushunchalar xaritasi nima", "munosabat")]
    [InlineData("Ikki kanalli kodlash nima?", "Paivio")]
    [InlineData("kognitiv yuklama haqida ayt", "Sveller")]
    [InlineData("Sertifikat beriladimi", "ishlab chiqilmoqda")]
    [InlineData("Studiya qachon ishga tushadi", "ishlab chiqilmoqda")]
    [InlineData("Telefonda ishlaydimi", "telefon")]
    [InlineData("salom", "Tasvir.uz yordamchisi")]
    [InlineData("rahmat", "Arziydi")]
    [InlineData("matematika darslari bormi", "1–4-sinf")]
    [InlineData("masala ishlash", "1–4-sinf")]
    [InlineData("video ochilmayapti", "YouTube")]
    [InlineData("sxemani qanday yuklab olaman", "PNG")]
    [InlineData("dars nega yopiq", "ketma-ket")]
    public void Finds_right_answer(string question, string expected)
    {
        var answer = _provider.Answer(question);

        Assert.True(answer.IsMatched, $"Javob topilmadi: {question}");
        Assert.Contains(expected, answer.Answer, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("Toshkentda ertaga ob-havo qanday?")]
    [InlineData("futbol natijalari")]
    [InlineData("kvant fizikasini tushuntir")]
    [InlineData("???")]
    public void Admits_when_it_does_not_know(string question)
    {
        var answer = _provider.Answer(question);

        Assert.False(answer.IsMatched);
        Assert.Contains("javob bera olmayman", answer.Answer);
        Assert.NotEmpty(answer.Suggestions);
    }

    [Theory]
    [InlineData("o‘qituvchi", "oqituvchi")]
    [InlineData("o'qituvchi", "oqituvchi")]
    [InlineData("oʻqituvchi", "oqituvchi")]
    [InlineData("öqituvçi", "oqituvchi")]
    [InlineData("Ta’lim", "talim")]
    public void Normalizes_spelling_variants(string input, string expected)
    {
        Assert.Equal(expected, KnowledgeBaseAnswerProvider.Normalize(input));
    }

    [Fact]
    public void Every_suggestion_gets_a_real_answer()
    {
        var suggestions = _provider.DefaultSuggestions
            .Concat(KnowledgeBase_Questions())
            .Distinct();

        foreach (var question in suggestions)
        {
            Assert.True(_provider.Answer(question).IsMatched, $"Tavsiya javobsiz: {question}");
        }
    }

    // Javob ichidagi "bog‘liq savollar" ham ishlashi kerak
    private IEnumerable<string> KnowledgeBase_Questions() =>
        _provider.DefaultSuggestions.SelectMany(q => _provider.Answer(q).Suggestions);
}

public sealed class AssistantApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-assistant-tests-").FullName;

    public int PermitPerMinute { get; init; } = 10_000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("Assistant:RateLimit:PermitPerMinute", PermitPerMinute.ToString());
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

public sealed class AssistantApiTests(AssistantApiFactory factory) : IClassFixture<AssistantApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Works_without_sign_in()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/assistant/ask", new { question = "Parolimni unutdim" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var answer = await response.Content.ReadFromJsonAsync<AssistantAnswer>(Json);
        Assert.True(answer!.IsMatched);
        Assert.Equal("/parolni-tiklash", answer.Link!.Href);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Rejects_empty_question(string question)
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/assistant/ask", new { question });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_too_long_question()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/assistant/ask", new { question = new string('a', 501) });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Rate_limit_applies()
    {
        await using var limited = new AssistantApiFactory { PermitPerMinute = 2 };
        var client = limited.CreateClient();

        for (var i = 0; i < 2; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/assistant/ask", new { question = "salom" })).StatusCode);
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await client.PostAsJsonAsync("/api/assistant/ask", new { question = "salom" })).StatusCode);
    }
}
