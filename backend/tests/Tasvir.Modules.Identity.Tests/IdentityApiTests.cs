using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasvir.Modules.Identity.Application.Abstractions;

namespace Tasvir.Modules.Identity.Tests;

public sealed class TestClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;
    public override DateTimeOffset GetUtcNow() => Now;
    public void Advance(TimeSpan by) => Now += by;
}

public sealed class CapturingEmailSender : IEmailSender
{
    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    public Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        Sent.Enqueue(message);
        return Task.CompletedTask;
    }

    public string LastToken(string to) =>
        Regex.Match(Sent.Last(m => m.To == to).TextBody, @"token=([\w-]+)").Groups[1].Value;
}

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-tests-").FullName;

    public int PermitPerMinute { get; init; } = 10_000;
    public TestClock Clock { get; } = new();
    public CapturingEmailSender Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("Identity:RateLimit:PermitPerMinute", PermitPerMinute.ToString());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(Emails);
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);
        });
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

public sealed class IdentityApiTests : IClassFixture<IdentityApiFactory>
{
    private const string Password = "Yaxshi-parol-2026";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record UserBody(Guid Id, string FullName, string Email, bool EmailConfirmed);
    private sealed record SessionBody(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt, bool IsPersistent, UserBody User);
    private sealed record ProblemBody(string Title, string Code, Dictionary<string, string>? Errors);

    private readonly IdentityApiFactory factory;
    private readonly HttpClient _client;

    public IdentityApiTests(IdentityApiFactory factory)
    {
        this.factory = factory;
        _client = factory.CreateClient();
        // JWT tekshiruvi haqiqiy soatdan foydalanadi: oldingi test surgan soat keyingisiga ta’sir qilmasin
        factory.Clock.Now = DateTimeOffset.UtcNow;
    }

    private static string NewEmail() => $"oqituvchi-{Guid.NewGuid():N}@example.com";

    private Task<HttpResponseMessage> Post(string path, object body) =>
        _client.PostAsJsonAsync($"/api/identity/{path}", body);

