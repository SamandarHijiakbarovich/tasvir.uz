using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Infrastructure;

namespace Tasvir.Modules.Academy.Tests;

public sealed class AcademyApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dir = Directory.CreateTempSubdirectory("tasvir-academy-tests-").FullName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Content", $"Data Source={Path.Combine(_dir, "content.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Identity", $"Data Source={Path.Combine(_dir, "identity.db")};Pooling=False");
        builder.UseSetting("ConnectionStrings:Academy", $"Data Source={Path.Combine(_dir, "academy.db")};Pooling=False");
        builder.UseSetting("Identity:RateLimit:PermitPerMinute", "10000");
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

public sealed class AcademyApiTests(AcademyApiFactory factory) : IClassFixture<AcademyApiFactory>
{
    private const string CourseSlug = "vizuallashtirish-asoslari";
    private const string Lesson1 = "nega-korgazmali-material-ishlaydi";
    private const string Lesson2 = "mazmunga-mos-shaklni-tanlash";
    private const string Lesson3 = "birinchi-sxemangiz";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private sealed record Session(string AccessToken);
    private sealed record Summary(string Slug, string Status, int CompletedSteps, int TotalSteps);
    private sealed record Course(int CompletedLessons, int TotalLessons, string? NextLessonSlug, bool HasStarted, List<Summary> Lessons);
    private sealed record Practice(string Mode, int MinItems, int MaxItems, List<string>? Checklist);
    private sealed record Step(Guid Id, string Kind, bool IsCompleted, Practice? Practice, JsonElement? Quiz, JsonElement? SavedResponse);
    private sealed record Lesson(string Status, string? NextLessonSlug, List<Step> Steps);
    private sealed record StepResult(bool StepCompleted, bool? IsCorrect, string? Feedback, bool LessonCompleted, string? NextLessonSlug);
    private sealed record Problem(string Code);
    private sealed record WorkItem(string LessonSlug, string Mode, string? Text, List<string>? Items);

    private readonly HttpClient _client = factory.CreateClient();

