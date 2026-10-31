using System.Globalization;
using Tasvir.Modules.Academy.Domain;
using Tasvir.SharedKernel;

namespace Tasvir.Modules.Academy.Application;

public sealed class AcademyService(IAcademyRepository repository, TimeProvider clock)
{
    private const int MinTextLength = 10;
    private const int MaxTextLength = 2000;
    private const int MaxItemLength = 80;

    private static readonly Error CourseNotFound = new("course_not_found", "Kurs topilmadi.", ErrorKind.NotFound);
    private static readonly Error LessonNotFound = new("lesson_not_found", "Dars topilmadi.", ErrorKind.NotFound);
    private static readonly Error StepNotFound = new("step_not_found", "Dars bosqichi topilmadi.", ErrorKind.NotFound);
    private static readonly Error LessonLocked = new("lesson_locked", "Bu dars oldingi darsni tugatgandan so‘ng ochiladi.", ErrorKind.Conflict);
    private static readonly Error StepOutOfOrder = new("step_out_of_order", "Avval oldingi bosqichni yakunlang.", ErrorKind.Conflict);

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<IReadOnlyList<CourseDto>>> GetCoursesAsync(Guid userId, CancellationToken ct)
    {
        var courses = await repository.GetCoursesAsync(ct);
        var result = new List<CourseDto>();
        foreach (var course in courses)
        {
            result.Add(await BuildCourseAsync(userId, course, ct));
        }

        return result;
    }

    public async Task<Result<CourseDto>> GetCourseAsync(Guid userId, string slug, CancellationToken ct)
    {
        var course = await repository.FindCourseBySlugAsync(slug, ct);
        return course is null ? CourseNotFound : await BuildCourseAsync(userId, course, ct);
    }

    private async Task<CourseDto> BuildCourseAsync(Guid userId, Course course, CancellationToken ct)
    {
        var state = await LoadStateAsync(userId, course, ct);
        var lessons = course.Lessons.Select(l => new LessonSummaryDto(
            l.Slug,
            l.Title,
            l.Summary,
            l.EstimatedMinutes,
            state.StatusOf(l),
            l.Steps.Count(s => state.Completions.ContainsKey(s.Id)),
            l.Steps.Count)).ToList();

        return new CourseDto(
            course.Slug,
            course.Title,
            course.Description,
            lessons.Count(l => l.Status == LessonStatus.Completed),
            lessons.Count,
            state.NextLesson()?.Slug,
            state.Progress.Count > 0,
            lessons);
    }

    public async Task<Result<LessonDto>> GetLessonAsync(Guid userId, string lessonSlug, CancellationToken ct)
    {
        var course = await repository.FindCourseByLessonSlugAsync(lessonSlug, ct);
        if (course is null)
        {
            return LessonNotFound;
        }

        var lesson = course.Lessons.Single(l => l.Slug == lessonSlug);
        var state = await LoadStateAsync(userId, course, ct);
        if (state.StatusOf(lesson) == LessonStatus.Locked)
        {
            return LessonLocked;
        }

        return new LessonDto(
            lesson.Slug,
            lesson.Title,
            lesson.Summary,
            lesson.EstimatedMinutes,
            course.Slug,
            course.Title,
            lesson.SortOrder,
            course.Lessons.Count,
            state.StatusOf(lesson),
            state.NextLesson()?.Slug,
            lesson.Steps.Select(s => ToDto(s, state)).ToList());
    }

    /// <summary>"Darsni boshlash": takroriy chaqiruv xavfsiz, mavjud holat saqlanib qoladi.</summary>
    public async Task<Result<LessonSummaryDto>> StartLessonAsync(Guid userId, string lessonSlug, CancellationToken ct)
    {
        var course = await repository.FindCourseByLessonSlugAsync(lessonSlug, ct);
        if (course is null)
        {
            return LessonNotFound;
        }

        var lesson = course.Lessons.Single(l => l.Slug == lessonSlug);
        var state = await LoadStateAsync(userId, course, ct);
        if (state.StatusOf(lesson) == LessonStatus.Locked)
        {
            return LessonLocked;
        }

        if (!state.Progress.ContainsKey(lesson.Id))
        {
            repository.Add(new LessonProgress(userId, lesson.Id, Now));
            await repository.TrySaveChangesAsync(ct);
        }

        var isCompleted = state.StatusOf(lesson) == LessonStatus.Completed;
        return new LessonSummaryDto(
            lesson.Slug,
            lesson.Title,
            lesson.Summary,
            lesson.EstimatedMinutes,
            isCompleted ? LessonStatus.Completed : LessonStatus.InProgress,
            lesson.Steps.Count(s => state.Completions.ContainsKey(s.Id)),
            lesson.Steps.Count);
    }

