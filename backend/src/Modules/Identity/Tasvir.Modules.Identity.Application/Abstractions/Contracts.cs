using Tasvir.Modules.Identity.Domain;

namespace Tasvir.Modules.Identity.Application.Abstractions;

public interface IIdentityRepository
{
    Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken ct);
    Task<User?> FindUserByIdAsync(Guid id, CancellationToken ct);
    /// <summary>Yangi foydalanuvchini saqlaydi. Email band bo‘lsa (poyga holatida ham) false qaytaradi.</summary>
    Task<bool> TryAddUserAsync(User user, CancellationToken ct);

    Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash, CancellationToken ct);
    void AddRefreshToken(RefreshToken token);
    Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct);
    Task RevokeAllSessionsAsync(Guid userId, DateTime now, CancellationToken ct);
    Task DeleteExpiredRefreshTokensAsync(Guid userId, DateTime now, CancellationToken ct);

    Task<OneTimeToken?> FindOneTimeTokenAsync(string tokenHash, OneTimeTokenPurpose purpose, CancellationToken ct);
    /// <summary>Shu maqsaddagi avvalgi havolalarni bekor qilib, yangisini qo‘shadi.</summary>
    Task ReplaceOneTimeTokenAsync(OneTimeToken token, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string passwordHash, string password);
}

public sealed record AccessToken(string Value, DateTime ExpiresAt);

public interface ITokenService
{
    AccessToken CreateAccessToken(User user, DateTime now);
    /// <summary>Kriptografik tasodifiy, URL uchun xavfsiz token.</summary>
    string CreateOpaqueToken();
    string Hash(string token);
}

public sealed record EmailMessage(string To, string Subject, string TextBody);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct);
}

public sealed class IdentityOptions
{
    public const string Section = "Identity";

    /// <summary>Emaildagi havolalar uchun frontend manzili.</summary>
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public int AccessTokenMinutes { get; set; } = 15;
    /// <summary>"Meni eslab qolish" belgilanganda.</summary>
    public int PersistentSessionDays { get; set; } = 30;
    public int SessionHours { get; set; } = 24;
    public int EmailConfirmationHours { get; set; } = 48;
    public int PasswordResetMinutes { get; set; } = 60;
    /// <summary>Bir vaqtdagi so‘rovlar eski tokenni qayta yuborganda o‘g‘irlik deb hisoblanmaydigan oraliq.</summary>
    public int RefreshReuseGraceSeconds { get; set; } = 30;
}
