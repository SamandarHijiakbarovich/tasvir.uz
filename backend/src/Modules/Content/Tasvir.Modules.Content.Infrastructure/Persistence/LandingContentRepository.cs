using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Content.Application.Abstractions;

namespace Tasvir.Modules.Content.Infrastructure.Persistence;

internal sealed class LandingContentRepository(ContentDbContext db) : ILandingContentRepository
{
    public async Task<LandingContentSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        // DbContext bir vaqtda bitta so‘rovni bajaradi, shuning uchun so‘rovlar ketma-ket.
        var partners = await db.Partners.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var criteria = await db.ResultCriteria.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var stats = await db.ResultStats.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var plans = await db.PricingPlans.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var testimonials = await db.Testimonials.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);
        var faq = await db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

        return new LandingContentSnapshot(partners, criteria, stats, plans, testimonials, faq);
    }
}
