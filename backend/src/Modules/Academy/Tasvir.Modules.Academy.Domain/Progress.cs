using Tasvir.SharedKernel;

namespace Tasvir.Modules.Academy.Domain;

/// <summary>
/// Foydalanuvchining dars bo‘yicha holati. UserId Identity modulidan keladi, lekin tashqi kalit yo‘q:
/// modullar bir-birining jadvallariga bog‘lanmaydi.
/// </summary>
public sealed class LessonProgress : Entity
{
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private LessonProgress() { }

    public LessonProgress(Guid userId, Guid lessonId, DateTime now)
    {
        UserId = userId;
        LessonId = lessonId;
        StartedAt = now;
    }

    public void Complete(DateTime now) => CompletedAt ??= now;
}

/// <summary>
/// Test yoki mashqqa har bir urinish (xatosi bilan). Bosqich yakunlanishidan farqli o‘laroq, xato javoblar ham
/// yoziladi: test savollari sifatini tahlil qilish (item-analiz) birinchi urinishlarga tayanadi.
/// </summary>
public sealed class StepAttempt : Entity
{
    public const int MaxAnswerLength = 40;

    public Guid UserId { get; private set; }
    public Guid StepId { get; private set; }
    public string Answer { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    private StepAttempt() { }

    public StepAttempt(Guid userId, Guid stepId, string answer, bool isCorrect, DateTime now)
    {
        UserId = userId;
        StepId = stepId;
        Answer = answer.Length > MaxAnswerLength ? answer[..MaxAnswerLength] : answer;
        IsCorrect = isCorrect;
        AttemptedAt = now;
    }
}

public sealed class StepCompletion : Entity
{
    public Guid UserId { get; private set; }
    public Guid StepId { get; private set; }
    public DateTime CompletedAt { get; private set; }
    /// <summary>Foydalanuvchi javobi (amaliy topshiriq natijasi); o‘qish bosqichida bo‘sh.</summary>
    public string? ResponseJson { get; private set; }

    private StepCompletion() { }

    public StepCompletion(Guid userId, Guid stepId, DateTime now, string? responseJson)
    {
        UserId = userId;
        StepId = stepId;
        CompletedAt = now;
        ResponseJson = responseJson;
    }
}
