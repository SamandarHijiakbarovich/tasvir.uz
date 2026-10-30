using Tasvir.SharedKernel;

namespace Tasvir.Modules.Identity.Domain;

/// <summary>
/// Bazada faqat token xeshi saqlanadi. Har yangilashda token almashadi (rotation);
/// bir "oila" bitta kirish sessiyasini bildiradi va o‘g‘irlik aniqlansa butunlay bekor qilinadi.
/// </summary>
public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private set; }
    public Guid FamilyId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public bool IsPersistent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConsumedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { }

    public RefreshToken(Guid userId, Guid familyId, string tokenHash, bool isPersistent, DateTime now, DateTime expiresAt)
    {
        UserId = userId;
        FamilyId = familyId;
        TokenHash = Guard.NotEmpty(tokenHash, nameof(tokenHash));
        IsPersistent = isPersistent;
        CreatedAt = now;
        ExpiresAt = expiresAt;
    }

    public bool IsUsable(DateTime now) => RevokedAt is null && ExpiresAt > now;

    public void Consume(DateTime now) => ConsumedAt ??= now;
}

public enum OneTimeTokenPurpose
{
    EmailConfirmation = 1,
    PasswordReset = 2,
}

/// <summary>Email orqali yuboriladigan bir martalik havola tokeni. Bazada faqat xeshi saqlanadi.</summary>
public sealed class OneTimeToken : Entity
{
    public Guid UserId { get; private set; }
    public OneTimeTokenPurpose Purpose { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    private OneTimeToken() { }

    public OneTimeToken(Guid userId, OneTimeTokenPurpose purpose, string tokenHash, DateTime expiresAt)
    {
        UserId = userId;
        Purpose = purpose;
        TokenHash = Guard.NotEmpty(tokenHash, nameof(tokenHash));
        ExpiresAt = expiresAt;
    }

    public bool IsUsable(DateTime now) => UsedAt is null && ExpiresAt > now;

    public void MarkUsed(DateTime now) => UsedAt = now;
}
