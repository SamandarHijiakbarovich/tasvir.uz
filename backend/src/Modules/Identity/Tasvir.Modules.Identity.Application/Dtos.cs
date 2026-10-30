namespace Tasvir.Modules.Identity.Application;

public sealed record RegisterRequest(string? FullName, string? Email, string? Password);
public sealed record LoginRequest(string? Email, string? Password, bool RememberMe);
public sealed record RefreshRequest(string? RefreshToken);
public sealed record ConfirmEmailRequest(string? Token);
public sealed record ForgotPasswordRequest(string? Email);
public sealed record ResetPasswordRequest(string? Token, string? Password);
public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);

public sealed record UserDto(Guid Id, string FullName, string Email, bool EmailConfirmed, DateTime CreatedAt);

public sealed record AuthSessionDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    bool IsPersistent,
    UserDto User);
