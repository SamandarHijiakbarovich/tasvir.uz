using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Classroom.Domain;

namespace Tasvir.Modules.Classroom.Application;

public interface IClassroomRepository
{
    Task<List<StudyGroup>> ListOwnedAsync(Guid userId, CancellationToken ct);
    Task<List<StudyGroup>> ListJoinedAsync(Guid userId, CancellationToken ct);
    Task<StudyGroup?> FindGroupAsync(Guid id, CancellationToken ct);
    Task<StudyGroup?> FindGroupByCodeAsync(string code, CancellationToken ct);
    Task<int> CountOwnedAsync(Guid userId, CancellationToken ct);
    Task<Dictionary<Guid, int>> CountMembersAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken ct);
    Task<List<GroupMember>> ListMembersAsync(Guid groupId, CancellationToken ct);
    Task<GroupMember?> FindMemberAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<List<Submission>> ListSubmissionsAsync(Guid groupId, CancellationToken ct);
    Task<List<Submission>> ListSubmissionsOfAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task<Submission?> FindSubmissionAsync(Guid id, CancellationToken ct);
    Task<Submission?> FindSubmissionAsync(Guid groupId, Guid userId, Guid materialId, CancellationToken ct);
    Task<List<SubmissionReview>> ListReviewsAsync(IReadOnlyCollection<Guid> submissionIds, CancellationToken ct);
    Task<SubmissionReview?> FindReviewAsync(Guid submissionId, int version, CancellationToken ct);
    Task<List<SurveyResponse>> ListSurveyResponsesAsync(Guid groupId, CancellationToken ct);
    Task<SurveyResponse?> FindSurveyResponseAsync(Guid groupId, Guid userId, CancellationToken ct);
    void Add(StudyGroup group);
    void Add(GroupMember member);
    void Add(Submission submission);
    void Add(SubmissionReview review);
    void Add(SurveyResponse response);
    void Remove(GroupMember member);
    Task RemoveGroupAsync(StudyGroup group, CancellationToken ct);
    /// <summary>Talabaning shu guruhdagi ishlari, baholari va so‘rovnoma javoblarini o‘chiradi.</summary>
    Task RemoveMemberDataAsync(Guid groupId, Guid userId, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    /// <summary>Saqlaydi. Noyob indeks ikkinchi nusxani ushlab qolsa (ikki marta bosish), false qaytaradi.</summary>
    Task<bool> TrySaveChangesAsync(CancellationToken ct);
}

// Boshqa modullardan kerakli ma’lumotlar. Modullar bir-biriga bog‘lanmasligi uchun tatbiqi xostda (Tasvir.Api).

public interface IMemberDirectory
{
    Task<string?> GetNameAsync(Guid userId, CancellationToken ct);
}

public sealed record SharedMaterial(string TemplateKey, string Title, string? Subject, int? Grade, string ContentJson);

public interface IMaterialSource
{
    /// <summary>Foydalanuvchining o‘z materiali; boshqa odamniki yoki yo‘q bo‘lsa, null.</summary>
    Task<SharedMaterial?> GetOwnAsync(Guid userId, Guid materialId, CancellationToken ct);
}

public sealed record CourseProgress(string Title, int CompletedLessons, int TotalLessons);
public sealed record StudentLearning(Guid UserId, IReadOnlyList<CourseProgress> Courses, int FirstTryCorrect, int FirstTryTotal, DateTime? LastActivityAt);
public sealed record ItemOption(string Text, bool IsCorrect, int FirstTryCount);
public sealed record ItemAnalysis(
    string CourseTitle,
    string LessonTitle,
    string Question,
    string Kind,
    int Respondents,
    double DifficultyIndex,
    double? DiscriminationIndex,
    double AverageAttempts,
    IReadOnlyList<ItemOption> Options);
public sealed record GroupLearning(IReadOnlyList<StudentLearning> Students, IReadOnlyList<ItemAnalysis> Items);

public interface ILearningInsights
{
    Task<GroupLearning> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);
}

public static class ClassroomJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}

public static class DependencyInjection
{
    public static IServiceCollection AddClassroomApplication(this IServiceCollection services)
    {
        services.AddScoped<ClassroomService>();
        return services;
    }
}
