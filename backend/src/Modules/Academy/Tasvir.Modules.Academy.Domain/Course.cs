using Tasvir.SharedKernel;

namespace Tasvir.Modules.Academy.Domain;

public sealed class Course : Entity
{
    private readonly List<Lesson> _lessons = [];

    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public IReadOnlyList<Lesson> Lessons => _lessons;

    private Course() { }

    public Course(string slug, string title, string description)
    {
        Slug = Guard.NotEmpty(slug, nameof(slug));
        Title = Guard.NotEmpty(title, nameof(title));
        Description = Guard.NotEmpty(description, nameof(description));
    }

    public Lesson AddLesson(string slug, string title, string summary, int estimatedMinutes)
    {
        var lesson = new Lesson(Id, slug, title, summary, estimatedMinutes, _lessons.Count + 1);
        _lessons.Add(lesson);
        return lesson;
    }
}

public sealed class Lesson : Entity, IOrdered
{
    private readonly List<LessonStep> _steps = [];

    public Guid CourseId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public int EstimatedMinutes { get; private set; }
    public int SortOrder { get; private set; }
    public IReadOnlyList<LessonStep> Steps => _steps;

    private Lesson() { }

    internal Lesson(Guid courseId, string slug, string title, string summary, int estimatedMinutes, int sortOrder)
    {
        CourseId = courseId;
        Slug = Guard.NotEmpty(slug, nameof(slug));
        Title = Guard.NotEmpty(title, nameof(title));
        Summary = Guard.NotEmpty(summary, nameof(summary));
        EstimatedMinutes = estimatedMinutes;
        SortOrder = sortOrder;
    }

    public Lesson AddStep(LessonStepKind kind, string title, string contentJson)
    {
        _steps.Add(new LessonStep(Id, kind, title, contentJson, _steps.Count + 1));
        return this;
    }
}

public enum LessonStepKind
{
    Reading = 1,
    Quiz = 2,
    Practice = 3,
}

/// <summary>Dars bosqichi. Mazmuni turiga qarab JSON ko‘rinishida saqlanadi (Application qatlamidagi modellar).</summary>
public sealed class LessonStep : Entity, IOrdered
{
    public Guid LessonId { get; private set; }
    public LessonStepKind Kind { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string ContentJson { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private LessonStep() { }

    internal LessonStep(Guid lessonId, LessonStepKind kind, string title, string contentJson, int sortOrder)
    {
        LessonId = lessonId;
        Kind = kind;
        Title = Guard.NotEmpty(title, nameof(title));
        ContentJson = Guard.NotEmpty(contentJson, nameof(contentJson));
        SortOrder = sortOrder;
    }
}
