using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Tasvir.Modules.Identity.Application.Abstractions;
using Tasvir.Modules.Identity.Domain;

namespace Tasvir.Modules.Identity.Infrastructure.Security;

/// <summary>ASP.NET Core’ning tekshirilgan PBKDF2 xeshlovchisi; o‘zimizning kriptografiya yozilmaydi.</summary>
internal sealed class PasswordService : IPasswordService
{
    private static readonly object Subject = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Subject, password);

    public bool Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(Subject, passwordHash, password) != PasswordVerificationResult.Failed;
}

public sealed class JwtOptions
{
    public const string Section = "Identity:Jwt";

    public string Issuer { get; set; } = "tasvir.uz";
    public string Audience { get; set; } = "tasvir.uz";
    /// <summary>Kamida 32 baytli maxfiy kalit. Repozitoriyda saqlanmaydi.</summary>
    public string SigningKey { get; set; } = string.Empty;
}

internal sealed class TokenService(JwtOptions jwt, Application.Abstractions.IdentityOptions options) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();
    private readonly SigningCredentials _credentials =
        new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256);

    public AccessToken CreateAccessToken(User user, DateTime now)
    {
        var expiresAt = now.AddMinutes(options.AccessTokenMinutes);
        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now,
            NotBefore = now,
            Expires = expiresAt,
            SigningCredentials = _credentials,
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            ]),
        });
        return new AccessToken(token, expiresAt);
    }

    public string CreateOpaqueToken() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));

    // Token tasodifiy 256 bit bo‘lgani uchun tuzsiz SHA-256 yetarli
    public string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
