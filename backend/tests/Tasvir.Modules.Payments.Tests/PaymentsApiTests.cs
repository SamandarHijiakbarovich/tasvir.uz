using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Tasvir.Modules.Payments.Application;

namespace Tasvir.Modules.Payments.Tests;

public sealed class PaymentsApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-payments-tests-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        foreach (var module in new[] { "Content", "Identity", "Academy", "Studio", "Classroom", "Payments" })
        {
            builder.UseSetting($"ConnectionStrings:{module}", $"Data Source={Path.Combine(_dir, module.ToLowerInvariant() + ".db")};Pooling=False");
        }

        builder.UseSetting("Identity:RateLimit:PermitPerMinute", "10000");
        // appsettings.json da narx yo‘q (real narxni egasi qo‘yadi): sinov uchun beriladi
        builder.UseSetting("Payments:Plans:0:Price", "49000");
        builder.UseSetting("Payments:Payme:MerchantId", "test-merchant");
        builder.UseSetting("Payments:Payme:Key", TestOptions.PaymeKey);
        // Click kaliti berilmagan: sayt uni "ulanmagan" deb ko‘rsatishi kerak
        builder.UseSetting("Payments:Click:ServiceId", "");
        builder.UseSetting("Payments:Click:SecretKey", "");
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

public sealed class PaymentsApiTests(PaymentsApiFactory factory) : IClassFixture<PaymentsApiFactory>
{
    private const string Api = "/api/payments";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken);
    private sealed record Problem(string Code);

    private async Task<HttpClient> SignIn()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Tölov Sinovçi",
            email = $"tolov-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
            acceptPrivacy = true,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = (await response.Content.ReadFromJsonAsync<Session>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private async Task<JsonObject> Payme(string method, object parameters)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Api}/payme")
        {
            Content = JsonContent.Create(new { jsonrpc = "2.0", id = 1, method, @params = parameters }),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"Paycom:{TestOptions.PaymeKey}")));
        var response = await factory.CreateClient().SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();
    }

    [Fact]
    public async Task Catalog_is_public_and_shows_provider_state()
    {
        var catalog = (await factory.CreateClient().GetFromJsonAsync<CatalogDto>($"{Api}/plans", Json))!;
        Assert.True(catalog.Plans.Single(p => p.Key == "pro-1").IsAvailable);
        // Narxi belgilanmagan tarifni sotib bo‘lmaydi
        Assert.False(catalog.Plans.Single(p => p.Key == "pro-12").IsAvailable);
        Assert.True(catalog.Providers.Single(p => p.Key == "payme").IsAvailable);
        Assert.False(catalog.Providers.Single(p => p.Key == "click").IsAvailable);
    }

    [Fact]
    public async Task Orders_require_sign_in()
    {
        var response = await factory.CreateClient().PostAsJsonAsync($"{Api}/orders", new { planKey = "pro-1", provider = "payme" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unconfigured_provider_and_unknown_plan_are_refused()
    {
        var client = await SignIn();
        var click = await client.PostAsJsonAsync($"{Api}/orders", new { planKey = "pro-1", provider = "click" });
        Assert.Equal(HttpStatusCode.Conflict, click.StatusCode);
        Assert.Equal("provider_unavailable", (await click.Content.ReadFromJsonAsync<Problem>(Json))!.Code);

        var plan = await client.PostAsJsonAsync($"{Api}/orders", new { planKey = "yoq", provider = "payme" });
        Assert.Equal(HttpStatusCode.BadRequest, plan.StatusCode);
    }

    [Fact]
    public async Task Payme_payment_activates_subscription()
    {
        var client = await SignIn();
        var response = await client.PostAsJsonAsync($"{Api}/orders", new { planKey = "pro-1", provider = "payme" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = (await response.Content.ReadFromJsonAsync<CreatedOrderDto>(Json))!;

        // Checkout havolasi: base64 ichida kassa, buyurtma va summa (tiyinda)
        const string checkout = "https://checkout.test.paycom.uz/";
        Assert.StartsWith(checkout, created.PaymentUrl);
        var encoded = created.PaymentUrl[checkout.Length..];
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        Assert.Contains("m=test-merchant", decoded);
        Assert.Contains($"ac.order_id={created.Order.Id:N}", decoded);
        Assert.Contains($"a={(long)(created.Order.Amount * 100)}", decoded);

        // Boshqa foydalanuvchi buyurtmani ko‘ra olmaydi
        var stranger = await SignIn();
        Assert.Equal(HttpStatusCode.NotFound, (await stranger.GetAsync($"{Api}/orders/{created.Order.Id}")).StatusCode);

        var account = new { order_id = created.Order.Id.ToString("N") };
        var amount = (long)(created.Order.Amount * 100);
        Assert.True((bool?)(await Payme("CheckPerformTransaction", new { amount, account }))["result"]?["allow"]);
        var transaction = new { id = $"payme-{Guid.NewGuid():N}", time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), amount, account };
        Assert.Equal(1, (int?)(await Payme("CreateTransaction", transaction))["result"]?["state"]);
        Assert.Equal(2, (int?)(await Payme("PerformTransaction", new { transaction.id }))["result"]?["state"]);

        var order = (await client.GetFromJsonAsync<OrderDto>($"{Api}/orders/{created.Order.Id}", Json))!;
        Assert.Equal("paid", order.Status);

        var mine = (await client.GetFromJsonAsync<MyPaymentsDto>($"{Api}/me", Json))!;
        Assert.True(mine.Subscription.IsActive);
        Assert.Equal("Pro", mine.Subscription.PlanName);
        Assert.Single(mine.Orders);
    }

    [Fact]
    public async Task Payme_without_auth_gets_json_rpc_error()
    {
        var response = await factory.CreateClient().PostAsJsonAsync($"{Api}/payme", new { id = 3, method = "CheckTransaction", @params = new { id = "x" } });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(PaymeMerchantService.Codes.InsufficientPrivilege, (int?)body["error"]?["code"]);
    }

    [Fact]
    public async Task Click_callback_without_keys_answers_with_error()
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string> { ["click_trans_id"] = "1", ["action"] = "0" });
        var response = await factory.CreateClient().PostAsync($"{Api}/click/prepare", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(ClickMerchantService.Codes.BadRequest, (int?)body["error"]);
    }
}
