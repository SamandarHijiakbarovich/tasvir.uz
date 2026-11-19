using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Tasvir.Modules.Studio.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Studio.Application;

public interface ILibraryRepository
{
    Task<List<LibraryItem>> ListAsync(Guid userId, CancellationToken ct);
    Task<int> CountAsync(Guid userId, CancellationToken ct);
    Task<long> TotalSizeAsync(Guid userId, CancellationToken ct);
    /// <summary>Faqat egasining elementi qaytadi.</summary>
    Task<LibraryItem?> FindAsync(Guid userId, Guid id, CancellationToken ct);
    Task<LibraryItem?> FindAnyAsync(Guid id, CancellationToken ct);
    Task<byte[]?> ReadFileAsync(Guid id, CancellationToken ct);
    void Add(LibraryItem item, LibraryFile? file);
    Task RemoveAsync(LibraryItem item, CancellationToken ct);

    Task<List<ShareLink>> ListSharesAsync(Guid userId, CancellationToken ct);
    Task<ShareLink?> FindShareAsync(string targetKind, Guid targetId, CancellationToken ct);
    Task<ShareLink?> FindShareByTokenAsync(string token, CancellationToken ct);
    void AddShare(ShareLink share);
    void RemoveShare(ShareLink share);

    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record SaveLinkRequest(string? Title, string? Subject, string? Topic, string? Url);

/// <summary>Fayl base64 ko‘rinishida keladi (sayt serveri uni brauzerdan olib yuboradi).</summary>
public sealed record UploadFileRequest(string? Title, string? Subject, string? Topic, string? FileName, string? DataBase64);

/// <summary>Url faqat havolalar uchun: fayl almashtirilmaydi, yangisini yuklash kerak.</summary>
public sealed record UpdateLibraryItemRequest(string? Title, string? Subject, string? Topic, string? Url);

public sealed record LibraryItemDto(
    Guid Id,
    string Kind,
    string Title,
    string? Subject,
    string? Topic,
    string? Url,
    string? FileName,
    string? ContentType,
    long SizeBytes,
    string? ShareToken,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record LibraryDto(IReadOnlyList<LibraryItemDto> Items, long UsedBytes, long QuotaBytes, long MaxFileBytes);

public sealed record ShareRequest(string? Kind, Guid Id);

public sealed record ShareDto(string Kind, Guid Id, string Token);

/// <summary>Ochiq havola orqali ko‘rinadigan material: Material yoki Item dan bittasi to‘ladi.</summary>
public sealed record SharedDto(string Kind, MaterialDto? Material, LibraryItemDto? Item);

public sealed record FileContent(string FileName, string ContentType, byte[] Data);

public sealed partial class LibraryService(ILibraryRepository repository, IMaterialRepository materials, TimeProvider clock)
{
    /// <summary>Bitta fayl hajmi. Sayt Vercel da: u yerda so‘rov tanasi 4,5 MB dan oshmaydi.</summary>
    public const long MaxFileBytes = 4 * 1024 * 1024;
    /// <summary>Bir foydalanuvchining barcha fayllari (bepul bazada joy cheklangan).</summary>
    public const long QuotaBytes = 50 * 1024 * 1024;
    public const int MaxItemsPerUser = 300;

    private const int MaxTitleLength = 120;
    private const int MaxSubjectLength = 40;
    private const int MaxTopicLength = 80;
    private const int MaxUrlLength = 500;
    private const int MaxFileNameLength = 160;

    private sealed record FileType(string ContentType, Func<byte[], bool> Matches);

    // Faqat brauzer xavfsiz ko‘rsata oladigan yoki yuklab olinadigan turlar; mazmuni ham tekshiriladi (kengaytmaga ishonilmaydi)
    private static readonly Dictionary<string, FileType> FileTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new("application/pdf", d => StartsWith(d, "%PDF-"u8)),
        [".pptx"] = new("application/vnd.openxmlformats-officedocument.presentationml.presentation", d => StartsWith(d, [0x50, 0x4B, 0x03, 0x04])),
        [".ppt"] = new("application/vnd.ms-powerpoint", d => StartsWith(d, [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1])),
        [".png"] = new("image/png", d => StartsWith(d, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])),
        [".jpg"] = new("image/jpeg", d => StartsWith(d, [0xFF, 0xD8, 0xFF])),
        [".jpeg"] = new("image/jpeg", d => StartsWith(d, [0xFF, 0xD8, 0xFF])),
        [".webp"] = new("image/webp", d => d.Length > 12 && StartsWith(d, "RIFF"u8) && d.AsSpan(8, 4).SequenceEqual("WEBP"u8)),
    };

