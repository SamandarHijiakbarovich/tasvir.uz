using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Application;

public sealed record StudentCourseProgressDto(string CourseSlug, string CourseTitle, int CompletedLessons, int TotalLessons);

/// <summary>FirstTryCorrect / FirstTryTotal: testlarga birinchi urinishdagi natija.</summary>
public sealed record StudentLearningDto(
    Guid UserId,
    IReadOnlyList<StudentCourseProgressDto> Courses,
    int FirstTryCorrect,
    int FirstTryTotal,
    DateTime? LastActivityAt);

public sealed record ItemOptionStatDto(string Text, bool IsCorrect, int FirstTryCount);

/// <summary>
/// Test savoli tahlili. DifficultyIndex: birinchi urinishda to‘g‘ri javob berganlar ulushi (0–1).
/// DiscriminationIndex: kuchli va zaif talabalar (27%) ulushlari farqi; javob bergan 4 tadan kam bo‘lsa, hisoblanmaydi.
/// </summary>
public sealed record ItemStatDto(
    Guid StepId,
    string CourseTitle,
    string LessonTitle,
    string Question,
    string Kind,
    int Respondents,
    double DifficultyIndex,
    double? DiscriminationIndex,
    double AverageAttempts,
    IReadOnlyList<ItemOptionStatDto> Options);

public sealed record GroupLearningDto(IReadOnlyList<StudentLearningDto> Students, IReadOnlyList<ItemStatDto> Items);

/// <summary>Guruh natijalarini hisoblaydi. Kirish huquqini chaqiruvchi modul tekshiradi.</summary>
public sealed class AcademyInsightsService(IAcademyRepository repository)
{
    private const int MinRespondentsForDiscrimination = 4;
    private const double ExtremeGroupShare = 0.27;

    public async Task<GroupLearningDto> GetGroupLearningAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct)
    {
        var courses = await repository.GetCoursesAsync(ct);
        var completions = await repository.GetCompletionsOfUsersAsync(userIds, ct);
        var attempts = await repository.GetAttemptsOfUsersAsync(userIds, ct);

        var firstTries = attempts
            .GroupBy(a => (a.UserId, a.StepId))
            .ToDictionary(g => g.Key, g => g.MinBy(a => a.AttemptedAt)!);
        var scoreByUser = firstTries.Values
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.Count(a => a.IsCorrect) / (double)g.Count());

        return new GroupLearningDto(
            userIds.Select(id => BuildStudent(id, courses, completions, attempts, firstTries)).ToList(),
            BuildItems(courses, attempts, firstTries, scoreByUser));
    }

    private static StudentLearningDto BuildStudent(
        Guid userId,
        List<Course> courses,
        List<StepCompletion> completions,
        List<StepAttempt> attempts,
        Dictionary<(Guid UserId, Guid StepId), StepAttempt> firstTries)
    {
        var done = completions.Where(c => c.UserId == userId).Select(c => c.StepId).ToHashSet();
        var own = firstTries.Values.Where(a => a.UserId == userId).ToList();

        DateTime? last = completions.Where(c => c.UserId == userId).Select(c => (DateTime?)c.CompletedAt)
            .Concat(attempts.Where(a => a.UserId == userId).Select(a => (DateTime?)a.AttemptedAt))
            .Max();

        return new StudentLearningDto(
            userId,
            courses.Select(c => new StudentCourseProgressDto(
                c.Slug,
                c.Title,
                c.Lessons.Count(l => l.Steps.All(s => done.Contains(s.Id))),
                c.Lessons.Count)).ToList(),
            own.Count(a => a.IsCorrect),
            own.Count,
            last);
    }

    private static List<ItemStatDto> BuildItems(
        List<Course> courses,
        List<StepAttempt> attempts,
        Dictionary<(Guid UserId, Guid StepId), StepAttempt> firstTries,
        Dictionary<Guid, double> scoreByUser)
    {
        var items = new List<ItemStatDto>();
        foreach (var course in courses)
        {
            foreach (var lesson in course.Lessons)
            {
                foreach (var step in lesson.Steps.Where(s => s.Kind is LessonStepKind.Quiz or LessonStepKind.Exercise))
                {
                    var first = firstTries.Values.Where(a => a.StepId == step.Id).ToList();
                    if (first.Count == 0)
                    {
                        continue;
                    }

                    var (question, kind, options) = Describe(step, first);
                    items.Add(new ItemStatDto(
                        step.Id,
                        course.Title,
                        lesson.Title,
                        question,
                        kind,
                        first.Count,
                        first.Count(a => a.IsCorrect) / (double)first.Count,
                        Discrimination(first, scoreByUser),
                        attempts.Count(a => a.StepId == step.Id) / (double)first.Count,
                        options));
                }
            }
        }

        return items;
    }

    private static (string Question, string Kind, IReadOnlyList<ItemOptionStatDto> Options) Describe(LessonStep step, List<StepAttempt> first)
    {
        if (step.Kind == LessonStepKind.Exercise)
        {
            return (StepContentJson.Deserialize<ExerciseContent>(step.ContentJson).Prompt, "exercise", []);
        }

        var quiz = StepContentJson.Deserialize<QuizContent>(step.ContentJson);
        var options = quiz.Options
            .Select(o => new ItemOptionStatDto(o.Text, o.Id == quiz.CorrectOptionId, first.Count(a => a.Answer == o.Id)))
            .ToList();
        return (quiz.Question, "quiz", options);
    }

    private static double? Discrimination(List<StepAttempt> first, Dictionary<Guid, double> scoreByUser)
    {
        if (first.Count < MinRespondentsForDiscrimination)
        {
            return null;
        }

        var ranked = first.OrderByDescending(a => scoreByUser[a.UserId]).ToList();
        var size = (int)Math.Ceiling(ranked.Count * ExtremeGroupShare);
        var upper = ranked.Take(size).Count(a => a.IsCorrect) / (double)size;
        var lower = ranked.TakeLast(size).Count(a => a.IsCorrect) / (double)size;
        return upper - lower;
    }
}
