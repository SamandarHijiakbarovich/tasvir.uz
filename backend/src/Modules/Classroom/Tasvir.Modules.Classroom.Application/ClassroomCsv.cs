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
        Row(csv, new[] { "F.I.Sh." }
            .Concat(courses.Select(c => $"{c}: tugatilgan darslar"))
            .Concat(["Testlar: birinchi urinishda to‘g‘ri, %", "Topshirilgan ishlar", "Baholangan ishlar"])
            .Concat(rubric.Select(c => $"{c.Title} (o‘rtacha, 0–{ClassroomCatalog.MaxCriterionScore})"))
            .Concat([$"Jami ball (o‘rtacha, 0–{ClassroomCatalog.MaxTotalScore})"]));

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
        Row(csv, ["Kurs", "Dars", "Savol", "Javob berganlar", "Qiyinlik indeksi (0–1)", "Farqlash indeksi", "O‘rtacha urinishlar", "Javoblar (birinchi urinish)"]);
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
        Row(csv, ["Savol", "O‘rtacha (1–5)", "1", "2", "3", "4", "5"]);
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

    private static string Number(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');

    private static void Row(StringBuilder csv, IEnumerable<string> cells) =>
        csv.Append(string.Join(Separator, cells.Select(Escape))).Append("\r\n");

    private static string Escape(string cell)
    {
        // Ism yoki izoh Excel’da formula bo‘lib ishlab ketmasin
        if (cell.Length > 0 && cell[0] is '=' or '+' or '-' or '@' or '\t' or '\r')
        {
            cell = "'" + cell;
        }

        return cell.IndexOfAny([Separator, '"', '\n', '\r']) >= 0
            ? "\"" + cell.Replace("\"", "\"\"") + "\""
            : cell;
    }
}