    public static IReadOnlyCollection<string> AllowedExtensions => FileTypes.Keys;

    private static readonly Error NotFound = new("library_item_not_found", "Material topilmadi.", ErrorKind.NotFound);
    private static readonly Error ShareNotFound = new("share_not_found", "Havola topilmadi yoki egasi ulaşişni töxtatgan.", ErrorKind.NotFound);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<LibraryDto>> ListAsync(Guid userId, CancellationToken ct)
    {
        var items = await repository.ListAsync(userId, ct);
        var shares = (await repository.ListSharesAsync(userId, ct))
            .Where(s => s.TargetKind == ShareLink.ItemTarget)
            .ToDictionary(s => s.TargetId, s => s.Token);
        var used = items.Sum(i => i.SizeBytes);
        return new LibraryDto(items.Select(i => ToDto(i, shares.GetValueOrDefault(i.Id))).ToList(), used, QuotaBytes, MaxFileBytes);
    }

    public async Task<Result<LibraryItemDto>> AddLinkAsync(Guid userId, SaveLinkRequest request, CancellationToken ct)
    {
        if (ValidateMeta(request.Title, request.Subject, request.Topic) is { } error)
        {
            return error;
        }

        if (ValidateUrl(request.Url, out var url) is { } urlError)
        {
            return urlError;
        }

        if (await repository.CountAsync(userId, ct) >= MaxItemsPerUser)
        {
            return TooMany();
        }

        var item = LibraryItem.Link(userId, request.Title!.Trim(), Clean(request.Subject), Clean(request.Topic), url, Now);
        repository.Add(item, null);
        await repository.SaveChangesAsync(ct);
        return ToDto(item, null);
    }

