using System.Text.Json;

namespace Tasvir.Modules.Classroom.Application;

public sealed record CreateGroupRequest(string? Name);
public sealed record JoinGroupRequest(string? Code, bool AcceptSharing);
public sealed record SubmitWorkRequest(Guid MaterialId);
public sealed record SaveReviewRequest(IReadOnlyDictionary<string, int>? Scores, string? Comment);
public sealed record SaveSurveyRequest(IReadOnlyDictionary<string, int>? Answers, string? Comment);

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
    SurveySummaryDto Survey);

public sealed record StudentGroupDto(
    GroupSummaryDto Group,
    DateTime JoinedAt,
    IReadOnlyList<SubmissionSummaryDto> Submissions,
    IReadOnlyList<SurveyQuestion> SurveyQuestions,
    IReadOnlyDictionary<string, int>? SurveyAnswers,
    string? SurveyComment);

public sealed record GroupExportDto(string FileName, string Csv);
