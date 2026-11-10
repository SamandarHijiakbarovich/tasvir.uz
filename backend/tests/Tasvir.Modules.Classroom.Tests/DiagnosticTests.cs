using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tasvir.Modules.Classroom.Application;

namespace Tasvir.Modules.Classroom.Tests;

public sealed class DiagnosticStatisticsTests
{
    [Theory]
    [InlineData(1.0, 1, 0.5)]            // df=1 — Koshi taqsimoti: P(|T| ≥ 1) = 0,5
    [InlineData(2.228139, 10, 0.05)]     // jadvaldagi kritik qiymat
    [InlineData(2.0, 8, 0.0805)]
    public void Two_tailed_p_matches_student_table(double t, double df, double expected)
    {
        Assert.Equal(expected, DiagnosticStatistics.TwoTailedP(t, df), 4);
    }

    [Fact]
    public void Paired_test_matches_hand_calculation()
    {
        // O‘sishlar: 2, 3, 1, 4 → o‘rtacha 2,5; s = 1,291; t = 2,5 / (1,291 / 2) = 3,873
        var result = DiagnosticStatistics.Paired([(5, 7), (6, 9), (7, 8), (8, 12)])!;

        Assert.Equal(4, result.N);
        Assert.Equal(6.5, result.MeanPre, 6);
        Assert.Equal(9.0, result.MeanPost, 6);
        Assert.Equal(2.5, result.MeanGain, 6);
        Assert.Equal(3.873, result.T!.Value, 3);
        Assert.Equal(3, result.Df);
        Assert.Equal(0.0305, result.P!.Value, 3);
        Assert.Equal(1.936, result.EffectSize!.Value, 3);
    }

    [Fact]
    public void Paired_test_needs_two_students_and_variation()
    {
        Assert.Null(DiagnosticStatistics.Paired([(5, 7)]));

        var same = DiagnosticStatistics.Paired([(5, 7), (6, 8)])!;
        Assert.Null(same.T);
        Assert.Null(same.P);
    }

    [Fact]
    public void Welch_test_matches_hand_calculation()
    {
        // O‘rtachalar 3 va 5, s ikkalasida 1,581, xato 1 → t = −2, df = 8
        var result = DiagnosticStatistics.Welch([1, 2, 3, 4, 5], [3, 4, 5, 6, 7])!;

        Assert.Equal(-2.0, result.T!.Value, 6);
        Assert.Equal(8.0, result.Df!.Value, 6);
        Assert.Equal(0.0805, result.P!.Value, 4);
        Assert.Equal(-1.265, result.EffectSize!.Value, 3);
    }
}