    public async Task<Result<StepResultDto>> CompleteStepAsync(Guid userId, string lessonSlug, Guid stepId, CompleteStepRequest request, CancellationToken ct)
    {
        var course = await repository.FindCourseByLessonSlugAsync(lessonSlug, ct);
        if (course is null)
        {
            return LessonNotFound;
        }

        var lesson = course.Lessons.Single(l => l.Slug == lessonSlug);
        var step = lesson.Steps.SingleOrDefault(s => s.Id == stepId);
        if (step is null)
        {
            return StepNotFound;
        }

        var state = await LoadStateAsync(userId, course, ct);
        if (state.StatusOf(lesson) == LessonStatus.Locked)
        {
            return LessonLocked;
        }

        if (lesson.Steps.Any(s => s.SortOrder < step.SortOrder && !state.Completions.ContainsKey(s.Id)))
        {
            return StepOutOfOrder;
        }

        // Yakunlangan bosqich qayta yuborilsa, natija o‘zgarmaydi (masalan, ikki marta bosish)
        if (state.Completions.ContainsKey(step.Id))
        {
            return new StepResultDto(true, null, null, state.StatusOf(lesson) == LessonStatus.Completed, state.NextLesson()?.Slug);
        }

        var evaluation = Evaluate(step, request);
        if (evaluation.Error is { } error)
        {
            return error;
        }

        var now = Now;
        if (evaluation.IsCorrect is { } isCorrect)
        {
            repository.Add(new StepAttempt(userId, step.Id, (request.OptionId ?? request.Text ?? string.Empty).Trim(), isCorrect, now));
        }

        if (!evaluation.Passed)
        {
            await repository.TrySaveChangesAsync(ct);
            return new StepResultDto(false, evaluation.IsCorrect, evaluation.Feedback, false, null);
        }

        if (!state.Progress.TryGetValue(lesson.Id, out var progress))
        {
            progress = new LessonProgress(userId, lesson.Id, now);
            repository.Add(progress);
            state.Progress[lesson.Id] = progress;
        }

        var completion = new StepCompletion(userId, step.Id, now, evaluation.ResponseJson);
        repository.Add(completion);
        state.Completions[step.Id] = completion;

        var lessonCompleted = lesson.Steps.All(s => state.Completions.ContainsKey(s.Id));
        if (lessonCompleted)
        {
            progress.Complete(now);
        }

        await repository.TrySaveChangesAsync(ct);

        return new StepResultDto(true, evaluation.IsCorrect, evaluation.Feedback, lessonCompleted, state.NextLesson()?.Slug);
    }

    public async Task<Result<IReadOnlyList<MyWorkItemDto>>> GetMyWorkAsync(Guid userId, CancellationToken ct)
    {
        var courses = await repository.GetCoursesAsync(ct);
        var steps = courses
            .SelectMany(c => c.Lessons)
            .SelectMany(l => l.Steps.Where(s => s.Kind == LessonStepKind.Practice).Select(s => (Lesson: l, Step: s)))
            .ToList();
        var completions = await repository.GetStepCompletionsAsync(userId, steps.Select(x => x.Step.Id).ToList(), ct);
        var byStep = completions.ToDictionary(c => c.StepId);

        var items = new List<MyWorkItemDto>();
        foreach (var (lesson, step) in steps)
        {
            if (!byStep.TryGetValue(step.Id, out var completion) || completion.ResponseJson is null)
            {
                continue;
            }

            var practice = StepContentJson.Deserialize<PracticeContent>(step.ContentJson);
            // O‘zini tekshirish ro‘yxati ish natijasi emas
            if (practice.Mode == PracticeModes.Checklist)
            {
                continue;
            }

            var response = StepContentJson.Deserialize<CompleteStepRequest>(completion.ResponseJson);
            items.Add(new MyWorkItemDto(step.Id, lesson.Slug, lesson.Title, step.Title, practice.Mode, response.Text, response.Items, completion.CompletedAt));
        }

        return items.OrderByDescending(i => i.CompletedAt).ToList();
    }

