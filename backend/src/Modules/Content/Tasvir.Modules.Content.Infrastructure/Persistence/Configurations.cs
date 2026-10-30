using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasvir.Modules.Content.Domain;

namespace Tasvir.Modules.Content.Infrastructure.Persistence;

internal sealed class PricingPlanConfiguration : IEntityTypeConfiguration<PricingPlan>
{
    public void Configure(EntityTypeBuilder<PricingPlan> b)
    {
        b.ToTable("pricing_plans");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(80);
        b.Property(x => x.Description).HasMaxLength(200);
        b.Property(x => x.Price).HasPrecision(12, 2);
        b.Property(x => x.PriceUnit).HasMaxLength(40);
        b.PrimitiveCollection(x => x.Features);
        b.Property(x => x.CtaText).HasMaxLength(60);
        b.Property(x => x.CtaHref).HasMaxLength(300);
        b.HasIndex(x => x.SortOrder);
    }
}

internal sealed class FaqItemConfiguration : IEntityTypeConfiguration<FaqItem>
{
    public void Configure(EntityTypeBuilder<FaqItem> b)
    {
        b.ToTable("faq_items");
        b.HasKey(x => x.Id);
        b.Property(x => x.Question).HasMaxLength(300);
        b.Property(x => x.Answer).HasMaxLength(2000);
        b.HasIndex(x => x.SortOrder);
    }
}

internal sealed class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> b)
    {
        b.ToTable("testimonials");
        b.HasKey(x => x.Id);
        b.Property(x => x.Quote).HasMaxLength(1000);
        b.Property(x => x.AuthorName).HasMaxLength(120);
        b.Property(x => x.AuthorRole).HasMaxLength(200);
        b.Property(x => x.AvatarUrl).HasMaxLength(500);
        b.HasIndex(x => x.SortOrder);
    }
}

internal sealed class ResultCriterionConfiguration : IEntityTypeConfiguration<ResultCriterion>
{
    public void Configure(EntityTypeBuilder<ResultCriterion> b)
    {
        b.ToTable("result_criteria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Label).HasMaxLength(120);
        b.Property(x => x.ExperimentValue).HasPrecision(5, 2);
        b.Property(x => x.ControlValue).HasPrecision(5, 2);
        b.HasIndex(x => x.SortOrder);
    }
}

internal sealed class ResultStatConfiguration : IEntityTypeConfiguration<ResultStat>
{
    public void Configure(EntityTypeBuilder<ResultStat> b)
    {
        b.ToTable("result_stats");
        b.HasKey(x => x.Id);
        b.Property(x => x.Label).HasMaxLength(160);
        b.Property(x => x.Value).HasPrecision(12, 2);
        b.Property(x => x.Suffix).HasMaxLength(10);
        b.HasIndex(x => x.SortOrder);
    }
}

internal sealed class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> b)
    {
        b.ToTable("partners");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(160);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.HasIndex(x => x.SortOrder);
    }
}
