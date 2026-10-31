using System.Text.Json;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Classroom.Application;
using Tasvir.Modules.Identity.Application;
using Tasvir.Modules.Studio.Application;

namespace Tasvir.Api;

// Classroom moduliga boshqa modullar ma’lumotini yetkazadi. Modullar bir-birini bilmaydi, ularni faqat xost tutashtiradi:
// keyin biror modul alohida xizmatga ajratilsa, shu fayl HTTP mijozga almashadi, Classroom o‘zgarmaydi.

internal sealed class IdentityMemberDirectory(AuthService auth) : IMemberDirectory
{
    public async Task<string?> GetNameAsync(Guid userId, CancellationToken ct) =>
        (await auth.GetCurrentUserAsync(userId, ct)).Value?.FullName;
}

internal sealed class StudioMaterialSource(StudioService studio) : IMaterialSource
{
    public async Task<SharedMaterial?> GetOwnAsync(Guid userId, Guid materialId, CancellationToken ct)
    {
        var material = (await studio.GetAsync(userId, materialId, ct)).Value;
        return material is null
            ? null
            : new SharedMaterial(material.TemplateKey, material.Title, material.Subject, material.Grade, JsonSerializer.Serialize(material.Content, ClassroomJson.Options));
    }
}

internal sealed class AcademyLearningInsights(AcademyInsightsService academy) : ILearningInsights
{
    public async Task<GroupLearning> GetAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct)
    {
        var data = await academy.GetGroupLearningAsync(userIds, ct);
        return new GroupLearning(
            data.Students.Select(s => new StudentLearning(
                s.UserId,
                s.Courses.Select(c => new CourseProgress(c.CourseTitle, c.CompletedLessons, c.TotalLessons)).ToList(),
                s.FirstTryCorrect,
                s.FirstTryTotal,
                s.LastActivityAt)).ToList(),
            data.Items.Select(i => new ItemAnalysis(
                i.CourseTitle,
                i.LessonTitle,
                i.Question,
                i.Kind,
                i.Respondents,
                i.DifficultyIndex,
                i.DiscriminationIndex,
                i.AverageAttempts,
                i.Options.Select(o => new ItemOption(o.Text, o.IsCorrect, o.FirstTryCount)).ToList())).ToList());
    }
}

internal static class ClassroomAdapters
{
    public static IServiceCollection AddClassroomAdapters(this IServiceCollection services)
    {
        services.AddScoped<IMemberDirectory, IdentityMemberDirectory>();
        services.AddScoped<IMaterialSource, StudioMaterialSource>();
        services.AddScoped<ILearningInsights, AcademyLearningInsights>();
        return services;
    }
}
