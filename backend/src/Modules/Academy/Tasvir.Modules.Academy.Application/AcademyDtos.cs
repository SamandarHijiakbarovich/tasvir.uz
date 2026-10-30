namespace Tasvir.Modules.Academy.Application;

public static class LessonStatus
{
    public const string Locked = "locked";
    public const string Available = "available";
    public const string InProgress = "inProgress";
    public const string Completed = "completed";
}

public sealed record LessonSummaryDto(
    string Slug,
    string Title,
    string Summary,
    int EstimatedMinutes,
    string Status,
    int CompletedSteps,
    int TotalSteps);

public sealed record CourseDto(
    string Slug,
    string Title,
    string Description,
    int CompletedLessons,
    int TotalLessons,
    /// <summary>Keyingi o‘tiladigan dars; kurs tugagan bo‘lsa null.</summary>
    string? NextLessonSlug,
    bool HasStarted,
    IReadOnlyList<LessonSummaryDto> Lessons);

public sealed record QuizOptionDto(string Id, string Text);
public sealed record QuizDto(string Question, IReadOnlyList<QuizOptionDto> Options);
public sealed record PracticeDto(string Mode, string Prompt, string? Placeholder, int MinItems, int MaxItems, IReadOnlyList<string>? Checklist);

public sealed record ExerciseDto(string Prompt, string? Unit);

public sealed record StepDto(
    Guid Id,
    string Kind,
    string Title,
    bool IsCompleted,
    ReadingContent? Reading,
    QuizDto? Quiz,
    PracticeDto? Practice,
    VideoContent? Video,
    ExerciseDto? Exercise,
    /// <summary>Foydalanuvchining saqlangan javobi (dars qayta ochilganda ko‘rsatiladi).</summary>
    CompleteStepRequest? SavedResponse);

public sealed record LessonDto(
    string Slug,
    string Title,
    string Summary,
    int EstimatedMinutes,
    string CourseSlug,
    string CourseTitle,
    int Position,
    int TotalLessons,
    string Status,
    string? NextLessonSlug,
    IReadOnlyList<StepDto> Steps);

/// <summary>"Mening ishlarim": foydalanuvchi amaliy topshiriqlarda yaratgan natijalar.</summary>
public sealed record MyWorkItemDto(
    Guid StepId,
    string LessonSlug,
    string LessonTitle,
    string StepTitle,
    string Mode,
    string? Text,
    IReadOnlyList<string>? Items,
    DateTime CompletedAt);

public sealed record CompleteStepRequest(string? OptionId, string? Text, IReadOnlyList<string>? Items);

public sealed record StepResultDto(
    bool StepCompleted,
    /// <summary>Faqat test bosqichida: javob to‘g‘rimi.</summary>
    bool? IsCorrect,
    string? Feedback,
    bool LessonCompleted,
    string? NextLessonSlug);
