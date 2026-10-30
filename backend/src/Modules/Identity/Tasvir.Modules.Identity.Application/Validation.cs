using System.Net.Mail;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Identity.Application;

internal static class Validation
{
    public const int MinPasswordLength = 8;
    // PBKDF2 ni juda uzun parollar bilan yuklashning oldini oladi
    public const int MaxPasswordLength = 128;

    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "12345678", "123456789", "1234567890", "password", "password1", "password123", "qwerty123", "qwertyuiop",
        "11111111", "00000000", "87654321", "abcd1234", "abc12345", "parol123", "parol1234", "tasvir123", "tasviruz",
    };

    public static Error Invalid(Dictionary<string, string> fields) =>
        new("validation_failed", "Ma’lumotlarni tekshirib, qayta urining.", ErrorKind.Validation, fields);

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254)
        {
            return false;
        }

        var value = email.Trim();
        return MailAddress.TryCreate(value, out var address)
            && address.Address == value
            && address.Host.Contains('.');
    }

    public static string? CheckPassword(string? password, string? email = null)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
        {
            return $"Parol kamida {MinPasswordLength} ta belgidan iborat bo‘lsin.";
        }

        if (password.Length > MaxPasswordLength)
        {
            return $"Parol {MaxPasswordLength} ta belgidan oshmasin.";
        }

        if (CommonPasswords.Contains(password) || password.Distinct().Count() <= 2)
        {
            return "Bu parolni topish juda oson. Boshqasini tanlang.";
        }

        if (email is not null && password.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return "Parol emailingiz bilan bir xil bo‘lmasin.";
        }

        return null;
    }
}
