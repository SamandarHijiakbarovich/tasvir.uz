using Tasvir.Modules.Content.Domain;

namespace Tasvir.Modules.Content.Application.Abstractions;

/// <summary>Landing sahifa ma’lumotlarini o‘qish porti. Amalga oshirilishi Infrastructure qatlamida.</summary>
public interface ILandingContentRepository
{
    Task<LandingContentSnapshot> GetAsync(CancellationToken cancellationToken);
}

public sealed record LandingContentSnapshot(
    IReadOnlyList<Partner> Partners,
    IReadOnlyList<ResultCriterion> ResultCriteria,
    IReadOnlyList<ResultStat> ResultStats,
    IReadOnlyList<PricingPlan> PricingPlans,
    IReadOnlyList<Testimonial> Testimonials,
    IReadOnlyList<FaqItem> Faq);
