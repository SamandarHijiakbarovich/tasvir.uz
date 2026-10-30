namespace Tasvir.SharedKernel;

public static class Guard
{
    public static string NotEmpty(string? value, string name) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} bo‘sh bo‘lishi mumkin emas.", name)
            : value.Trim();

    public static decimal? NonNegative(decimal? value, string name) =>
        value is < 0
            ? throw new ArgumentOutOfRangeException(name, value, $"{name} manfiy bo‘lishi mumkin emas.")
            : value;
}
