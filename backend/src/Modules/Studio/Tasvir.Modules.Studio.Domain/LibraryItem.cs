using Tasvir.SharedKernel;

namespace Tasvir.Modules.Studio.Domain;

/// <summary>
/// O‘qituvchi kabinetiga saqlagan tashqi material: yuklangan fayl (PDF, PowerPoint, rasm) yoki havola (Google Slides, Canva va h.k.).
/// Fayl mazmuni alohida <see cref="LibraryFile"/> da turadi: ro‘yxat o‘qilganda katta ma’lumot yuklanmaydi.
/// </summary>
public sealed class LibraryItem : Entity
{
    public const string FileKind = "file";
    public const string LinkKind = "link";

    public Guid UserId { get; private set; }
    /// <summary>file | link</summary>
    public string Kind { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public string? Topic { get; private set; }
    public string? Url { get; private set; }
    public string? FileName { get; private set; }
    public string? ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private LibraryItem() { }

    private LibraryItem(Guid userId, string kind, string title, string? subject, string? topic, DateTime now)
    {
        UserId = userId;
        Kind = kind;
        CreatedAt = now;
        Rename(title, subject, topic, now);
    }

    public static LibraryItem Link(Guid userId, string title, string? subject, string? topic, string url, DateTime now) =>
        new(userId, LinkKind, title, subject, topic, now) { Url = Guard.NotEmpty(url, nameof(url)) };

    public static LibraryItem File(Guid userId, string title, string? subject, string? topic, string fileName, string contentType, long sizeBytes, DateTime now) =>
        new(userId, FileKind, title, subject, topic, now)
        {
            FileName = Guard.NotEmpty(fileName, nameof(fileName)),
            ContentType = Guard.NotEmpty(contentType, nameof(contentType)),
            SizeBytes = sizeBytes,
        };

    public void Rename(string title, string? subject, string? topic, DateTime now)
    {
        Title = Guard.NotEmpty(title, nameof(title));
        Subject = subject;
        Topic = topic;
        UpdatedAt = now;
    }

    public void ChangeUrl(string url, DateTime now)
    {
        if (Kind != LinkKind)
        {
            throw new InvalidOperationException("Faqat havolaning manzili o‘zgaradi.");
        }

        Url = Guard.NotEmpty(url, nameof(url));
        UpdatedAt = now;
    }
}

/// <summary>Yuklangan fayl mazmuni. Id — <see cref="LibraryItem"/> ning Id si.</summary>
public sealed class LibraryFile
{
    public Guid Id { get; private set; }
    public byte[] Data { get; private set; } = [];

    private LibraryFile() { }

    public LibraryFile(Guid id, byte[] data)
    {
        Id = id;
        Data = data;
    }
}

/// <summary>
/// Ochiq ulashish havolasi: tokenni bilgan har kim materialni kirmasdan ko‘ra oladi (tahrirlay olmaydi).
/// TargetKind: material (Studiyada yaratilgan) | item (yuklangan fayl yoki havola).
/// </summary>
public sealed class ShareLink
{
    public const string MaterialTarget = "material";
    public const string ItemTarget = "item";

    public string Token { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string TargetKind { get; private set; } = string.Empty;
    public Guid TargetId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ShareLink() { }

    public ShareLink(string token, Guid userId, string targetKind, Guid targetId, DateTime now)
    {
        Token = Guard.NotEmpty(token, nameof(token));
        UserId = userId;
        TargetKind = targetKind;
        TargetId = targetId;
        CreatedAt = now;
    }
}