    private async Task<SessionBody> Register(string email, string password = Password)
    {
        var response = await Post("register", new { fullName = "Test O‘qituvchi", email, password });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<SessionBody>(Json))!;
    }

    private async Task<HttpResponseMessage> GetMe(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/identity/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return await _client.SendAsync(request);
    }

    private static async Task<ProblemBody> Problem(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ProblemBody>(Json))!;

    [Fact]
    public async Task Register_opens_session_and_sends_confirmation_email()
    {
        var email = NewEmail();
        var session = await Register(email);

        Assert.Equal(email, session.User.Email);
        Assert.False(session.User.EmailConfirmed);
        Assert.Contains(factory.Emails.Sent, m => m.To == email && m.TextBody.Contains("/email-tasdiqlash?token="));

        var me = await GetMe(session.AccessToken);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Register_reports_field_errors()
    {
        var response = await Post("register", new { fullName = "A", email = "email-emas", password = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await Problem(response);
        Assert.Equal("validation_failed", problem.Code);
        Assert.Equal(["email", "fullName", "password"], problem.Errors!.Keys.Order());
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("aaaaaaaaaa")]
    public async Task Register_rejects_weak_passwords(string password)
    {
        var response = await Post("register", new { fullName = "Test Foydalanuvchi", email = NewEmail(), password });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("password", (await Problem(response)).Errors!.Keys);
    }

    [Fact]
    public async Task Register_rejects_duplicate_email_ignoring_case()
    {
        var email = NewEmail();
        await Register(email);

        var response = await Post("register", new { fullName = "Boshqa Odam", email = email.ToUpperInvariant(), password = Password });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("email_taken", (await Problem(response)).Code);
    }

    [Fact]
    public async Task Login_gives_same_error_for_wrong_password_and_unknown_email()
    {
        var email = NewEmail();
        await Register(email);

        var wrong = await Post("login", new { email, password = "Noto‘g‘ri-parol-1" });
        var unknown = await Post("login", new { email = NewEmail(), password = Password });

        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal((await Problem(wrong)).Title, (await Problem(unknown)).Title);
    }

    [Fact]
    public async Task Login_without_remember_me_gets_short_session()
    {
        var email = NewEmail();
        await Register(email);

        var response = await Post("login", new { email, password = Password, rememberMe = false });
        var session = (await response.Content.ReadFromJsonAsync<SessionBody>(Json))!;

        Assert.False(session.IsPersistent);
        Assert.True(session.RefreshTokenExpiresAt < factory.Clock.Now.UtcDateTime.AddDays(2));
    }

    [Fact]
    public async Task Account_locks_after_five_failures_and_unlocks_later()
    {
        var email = NewEmail();
        await Register(email);

        for (var i = 0; i < 5; i++)
        {
            await Post("login", new { email, password = "Noto‘g‘ri-parol-1" });
        }

        var locked = await Post("login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.TooManyRequests, locked.StatusCode);
        Assert.Equal("locked_out", (await Problem(locked)).Code);

        factory.Clock.Advance(TimeSpan.FromMinutes(16));
        var afterLockout = await Post("login", new { email, password = Password });
        Assert.Equal(HttpStatusCode.OK, afterLockout.StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_token()
    {
        var session = await Register(NewEmail());

        var response = await Post("refresh", new { refreshToken = session.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var next = (await response.Content.ReadFromJsonAsync<SessionBody>(Json))!;
        Assert.NotEqual(session.RefreshToken, next.RefreshToken);
    }

    [Fact]
    public async Task Refresh_allows_concurrent_reuse_within_grace_period()
    {
        var session = await Register(NewEmail());

        var first = await Post("refresh", new { refreshToken = session.RefreshToken });
        var second = await Post("refresh", new { refreshToken = session.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
    }

    [Fact]
    public async Task Refresh_reuse_after_grace_period_revokes_whole_family()
    {
        var session = await Register(NewEmail());
        var rotated = (await (await Post("refresh", new { refreshToken = session.RefreshToken }))
            .Content.ReadFromJsonAsync<SessionBody>(Json))!;

        factory.Clock.Advance(TimeSpan.FromMinutes(1));

        var reused = await Post("refresh", new { refreshToken = session.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);

        // Yangi token ham bekor bo‘lgan: o‘g‘ri ham, egasi ham qayta kirishi kerak
        var afterTheft = await Post("refresh", new { refreshToken = rotated.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, afterTheft.StatusCode);
    }

    [Fact]
    public async Task Refresh_fails_after_expiry_and_does_not_extend_session()
    {
        var session = await Register(NewEmail());

        factory.Clock.Advance(TimeSpan.FromDays(29));
        var rotated = await Post("refresh", new { refreshToken = session.RefreshToken });
        var next = (await rotated.Content.ReadFromJsonAsync<SessionBody>(Json))!;
        Assert.Equal(session.RefreshTokenExpiresAt, next.RefreshTokenExpiresAt);

        factory.Clock.Advance(TimeSpan.FromDays(2));
        var expired = await Post("refresh", new { refreshToken = next.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, expired.StatusCode);
    }

    [Fact]
    public async Task Logout_revokes_session()
    {
        var session = await Register(NewEmail());

        var logout = await Post("logout", new { refreshToken = session.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        var refresh = await Post("refresh", new { refreshToken = session.RefreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Me_requires_valid_token()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync("/api/identity/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await GetMe("soxta-token")).StatusCode);
    }

    [Fact]
    public async Task Email_confirmation_works_once()
    {
        var email = NewEmail();
        var session = await Register(email);
        var token = factory.Emails.LastToken(email);

        var confirmed = await Post("email/confirm", new { token });
        Assert.Equal(HttpStatusCode.OK, confirmed.StatusCode);
        Assert.True((await confirmed.Content.ReadFromJsonAsync<UserBody>(Json))!.EmailConfirmed);

        var again = await Post("email/confirm", new { token });
        Assert.Equal(HttpStatusCode.BadRequest, again.StatusCode);

        var me = await (await GetMe(session.AccessToken)).Content.ReadFromJsonAsync<UserBody>(Json);
        Assert.True(me!.EmailConfirmed);
    }

    [Fact]
    public async Task Forgot_password_answers_the_same_for_unknown_email()
    {
        var unknownEmail = NewEmail();

        var response = await Post("password/forgot", new { email = unknownEmail });

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.DoesNotContain(factory.Emails.Sent, m => m.To == unknownEmail);
    }

    [Fact]
    public async Task Reset_password_changes_password_and_closes_sessions()
    {
        var email = NewEmail();
        var session = await Register(email);
        const string newPassword = "Yangi-parol-2026";

        await Post("password/forgot", new { email });
        var token = factory.Emails.LastToken(email);

        var weak = await Post("password/reset", new { token, password = "123" });
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);

        var reset = await Post("password/reset", new { token, password = newPassword });
        Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Post("refresh", new { refreshToken = session.RefreshToken })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await Post("login", new { email, password = Password })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Post("login", new { email, password = newPassword })).StatusCode);

        var reused = await Post("password/reset", new { token, password = "Yana-bir-parol-2026" });
        Assert.Equal(HttpStatusCode.BadRequest, reused.StatusCode);
    }

    [Fact]
    public async Task Reset_link_expires()
    {
        var email = NewEmail();
        await Register(email);
        await Post("password/forgot", new { email });
        var token = factory.Emails.LastToken(email);

        factory.Clock.Advance(TimeSpan.FromHours(2));

        var response = await Post("password/reset", new { token, password = "Yangi-parol-2026" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_token", (await Problem(response)).Code);
    }

    [Fact]
    public async Task Confirmation_token_cannot_reset_password()
    {
        var email = NewEmail();
        await Register(email);
        var confirmationToken = factory.Emails.LastToken(email);

        var response = await Post("password/reset", new { token = confirmationToken, password = "Yangi-parol-2026" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Change_password_requires_current_password_and_closes_other_sessions()
    {
        var email = NewEmail();
        var session = await Register(email);
        const string newPassword = "Yangi-parol-2026";

        using var wrong = new HttpRequestMessage(HttpMethod.Post, "/api/identity/password/change")
        {
            Content = JsonContent.Create(new { currentPassword = "Noto‘g‘ri-parol-1", newPassword }),
        };
        wrong.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var rejected = await _client.SendAsync(wrong);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains("currentPassword", (await Problem(rejected)).Errors!.Keys);

        using var right = new HttpRequestMessage(HttpMethod.Post, "/api/identity/password/change")
        {
            Content = JsonContent.Create(new { currentPassword = Password, newPassword }),
        };
        right.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        var changed = await _client.SendAsync(right);
        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        var next = (await changed.Content.ReadFromJsonAsync<SessionBody>(Json))!;

        Assert.Equal(HttpStatusCode.Unauthorized, (await Post("refresh", new { refreshToken = session.RefreshToken })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await Post("refresh", new { refreshToken = next.RefreshToken })).StatusCode);
    }

    [Fact]
    public async Task Tokens_are_not_stored_in_plain_text()
    {
        var email = NewEmail();
        var session = await Register(email);

        using var scope = factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();

        Assert.Null(await repository.FindRefreshTokenAsync(session.RefreshToken, CancellationToken.None));
        var user = await repository.FindUserByEmailAsync(email.ToUpperInvariant(), CancellationToken.None);
        Assert.DoesNotContain(Password, user!.PasswordHash);
    }

    [Fact]
    public async Task Rate_limit_stops_password_guessing()
    {
        await using var limited = new IdentityApiFactory { PermitPerMinute = 3 };
        var client = limited.CreateClient();
        var body = new { email = NewEmail(), password = "Noto‘g‘ri-parol-1" };

        for (var i = 0; i < 3; i++)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/identity/login", body)).StatusCode);
        }

        var blocked = await client.PostAsJsonAsync("/api/identity/login", body);
        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.Equal("rate_limited", (await Problem(blocked)).Code);
    }
}