    // Har test o‘z foydalanuvchisi bilan ishlaydi
    private async Task<HttpClient> SignedInClient()
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/identity/register", new
        {
            fullName = "Test O‘qituvchi",
            email = $"talaba-{Guid.NewGuid():N}@example.com",
            password = "Yaxshi-parol-2026",
        });
        var session = (await response.Content.ReadFromJsonAsync<Session>(Json))!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.AccessToken);
        return client;
    }

    private static async Task<T> Read<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    private static Task<HttpResponseMessage> Complete(HttpClient client, string lesson, Guid step, object body) =>
        client.PostAsJsonAsync($"/api/academy/lessons/{lesson}/steps/{step}/complete", body);

    // Seed’dagi to‘g‘ri javoblar: test mazmun o‘zgarsa buzilishi kerak
    private static async Task FinishLesson(HttpClient client, string slug, Dictionary<int, string> quizAnswers)
    {
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{slug}"));
        for (var i = 0; i < lesson.Steps.Count; i++)
        {
            var step = lesson.Steps[i];
            object body = step.Kind switch
            {
                "quiz" => new { optionId = quizAnswers[i] },
                "practice" when step.Practice!.Mode == "text" => new { text = "Fotosintez bosqichlari: ketma-ketlik, blok-sxema." },
                "practice" when step.Practice!.Mode == "process" => new { items = new[] { "Yorug‘lik", "Suv parchalanishi", "Glyukoza" } },
                "practice" => new { items = step.Practice!.Checklist },
                _ => new { },
            };

            var result = await Read<StepResult>(await Complete(client, slug, step.Id, body));
            Assert.True(result.StepCompleted, $"{slug} bosqichi {i} yakunlanmadi: {result.Feedback}");
        }
    }

    private static readonly Dictionary<int, string> Lesson1Answers = new() { [2] = "b", [3] = "a" };
    private static readonly Dictionary<int, string> Lesson2Answers = new() { [1] = "b", [2] = "c" };

    [Theory]
    [InlineData("/api/academy/courses/vizuallashtirish-asoslari")]
    [InlineData("/api/academy/lessons/nega-korgazmali-material-ishlaydi")]
    public async Task Requires_sign_in(string path)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.GetAsync(path)).StatusCode);
    }

    [Fact]
    public async Task New_user_sees_first_lesson_available_and_others_locked()
    {
        var client = await SignedInClient();

        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));

        Assert.False(course.HasStarted);
        Assert.Equal(0, course.CompletedLessons);
        Assert.Equal(Lesson1, course.NextLessonSlug);
        Assert.Equal(8, course.TotalLessons);
        Assert.Equal("available", course.Lessons[0].Status);
        Assert.All(course.Lessons.Skip(1), l => Assert.Equal("locked", l.Status));
    }

    [Fact]
    public async Task Locked_lesson_cannot_be_opened_or_started()
    {
        var client = await SignedInClient();

        var open = await client.GetAsync($"/api/academy/lessons/{Lesson2}");
        var start = await client.PostAsync($"/api/academy/lessons/{Lesson2}/start", null);

        Assert.Equal(HttpStatusCode.Conflict, open.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, start.StatusCode);
        Assert.Equal("lesson_locked", (await Read<Problem>(open)).Code);
    }

    [Fact]
    public async Task Starting_lesson_marks_it_in_progress_and_is_idempotent()
    {
        var client = await SignedInClient();

        await client.PostAsync($"/api/academy/lessons/{Lesson1}/start", null);
        var again = await client.PostAsync($"/api/academy/lessons/{Lesson1}/start", null);

        Assert.Equal(HttpStatusCode.OK, again.StatusCode);
        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.True(course.HasStarted);
        Assert.Equal("inProgress", course.Lessons[0].Status);
    }

    [Fact]
    public async Task Lesson_response_does_not_leak_quiz_answers()
    {
        var client = await SignedInClient();

        var raw = await client.GetStringAsync($"/api/academy/lessons/{Lesson1}");

        Assert.DoesNotContain("correctOptionId", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("explanation", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("To‘g‘ri.", raw);
    }

    [Fact]
    public async Task Steps_must_be_completed_in_order()
    {
        var client = await SignedInClient();
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));

        var response = await Complete(client, Lesson1, lesson.Steps[2].Id, new { optionId = "b" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("step_out_of_order", (await Read<Problem>(response)).Code);
    }

    [Fact]
    public async Task Wrong_quiz_answer_does_not_complete_step_and_can_be_retried()
    {
        var client = await SignedInClient();
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));
        await Complete(client, Lesson1, lesson.Steps[0].Id, new { });
        await Complete(client, Lesson1, lesson.Steps[1].Id, new { });

        var wrong = await Read<StepResult>(await Complete(client, Lesson1, lesson.Steps[2].Id, new { optionId = "a" }));
        Assert.False(wrong.StepCompleted);
        Assert.False(wrong.IsCorrect);

        var right = await Read<StepResult>(await Complete(client, Lesson1, lesson.Steps[2].Id, new { optionId = "b" }));
        Assert.True(right.StepCompleted);
        Assert.True(right.IsCorrect);
        Assert.False(right.LessonCompleted);
    }

    [Fact]
    public async Task Unknown_quiz_option_is_rejected()
    {
        var client = await SignedInClient();
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));
        await Complete(client, Lesson1, lesson.Steps[0].Id, new { });
        await Complete(client, Lesson1, lesson.Steps[1].Id, new { });

        var response = await Complete(client, Lesson1, lesson.Steps[2].Id, new { optionId = "z" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Too_short_practice_answer_is_rejected()
    {
        var client = await SignedInClient();
        await FinishUpTo(client, Lesson1, 4, Lesson1Answers);
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));

        var response = await Complete(client, Lesson1, lesson.Steps[4].Id, new { text = "qisqa" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task FinishUpTo(HttpClient client, string slug, int count, Dictionary<int, string> answers)
    {
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{slug}"));
        for (var i = 0; i < count; i++)
        {
            object body = lesson.Steps[i].Kind == "quiz" ? new { optionId = answers[i] } : new { };
            await Complete(client, slug, lesson.Steps[i].Id, body);
        }
    }

    [Fact]
    public async Task Finishing_lesson_unlocks_next_and_saves_answer()
    {
        var client = await SignedInClient();

        await FinishLesson(client, Lesson1, Lesson1Answers);

        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(1, course.CompletedLessons);
        Assert.Equal(Lesson2, course.NextLessonSlug);
        Assert.Equal(["completed", "available", "locked"], course.Lessons.Take(3).Select(l => l.Status));

        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));
        Assert.Equal("completed", lesson.Status);
        Assert.Equal("Fotosintez bosqichlari: ketma-ketlik, blok-sxema.", lesson.Steps[4].SavedResponse!.Value.GetProperty("text").GetString());
    }

    [Fact]
    public async Task Process_practice_requires_item_count_and_checklist_requires_all_ticks()
    {
        var client = await SignedInClient();
        await FinishLesson(client, Lesson1, Lesson1Answers);
        await FinishLesson(client, Lesson2, Lesson2Answers);
        await FinishUpTo(client, Lesson3, 1, []);
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson3}"));

        var tooFew = await Complete(client, Lesson3, lesson.Steps[1].Id, new { items = new[] { "Bitta", "  " } });
        Assert.Equal(HttpStatusCode.BadRequest, tooFew.StatusCode);

        var ok = await Read<StepResult>(await Complete(client, Lesson3, lesson.Steps[1].Id, new { items = new[] { "Bir", "Ikki", "Uch" } }));
        Assert.True(ok.StepCompleted);

        var partial = await Complete(client, Lesson3, lesson.Steps[2].Id, new { items = lesson.Steps[2].Practice!.Checklist!.Take(2) });
        Assert.Equal(HttpStatusCode.BadRequest, partial.StatusCode);

        var done = await Read<StepResult>(await Complete(client, Lesson3, lesson.Steps[2].Id, new { items = lesson.Steps[2].Practice!.Checklist }));
        Assert.True(done.LessonCompleted);
        Assert.Equal("aqliy-xarita-va-tushunchalar-xaritasi", done.NextLessonSlug);

        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(3, course.CompletedLessons);
    }

    [Fact]
    public async Task Completing_same_step_twice_is_safe()
    {
        var client = await SignedInClient();
        var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{Lesson1}"));

        var first = await Complete(client, Lesson1, lesson.Steps[0].Id, new { });
        var second = await Complete(client, Lesson1, lesson.Steps[0].Id, new { });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(1, course.Lessons[0].CompletedSteps);
    }

    [Fact]
    public async Task Progress_is_separate_for_each_user()
    {
        var first = await SignedInClient();
        var second = await SignedInClient();

        await FinishLesson(first, Lesson1, Lesson1Answers);

        var course = await Read<Course>(await second.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(0, course.CompletedLessons);
        Assert.Equal(Lesson1, course.NextLessonSlug);
    }

    // To‘g‘ri javob seed’dan o‘qiladi: yangi dars qo‘shilsa ham test ishlayveradi
    private async Task<string> CorrectOption(Guid stepId)
    {
        using var scope = factory.Services.CreateScope();
        var courses = await scope.ServiceProvider.GetRequiredService<IAcademyRepository>().GetCoursesAsync(CancellationToken.None);
        var step = courses.SelectMany(c => c.Lessons).SelectMany(l => l.Steps).Single(s => s.Id == stepId);
        return StepContentJson.Deserialize<QuizContent>(step.ContentJson).CorrectOptionId;
    }

    private async Task FinishEveryLesson(HttpClient client)
    {
        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        foreach (var summary in course.Lessons)
        {
            var lesson = await Read<Lesson>(await client.GetAsync($"/api/academy/lessons/{summary.Slug}"));
            foreach (var step in lesson.Steps)
            {
                object body = step.Kind switch
                {
                    "quiz" => new { optionId = await CorrectOption(step.Id) },
                    "practice" when step.Practice!.Mode == "text" => new { text = $"{summary.Slug} bo‘yicha javobim." },
                    "practice" when step.Practice!.Mode == "process" => new { items = new[] { "Bir", "Ikki", "Uch" } },
                    "practice" => new { items = step.Practice!.Checklist },
                    _ => new { },
                };
                var result = await Read<StepResult>(await Complete(client, summary.Slug, step.Id, body));
                Assert.True(result.StepCompleted, $"{summary.Slug}: {step.Id}");
            }
        }
    }

    [Fact]
    public async Task Whole_course_can_be_completed()
    {
        var client = await SignedInClient();

        await FinishEveryLesson(client);

        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(8, course.CompletedLessons);
        Assert.Null(course.NextLessonSlug);
    }

    [Fact]
    public async Task Seeding_again_adds_nothing_and_keeps_progress()
    {
        var client = await SignedInClient();
        await FinishLesson(client, Lesson1, Lesson1Answers);

        await factory.Services.InitializeAcademyDatabaseAsync();

        var course = await Read<Course>(await client.GetAsync($"/api/academy/courses/{CourseSlug}"));
        Assert.Equal(8, course.TotalLessons);
        Assert.Equal(1, course.CompletedLessons);
    }

    [Fact]
    public async Task My_work_lists_only_own_results_without_checklists()
    {
        var first = await SignedInClient();
        var second = await SignedInClient();

        await FinishEveryLesson(first);

        var work = await Read<List<WorkItem>>(await first.GetAsync("/api/academy/my-work"));
        Assert.Contains(work, w => w.Mode == "process" && w.Items!.SequenceEqual(["Bir", "Ikki", "Uch"]));
        Assert.Contains(work, w => w.Mode == "text" && w.LessonSlug == Lesson1);
        Assert.DoesNotContain(work, w => w.Mode == "checklist");

        Assert.Empty(await Read<List<WorkItem>>(await second.GetAsync("/api/academy/my-work")));
    }

    [Fact]
    public async Task Unknown_lesson_returns_not_found()
    {
        var client = await SignedInClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/academy/lessons/yoq-dars")).StatusCode);
    }
}