    private sealed record Evaluation(bool Passed, bool? IsCorrect = null, string? Feedback = null, string? ResponseJson = null, Error? Error = null);

    private static Evaluation Evaluate(LessonStep step, CompleteStepRequest request) => step.Kind switch
    {
        LessonStepKind.Reading or LessonStepKind.Video => new Evaluation(true),
        LessonStepKind.Exercise => EvaluateExercise(StepContentJson.Deserialize<ExerciseContent>(step.ContentJson), request),
        LessonStepKind.Quiz => EvaluateQuiz(StepContentJson.Deserialize<QuizContent>(step.ContentJson), request),
        LessonStepKind.Practice => EvaluatePractice(StepContentJson.Deserialize<PracticeContent>(step.ContentJson), request),
        _ => throw new InvalidOperationException($"Noma’lum bosqich turi: {step.Kind}"),
    };

    private static Evaluation EvaluateExercise(ExerciseContent exercise, CompleteStepRequest request)
    {
        var answer = NormalizeNumber(request.Text);
        if (answer is null)
        {
            return Invalid("Javobni son ko‘rinishida yozing.");
        }

        return answer == NormalizeNumber(exercise.Answer)
            ? new Evaluation(true, true, exercise.Explanation, StepContentJson.Serialize(new CompleteStepRequest(null, request.Text!.Trim(), null)))
            : new Evaluation(false, false, $"Hali to‘g‘ri emas. {exercise.Hint}");
    }

    /// <summary>"1 200", "1200" va "12,5" kabi yozuvlarni bir xil songa keltiradi. Son bo‘lmasa, null.</summary>
    private static decimal? NormalizeNumber(string? text)
    {
        var cleaned = text?.Replace(" ", string.Empty).Replace(",", ".");
        return decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;
    }

    private static Evaluation EvaluateQuiz(QuizContent quiz, CompleteStepRequest request)
    {
        if (quiz.Options.All(o => o.Id != request.OptionId))
        {
            return Invalid("Javoblardan birini tanlang.");
        }

        return request.OptionId == quiz.CorrectOptionId
            ? new Evaluation(true, true, quiz.Explanation, StepContentJson.Serialize(new CompleteStepRequest(request.OptionId, null, null)))
            : new Evaluation(false, false, quiz.WrongHint);
    }

    private static Evaluation EvaluatePractice(PracticeContent practice, CompleteStepRequest request)
    {
        switch (practice.Mode)
        {
            case PracticeModes.Text:
                var text = request.Text?.Trim() ?? string.Empty;
                if (text.Length is < MinTextLength or > MaxTextLength)
                {
                    return Invalid($"Javobingizni {MinTextLength}–{MaxTextLength} belgi oralig‘ida yozing.");
                }

                return new Evaluation(true, ResponseJson: StepContentJson.Serialize(new CompleteStepRequest(null, text, null)));

            case PracticeModes.Process:
                var items = (request.Items ?? []).Select(i => i.Trim()).Where(i => i.Length > 0).ToList();
                if (items.Count < practice.MinItems || items.Count > practice.MaxItems)
                {
                    return Invalid($"Sxemada {practice.MinItems} tadan {practice.MaxItems} tagacha bosqich bo‘lsin.");
                }

                if (items.Any(i => i.Length > MaxItemLength))
                {
                    return Invalid($"Har bir bosqich nomi {MaxItemLength} belgidan oshmasin: sxemada qisqa yozuv yaxshi o‘qiladi.");
                }

                return new Evaluation(true, ResponseJson: StepContentJson.Serialize(new CompleteStepRequest(null, null, items)));

            case PracticeModes.Checklist:
                var checklist = practice.Checklist ?? [];
                var ticked = request.Items ?? [];
                if (checklist.Any(c => !ticked.Contains(c)))
                {
                    return Invalid("Barcha bandlarni tekshirib belgilang. Biror band bajarilmagan bo‘lsa, orqaga qaytib sxemani tuzating.");
                }

                return new Evaluation(true, ResponseJson: StepContentJson.Serialize(new CompleteStepRequest(null, null, checklist)));

            default:
                throw new InvalidOperationException($"Noma’lum topshiriq turi: {practice.Mode}");
        }
    }