    public async Task<Result<LibraryItemDto>> AddFileAsync(Guid userId, UploadFileRequest request, CancellationToken ct)
    {
        if (ValidateMeta(request.Title, request.Subject, request.Topic) is { } error)
        {
            return error;
        }

        var fileName = Path.GetFileName(request.FileName?.Trim() ?? string.Empty);
        if (fileName.Length is 0 or > MaxFileNameLength)
        {
            return Invalid($"Fayl nomi 1–{MaxFileNameLength} belgi bölsin.");
        }

        if (!FileTypes.TryGetValue(Path.GetExtension(fileName), out var type))
        {
            return Invalid("Faqat PDF, PowerPoint (PPTX, PPT) va rasm (PNG, JPG, WEBP) fayllari saqlanadi.");
        }

        byte[] data;
        try
        {
            data = Convert.FromBase64String(request.DataBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            return Invalid("Fayl buzilgan. Qayta yuklang.");
        }

        if (data.Length == 0)
        {
            return Invalid("Fayl böş.");
        }

        if (data.Length > MaxFileBytes)
        {
            return Invalid($"Fayl hajmi {MaxFileBytes / 1024 / 1024} MB dan oşmasin. Katta taqdimotni PDF qilib siqing yoki havola sifatida saqlang.");
        }

        if (!type.Matches(data))
        {
            return Invalid("Fayl mazmuni uning turiga mos emas. Faylni asl dasturda qayta saqlab yuklang.");
        }

        if (await repository.CountAsync(userId, ct) >= MaxItemsPerUser)
        {
            return TooMany();
        }

        if (await repository.TotalSizeAsync(userId, ct) + data.Length > QuotaBytes)
        {
            return Invalid($"Fayllar uçun joy ({QuotaBytes / 1024 / 1024} MB) töldi. Keraksiz fayllarni öçiring.");
        }

        var item = LibraryItem.File(userId, request.Title!.Trim(), Clean(request.Subject), Clean(request.Topic), fileName, type.ContentType, data.Length, Now);
        repository.Add(item, new LibraryFile(item.Id, data));
        await repository.SaveChangesAsync(ct);
        return ToDto(item, null);
    }

    public async Task<Result<LibraryItemDto>> UpdateAsync(Guid userId, Guid id, UpdateLibraryItemRequest request, CancellationToken ct)
    {
        var item = await repository.FindAsync(userId, id, ct);
        if (item is null)
        {
            return NotFound;
        }

        if (ValidateMeta(request.Title, request.Subject, request.Topic) is { } error)
        {
            return error;
        }

        if (item.Kind == LibraryItem.LinkKind && request.Url is not null)
        {
            if (ValidateUrl(request.Url, out var url) is { } urlError)
            {
                return urlError;
            }

            item.ChangeUrl(url, Now);
        }

        item.Rename(request.Title!.Trim(), Clean(request.Subject), Clean(request.Topic), Now);
        await repository.SaveChangesAsync(ct);
        var share = await repository.FindShareAsync(ShareLink.ItemTarget, item.Id, ct);
        return ToDto(item, share?.Token);
    }

    public async Task<Result<Unit>> DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var item = await repository.FindAsync(userId, id, ct);
        if (item is null)
        {
            return NotFound;
        }

        if (await repository.FindShareAsync(ShareLink.ItemTarget, item.Id, ct) is { } share)
        {
            repository.RemoveShare(share);
        }

        await repository.RemoveAsync(item, ct);
        await repository.SaveChangesAsync(ct);
        return Unit.Value;
    }

