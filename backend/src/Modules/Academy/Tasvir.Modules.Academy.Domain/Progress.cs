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
