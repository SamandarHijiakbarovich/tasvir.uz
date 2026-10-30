namespace Tasvir.Modules.Content.Application.Landing;

// API shartnomasi (contract). Frontenddagi src/features/landing/api/types.ts bilan mos bo‘lishi kerak.

public sealed record LandingContentDto(
    IReadOnlyList<PartnerDto> Partners,
    ResultsDto Results,
    IReadOnlyList<PricingPlanDto> PricingPlans,
    IReadOnlyList<TestimonialDto> Testimonials,
    IReadOnlyList<FaqItemDto> Faq);

public sealed record PartnerDto(string Name, string? LogoUrl);

public sealed record ResultsDto(IReadOnlyList<ResultCriterionDto> Criteria, IReadOnlyList<ResultStatDto> Stats);

public sealed record ResultCriterionDto(string Label, decimal? ExperimentValue, decimal? ControlValue);

public sealed record ResultStatDto(string Label, decimal? Value, string? Suffix);

public sealed record PricingPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal? Price,
    string PriceUnit,
    IReadOnlyList<string> Features,
    bool IsFeatured,
    string CtaText,
    string CtaHref);

public sealed record TestimonialDto(string Quote, string AuthorName, string AuthorRole, string? AvatarUrl, bool IsVerified);

public sealed record FaqItemDto(Guid Id, string Question, string Answer);
