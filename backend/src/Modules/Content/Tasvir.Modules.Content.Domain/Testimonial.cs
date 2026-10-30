using Tasvir.SharedKernel;

namespace Tasvir.Modules.Content.Domain;

/// <summary>
/// Foydalanuvchi sharhi. Faqat egasining ruxsati bilan olingan real fikr <see cref="IsVerified"/> = true bo‘ladi;
/// qolganlari sahifada namuna joy sifatida ko‘rsatiladi.
/// </summary>
public sealed class Testimonial : Entity, IOrdered
{
    private Testimonial() { }

    public Testimonial(string quote, string authorName, string authorRole, string? avatarUrl, bool isVerified, int sortOrder)
    {
        Quote = Guard.NotEmpty(quote, nameof(quote));
        AuthorName = Guard.NotEmpty(authorName, nameof(authorName));
        AuthorRole = Guard.NotEmpty(authorRole, nameof(authorRole));
        AvatarUrl = avatarUrl;
        IsVerified = isVerified;
        SortOrder = sortOrder;
    }

    public string Quote { get; private set; } = null!;
    public string AuthorName { get; private set; } = null!;
    public string AuthorRole { get; private set; } = null!;
    public string? AvatarUrl { get; private set; }
    public bool IsVerified { get; private set; }
    public int SortOrder { get; private set; }
}