    public async Task<Result<FileContent>> GetFileAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var item = await repository.FindAsync(userId, id, ct);
        return item is null ? NotFound : await ReadFile(item, ct);
    }

    // ---- Ulashish ----

    public async Task<Result<IReadOnlyList<ShareDto>>> ListSharesAsync(Guid userId, CancellationToken ct)
    {
        var shares = await repository.ListSharesAsync(userId, ct);
        return shares.Select(s => new ShareDto(s.TargetKind, s.TargetId, s.Token)).ToList();
    }

    /// <summary>Havola yaratadi; avval yaratilgan bo‘lsa, o‘shani qaytaradi.</summary>
    public async Task<Result<ShareDto>> ShareAsync(Guid userId, ShareRequest request, CancellationToken ct)
    {
        if (await OwnsTarget(userId, request.Kind, request.Id, ct) is { } error)
        {
            return error;
        }

        var share = await repository.FindShareAsync(request.Kind!, request.Id, ct);
        if (share is null)
        {
            share = new ShareLink(NewToken(), userId, request.Kind!, request.Id, Now);
            repository.AddShare(share);
            await repository.SaveChangesAsync(ct);
        }

        return new ShareDto(share.TargetKind, share.TargetId, share.Token);
    }

    public async Task<Result<Unit>> UnshareAsync(Guid userId, string kind, Guid id, CancellationToken ct)
    {
        if (await OwnsTarget(userId, kind, id, ct) is { } error)
        {
            return error;
        }

        if (await repository.FindShareAsync(kind, id, ct) is { } share)
        {
            repository.RemoveShare(share);
            await repository.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }

    public async Task<Result<SharedDto>> GetSharedAsync(string token, CancellationToken ct)
    {
        var share = await FindShare(token, ct);
        if (share is null)
        {
            return ShareNotFound;
        }

        if (share.TargetKind == ShareLink.MaterialTarget)
        {
            var material = await materials.FindAnyAsync(share.TargetId, ct);
            return material is null ? ShareNotFound : new SharedDto(share.TargetKind, StudioService.ToDto(material), null);
        }

        var item = await repository.FindAnyAsync(share.TargetId, ct);
        return item is null ? ShareNotFound : new SharedDto(share.TargetKind, null, ToDto(item, share.Token));
    }

    public async Task<Result<FileContent>> GetSharedFileAsync(string token, CancellationToken ct)
    {
        var share = await FindShare(token, ct);
        if (share is null || share.TargetKind != ShareLink.ItemTarget)
        {
            return ShareNotFound;
        }

        var item = await repository.FindAnyAsync(share.TargetId, ct);
        return item is null ? ShareNotFound : await ReadFile(item, ct);
    }

    /// <summary>Studiya materiali o‘chirilganda uning ochiq havolasi ham o‘chadi.</summary>
    public async Task RemoveMaterialShareAsync(Guid materialId, CancellationToken ct)
    {
        if (await repository.FindShareAsync(ShareLink.MaterialTarget, materialId, ct) is { } share)
        {
            repository.RemoveShare(share);
            await repository.SaveChangesAsync(ct);
        }
    }

    private async Task<Error?> OwnsTarget(Guid userId, string? kind, Guid id, CancellationToken ct) => kind switch
    {
        ShareLink.MaterialTarget => await materials.FindAsync(userId, id, ct) is null ? NotFound : null,
        ShareLink.ItemTarget => await repository.FindAsync(userId, id, ct) is null ? NotFound : null,
        _ => Invalid("Nimani ulaşiş noma’lum."),
    };

    private Task<ShareLink?> FindShare(string token, CancellationToken ct) =>
        TokenPattern().IsMatch(token) ? repository.FindShareByTokenAsync(token, ct) : Task.FromResult<ShareLink?>(null);

    private async Task<Result<FileContent>> ReadFile(LibraryItem item, CancellationToken ct)
    {
        if (item.Kind != LibraryItem.FileKind)
        {
            return NotFound;
        }

        var data = await repository.ReadFileAsync(item.Id, ct);
        return data is null ? NotFound : new FileContent(item.FileName!, item.ContentType!, data);
    }

    private static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    [GeneratedRegex("^[A-Za-z0-9_-]{22}$")]
    private static partial Regex TokenPattern();

    private static Error? ValidateMeta(string? title, string? subject, string? topic)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > MaxTitleLength)
        {
            return Invalid($"Nomi 1–{MaxTitleLength} belgi bölsin.");
        }

        if (subject?.Trim().Length > MaxSubjectLength)
        {
            return Invalid($"Fan nomi {MaxSubjectLength} belgidan oşmasin.");
        }

        return topic?.Trim().Length > MaxTopicLength ? Invalid($"Mavzu {MaxTopicLength} belgidan oşmasin.") : null;
    }

    private static Error? ValidateUrl(string? value, out string url)
    {
        url = value?.Trim() ?? string.Empty;
        if (url.Length is 0 or > MaxUrlLength
            || !Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https")
            || string.IsNullOrEmpty(uri.Host))
        {
            return Invalid("Havola https:// bilan boşlanadigan töliq manzil bölsin.");
        }

        url = uri.AbsoluteUri;
        return null;
    }

    private static bool StartsWith(byte[] data, ReadOnlySpan<byte> prefix) => data.AsSpan().StartsWith(prefix);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Error TooMany() => Invalid($"Saqlangan materiallar soni {MaxItemsPerUser} tadan oşmasin. Keraksizlarini öçiring.");

    private static Error Invalid(string message) => new("invalid_library_item", message, ErrorKind.Validation);

    private static LibraryItemDto ToDto(LibraryItem item, string? shareToken) => new(
        item.Id, item.Kind, item.Title, item.Subject, item.Topic, item.Url, item.FileName, item.ContentType, item.SizeBytes, shareToken,
        item.CreatedAt, item.UpdatedAt);
}
