using Tasvir.Modules.Content.Application.Abstractions;

namespace Tasvir.Modules.Content.Application.Landing;

/// <summary>Use case: landing sahifa uchun barcha dinamik ma’lumotlarni bitta so‘rovda qaytarish.</summary>
public sealed class GetLandingContentHandler(ILandingContentRepository repository)
{
    public async Task<LandingContentDto> HandleAsync(CancellationToken cancellationToken)
    {
        var content = await repository.GetAsync(cancellationToken);

        return new LandingContentDto(
            Partners: content.Partners.Select(p => new PartnerDto(p.Name, p.LogoUrl)).ToList(),
            Results: new ResultsDto(
                Criteria: content.ResultCriteria
                    .Select(c => new ResultCriterionDto(c.Label, c.ExperimentValue, c.ControlValue))
                    .ToList(),
                Stats: content.ResultStats.Select(s => new ResultStatDto(s.Label, s.Value, s.Suffix)).ToList()),
            PricingPlans: content.PricingPlans
                .Select(p => new PricingPlanDto(
                    p.Id, p.Name, p.Description, p.Price, p.PriceUnit, p.Features, p.IsFeatured, p.CtaText, p.CtaHref))
                .ToList(),
            Testimonials: content.Testimonials
                .Select(t => new TestimonialDto(t.Quote, t.AuthorName, t.AuthorRole, t.AvatarUrl, t.IsVerified))
                .ToList(),
            Faq: content.Faq.Select(f => new FaqItemDto(f.Id, f.Question, f.Answer)).ToList());
    }
}
