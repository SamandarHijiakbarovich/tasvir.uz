using Tasvir.SharedKernel;

namespace Tasvir.Modules.Identity.Domain;

public sealed class User : Entity
{
    public const int MaxFailedLogins = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime? EmailConfirmedAt { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutEndsAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    /// <summary>Maxfiylik siyosatiga rozilik vaqti. Eski hisoblarda bo‘sh: kabinetda so‘raladi.</summary>
    public DateTime? PrivacyAcceptedAt { get; private set; }

    private User() { }

    public User(string fullName, string email, string passwordHash, DateTime now)
    {
        FullName = Guard.NotEmpty(fullName, nameof(fullName)).Trim();
        Email = Guard.NotEmpty(email, nameof(email)).Trim();
        NormalizedEmail = Normalize(Email);
        PasswordHash = Guard.NotEmpty(passwordHash, nameof(passwordHash));
        CreatedAt = now;
    }

    public static string Normalize(string email) => email.Trim().ToUpperInvariant();

    public bool IsLockedOut(DateTime now) => LockoutEndsAt > now;

    public void RecordFailedLogin(DateTime now)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= MaxFailedLogins)
        {
            LockoutEndsAt = now + LockoutDuration;
            FailedLoginCount = 0;
        }
    }

    public void RecordLogin(DateTime now)
    {
        FailedLoginCount = 0;
        LockoutEndsAt = null;
        LastLoginAt = now;
    }

    public void ConfirmEmail(DateTime now) => EmailConfirmedAt ??= now;

    public void AcceptPrivacy(DateTime now) => PrivacyAcceptedAt ??= now;

    public void SetPassword(string passwordHash)
    {
        PasswordHash = Guard.NotEmpty(passwordHash, nameof(passwordHash));
        FailedLoginCount = 0;
        LockoutEndsAt = null;
    }
}
