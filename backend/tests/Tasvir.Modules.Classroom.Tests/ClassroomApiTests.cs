using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Tasvir.Modules.Classroom.Application;

namespace Tasvir.Modules.Classroom.Tests;

public sealed class ClassroomApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-classroom-tests-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Studio", $"Data Source={Path.Combine(_dir, "studio.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Classroom", $"Data Source={Path.Combine(_dir, "classroom.db")};Pooling=False");
        builder.UseSetting("Identity:RateLimit:PermitPerMinute", "10000");
        builder.UseSetting("Classroom:RateLimit:PermitPerMinute", "10000");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            Directory.Delete(_dir, recursive: true);
        }
        catch (IOException)
        {
            // Vaqtinchalik papka; Windows faylni band qilgan bo‘lsa, muhim emas
        }
    }
}

public sealed class ClassroomApiTests(ClassroomApiFactory factory) : IClassFixture<ClassroomApiFactory>
{
    private const string Api = "/api/classroom";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken, UserInfo User);
    private sealed record UserInfo(Guid Id, DateTime? PrivacyAcceptedAt);
    private sealed record Created(Guid Id);
    private sealed record Problem(string Code);
    private sealed record Step(Guid Id, string Kind);
    private sealed record Lesson(List<Step> Steps);

    private async Task<(HttpClient Client, Guid UserId)> SignIn(string name)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = name,
            email = $"guruh-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
            acceptPrivacy = true,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var session = (await response.Content.ReadFromJsonAsync<Session>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return (client, session.User.Id);
    }

    private static async Task<T> Read<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    private static async Task<GroupSummaryDto> CreateGroup(HttpClient teacher, string name = "Biologiya 3-kurs")
    {
        var response = await teacher.PostAsJsonAsync($"{Api}/groups", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Read<GroupSummaryDto>(response);
    }

    private static async Task Join(HttpClient student, string code)
    {
        var response = await student.PostAsJsonAsync($"{Api}/groups/join", new { code, acceptSharing = true });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<Guid> CreateMaterial(HttpClient client, string title = "Fotosintez")
    {
        var response = await client.PostAsJsonAsync("/api/studio/materials", new
        {
            templateKey = "process",
            title,
            subject = "Biologiya",
            grade = 6,
            content = new { accent = "sun", items = new[] { new { heading = "Birinchi", text = "Izoh" }, new { heading = "Ikkinchi", text = "Izoh" } } },
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await Read<Created>(response)).Id;
    }

    private static async Task<SubmissionSummaryDto> Submit(HttpClient student, Guid groupId, Guid materialId)
    {
        var response = await student.PostAsJsonAsync($"{Api}/groups/{groupId}/submissions", new { materialId });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Read<SubmissionSummaryDto>(response);
    }

    private static Dictionary<string, int> Scores(int value) =>
        ClassroomCatalog.Rubric.ToDictionary(c => c.Key, _ => value);

    private static Dictionary<string, int> SurveyAnswers(int value) =>
        ClassroomCatalog.Survey.ToDictionary(q => q.Key, _ => value);

    [Fact]
    public async Task Requires_sign_in()
    {
        var response = await factory.CreateClient().GetAsync($"{Api}/groups");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Creator_is_teacher_and_sees_join_code()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");

        var group = await CreateGroup(teacher);

        Assert.Equal(8, group.JoinCode!.Length);
        Assert.Equal("O‘qituvchi", group.TeacherName);
        var mine = await Read<MyGroupsDto>(await teacher.GetAsync($"{Api}/groups"));
        Assert.Single(mine.Teaching);
        Assert.Empty(mine.Studying);
    }

    [Fact]
    public async Task Group_name_is_validated()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var response = await teacher.PostAsJsonAsync($"{Api}/groups", new { name = "A" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Joining_requires_explicit_consent_to_share_results()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);

        var response = await student.PostAsJsonAsync($"{Api}/groups/join", new { code = group.JoinCode, acceptSharing = false });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty((await Read<MyGroupsDto>(await student.GetAsync($"{Api}/groups"))).Studying);
    }

    [Fact]
    public async Task Student_joins_by_code_ignoring_case_and_spaces_and_does_not_see_code()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);

        await Join(student, $" {group.JoinCode!.ToLowerInvariant()[..4]} {group.JoinCode.ToLowerInvariant()[4..]} ");
        await Join(student, group.JoinCode); // takroriy qo‘shilish xavfsiz

        var mine = await Read<MyGroupsDto>(await student.GetAsync($"{Api}/groups"));
        var joined = Assert.Single(mine.Studying);
        Assert.Null(joined.JoinCode);
        Assert.Equal(1, joined.MemberCount);
    }

    [Fact]
    public async Task Wrong_code_and_own_group_are_rejected()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var group = await CreateGroup(teacher);

        var wrong = await teacher.PostAsJsonAsync($"{Api}/groups/join", new { code = "ZZZZZZZZ", acceptSharing = true });
        Assert.Equal(HttpStatusCode.NotFound, wrong.StatusCode);

        var own = await teacher.PostAsJsonAsync($"{Api}/groups/join", new { code = group.JoinCode, acceptSharing = true });
        Assert.Equal(HttpStatusCode.Conflict, own.StatusCode);
        Assert.Equal("own_group", (await Read<Problem>(own)).Code);
    }

    [Fact]
    public async Task Outsiders_and_students_cannot_open_teacher_data()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var (outsider, _) = await SignIn("Begona");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);

        foreach (var client in new[] { student, outsider })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Api}/groups/{group.Id}/dashboard")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"{Api}/groups/{group.Id}/export/natijalar")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"{Api}/groups/{group.Id}")).StatusCode);
        }

        Assert.Equal(HttpStatusCode.NotFound, (await outsider.GetAsync($"{Api}/groups/{group.Id}/me")).StatusCode);
    }

    [Fact]
    public async Task Cannot_submit_someone_elses_material()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var (other, _) = await SignIn("Boshqa");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var foreign = await CreateMaterial(other);

        var response = await student.PostAsJsonAsync($"{Api}/groups/{group.Id}/submissions", new { materialId = foreign });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Teacher_reviews_snapshot_and_resubmission_raises_version()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var materialId = await CreateMaterial(student);
        var submission = await Submit(student, group.Id, materialId);

        // Talaba keyin Studiyada o‘zgartirsa, topshirilgan nusxa o‘zgarmaydi
        await student.PutAsJsonAsync($"/api/studio/materials/{materialId}", new
        {
            templateKey = "process",
            title = "O‘zgargan sarlavha",
            content = new { accent = "sun", items = new[] { new { heading = "A", text = "B" }, new { heading = "C", text = "D" } } },
        });
        var opened = await Read<SubmissionDetailDto>(await teacher.GetAsync($"{Api}/submissions/{submission.Id}"));
        Assert.Equal("Fotosintez", opened.Summary.Title);
        Assert.True(opened.CanReview);
        Assert.Equal(8, opened.Rubric.Count);

        var saved = await teacher.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = Scores(2), comment = "Yaxshi, izohlarni qisqartiring." });
        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        var review = await Read<ReviewDto>(saved);
        Assert.Equal(16, review.TotalScore);
        Assert.Equal(24, review.MaxScore);

        var seen = await Read<SubmissionDetailDto>(await student.GetAsync($"{Api}/submissions/{submission.Id}"));
        Assert.False(seen.CanReview);
        Assert.Equal("Yaxshi, izohlarni qisqartiring.", seen.Summary.Review!.Comment);

        var again = await Submit(student, group.Id, materialId);
        Assert.Equal(submission.Id, again.Id);
        Assert.Equal(2, again.Version);
        Assert.Equal("O‘zgargan sarlavha", again.Title);
        Assert.Equal(1, again.Review!.Version); // eski baho ko‘rinadi, yangi versiya baholanmagan
    }

    [Fact]
    public async Task Student_cannot_review_and_scores_are_validated()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var submission = await Submit(student, group.Id, await CreateMaterial(student));

        var byStudent = await student.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = Scores(3) });
        Assert.Equal(HttpStatusCode.NotFound, byStudent.StatusCode);

        var tooHigh = await teacher.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = Scores(4) });
        Assert.Equal(HttpStatusCode.BadRequest, tooHigh.StatusCode);

        var missing = await teacher.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = new Dictionary<string, int> { ["accuracy"] = 3 } });
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
    }

    [Fact]
    public async Task Dashboard_counts_first_attempts_for_item_analysis()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (strong, _) = await SignIn("Kuchli Talaba");
        var (weak, _) = await SignIn("Zaif Talaba");
        var group = await CreateGroup(teacher);
        await Join(strong, group.JoinCode!);
        await Join(weak, group.JoinCode!);

        const string slug = "nega-korgazmali-material-ishlaydi";
        await PassLessonUpToQuiz(strong, slug);
        var quizId = await PassLessonUpToQuiz(weak, slug);

        await Answer(strong, slug, quizId, "b"); // to‘g‘ri
        await Answer(weak, slug, quizId, "a");   // xato
        await Answer(weak, slug, quizId, "b");   // ikkinchi urinishda to‘g‘ri

        var dashboard = await Read<GroupDashboardDto>(await teacher.GetAsync($"{Api}/groups/{group.Id}/dashboard"));

        var item = Assert.Single(dashboard.Items);
        Assert.Equal(2, item.Respondents);
        Assert.Equal(0.5, item.DifficultyIndex);
        Assert.Equal(1.5, item.AverageAttempts);
        Assert.Null(item.DiscriminationIndex); // javob berganlar kam
        Assert.Equal(1, item.Options.Single(o => o.IsCorrect).FirstTryCount);
        Assert.Equal(2, dashboard.Students.Count);
        Assert.Equal(1, dashboard.Students.Single(s => s.Name == "Kuchli Talaba").FirstTryCorrect);
        Assert.Equal(0, dashboard.Students.Single(s => s.Name == "Zaif Talaba").FirstTryCorrect);
        Assert.All(dashboard.Students, s => Assert.Equal(2, s.Courses.Count));
    }

    private static async Task<Guid> PassLessonUpToQuiz(HttpClient client, string slug)
    {
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{slug}"));
        foreach (var step in lesson.Steps)
        {
            if (step.Kind == "quiz")
            {
                return step.Id;
            }

            Assert.Equal("reading", step.Kind);
            await client.PostAsJsonAsync($"/api/academy/lessons/{slug}/steps/{step.Id}/complete", new { });
        }

        throw new InvalidOperationException("Darsda test yo‘q.");
    }

    private static async Task Answer(HttpClient client, string slug, Guid stepId, string optionId)
    {
        var response = await client.PostAsJsonAsync($"/api/academy/lessons/{slug}/steps/{stepId}/complete", new { optionId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Survey_is_aggregated_and_comments_are_anonymous()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (first, _) = await SignIn("Birinchi Talaba");
        var (second, _) = await SignIn("Ikkinchi Talaba");
        var group = await CreateGroup(teacher);
        await Join(first, group.JoinCode!);
        await Join(second, group.JoinCode!);

        var incomplete = await first.PutAsJsonAsync($"{Api}/groups/{group.Id}/survey", new { answers = new Dictionary<string, int> { ["usability"] = 5 } });
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await first.PutAsJsonAsync($"{Api}/groups/{group.Id}/survey", new { answers = SurveyAnswers(5), comment = "Foydali bo‘ldi" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await second.PutAsJsonAsync($"{Api}/groups/{group.Id}/survey", new { answers = SurveyAnswers(3) })).StatusCode);
        // Javobni o‘zgartirish mumkin, ikkinchi nusxa paydo bo‘lmaydi
        Assert.Equal(HttpStatusCode.NoContent, (await second.PutAsJsonAsync($"{Api}/groups/{group.Id}/survey", new { answers = SurveyAnswers(4) })).StatusCode);

        var text = await teacher.GetStringAsync($"{Api}/groups/{group.Id}/dashboard");
        var dashboard = JsonSerializer.Deserialize<GroupDashboardDto>(text, Json)!;

        Assert.Equal(2, dashboard.Survey.Respondents);
        Assert.Equal(4.5, dashboard.Survey.Questions[0].Average);
        Assert.Equal([0, 0, 0, 1, 1], dashboard.Survey.Questions[0].Counts);
        Assert.Equal(["Foydali bo‘ldi"], dashboard.Survey.Comments);
    }

    [Fact]
    public async Task Export_is_excel_friendly_and_neutralizes_formulas()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("=HYPERLINK(1) O‘g‘il; Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var submission = await Submit(student, group.Id, await CreateMaterial(student));
        await teacher.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = Scores(3) });

        var response = await teacher.GetAsync($"{Api}/groups/{group.Id}/export/natijalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType!.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(Encoding.UTF8.GetPreamble(), bytes[..3]);
        var lines = Encoding.UTF8.GetString(bytes[3..]).Split("\r\n");
        Assert.StartsWith("F.I.Sh.;", lines[0]);
        Assert.Contains("Ilmiy to‘g‘rilik", lines[0]);
        Assert.StartsWith("\"'=HYPERLINK(1) O‘g‘il; Talaba\";", lines[1]);
        Assert.EndsWith(";24", lines[1]);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"{Api}/groups/{group.Id}/export/test-tahlili")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"{Api}/groups/{group.Id}/export/sorovnoma")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync($"{Api}/groups/{group.Id}/export/boshqa")).StatusCode);
    }

    [Fact]
    public async Task Leaving_removes_students_group_data()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, studentId) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var submission = await Submit(student, group.Id, await CreateMaterial(student));
        await teacher.PutAsJsonAsync($"{Api}/submissions/{submission.Id}/review", new { scores = Scores(1) });
        await student.PutAsJsonAsync($"{Api}/groups/{group.Id}/survey", new { answers = SurveyAnswers(5) });

        var left = await student.DeleteAsync($"{Api}/groups/{group.Id}/members/{studentId}");

        Assert.Equal(HttpStatusCode.NoContent, left.StatusCode);
        var dashboard = await Read<GroupDashboardDto>(await teacher.GetAsync($"{Api}/groups/{group.Id}/dashboard"));
        Assert.Empty(dashboard.Students);
        Assert.Empty(dashboard.Submissions);
        Assert.Equal(0, dashboard.Survey.Respondents);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync($"{Api}/submissions/{submission.Id}")).StatusCode);
    }

    [Fact]
    public async Task Student_cannot_remove_another_student()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (first, _) = await SignIn("Birinchi");
        var (second, secondId) = await SignIn("Ikkinchi");
        var group = await CreateGroup(teacher);
        await Join(first, group.JoinCode!);
        await Join(second, group.JoinCode!);

        Assert.Equal(HttpStatusCode.NotFound, (await first.DeleteAsync($"{Api}/groups/{group.Id}/members/{secondId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await teacher.DeleteAsync($"{Api}/groups/{group.Id}/members/{secondId}")).StatusCode);
    }

    [Fact]
    public async Task Deleting_group_removes_it_for_everyone()
    {
        var (teacher, _) = await SignIn("O‘qituvchi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        var submission = await Submit(student, group.Id, await CreateMaterial(student));

        Assert.Equal(HttpStatusCode.NoContent, (await teacher.DeleteAsync($"{Api}/groups/{group.Id}")).StatusCode);

        Assert.Empty((await Read<MyGroupsDto>(await student.GetAsync($"{Api}/groups"))).Studying);
        Assert.Equal(HttpStatusCode.NotFound, (await student.GetAsync($"{Api}/submissions/{submission.Id}")).StatusCode);
    }

    [Fact]
    public async Task Registration_records_privacy_consent_and_requires_it()
    {
        var client = factory.CreateClient();
        var refused = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Roziliksiz",
            email = $"rozi-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
            acceptPrivacy = false,
        });
        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);

        var accepted = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Rozi",
            email = $"rozi-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
            acceptPrivacy = true,
        });
        Assert.NotNull((await Read<Session>(accepted)).User.PrivacyAcceptedAt);
    }
}
