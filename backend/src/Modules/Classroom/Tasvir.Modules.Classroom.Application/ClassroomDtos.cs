using System.Text.Json;

namespace Tasvir.Modules.Classroom.Application;

public sealed record CreateGroupRequest(string? Name);
public sealed record JoinGroupRequest(string? Code, bool AcceptSharing);
public sealed record SubmitWorkRequest(Guid MaterialId);
public sealed record SaveReviewRequest(IReadOnlyDictionary<string, int>? Scores, string? Comment);
public sealed record SaveSurveyRequest(IReadOnlyDictionary<string, int>? Answers, string? Comment);
public sealed record SubmitDiagnosticRequest(IReadOnlyDictionary<string, string>? Answers);
public sealed record SetDiagnosticStateRequest(bool IsOpen);

public sealed record GroupSummaryDto(Guid Id, string Name, string TeacherName, int MemberCount, DateTime CreatedAt, string? JoinCode);
public sealed record MyGroupsDto(IReadOnlyList<GroupSummaryDto> Teaching, IReadOnlyList<GroupSummaryDto> Studying);

public sealed record ReviewDto(int Version, IReadOnlyDictionary<string, int> Scores, int TotalScore, int MaxScore, string? Comment, DateTime ReviewedAt);

public sealed record SubmissionSummaryDto(
    Guid Id,
    Guid UserId,
    string StudentName,
    string TemplateKey,
    string Title,
    string? Subject,
    int? Grade,
    int Version,
    DateTime SubmittedAt,
    /// <summary>Oxirgi baho; talaba keyin qayta topshirgan bo‘lsa, Version dan kichik bo‘ladi.</summary>
    ReviewDto? Review);

public sealed record SubmissionDetailDto(
    SubmissionSummaryDto Summary,
    Guid GroupId,
    string GroupName,
    bool CanReview,
    JsonElement Content,
    IReadOnlyList<RubricCriterion> Rubric,
    int MaxCriterionScore);

public sealed record StudentRowDto(
    Guid UserId,
    string Name,
    DateTime JoinedAt,
    IReadOnlyList<CourseProgress> Courses,
    int FirstTryCorrect,
    int FirstTryTotal,
    DateTime? LastActivityAt,
    int SubmissionCount,
    int ReviewedCount,
    /// <summary>Baholangan ishlar bo‘yicha o‘rtacha foiz; baho bo‘lmasa, null.</summary>
    double? AverageScorePercent);

public sealed record SurveyQuestionSummaryDto(string Key, string Text, double? Average, IReadOnlyList<int> Counts);
public sealed record SurveySummaryDto(int Respondents, IReadOnlyList<SurveyQuestionSummaryDto> Questions, IReadOnlyList<string> Comments);

public sealed record GroupDashboardDto(
    GroupSummaryDto Group,
    IReadOnlyList<StudentRowDto> Students,
    IReadOnlyList<SubmissionSummaryDto> Submissions,
    IReadOnlyList<ItemAnalysis> Items,
    SurveySummaryDto Survey,
    DiagnosticsDto Diagnostics);

public sealed record StudentGroupDto(
    GroupSummaryDto Group,
    DateTime JoinedAt,
    IReadOnlyList<SubmissionSummaryDto> Submissions,
    IReadOnlyList<SurveyQuestion> SurveyQuestions,
    IReadOnlyDictionary<string, int>? SurveyAnswers,
    string? SurveyComment,
    IReadOnlyList<StudentDiagnosticStageDto> Diagnostics);

public sealed record GroupExportDto(string FileName, string Csv);

// Diagnostika. To‘g‘ri javob talabaga yuborilmaydi: yakuniy testda ham shu savollar beriladi.
public sealed record DiagnosticQuestionDto(string Key, string Text, IReadOnlyList<DiagnosticOption> Options);
public sealed record DiagnosticTestDto(Guid GroupId, string GroupName, string Stage, string Title, IReadOnlyList<DiagnosticQuestionDto> Questions);
public sealed record DiagnosticScoreDto(string Stage, int Score, int MaxScore, DateTime SubmittedAt);
public sealed record StudentDiagnosticStageDto(string Stage, string Title, bool IsOpen, DiagnosticScoreDto? Result);

public sealed record DiagnosticStageStateDto(string Stage, string Title, bool IsOpen, int Respondents);
public sealed record DiagnosticStudentRowDto(Guid UserId, string Name, int? Pre, int? Post);
/// <summary>Savolga to‘g‘ri javob berganlar ulushi; shu bosqichda javob bo‘lmasa, null.</summary>
public sealed record DiagnosticItemDto(string Key, string Topic, string Text, double? PreCorrectShare, double? PostCorrectShare);
public sealed record DiagnosticsDto(
    int MaxScore,
    IReadOnlyList<DiagnosticStageStateDto> Stages,
    IReadOnlyList<DiagnosticStudentRowDto> Students,
    IReadOnlyList<DiagnosticItemDto> Items,
    /// <summary>Ikkala testni topshirgan talabalar bo‘yicha; ular ikkitadan kam bo‘lsa, null.</summary>
    PairedTestDto? Paired);

/// <summary>Tajriba (A) va nazorat (B) guruhlarini solishtirish. Gain: o‘sish, faqat ikkala testni topshirganlar.</summary>
public sealed record GroupComparisonDto(GroupSummaryDto GroupA, GroupSummaryDto GroupB, int MaxScore, TwoSampleTestDto? Pre, TwoSampleTestDto? Post, TwoSampleTestDto? Gain);
