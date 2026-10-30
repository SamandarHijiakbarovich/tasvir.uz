using Tasvir.SharedKernel;

namespace Tasvir.Modules.Content.Domain;

/// <summary>Tajriba-sinov mezoni: tajriba va nazorat guruhlari natijasi (foizda). Ma’lumot hali yo‘q bo‘lsa — null.</summary>
public sealed class ResultCriterion : Entity, IOrdered
{
    private ResultCriterion() { }

    public ResultCriterion(string label, decimal? experimentValue, decimal? controlValue, int sortOrder)
    {
        Label = Guard.NotEmpty(label, nameof(label));
        ExperimentValue = Percent(experimentValue, nameof(experimentValue));
        ControlValue = Percent(controlValue, nameof(controlValue));
        SortOrder = sortOrder;
    }

    public string Label { get; private set; } = null!;
    public decimal? ExperimentValue { get; private set; }
    public decimal? ControlValue { get; private set; }
    public int SortOrder { get; private set; }

    public void Record(decimal? experimentValue, decimal? controlValue)
    {
        ExperimentValue = Percent(experimentValue, nameof(experimentValue));
        ControlValue = Percent(controlValue, nameof(controlValue));
    }

    private static decimal? Percent(decimal? value, string name) =>
        value is < 0 or > 100
            ? throw new ArgumentOutOfRangeException(name, value, "Foiz 0 va 100 orasida bo‘lishi kerak.")
            : value;
}

/// <summary>Natijalar bo‘limidagi yirik ko‘rsatkich (masalan, ishtirokchilar soni).</summary>
public sealed class ResultStat : Entity, IOrdered
{
    private ResultStat() { }

    public ResultStat(string label, decimal? value, string? suffix, int sortOrder)
    {
        Label = Guard.NotEmpty(label, nameof(label));
        Value = Guard.NonNegative(value, nameof(value));
        Suffix = suffix;
        SortOrder = sortOrder;
    }

    public string Label { get; private set; } = null!;
    public decimal? Value { get; private set; }
    public string? Suffix { get; private set; }
    public int SortOrder { get; private set; }
}

/// <summary>Hamkor muassasa. Logotip bo‘lmasa nomi namuna joyda ko‘rsatiladi.</summary>
public sealed class Partner : Entity, IOrdered
{
    private Partner() { }

    public Partner(string name, string? logoUrl, int sortOrder)
    {
        Name = Guard.NotEmpty(name, nameof(name));
        LogoUrl = logoUrl;
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public int SortOrder { get; private set; }
}
