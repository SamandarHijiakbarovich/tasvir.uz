using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Identity.Application.Abstractions;
using Tasvir.Modules.Identity.Domain;

namespace Tasvir.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityRepository(IdentityModuleDbContext db) : IIdentityRepository
{
    public Task<User?> FindUserByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);

    public Task<User?> FindUserByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> TryAddUserAsync(User user, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.NormalizedEmail == user.NormalizedEmail, ct))
        {
            return false;
        }

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Ikki so‘rov bir vaqtda kelganda noyob indeks ushlab qoladi
            db.Entry(user).State = EntityState.Detached;
            return false;
        }
    }

    public Task<RefreshToken?> FindRefreshTokenAsync(string tokenHash, CancellationToken ct) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public void AddRefreshToken(RefreshToken token) => db.RefreshTokens.Add(token);

    public Task RevokeFamilyAsync(Guid familyId, DateTime now, CancellationToken ct) =>
        db.RefreshTokens
            .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

    public Task RevokeAllSessionsAsync(Guid userId, DateTime now, CancellationToken ct) =>
        db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);

    public Task DeleteExpiredRefreshTokensAsync(Guid userId, DateTime now, CancellationToken ct) =>
        db.RefreshTokens
            .Where(t => t.UserId == userId && t.ExpiresAt <= now)
            .ExecuteDeleteAsync(ct);

    public Task<OneTimeToken?> FindOneTimeTokenAsync(string tokenHash, OneTimeTokenPurpose purpose, CancellationToken ct) =>
        db.OneTimeTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.Purpose == purpose, ct);

    public async Task ReplaceOneTimeTokenAsync(OneTimeToken token, CancellationToken ct)
    {
        await db.OneTimeTokens
            .Where(t => t.UserId == token.UserId && t.Purpose == token.Purpose)
            .ExecuteDeleteAsync(ct);
        db.OneTimeTokens.Add(token);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
