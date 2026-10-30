using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Studio.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Studio.Application;

public interface IMaterialRepository
{
    Task<List<Material>> ListAsync(Guid userId, CancellationToken ct);
    Task<int> CountAsync(Guid userId, CancellationToken ct);
    /// <summary>Faqat egasining materiali qaytadi: boshqa foydalanuvchiniki "topilmadi" hisoblanadi.</summary>
    Task<Material?> FindAsync(Guid userId, Guid id, CancellationToken ct);
    void Add(Material material);
    void Remove(Material material);
    Task SaveChangesAsync(CancellationToken ct);
}

public sealed record MaterialItem(string? Heading, string? Text, IReadOnlyList<string>? Cells);

/// <summary>Material mazmuni. Accent: rang mavzusi, Note: izoh yoki xulosa, Columns: faqat qiyosiy jadvalda.</summary>
public sealed record MaterialContent(string? Accent, string? Note, IReadOnlyList<string>? Columns, IReadOnlyList<MaterialItem>? Items);

public sealed record SaveMaterialRequest(string? TemplateKey, string? Title, string? Subject, int? Grade, MaterialContent? Content);

public sealed record MaterialDto(
    Guid Id,
    string TemplateKey,
    string Title,
    string? Subject,
    int? Grade,
    MaterialContent Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed class StudioService(IMaterialRepository repository, TimeProvider clock)
{
    public const int MaxMaterialsPerUser = 500;

    private const int MaxTitleLength = 120;
    private const int MaxSubjectLength = 40;
    private const int MaxHeadingLength = 80;
    private const int MaxTextLength = 240;
    private const int MaxNoteLength = 240;
    private const int MaxCellLength = 80;

    private sealed record TemplateRules(int MinItems, int MaxItems, int MinColumns = 0, int MaxColumns = 0);

    private static readonly Dictionary<string, TemplateRules> Templates = new()
    {
        ["process"] = new(2, 8),
        ["timeline"] = new(2, 8),
        ["mindmap"] = new(2, 8),
        ["comparison"] = new(1, 8, 2, 4),
        ["infographic"] = new(2, 6),
    };

    private static readonly HashSet<string> Accents = ["sun", "mint", "coral", "brand"];

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private static readonly Error NotFound = new("material_not_found", "Material topilmadi.", ErrorKind.NotFound);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<IReadOnlyList<MaterialDto>>> ListAsync(Guid userId, CancellationToken ct)
    {
        var materials = await repository.ListAsync(userId, ct);
        return materials.Select(ToDto).ToList();
    }

    public async Task<Result<MaterialDto>> GetAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var material = await repository.FindAsync(userId, id, ct);
        return material is null ? NotFound : ToDto(material);
    }

    public async Task<Result<MaterialDto>> CreateAsync(Guid userId, SaveMaterialRequest request, CancellationToken ct)
    {
        if (await repository.CountAsync(userId, ct) >= MaxMaterialsPerUser)
        {
            return Invalid($"Materiallar soni {MaxMaterialsPerUser} tadan oshmasligi kerak. Keraksizlarini o‘chiring.");
        }

        if (request.TemplateKey is null || !Templates.ContainsKey(request.TemplateKey))
        {
            return Invalid("Bunday shablon yo‘q.");
        }

        if (Validate(request.TemplateKey, request) is { } error)
        {
            return error;
        }

        var material = new Material(userId, request.TemplateKey, request.Title!.Trim(), CleanSubject(request.Subject), request.Grade, Serialize(request.Content!), Now);
        repository.Add(material);
        await repository.SaveChangesAsync(ct);
        return ToDto(material);
    }

    public async Task<Result<MaterialDto>> UpdateAsync(Guid userId, Guid id, SaveMaterialRequest request, CancellationToken ct)
    {
        var material = await repository.FindAsync(userId, id, ct);
        if (material is null)
        {
            return NotFound;
        }

        // Shablon turi yaratilgandan keyin o‘zgarmaydi: mazmun tuzilishi unga bog‘liq
        if (Validate(material.TemplateKey, request) is { } error)
        {
            return error;
        }

        material.Update(request.Title!.Trim(), CleanSubject(request.Subject), request.Grade, Serialize(request.Content!), Now);
        await repository.SaveChangesAsync(ct);
        return ToDto(material);
    }

    public async Task<Result<MaterialDto>> DuplicateAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var source = await repository.FindAsync(userId, id, ct);
        if (source is null)
        {
            return NotFound;
        }

        if (await repository.CountAsync(userId, ct) >= MaxMaterialsPerUser)
        {
            return Invalid($"Materiallar soni {MaxMaterialsPerUser} tadan oshmasligi kerak. Keraksizlarini o‘chiring.");
        }

        var title = $"{source.Title} (nusxa)";
        if (title.Length > MaxTitleLength)
        {
            title = title[..MaxTitleLength];
        }

        var copy = new Material(userId, source.TemplateKey, title, source.Subject, source.Grade, source.ContentJson, Now);
        repository.Add(copy);
        await repository.SaveChangesAsync(ct);
        return ToDto(copy);
    }

    public async Task<Result<Unit>> DeleteAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var material = await repository.FindAsync(userId, id, ct);
        if (material is null)
        {
            return NotFound;
        }

        repository.Remove(material);
        await repository.SaveChangesAsync(ct);
        return Unit.Value;
    }

    private static Error? Validate(string templateKey, SaveMaterialRequest request)
    {
        var rules = Templates[templateKey];

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > MaxTitleLength)
        {
            return Invalid($"Sarlavha 1–{MaxTitleLength} belgi bo‘lsin.");
        }

        if (request.Subject?.Trim().Length > MaxSubjectLength)
        {
            return Invalid($"Fan nomi {MaxSubjectLength} belgidan oshmasin.");
        }

        if (request.Grade is < 1 or > 11)
        {
            return Invalid("Sinf 1 dan 11 gacha bo‘lsin.");
        }

        var content = request.Content;
        if (content is null)
        {
            return Invalid("Material mazmuni bo‘sh.");
        }

        if (content.Accent is not null && !Accents.Contains(content.Accent))
        {
            return Invalid("Bunday rang mavzusi yo‘q.");
        }

        if (content.Note?.Length > MaxNoteLength)
        {
            return Invalid($"Izoh {MaxNoteLength} belgidan oshmasin.");
        }

        var items = content.Items ?? [];
        if (items.Count < rules.MinItems || items.Count > rules.MaxItems)
        {
            return Invalid($"Bu shablonda {rules.MinItems} tadan {rules.MaxItems} tagacha element bo‘lsin.");
        }

        var columnCount = content.Columns?.Count ?? 0;
        if (rules.MaxColumns > 0)
        {
            if (columnCount < rules.MinColumns || columnCount > rules.MaxColumns)
            {
                return Invalid($"Jadvalda {rules.MinColumns} tadan {rules.MaxColumns} tagacha ustun bo‘lsin.");
            }

            if (content.Columns!.Any(c => string.IsNullOrWhiteSpace(c) || c.Length > MaxCellLength))
            {
                return Invalid($"Ustun nomi 1–{MaxCellLength} belgi bo‘lsin.");
            }
        }

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Heading))
            {
                return Invalid("Har bir elementga nom bering yoki bo‘sh elementni o‘chiring.");
            }

            if (item.Heading.Length > MaxHeadingLength)
            {
                return Invalid($"Element nomi {MaxHeadingLength} belgidan oshmasin: sxemada qisqa yozuv yaxshi o‘qiladi.");
            }

            if (item.Text?.Length > MaxTextLength)
            {
                return Invalid($"Izoh {MaxTextLength} belgidan oshmasin.");
            }

            if (rules.MaxColumns > 0)
            {
                if ((item.Cells?.Count ?? 0) != columnCount || item.Cells!.Any(c => c.Length > MaxCellLength))
                {
                    return Invalid($"Jadval kataklari ustunlar soniga teng va {MaxCellLength} belgidan qisqa bo‘lsin.");
                }
            }
        }

        return null;
    }

    private static Error Invalid(string message) => new("invalid_material", message, ErrorKind.Validation);

    private static string? CleanSubject(string? subject) => string.IsNullOrWhiteSpace(subject) ? null : subject.Trim();

    private static string Serialize(MaterialContent content) => JsonSerializer.Serialize(content, Json);

    private static MaterialDto ToDto(Material material) => new(
        material.Id,
        material.TemplateKey,
        material.Title,
        material.Subject,
        material.Grade,
        JsonSerializer.Deserialize<MaterialContent>(material.ContentJson, Json)!,
        material.CreatedAt,
        material.UpdatedAt);
}

public static class DependencyInjection
{
    public static IServiceCollection AddStudioApplication(this IServiceCollection services)
    {
        services.AddScoped<StudioService>();
        return services;
    }
}
