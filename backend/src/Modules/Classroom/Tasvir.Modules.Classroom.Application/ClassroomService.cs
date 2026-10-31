using System.Security.Cryptography;
using System.Text.Json;
using Tasvir.Modules.Classroom.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Classroom.Application;

public sealed class ClassroomService(
    IClassroomRepository repository,
    IMemberDirectory directory,
    IMaterialSource materials,
    ILearningInsights insights,
    TimeProvider clock)
{
    public const int MaxGroupsPerTeacher = 50;
    public const int MaxMembersPerGroup = 300;
    public const int MaxSubmissionsPerStudent = 100;

    private const int MinNameLength = 2;
    private const int MaxNameLength = 80;
    private const int MaxCommentLength = 1000;
    private const int JoinCodeLength = 8;
    private const int JoinCodeRetries = 5;
    // Adashtiradigan belgilarsiz: 0/O, 1/I/L
    private const string JoinCodeAlphabet = "23456789ABCDEFGHJKMNPQRSTUVWXYZ";

    // Boshqa birovning guruhi "yo‘q" hisoblanadi: uning borligi ham oshkor bo‘lmasin
    private static readonly Error GroupNotFound = new("group_not_found", "Guruh topilmadi.", ErrorKind.NotFound);
    private static readonly Error SubmissionNotFound = new("submission_not_found", "Ish topilmadi.", ErrorKind.NotFound);
    private static readonly Error MaterialNotFound = new("material_not_found", "Material topilmadi.", ErrorKind.NotFound);
    private static readonly Error InvalidCode = new("invalid_code", "Bunday kodli guruh topilmadi. Kodni o‘qituvchingizdan tekshirib oling.", ErrorKind.NotFound);
    private static readonly Error UnknownUser = new("invalid_session", "Sessiya tugagan. Qayta kiring.", ErrorKind.Unauthorized);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<MyGroupsDto>> GetMyGroupsAsync(Guid userId, CancellationToken ct)
    {
        var owned = await repository.ListOwnedAsync(userId, ct);
        var joined = await repository.ListJoinedAsync(userId, ct);
        var counts = await repository.CountMembersAsync(owned.Concat(joined).Select(g => g.Id).ToList(), ct);

        return new MyGroupsDto(
            owned.Select(g => ToSummary(g, counts, withCode: true)).ToList(),
            joined.Select(g => ToSummary(g, counts, withCode: false)).ToList());
    }

    public async Task<Result<GroupSummaryDto>> CreateGroupAsync(Guid userId, CreateGroupRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length is < MinNameLength or > MaxNameLength)
        {
            return Invalid("name", $"Guruh nomini kiriting ({MinNameLength}–{MaxNameLength} belgi).");
        }

        if (await repository.CountOwnedAsync(userId, ct) >= MaxGroupsPerTeacher)
        {
            return new Error("limit_reached", $"Bir o‘qituvchi {MaxGroupsPerTeacher} tagacha guruh yarata oladi.", ErrorKind.Conflict);
        }

        if (await directory.GetNameAsync(userId, ct) is not { } ownerName)
        {
            return UnknownUser;
        }

        for (var attempt = 0; attempt < JoinCodeRetries; attempt++)
        {
            var group = new StudyGroup(userId, ownerName, name, NewJoinCode(), Now);
            repository.Add(group);
            if (await repository.TrySaveChangesAsync(ct))
            {
                return ToSummary(group, new Dictionary<Guid, int>(), withCode: true);
            }
        }

        throw new InvalidOperationException("Noyob guruh kodini yaratib bo‘lmadi.");
    }

    public async Task<Result<GroupSummaryDto>> JoinAsync(Guid userId, JoinGroupRequest request, CancellationToken ct)
    {
        var code = new string((request.Code ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        if (code.Length != JoinCodeLength)
        {
            return Invalid("code", $"Guruh kodi {JoinCodeLength} ta belgidan iborat.");
        }

        if (!request.AcceptSharing)
        {
            return Invalid("acceptSharing", "Guruhga qo‘shilish uchun natijalaringiz o‘qituvchiga ko‘rinishiga rozilik bering.");
        }

        if (await repository.FindGroupByCodeAsync(code, ct) is not { } group)
        {
            return InvalidCode;
        }

        if (group.OwnerUserId == userId)
        {
            return new Error("own_group", "Bu guruhni o‘zingiz yaratgansiz: siz uning o‘qituvchisisiz.", ErrorKind.Conflict);
        }

        var members = await repository.ListMembersAsync(group.Id, ct);
        if (members.All(m => m.UserId != userId))
        {
            if (members.Count >= MaxMembersPerGroup)
            {
                return new Error("group_full", "Guruh to‘lgan. O‘qituvchingizga murojaat qiling.", ErrorKind.Conflict);
            }

            if (await directory.GetNameAsync(userId, ct) is not { } displayName)
            {
                return UnknownUser;
            }

            repository.Add(new GroupMember(group.Id, userId, displayName, Now));
            await repository.TrySaveChangesAsync(ct);
        }

        return ToSummary(group, new Dictionary<Guid, int> { [group.Id] = await MemberCountAsync(group.Id, ct) }, withCode: false);
    }

    public async Task<Result<Unit>> DeleteGroupAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        if (await FindOwnedAsync(userId, groupId, ct) is not { } group)
        {
            return GroupNotFound;
        }

        await repository.RemoveGroupAsync(group, ct);
        return Unit.Value;
    }

    /// <summary>Talaba guruhdan chiqadi yoki o‘qituvchi uni chiqaradi: shu guruhdagi ishlari va baholari ham o‘chadi.</summary>
    public async Task<Result<Unit>> RemoveMemberAsync(Guid actorId, Guid groupId, Guid memberUserId, CancellationToken ct)
    {
        var group = await repository.FindGroupAsync(groupId, ct);
        if (group is null || (actorId != memberUserId && group.OwnerUserId != actorId))
        {
            return GroupNotFound;
        }

        if (await repository.FindMemberAsync(groupId, memberUserId, ct) is not { } member)
        {
            return GroupNotFound;
        }

        await repository.RemoveMemberDataAsync(groupId, memberUserId, ct);
        repository.Remove(member);
        await repository.SaveChangesAsync(ct);
        return Unit.Value;
    }

    public async Task<Result<SubmissionSummaryDto>> SubmitAsync(Guid userId, Guid groupId, SubmitWorkRequest request, CancellationToken ct)
    {
        if (await repository.FindMemberAsync(groupId, userId, ct) is not { } member)
        {
            return GroupNotFound;
        }

        if (await materials.GetOwnAsync(userId, request.MaterialId, ct) is not { } material)
        {
            return MaterialNotFound;
        }

        var existing = await repository.FindSubmissionAsync(groupId, userId, request.MaterialId, ct);
        if (existing is null)
        {
            if ((await repository.ListSubmissionsOfAsync(groupId, userId, ct)).Count >= MaxSubmissionsPerStudent)
            {
                return new Error("limit_reached", $"Bir guruhga {MaxSubmissionsPerStudent} tagacha ish topshirish mumkin.", ErrorKind.Conflict);
            }

            existing = new Submission(groupId, userId, request.MaterialId, material.TemplateKey, material.Title, material.Subject, material.Grade, material.ContentJson, Now);
            repository.Add(existing);
        }
        else
        {
            existing.Resubmit(material.TemplateKey, material.Title, material.Subject, material.Grade, material.ContentJson, Now);
        }

        await repository.SaveChangesAsync(ct);
        var review = (await repository.ListReviewsAsync([existing.Id], ct)).MaxBy(r => r.Version);
        return ToSummary(existing, member.DisplayName, review);
    }

    public async Task<Result<SubmissionDetailDto>> GetSubmissionAsync(Guid userId, Guid submissionId, CancellationToken ct)
    {
        if (await repository.FindSubmissionAsync(submissionId, ct) is not { } submission
            || await repository.FindGroupAsync(submission.GroupId, ct) is not { } group
            || (group.OwnerUserId != userId && submission.UserId != userId))
        {
            return SubmissionNotFound;
        }

        var member = await repository.FindMemberAsync(group.Id, submission.UserId, ct);
        var review = (await repository.ListReviewsAsync([submission.Id], ct)).MaxBy(r => r.Version);

        return new SubmissionDetailDto(
            ToSummary(submission, member?.DisplayName ?? string.Empty, review),
            group.Id,
            group.Name,
            group.OwnerUserId == userId,
            JsonSerializer.Deserialize<JsonElement>(submission.ContentJson),
            ClassroomCatalog.Rubric,
            ClassroomCatalog.MaxCriterionScore);
    }

    public async Task<Result<ReviewDto>> SaveReviewAsync(Guid userId, Guid submissionId, SaveReviewRequest request, CancellationToken ct)
    {
        if (await repository.FindSubmissionAsync(submissionId, ct) is not { } submission
            || await FindOwnedAsync(userId, submission.GroupId, ct) is null)
        {
            return SubmissionNotFound;
        }

        var scores = request.Scores ?? new Dictionary<string, int>();
        if (ClassroomCatalog.Rubric.Any(c => !scores.TryGetValue(c.Key, out var score) || score is < 0 or > ClassroomCatalog.MaxCriterionScore))
        {
            return Invalid("scores", $"Har bir mezonga 0 dan {ClassroomCatalog.MaxCriterionScore} gacha ball qo‘ying.");
        }

        var comment = Clean(request.Comment);
        if (comment?.Length > MaxCommentLength)
        {
            return Invalid("comment", $"Izoh {MaxCommentLength} belgidan oshmasin.");
        }

        var known = ClassroomCatalog.Rubric.ToDictionary(c => c.Key, c => scores[c.Key]);
        var json = JsonSerializer.Serialize(known, ClassroomJson.Options);
        var total = known.Values.Sum();

        var review = await repository.FindReviewAsync(submission.Id, submission.Version, ct);
        if (review is null)
        {
            review = new SubmissionReview(submission.Id, submission.Version, json, total, comment, Now);
            repository.Add(review);
        }
        else
        {
            review.Update(json, total, comment, Now);
        }

        await repository.SaveChangesAsync(ct);
        return ToDto(review);
    }

    public async Task<Result<StudentGroupDto>> GetStudentViewAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        if (await repository.FindMemberAsync(groupId, userId, ct) is not { } member
            || await repository.FindGroupAsync(groupId, ct) is not { } group)
        {
            return GroupNotFound;
        }

        var submissions = await repository.ListSubmissionsOfAsync(groupId, userId, ct);
        var reviews = await repository.ListReviewsAsync(submissions.Select(s => s.Id).ToList(), ct);
        var survey = await repository.FindSurveyResponseAsync(groupId, userId, ct);

        return new StudentGroupDto(
            ToSummary(group, new Dictionary<Guid, int> { [groupId] = await MemberCountAsync(groupId, ct) }, withCode: false),
            member.JoinedAt,
            submissions.Select(s => ToSummary(s, member.DisplayName, LatestReview(reviews, s.Id))).ToList(),
            ClassroomCatalog.Survey,
            survey is null ? null : ReadScores(survey.AnswersJson),
            survey?.Comment);
    }

    public async Task<Result<Unit>> SaveSurveyAsync(Guid userId, Guid groupId, SaveSurveyRequest request, CancellationToken ct)
    {
        if (await repository.FindMemberAsync(groupId, userId, ct) is null)
        {
            return GroupNotFound;
        }

        var answers = request.Answers ?? new Dictionary<string, int>();
        if (ClassroomCatalog.Survey.Any(q => !answers.TryGetValue(q.Key, out var a) || a is < ClassroomCatalog.MinLikert or > ClassroomCatalog.MaxLikert))
        {
            return Invalid("answers", "Barcha savollarga javob bering.");
        }

        var comment = Clean(request.Comment);
        if (comment?.Length > MaxCommentLength)
        {
            return Invalid("comment", $"Izoh {MaxCommentLength} belgidan oshmasin.");
        }

        var json = JsonSerializer.Serialize(ClassroomCatalog.Survey.ToDictionary(q => q.Key, q => answers[q.Key]), ClassroomJson.Options);
        var response = await repository.FindSurveyResponseAsync(groupId, userId, ct);
        if (response is null)
        {
            repository.Add(new SurveyResponse(groupId, userId, json, comment, Now));
        }
        else
        {
            response.Update(json, comment, Now);
        }

        await repository.SaveChangesAsync(ct);
        return Unit.Value;
    }

    public async Task<Result<GroupDashboardDto>> GetDashboardAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        if (await FindOwnedAsync(userId, groupId, ct) is not { } group)
        {
            return GroupNotFound;
        }

        var data = await LoadGroupDataAsync(group, ct);

        return new GroupDashboardDto(
            ToSummary(group, new Dictionary<Guid, int> { [groupId] = data.Members.Count }, withCode: true),
            data.Students,
            data.Submissions,
            data.Learning.Items,
            await BuildSurveyAsync(groupId, ct));
    }

    /// <summary>kind: "natijalar", "test-tahlili" yoki "sorovnoma".</summary>
    public async Task<Result<GroupExportDto>> ExportAsync(Guid userId, Guid groupId, string kind, CancellationToken ct)
    {
        if (await FindOwnedAsync(userId, groupId, ct) is not { } group)
        {
            return GroupNotFound;
        }

        switch (kind)
        {
            case "natijalar":
                var data = await LoadGroupDataAsync(group, ct);
                return new GroupExportDto($"{kind}.csv", ClassroomCsv.Results(data.Students, data.Submissions));
            case "test-tahlili":
                var learning = await insights.GetAsync((await repository.ListMembersAsync(groupId, ct)).Select(m => m.UserId).ToList(), ct);
                return new GroupExportDto($"{kind}.csv", ClassroomCsv.Items(learning.Items));
            case "sorovnoma":
                return new GroupExportDto($"{kind}.csv", ClassroomCsv.Survey(await BuildSurveyAsync(groupId, ct)));
            default:
                return new Error("export_not_found", "Bunday hisobot yo‘q.", ErrorKind.NotFound);
        }
    }

    private sealed record GroupData(
        List<GroupMember> Members,
        GroupLearning Learning,
        List<StudentRowDto> Students,
        List<SubmissionSummaryDto> Submissions);

    private async Task<GroupData> LoadGroupDataAsync(StudyGroup group, CancellationToken ct)
    {
        var members = await repository.ListMembersAsync(group.Id, ct);
        var learning = await insights.GetAsync(members.Select(m => m.UserId).ToList(), ct);
        var submissions = await repository.ListSubmissionsAsync(group.Id, ct);
        var reviews = await repository.ListReviewsAsync(submissions.Select(s => s.Id).ToList(), ct);
        var names = members.ToDictionary(m => m.UserId, m => m.DisplayName);

        var summaries = submissions
            .Select(s => ToSummary(s, names.GetValueOrDefault(s.UserId, string.Empty), LatestReview(reviews, s.Id)))
            .ToList();

        var students = members.Select(member =>
        {
            var row = learning.Students.Single(s => s.UserId == member.UserId);
            var own = summaries.Where(s => s.UserId == member.UserId).ToList();
            var reviewed = own.Where(s => s.Review is not null).ToList();
            return new StudentRowDto(
                member.UserId,
                member.DisplayName,
                member.JoinedAt,
                row.Courses,
                row.FirstTryCorrect,
                row.FirstTryTotal,
                row.LastActivityAt,
                own.Count,
                reviewed.Count,
                reviewed.Count == 0 ? null : reviewed.Average(s => 100.0 * s.Review!.TotalScore / s.Review.MaxScore));
        }).ToList();

        return new GroupData(members, learning, students, summaries);
    }

    private async Task<SurveySummaryDto> BuildSurveyAsync(Guid groupId, CancellationToken ct)
    {
        var responses = await repository.ListSurveyResponsesAsync(groupId, ct);
        var answers = responses.Select(r => ReadScores(r.AnswersJson)).ToList();

        var questions = ClassroomCatalog.Survey.Select(q =>
        {
            var values = answers.Select(a => a[q.Key]).ToList();
            return new SurveyQuestionSummaryDto(
                q.Key,
                q.Text,
                values.Count == 0 ? null : values.Average(),
                Enumerable.Range(ClassroomCatalog.MinLikert, ClassroomCatalog.MaxLikert).Select(v => values.Count(x => x == v)).ToList());
        }).ToList();

        // Izohlar ism va vaqtsiz, tartibi ham yozilish vaqtini oshkor qilmaydi
        var comments = responses
            .Where(r => r.Comment is not null)
            .OrderBy(r => r.Id)
            .Select(r => r.Comment!)
            .ToList();

        return new SurveySummaryDto(responses.Count, questions, comments);
    }

    private async Task<StudyGroup?> FindOwnedAsync(Guid userId, Guid groupId, CancellationToken ct)
    {
        var group = await repository.FindGroupAsync(groupId, ct);
        return group?.OwnerUserId == userId ? group : null;
    }

    private async Task<int> MemberCountAsync(Guid groupId, CancellationToken ct) =>
        (await repository.CountMembersAsync([groupId], ct)).GetValueOrDefault(groupId);

    private static GroupSummaryDto ToSummary(StudyGroup group, IReadOnlyDictionary<Guid, int> counts, bool withCode) =>
        new(group.Id, group.Name, group.OwnerName, counts.GetValueOrDefault(group.Id), group.CreatedAt, withCode ? group.JoinCode : null);

    private static SubmissionSummaryDto ToSummary(Submission submission, string studentName, SubmissionReview? review) =>
        new(submission.Id,
            submission.UserId,
            studentName,
            submission.TemplateKey,
            submission.Title,
            submission.Subject,
            submission.Grade,
            submission.Version,
            submission.SubmittedAt,
            review is null ? null : ToDto(review));

    private static ReviewDto ToDto(SubmissionReview review) =>
        new(review.Version, ReadScores(review.ScoresJson), review.TotalScore, ClassroomCatalog.MaxTotalScore, review.Comment, review.ReviewedAt);

    private static SubmissionReview? LatestReview(List<SubmissionReview> reviews, Guid submissionId) =>
        reviews.Where(r => r.SubmissionId == submissionId).MaxBy(r => r.Version);

    private static Dictionary<string, int> ReadScores(string json) =>
        JsonSerializer.Deserialize<Dictionary<string, int>>(json, ClassroomJson.Options) ?? [];

    private static string? Clean(string? text)
    {
        var trimmed = text?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string NewJoinCode() => RandomNumberGenerator.GetString(JoinCodeAlphabet, JoinCodeLength);

    private static Error Invalid(string field, string message) =>
        new("validation_failed", message, ErrorKind.Validation, new Dictionary<string, string> { [field] = message });
}