public sealed class DiagnosticApiTests(ClassroomApiFactory factory) : IClassFixture<ClassroomApiFactory>
{
    private const string Api = "/api/classroom";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken, UserInfo User);
    private sealed record UserInfo(Guid Id);
    private sealed record Problem(string Code);

    private async Task<(HttpClient Client, Guid UserId)> SignIn(string name)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = name,
            email = $"diag-{Guid.NewGuid():N}@example.com",
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

    private static async Task<GroupSummaryDto> CreateGroup(HttpClient teacher, string name = "Tajriba guruhi")
    {
        var response = await teacher.PostAsJsonAsync($"{Api}/groups", new { name });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return await Read<GroupSummaryDto>(response);
    }

    private static async Task Join(HttpClient student, string code) =>
        Assert.Equal(HttpStatusCode.OK, (await student.PostAsJsonAsync($"{Api}/groups/join", new { code, acceptSharing = true })).StatusCode);

    private static async Task SetOpen(HttpClient teacher, Guid groupId, string stage, bool isOpen) =>
        Assert.Equal(HttpStatusCode.NoContent, (await teacher.PutAsJsonAsync($"{Api}/groups/{groupId}/diagnostics/{stage}/state", new { isOpen })).StatusCode);

    /// <summary>Dastlabki "correct" ta savolga to‘g‘ri, qolganiga noto‘g‘ri javob.</summary>
    private static Dictionary<string, string> Answers(int correct) =>
        DiagnosticCatalog.Questions.Select((q, i) => (q, i)).ToDictionary(
            x => x.q.Key,
            x => x.i < correct ? x.q.CorrectOptionId : x.q.Options.First(o => o.Id != x.q.CorrectOptionId).Id);

    private static Task<HttpResponseMessage> Take(HttpClient student, Guid groupId, string stage, int correct) =>
        student.PostAsJsonAsync($"{Api}/groups/{groupId}/diagnostics/{stage}", new { answers = Answers(correct) });

    [Fact]
    public async Task Test_opens_only_when_teacher_opens_it()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);

        var closed = await student.GetAsync($"{Api}/groups/{group.Id}/diagnostics/pre");
        Assert.Equal(HttpStatusCode.Conflict, closed.StatusCode);
        Assert.Equal("diagnostic_closed", (await Read<Problem>(closed)).Code);

        await SetOpen(teacher, group.Id, "pre", true);
        var open = await student.GetAsync($"{Api}/groups/{group.Id}/diagnostics/pre");
        Assert.Equal(HttpStatusCode.OK, open.StatusCode);

        // To‘g‘ri javob talabaga yuborilmaydi
        var body = await open.Content.ReadAsStringAsync();
        Assert.DoesNotContain("correct", body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(DiagnosticCatalog.MaxScore, (await Read<DiagnosticTestDto>(open)).Questions.Count);

        await SetOpen(teacher, group.Id, "pre", false);
        Assert.Equal(HttpStatusCode.Conflict, (await Take(student, group.Id, "pre", 5)).StatusCode);
    }

    [Fact]
    public async Task Test_is_scored_once_and_requires_all_answers()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var (student, _) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        await SetOpen(teacher, group.Id, "pre", true);

        var partial = Answers(3);
        partial.Remove(partial.Keys.First());
        var incomplete = await student.PostAsJsonAsync($"{Api}/groups/{group.Id}/diagnostics/pre", new { answers = partial });
        Assert.Equal(HttpStatusCode.BadRequest, incomplete.StatusCode);

        var first = await Take(student, group.Id, "pre", 7);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var score = await Read<DiagnosticScoreDto>(first);
        Assert.Equal(7, score.Score);
        Assert.Equal(DiagnosticCatalog.MaxScore, score.MaxScore);

        var again = await Take(student, group.Id, "pre", 15);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal("diagnostic_done", (await Read<Problem>(again)).Code);

        var view = await Read<StudentGroupDto>(await student.GetAsync($"{Api}/groups/{group.Id}/me"));
        Assert.Equal(7, view.Diagnostics.Single(d => d.Stage == "pre").Result!.Score);
        Assert.Null(view.Diagnostics.Single(d => d.Stage == "post").Result);
    }

    [Fact]
    public async Task Dashboard_shows_pre_post_and_paired_test()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var group = await CreateGroup(teacher);
        await SetOpen(teacher, group.Id, "pre", true);
        await SetOpen(teacher, group.Id, "post", true);

        (int Pre, int Post)[] scores = [(5, 9), (6, 8), (4, 10)];
        foreach (var (pre, post) in scores)
        {
            var (student, _) = await SignIn($"Talaba {pre}");
            await Join(student, group.JoinCode!);
            Assert.Equal(HttpStatusCode.Created, (await Take(student, group.Id, "pre", pre)).StatusCode);
            Assert.Equal(HttpStatusCode.Created, (await Take(student, group.Id, "post", post)).StatusCode);
        }

        var dashboard = await Read<GroupDashboardDto>(await teacher.GetAsync($"{Api}/groups/{group.Id}/dashboard"));
        var diagnostics = dashboard.Diagnostics;

        Assert.All(diagnostics.Stages, s => Assert.Equal(3, s.Respondents));
        Assert.Equal(3, diagnostics.Students.Count(s => s.Pre is not null && s.Post is not null));
        Assert.Equal(3, diagnostics.Paired!.N);
        Assert.Equal(4.0, diagnostics.Paired.MeanGain, 6);
        Assert.NotNull(diagnostics.Paired.P);

        // 1-savolga hamma ikkala testda to‘g‘ri javob bergan
        Assert.Equal(1.0, diagnostics.Items[0].PreCorrectShare);

        var csv = await (await teacher.GetAsync($"{Api}/groups/{group.Id}/export/diagnostika")).Content.ReadAsStringAsync();
        Assert.Contains("B1", csv);
        Assert.Contains("Juftlangan Styudent mezoni", csv);
    }

    [Fact]
    public async Task Only_teacher_opens_tests_and_compares_own_groups()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var (other, _) = await SignIn("Boşqa öqituvçi");
        var (student, _) = await SignIn("Talaba");
        var experiment = await CreateGroup(teacher, "Tajriba");
        var control = await CreateGroup(teacher, "Nazorat");
        var foreign = await CreateGroup(other, "Begona");
        await Join(student, experiment.JoinCode!);

        var byStudent = await student.PutAsJsonAsync($"{Api}/groups/{experiment.Id}/diagnostics/pre/state", new { isOpen = true });
        Assert.Equal(HttpStatusCode.NotFound, byStudent.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.PutAsJsonAsync($"{Api}/groups/{experiment.Id}/diagnostics/oraliq/state", new { isOpen = true })).StatusCode);

        Assert.Equal(HttpStatusCode.OK, (await teacher.GetAsync($"{Api}/groups/{experiment.Id}/compare/{control.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync($"{Api}/groups/{experiment.Id}/compare/{foreign.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await teacher.GetAsync($"{Api}/groups/{experiment.Id}/compare/{experiment.Id}")).StatusCode);
    }

    [Fact]
    public async Task Comparison_uses_both_groups_results()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var experiment = await CreateGroup(teacher, "Tajriba");
        var control = await CreateGroup(teacher, "Nazorat");

        async Task Fill(GroupSummaryDto group, int[] posts)
        {
            await SetOpen(teacher, group.Id, "pre", true);
            await SetOpen(teacher, group.Id, "post", true);
            foreach (var post in posts)
            {
                var (student, _) = await SignIn("Talaba");
                await Join(student, group.JoinCode!);
                await Take(student, group.Id, "pre", 5);
                await Take(student, group.Id, "post", post);
            }
        }

        await Fill(experiment, [10, 11, 12]);
        await Fill(control, [6, 7, 8]);

        var comparison = await Read<GroupComparisonDto>(await teacher.GetAsync($"{Api}/groups/{experiment.Id}/compare/{control.Id}"));
        Assert.Equal("Tajriba", comparison.GroupA.Name);
        Assert.Equal(11.0, comparison.Post!.MeanA, 6);
        Assert.Equal(7.0, comparison.Post.MeanB, 6);
        Assert.True(comparison.Post.P < DiagnosticStatistics.Alpha);
        Assert.Null(comparison.Pre!.T); // boshlang‘ich natijalar bir xil: farq yo‘q
    }

    [Fact]
    public async Task Leaving_group_removes_diagnostic_results()
    {
        var (teacher, _) = await SignIn("Öqituvçi");
        var (student, studentId) = await SignIn("Talaba");
        var group = await CreateGroup(teacher);
        await Join(student, group.JoinCode!);
        await SetOpen(teacher, group.Id, "pre", true);
        await Take(student, group.Id, "pre", 9);

        Assert.Equal(HttpStatusCode.NoContent, (await student.DeleteAsync($"{Api}/groups/{group.Id}/members/{studentId}")).StatusCode);
        await Join(student, group.JoinCode!);

        // Qayta qo‘shilgan talaba testni yana topshira oladi: eski natija o‘chgan
        Assert.Equal(HttpStatusCode.Created, (await Take(student, group.Id, "pre", 4)).StatusCode);
    }
}
