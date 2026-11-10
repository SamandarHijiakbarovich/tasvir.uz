using System.Globalization;
using System.Text;

namespace Tasvir.Modules.Classroom.Application;

/// <summary>
/// Excel’da ochiladigan CSV: ajratgich ";", kasr vergul bilan (o‘zbek va rus Excel sozlamalari shunday kutadi).
/// Faylning UTF-8 belgisi (BOM) javob qaytarilayotganda qo‘shiladi.
/// </summary>
public static class ClassroomCsv
{
    private const char Separator = ';';

    public static string Results(IReadOnlyList<StudentRowDto> students, IReadOnlyList<SubmissionSummaryDto> submissions)
    {
        var courses = students.FirstOrDefault()?.Courses.Select(c => c.Title).ToList() ?? [];
        var rubric = ClassroomCatalog.Rubric;

        var csv = new StringBuilder();
        Row(csv, new[] { "F.I.Ş." }
            .Concat(courses.Select(c => $"{c}: tugatilgan darslar"))
            .Concat(["Testlar: birinçi urinişda töğri, %", "Topşirilgan işlar", "Baholangan işlar"])
            .Concat(rubric.Select(c => $"{c.Title} (örtaça, 0–{ClassroomCatalog.MaxCriterionScore})"))
            .Concat([$"Jami ball (örtaça, 0–{ClassroomCatalog.MaxTotalScore})"]));

        foreach (var student in students)
        {
            var reviews = submissions.Where(s => s.UserId == student.UserId && s.Review is not null).Select(s => s.Review!).ToList();
            Row(csv, new[] { student.Name }
                .Concat(student.Courses.Select(c => c.CompletedLessons.ToString(CultureInfo.InvariantCulture)))
                .Concat([
                    student.FirstTryTotal == 0 ? string.Empty : Number(100.0 * student.FirstTryCorrect / student.FirstTryTotal),
                    student.SubmissionCount.ToString(CultureInfo.InvariantCulture),
                    student.ReviewedCount.ToString(CultureInfo.InvariantCulture),
                ])
                .Concat(rubric.Select(c => reviews.Count == 0 ? string.Empty : Number(reviews.Average(r => r.Scores[c.Key]))))
                .Concat([reviews.Count == 0 ? string.Empty : Number(reviews.Average(r => r.TotalScore))]));
        }

        return csv.ToString();
    }

    public static string Items(IReadOnlyList<ItemAnalysis> items)
    {
        var csv = new StringBuilder();
        Row(csv, ["Kurs", "Dars", "Savol", "Javob berganlar", "Qiyinlik indeksi (0–1)", "Farqlaş indeksi", "Örtaça urinişlar", "Javoblar (birinçi uriniş)"]);
        foreach (var item in items)
        {
            Row(csv, [
                item.CourseTitle,
                item.LessonTitle,
                item.Question,
                item.Respondents.ToString(CultureInfo.InvariantCulture),
                Number(item.DifficultyIndex),
                item.DiscriminationIndex is { } d ? Number(d) : string.Empty,
                Number(item.AverageAttempts),
                string.Join(" | ", item.Options.Select(o => $"{(o.IsCorrect ? "✓ " : string.Empty)}{o.Text}: {o.FirstTryCount}")),
            ]);
        }

        return csv.ToString();
    }

    public static string Survey(SurveySummaryDto survey)
    {
        var csv = new StringBuilder();
        Row(csv, ["Savol", "Örtaça (1–5)", "1", "2", "3", "4", "5"]);
        foreach (var question in survey.Questions)
        {
            Row(csv, new[] { question.Text, question.Average is { } a ? Number(a) : string.Empty }
                .Concat(question.Counts.Select(c => c.ToString(CultureInfo.InvariantCulture))));
        }

        Row(csv, ["Javob berganlar", survey.Respondents.ToString(CultureInfo.InvariantCulture)]);
        foreach (var comment in survey.Comments)
        {
            Row(csv, ["Izoh", comment]);
        }

        return csv.ToString();
    }

    public static string Diagnostics(DiagnosticsDto diagnostics, IReadOnlyList<Domain.DiagnosticResult> results)
    {
        var questions = DiagnosticCatalog.Questions;
        var csv = new StringBuilder();
        Row(csv, new[] { "F.I.Ş.", $"Boşlanğiç (0–{diagnostics.MaxScore})", $"Yakuniy (0–{diagnostics.MaxScore})", "Ösiş" }
            .Concat(questions.Select((_, i) => $"B{i + 1}"))
            .Concat(questions.Select((_, i) => $"Y{i + 1}")));

        // Savol ustunlari: 1 — to‘g‘ri, 0 — noto‘g‘ri, bo‘sh — test topshirilmagan
        string[] Marks(Guid userId, string stage)
        {
            var result = results.FirstOrDefault(r => r.UserId == userId && r.Stage == stage);
            var chosen = result is null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(result.AnswersJson, ClassroomJson.Options);
            return questions.Select(q => chosen is null ? string.Empty : chosen.GetValueOrDefault(q.Key) == q.CorrectOptionId ? "1" : "0").ToArray();
        }

        foreach (var student in diagnostics.Students)
        {
            Row(csv, new[]
                {
                    student.Name,
                    student.Pre?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    student.Post?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    student.Pre is { } pre && student.Post is { } post ? (post - pre).ToString(CultureInfo.InvariantCulture) : string.Empty,
                }
                .Concat(Marks(student.UserId, Domain.DiagnosticStage.Pre))
                .Concat(Marks(student.UserId, Domain.DiagnosticStage.Post)));
        }

        if (diagnostics.Paired is { } p)
        {
            Row(csv, []);
            Row(csv, ["Juftlangan Styudent mezoni (ikkala testni topşirganlar)"]);
            Row(csv, ["Talabalar soni", p.N.ToString(CultureInfo.InvariantCulture)]);
            Row(csv, ["Boşlanğiç örtaça", Number(p.MeanPre)]);
            Row(csv, ["Yakuniy örtaça", Number(p.MeanPost)]);
            Row(csv, ["Örtaça ösiş", Number(p.MeanGain)]);
            Row(csv, ["t", p.T is { } t ? Number(t) : string.Empty]);
            Row(csv, ["Erkinlik darajasi", p.Df.ToString(CultureInfo.InvariantCulture)]);
            Row(csv, ["p (ikki tomonlama)", p.P is { } pv ? pv.ToString("0.####", CultureInfo.InvariantCulture).Replace('.', ',') : string.Empty]);
            Row(csv, ["Effekt kattaligi (Koen dz)", p.EffectSize is { } d ? Number(d) : string.Empty]);
        }

        return csv.ToString();
    }

    private static string Number(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    private static void Row(StringBuilder csv, IEnumerable<string> cells) =>
        csv.Append(string.Join(Separator, cells.Select(Escape))).Append("\r\n");

    private static string Escape(string cell)
    {
        // Ism yoki izoh Excel’da formula bo‘lib ishlab ketmasin
        // Manfiy son ("-3") formula emas: uni o‘zgartirmaymiz, aks holda Excel uni matn deb o‘qiydi
        var isNumber = double.TryParse(cell.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        if (cell.Length > 0 && !isNumber && cell[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            cell = "'" + cell;
        }

        return cell.IndexOfAny([Separator, '"', '\n', '\r']) >= 0
            ? "\"" + cell.Replace("\"", "\"\"") + "\""
            : cell;
    }
}
