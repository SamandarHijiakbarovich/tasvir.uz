using Tasvir.SharedKernel;

namespace Tasvir.Modules.Content.Domain;

/// <summary>Narxlar bo‘limidagi tarif. Narx noma’lum bo‘lsa <see cref="Price"/> = null ("—" ko‘rsatiladi).</summary>
public sealed class PricingPlan : Entity, IOrdered
{
    private PricingPlan() { }

    public PricingPlan(
        string name,
        string description,
        decimal? price,
        string priceUnit,
        IEnumerable<string> features,
        bool isFeatured,
        string ctaText,
        string ctaHref,
        int sortOrder)
    {
        Name = Guard.NotEmpty(name, nameof(name));
        Description = Guard.NotEmpty(description, nameof(description));
        Price = Guard.NonNegative(price, nameof(price));
        PriceUnit = Guard.NotEmpty(priceUnit, nameof(priceUnit));
        Features = features.Select(f => Guard.NotEmpty(f, nameof(features))).ToList();
        IsFeatured = isFeatured;
        CtaText = Guard.NotEmpty(ctaText, nameof(ctaText));
        CtaHref = Guard.NotEmpty(ctaHref, nameof(ctaHref));
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public decimal? Price { get; private set; }
    public string PriceUnit { get; private set; } = null!;
    public List<string> Features { get; private set; } = [];
    public bool IsFeatured { get; private set; }
    public string CtaText { get; private set; } = null!;
    public string CtaHref { get; private set; } = null!;
    public int SortOrder { get; private set; }

    public void SetPrice(decimal? price) => Price = Guard.NonNegative(price, nameof(price));
}
