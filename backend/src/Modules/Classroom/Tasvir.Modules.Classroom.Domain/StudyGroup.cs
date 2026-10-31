using Tasvir.SharedKernel;

namespace Tasvir.Modules.Classroom.Domain;

/// <summary>
/// O‘qituvchi yaratgan guruh. Rol guruhga bog‘liq: yaratgan kishi shu guruhning o‘qituvchisi, kod bilan qo‘shilgan
/// kishi talabasi. UserId’lar boshqa modullardan keladi, tashqi kalit yo‘q: modullar bir-birining jadvallariga bog‘lanmaydi.
/// </summary>
public sealed class StudyGroup : Entity
{
    public Guid OwnerUserId { get; private set; }
    public string OwnerName { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string JoinCode { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private StudyGroup() { }

    public StudyGroup(Guid ownerUserId, string ownerName, string name, string joinCode, DateTime now)
    {
        OwnerUserId = ownerUserId;
        OwnerName = Guard.NotEmpty(ownerName, nameof(ownerName));
        Name = Guard.NotEmpty(name, nameof(name));
        JoinCode = Guard.NotEmpty(joinCode, nameof(joinCode));
        CreatedAt = now;
    }
}

/// <summary>Talaba. SharingAcceptedAt: natijalari o‘qituvchiga ko‘rinishiga bergan alohida rozilik vaqti.</summary>
public sealed class GroupMember : Entity
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public DateTime JoinedAt { get; private set; }
    public DateTime SharingAcceptedAt { get; private set; }

    private GroupMember() { }

    public GroupMember(Guid groupId, Guid userId, string displayName, DateTime now)
    {
        GroupId = groupId;
        UserId = userId;
        DisplayName = Guard.NotEmpty(displayName, nameof(displayName));
        JoinedAt = now;
        SharingAcceptedAt = now;
    }
}

/// <summary>
/// Talaba topshirgan ish. Mazmun topshirish paytidagi nusxa: talaba Studiyada keyin nimani o‘zgartirsa ham,
/// o‘qituvchi aynan topshirilganini baholaydi. Qayta topshirilsa, versiya oshadi.
/// </summary>
public sealed class Submission : Entity
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MaterialId { get; private set; }
    public string TemplateKey { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Subject { get; private set; }
    public int? Grade { get; private set; }
    public string ContentJson { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private Submission() { }

    public Submission(Guid groupId, Guid userId, Guid materialId, string templateKey, string title, string? subject, int? grade, string contentJson, DateTime now)
    {
        GroupId = groupId;
        UserId = userId;
        MaterialId = materialId;
        Resubmit(templateKey, title, subject, grade, contentJson, now);
    }

    public void Resubmit(string templateKey, string title, string? subject, int? grade, string contentJson, DateTime now)
    {
        TemplateKey = Guard.NotEmpty(templateKey, nameof(templateKey));
        Title = Guard.NotEmpty(title, nameof(title));
        Subject = subject;
        Grade = grade;
        ContentJson = Guard.NotEmpty(contentJson, nameof(contentJson));
        Version++;
        SubmittedAt = now;
    }
}

/// <summary>O‘qituvchining rubrika bo‘yicha bahosi. Har bir versiyaga bittadan: ishning takomillashuvi ko‘rinib turadi.</summary>
public sealed class SubmissionReview : Entity
{
    public Guid SubmissionId { get; private set; }
    public int Version { get; private set; }
    public string ScoresJson { get; private set; } = string.Empty;
    public int TotalScore { get; private set; }
    public string? Comment { get; private set; }
    public DateTime ReviewedAt { get; private set; }

    private SubmissionReview() { }

    public SubmissionReview(Guid submissionId, int version, string scoresJson, int totalScore, string? comment, DateTime now)
    {
        SubmissionId = submissionId;
        Version = version;
        Update(scoresJson, totalScore, comment, now);
    }

    public void Update(string scoresJson, int totalScore, string? comment, DateTime now)
    {
        ScoresJson = Guard.NotEmpty(scoresJson, nameof(scoresJson));
        TotalScore = totalScore;
        Comment = comment;
        ReviewedAt = now;
    }
}

/// <summary>Talabaning so‘rovnomadagi javoblari. O‘qituvchiga faqat umumlashtirilgan holda ko‘rsatiladi.</summary>
public sealed class SurveyResponse : Entity
{
    public Guid GroupId { get; private set; }
    public Guid UserId { get; private set; }
    public string AnswersJson { get; private set; } = string.Empty;
    public string? Comment { get; private set; }
    public DateTime SubmittedAt { get; private set; }

    private SurveyResponse() { }

    public SurveyResponse(Guid groupId, Guid userId, string answersJson, string? comment, DateTime now)
    {
        GroupId = groupId;
        UserId = userId;
        Update(answersJson, comment, now);
    }

    public void Update(string answersJson, string? comment, DateTime now)
    {
        AnswersJson = Guard.NotEmpty(answersJson, nameof(answersJson));
        Comment = comment;
        SubmittedAt = now;
    }
}
