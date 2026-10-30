using Tasvir.SharedKernel;

namespace Tasvir.Modules.Studio.Domain;

/// <summary>
/// O‘qituvchi yaratgan vizual material. Mazmun JSON ko‘rinishida saqlanadi (Application qatlami tekshiradi).
/// UserId Identity modulidan keladi, tashqi kalit yo‘q: modullar bir-birining jadvallariga bog‘lanmaydi.
/// </summary>
public sealed class Material : Entity
{
    public Guid UserId { get; private set; }
    public string TemplateKey { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public int? Grade { get; private set; }
    public string ContentJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Material() { }

    public Material(Guid userId, string templateKey, string title, string? subject, int? grade, string contentJson, DateTime now)
    {
        UserId = userId;
        TemplateKey = Guard.NotEmpty(templateKey, nameof(templateKey));
        CreatedAt = now;
        Update(title, subject, grade, contentJson, now);
    }

    public void Update(string title, string? subject, int? grade, string contentJson, DateTime now)
    {
        Title = Guard.NotEmpty(title, nameof(title));
        Subject = subject;
        Grade = grade;
        ContentJson = Guard.NotEmpty(contentJson, nameof(contentJson));
        UpdatedAt = now;
    }
}