    private static Evaluation Invalid(string message) =>
        new(false, Error: new Error("invalid_answer", message, ErrorKind.Validation));

    private static StepDto ToDto(LessonStep step, UserState state)
    {
        var saved = state.Completions.TryGetValue(step.Id, out var completion) && completion.ResponseJson is not null
            ? StepContentJson.Deserialize<CompleteStepRequest>(completion.ResponseJson)
            : null;
        var isCompleted = completion is not null;

        return step.Kind switch
        {
            LessonStepKind.Reading => new StepDto(step.Id, "reading", step.Title, isCompleted,
                StepContentJson.Deserialize<ReadingContent>(step.ContentJson), null, null, null, null, saved),
            LessonStepKind.Quiz => new StepDto(step.Id, "quiz", step.Title, isCompleted, null, ToQuizDto(step), null, null, null, saved),
            LessonStepKind.Practice => new StepDto(step.Id, "practice", step.Title, isCompleted, null, null, ToPracticeDto(step), null, null, saved),
            LessonStepKind.Video => new StepDto(step.Id, "video", step.Title, isCompleted, null, null, null,
                StepContentJson.Deserialize<VideoContent>(step.ContentJson), null, saved),
            LessonStepKind.Exercise => new StepDto(step.Id, "exercise", step.Title, isCompleted, null, null, null, null, ToExerciseDto(step), saved),
            _ => throw new InvalidOperationException($"Noma’lum bosqich turi: {step.Kind}"),
        };
    }

    // To‘g‘ri javob va izoh brauzerga faqat javob yuborilgandan keyin chiqadi
    private static QuizDto ToQuizDto(LessonStep step)
    {
        var quiz = StepContentJson.Deserialize<QuizContent>(step.ContentJson);
        return new QuizDto(quiz.Question, quiz.Options.Select(o => new QuizOptionDto(o.Id, o.Text)).ToList());
    }

    private static ExerciseDto ToExerciseDto(LessonStep step)
    {
        var exercise = StepContentJson.Deserialize<ExerciseContent>(step.ContentJson);
        return new ExerciseDto(exercise.Prompt, exercise.Unit);
    }

    private static PracticeDto ToPracticeDto(LessonStep step)
    {
        var practice = StepContentJson.Deserialize<PracticeContent>(step.ContentJson);
        return new PracticeDto(practice.Mode, practice.Prompt, practice.Placeholder, practice.MinItems, practice.MaxItems, practice.Checklist);
    }

    private async Task<UserState> LoadStateAsync(Guid userId, Course course, CancellationToken ct)
    {
        var lessonIds = course.Lessons.Select(l => l.Id).ToList();
        var stepIds = course.Lessons.SelectMany(l => l.Steps).Select(s => s.Id).ToList();
        var progress = await repository.GetLessonProgressAsync(userId, lessonIds, ct);
        var completions = await repository.GetStepCompletionsAsync(userId, stepIds, ct);
        return new UserState(course, progress.ToDictionary(p => p.LessonId), completions.ToDictionary(c => c.StepId));
    }

    private sealed record UserState(Course Course, Dictionary<Guid, LessonProgress> Progress, Dictionary<Guid, StepCompletion> Completions)
    {
        private bool IsCompleted(Lesson lesson) => lesson.Steps.All(s => Completions.ContainsKey(s.Id));

        public string StatusOf(Lesson lesson)
        {
            if (IsCompleted(lesson))
            {
                return LessonStatus.Completed;
            }

            // Darslar ketma-ket ochiladi: har biri oldingisida o‘rganilganga tayanadi
            if (Course.Lessons.Any(l => l.SortOrder < lesson.SortOrder && !IsCompleted(l)))
            {
                return LessonStatus.Locked;
            }

            return Progress.ContainsKey(lesson.Id) ? LessonStatus.InProgress : LessonStatus.Available;
        }

        public Lesson? NextLesson() => Course.Lessons.FirstOrDefault(l => !IsCompleted(l));
    }
}
