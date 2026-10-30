using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Application;

public interface IAcademyRepository
{
    Task<Course?> FindCourseBySlugAsync(string slug, CancellationToken ct);
    Task<Course?> FindCourseByLessonSlugAsync(string lessonSlug, CancellationToken ct);
    Task<List<Course>> GetCoursesAsync(CancellationToken ct);
    Task<List<LessonProgress>> GetLessonProgressAsync(Guid userId, IReadOnlyCollection<Guid> lessonIds, CancellationToken ct);
    Task<List<StepCompletion>> GetStepCompletionsAsync(Guid userId, IReadOnlyCollection<Guid> stepIds, CancellationToken ct);
    void Add(LessonProgress progress);
    void Add(StepCompletion completion);
    /// <summary>Saqlaydi. Ikki so‘rov bir vaqtda bir xil yozuvni qo‘shsa (ikki marta bosish), false qaytaradi.</summary>
    Task<bool> TrySaveChangesAsync(CancellationToken ct);
}

// Bosqich mazmuni (LessonStep.ContentJson). To‘g‘ri javoblar faqat shu yerda: brauzerga DTO orqali chiqmaydi.
public sealed record ReadingExample(string BeforeLabel, string Before, string AfterLabel, IReadOnlyList<string> After);
public sealed record ReadingContent(IReadOnlyList<string> Paragraphs, string? Callout = null, ReadingExample? Example = null);

public sealed record QuizOption(string Id, string Text);
public sealed record QuizContent(string Question, IReadOnlyList<QuizOption> Options, string CorrectOptionId, string Explanation, string WrongHint);

public static class PracticeModes
{
    public const string Text = "text";
    public const string Process = "process";
    public const string Checklist = "checklist";
}

public sealed record PracticeContent(
    string Mode,
    string Prompt,
    string? Placeholder = null,
    int MinItems = 0,
    int MaxItems = 0,
    IReadOnlyList<string>? Checklist = null);

public static class StepContentJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options) ?? throw new InvalidOperationException("Bosqich mazmuni bo‘sh.");
}

public static class DependencyInjection
{
    public static IServiceCollection AddAcademyApplication(this IServiceCollection services)
    {
        services.AddScoped<AcademyService>();
        return services;
    }
}
