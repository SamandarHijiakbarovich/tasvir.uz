using Microsoft.Extensions.Logging;
using Tasvir.Modules.Identity.Application.Abstractions;
using Tasvir.Modules.Identity.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Identity.Application;

public sealed class AuthService(
    IIdentityRepository repository,
    IPasswordService passwords,
    ITokenService tokens,
    IEmailSender emailSender,
    TimeProvider clock,
    IdentityOptions options,
    ILogger<AuthService> logger)
{
    private static readonly Error InvalidCredentials =
        new("invalid_credentials", "Email yoki parol noto‘g‘ri.", ErrorKind.Unauthorized);
    private static readonly Error InvalidSession =
        new("invalid_session", "Sessiya tugagan. Qayta kiring.", ErrorKind.Unauthorized);
    private static readonly Error InvalidLink =
        new("invalid_token", "Havola yaroqsiz yoki muddati o‘tgan.", ErrorKind.Validation);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<AuthSessionDto>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var fields = new Dictionary<string, string>();
        var fullName = request.FullName?.Trim() ?? string.Empty;

        if (fullName.Length is < 2 or > 100)
        {
            fields["fullName"] = "Ism va familiyangizni kiriting (2–100 belgi).";
        }

        if (!Validation.IsValidEmail(request.Email))
        {
            fields["email"] = "Email manzilini to‘g‘ri kiriting.";
        }

        if (Validation.CheckPassword(request.Password, request.Email) is { } passwordError)
        {
            fields["password"] = passwordError;
        }

        if (fields.Count > 0)
        {
            return Validation.Invalid(fields);
        }

        var user = new User(fullName, request.Email!, passwords.Hash(request.Password!), Now);

        if (!await repository.TryAddUserAsync(user, ct))
        {
            return new Error("email_taken", "Bu email bilan hisob allaqachon mavjud.", ErrorKind.Conflict,
                new Dictionary<string, string> { ["email"] = "Bu email band. Kirishni yoki parolni tiklashni sinab ko‘ring." });
        }

        user.RecordLogin(Now);
        var session = await IssueSessionAsync(user, isPersistent: true, familyId: Guid.NewGuid(), ct);
        await SendConfirmationEmailAsync(user, ct);
        return session;
    }

    public async Task<Result<AuthSessionDto>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var password = request.Password ?? string.Empty;

        // Juda uzun parol hech qachon to‘g‘ri bo‘lmaydi — xeshlashga yetkazmaymiz
        if (string.IsNullOrWhiteSpace(request.Email) || password.Length is 0 or > Validation.MaxPasswordLength)
        {
            return InvalidCredentials;
        }

        var user = await repository.FindUserByEmailAsync(User.Normalize(request.Email), ct);
        if (user is null)
        {
            // Javob vaqtidan email bor-yo‘qligini bilib bo‘lmasin
            passwords.Hash(password);
            return InvalidCredentials;
        }

        if (user.IsLockedOut(Now))
        {
            return new Error("locked_out", "Urinishlar juda ko‘p. 15 daqiqadan so‘ng qayta urining yoki parolni tiklang.", ErrorKind.TooManyRequests);
        }

        if (!passwords.Verify(user.PasswordHash, password))
        {
            user.RecordFailedLogin(Now);
            await repository.SaveChangesAsync(ct);
            logger.LogInformation("Kirish muvaffaqiyatsiz: foydalanuvchi {UserId}", user.Id);
            return InvalidCredentials;
        }

        user.RecordLogin(Now);
        return await IssueSessionAsync(user, request.RememberMe, Guid.NewGuid(), ct);
    }

    public async Task<Result<AuthSessionDto>> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return InvalidSession;
        }

        var current = await repository.FindRefreshTokenAsync(tokens.Hash(request.RefreshToken), ct);
        if (current is null || !current.IsUsable(Now))
        {
            return InvalidSession;
        }

        if (current.ConsumedAt is { } consumedAt && Now - consumedAt > TimeSpan.FromSeconds(options.RefreshReuseGraceSeconds))
        {
            // Eski token qayta ishlatildi: u o‘g‘irlangan bo‘lishi mumkin, butun sessiyani yopamiz
            await repository.RevokeFamilyAsync(current.FamilyId, Now, ct);
            logger.LogWarning("Refresh token qayta ishlatildi: foydalanuvchi {UserId}, oila {FamilyId}", current.UserId, current.FamilyId);
            return InvalidSession;
        }

        var user = await repository.FindUserByIdAsync(current.UserId, ct);
        if (user is null)
        {
            return InvalidSession;
        }

        current.Consume(Now);
        return await IssueSessionAsync(user, current.IsPersistent, current.FamilyId, ct, current.ExpiresAt);
    }

    public async Task<Result<Unit>> LogoutAsync(RefreshRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.RefreshToken))
        {
            var current = await repository.FindRefreshTokenAsync(tokens.Hash(request.RefreshToken), ct);
            if (current is not null)
            {
                await repository.RevokeFamilyAsync(current.FamilyId, Now, ct);
            }
        }

        return Unit.Value;
    }

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await repository.FindUserByIdAsync(userId, ct);
        return user is null ? InvalidSession : ToDto(user);
    }

    public async Task<Result<UserDto>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken ct)
    {
        var token = await FindUsableTokenAsync(request.Token, OneTimeTokenPurpose.EmailConfirmation, ct);
        var user = token is null ? null : await repository.FindUserByIdAsync(token.UserId, ct);
        if (user is null)
        {
            return InvalidLink;
        }

        token!.MarkUsed(Now);
        user.ConfirmEmail(Now);
        await repository.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task<Result<Unit>> ResendConfirmationAsync(Guid userId, CancellationToken ct)
    {
        var user = await repository.FindUserByIdAsync(userId, ct);
        if (user is null)
        {
            return InvalidSession;
        }

        if (user.EmailConfirmedAt is null)
        {
            await SendConfirmationEmailAsync(user, ct);
        }

        return Unit.Value;
    }

    /// <summary>Email bazada bor-yo‘qligidan qat’i nazar bir xil javob beradi.</summary>
    public async Task<Result<Unit>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        if (!Validation.IsValidEmail(request.Email))
        {
            return Validation.Invalid(new() { ["email"] = "Email manzilini to‘g‘ri kiriting." });
        }

        var user = await repository.FindUserByEmailAsync(User.Normalize(request.Email!), ct);
        if (user is not null)
        {
            var link = await CreateLinkAsync(user, OneTimeTokenPurpose.PasswordReset, TimeSpan.FromMinutes(options.PasswordResetMinutes), "parolni-yangilash", ct);
            await SendSafelyAsync(new EmailMessage(
                user.Email,
                "Tasvir.uz — parolni tiklash",
                $"""
                Assalomu alaykum, {user.FullName}!

                Parolni tiklash so‘rovini oldik. Yangi parol o‘rnatish uchun havolani oching:
                {link}

                Havola {options.PasswordResetMinutes} daqiqa amal qiladi va bir marta ishlaydi.
                Agar buni siz so‘ramagan bo‘lsangiz, bu xatni e’tiborsiz qoldiring — parolingiz o‘zgarmaydi.
                """), ct);
        }

        return Unit.Value;
    }

    public async Task<Result<Unit>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var token = await FindUsableTokenAsync(request.Token, OneTimeTokenPurpose.PasswordReset, ct);
        var user = token is null ? null : await repository.FindUserByIdAsync(token.UserId, ct);
        if (user is null)
        {
            return InvalidLink;
        }

        if (Validation.CheckPassword(request.Password, user.Email) is { } passwordError)
        {
            return Validation.Invalid(new() { ["password"] = passwordError });
        }

        token!.MarkUsed(Now);
        user.SetPassword(passwords.Hash(request.Password!));
        // Havola emailga kelgani uchun email egasi tasdiqlangan hisoblanadi
        user.ConfirmEmail(Now);
        await repository.RevokeAllSessionsAsync(user.Id, Now, ct);
        await repository.SaveChangesAsync(ct);
        return Unit.Value;
    }

    public async Task<Result<AuthSessionDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await repository.FindUserByIdAsync(userId, ct);
        if (user is null)
        {
            return InvalidSession;
        }

        if (string.IsNullOrEmpty(request.CurrentPassword)
            || request.CurrentPassword.Length > Validation.MaxPasswordLength
            || !passwords.Verify(user.PasswordHash, request.CurrentPassword))
        {
            return Validation.Invalid(new() { ["currentPassword"] = "Joriy parol noto‘g‘ri." });
        }

        if (Validation.CheckPassword(request.NewPassword, user.Email) is { } passwordError)
        {
            return Validation.Invalid(new() { ["newPassword"] = passwordError });
        }

        user.SetPassword(passwords.Hash(request.NewPassword!));
        // Boshqa qurilmalardagi sessiyalar yopiladi, joriy qurilma uchun yangisi ochiladi
        await repository.RevokeAllSessionsAsync(user.Id, Now, ct);
        return await IssueSessionAsync(user, isPersistent: true, Guid.NewGuid(), ct);
    }

    // Yangilashda sessiya muddati uzaymaydi deb emas, balki sirpanuvchi bo‘lmasligi uchun mavjud muddat saqlanadi
    private async Task<AuthSessionDto> IssueSessionAsync(User user, bool isPersistent, Guid familyId, CancellationToken ct, DateTime? expiresAtOverride = null)
    {
        var now = Now;
        var access = tokens.CreateAccessToken(user, now);
        var refreshValue = tokens.CreateOpaqueToken();
        var expiresAt = expiresAtOverride
            ?? (isPersistent ? now.AddDays(options.PersistentSessionDays) : now.AddHours(options.SessionHours));

        await repository.DeleteExpiredRefreshTokensAsync(user.Id, now, ct);
        repository.AddRefreshToken(new RefreshToken(user.Id, familyId, tokens.Hash(refreshValue), isPersistent, now, expiresAt));
        await repository.SaveChangesAsync(ct);

        return new AuthSessionDto(access.Value, access.ExpiresAt, refreshValue, expiresAt, isPersistent, ToDto(user));
    }

    private async Task SendConfirmationEmailAsync(User user, CancellationToken ct)
    {
        var link = await CreateLinkAsync(user, OneTimeTokenPurpose.EmailConfirmation, TimeSpan.FromHours(options.EmailConfirmationHours), "email-tasdiqlash", ct);
        await SendSafelyAsync(new EmailMessage(
            user.Email,
            "Tasvir.uz — emailingizni tasdiqlang",
            $"""
            Assalomu alaykum, {user.FullName}!

            Tasvir.uz’ga xush kelibsiz. Emailingizni tasdiqlash uchun havolani oching:
            {link}

            Havola {options.EmailConfirmationHours} soat amal qiladi.
            Agar siz ro‘yxatdan o‘tmagan bo‘lsangiz, bu xatni e’tiborsiz qoldiring.
            """), ct);
    }

    private async Task<string> CreateLinkAsync(User user, OneTimeTokenPurpose purpose, TimeSpan lifetime, string path, CancellationToken ct)
    {
        var value = tokens.CreateOpaqueToken();
        await repository.ReplaceOneTimeTokenAsync(new OneTimeToken(user.Id, purpose, tokens.Hash(value), Now + lifetime), ct);
        await repository.SaveChangesAsync(ct);
        return $"{options.FrontendUrl.TrimEnd('/')}/{path}?token={value}";
    }

    private async Task<OneTimeToken?> FindUsableTokenAsync(string? value, OneTimeTokenPurpose purpose, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        var token = await repository.FindOneTimeTokenAsync(tokens.Hash(value), purpose, ct);
        return token is not null && token.IsUsable(Now) ? token : null;
    }

    // Pochta serveri ishlamasa ham ro‘yxatdan o‘tish yoki javob buzilmasin — xat qayta so‘ralishi mumkin
    private async Task SendSafelyAsync(EmailMessage message, CancellationToken ct)
    {
        try
        {
            await emailSender.SendAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Email yuborilmadi: {Subject}", message.Subject);
        }
    }

    private static UserDto ToDto(User user) =>
        new(user.Id, user.FullName, user.Email, user.EmailConfirmedAt is not null, user.CreatedAt);
}
