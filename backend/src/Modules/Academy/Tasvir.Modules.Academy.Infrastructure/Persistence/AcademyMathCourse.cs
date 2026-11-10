using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Infrastructure.Persistence;

/// <summary>
/// "Boshlang‘ich matematika": YouTube’dagi o‘zbek tilidagi darslar va ularning ortidan mashqlar.
/// Videolar faqat embed qilinadi, yuklab olinmaydi. Video mualliflarga tegishli.
/// </summary>
internal static partial class AcademySeeder
{
    public const string MathCourseSlug = "boshlangich-matematika";

    private static async Task SeedMathCourseAsync(AcademyDbContext db, CancellationToken ct)
    {
        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(c => c.Slug == MathCourseSlug, ct);

        if (course is null)
        {
            course = new Course(
                MathCourseSlug,
                "Boşlanğiç matematika",
                "1–4-sinf mavzulari: video darsni köring, keyin misol va masalalarni özingiz işlang.");
            db.Courses.Add(course);
        }

        MathLesson(course, "ayirish-hadlari", "Ayiriş hadlari", "1-sinf: kamayuvçi, ayriluvçi va ayirma nima?",
            "RM3JmJPRlow", "1-sinf Matematika 33-dars: Ayiriş hadlari",
            [
                new("9 − 4 = ?", "5", null,"9 tadan 4 tasini olib taşlang va qolganini sanang.", "Töğri: 9 − 4 = 5."),
                new("15 − 7 = ?", "8", null, "Avval 15 dan 5 ni ayiring, 10 çiqadi. Yana 2 ni ayiring.", "Töğri: 15 − 7 = 8."),
                new("12 − 5 = 7 tengligida kamayuvçi qaysi son?", "12", null, "Kamayuvçi: birinçi turadigan, undan ayriladigan son.", "Töğri: kamayuvçi 12."),
                new("18 − 6 = 12 tengligida ayriluvçi qaysi son?", "6", null, "Ayriluvçi: kamayuvçidan olib taşlanadigan son.", "Töğri: ayriluvçi 6."),
                new("Savatda 14 ta olma bor edi. 6 tasi yeyildi. Savatda neçta olma qoldi?", "8", "ta", "Qolganini topiş uçun ayiramiz: 14 − 6.", "Töğri: 14 − 6 = 8 ta olma."),
            ]);

        MathLesson(course, "20-ichida-qoshish-ayirish", "20 içida qöşiş va ayiriş", "2-sinf: 20 gaça sonlarni qöşiş va ayirişni takrorlaymiz.",
            "0mG288VPwDY", "2-SINF MATEMATIKA 1-DARS",
            [
                new("8 + 7 = ?", "15", null, "8 ga 2 ni qöşsak 10 böladi. Yana 5 ni qöşing.", "Töğri: 8 + 7 = 15."),
                new("13 + 6 = ?", "19", null, "Birliklarni qöşing: 3 + 6. Önlik özgarmaydi.", "Töğri: 13 + 6 = 19."),
                new("17 − 9 = ?", "8", null, "17 dan 7 ni ayiring, 10 çiqadi. Yana 2 ni ayiring.", "Töğri: 17 − 9 = 8."),
                new("20 − 12 = ?", "8", null, "Avval 10 ni ayiring, keyin 2 ni.", "Töğri: 20 − 12 = 8."),
                new("Alida 9 ta, Valida 8 ta daftar bor. Ikkalasida jami neçta daftar bor?", "17", "ta", "“Jami” sözi qöşişni bildiradi: 9 + 8.", "Töğri: 9 + 8 = 17 ta daftar."),
            ]);

        MathLesson(course, "yigindi-va-ayirma", "Yiğindi va ayirmani topiş", "2-sinf: ikki xonali sonlarni qöşiş va ayiriş.",
            "SamHEJXa1sQ", "3-dars / Yiğindi va ayirmani topiş / 2-sinf matematika",
            [
                new("34 + 25 = ?", "59", null, "Birliklarni alohida (4 + 5), önliklarni alohida (30 + 20) qöşing.", "Töğri: 34 + 25 = 59."),
                new("67 − 23 = ?", "44", null, "Birliklarni (7 − 3), keyin önliklarni (60 − 20) ayiring.", "Töğri: 67 − 23 = 44."),
                new("48 + 16 = ?", "64", null, "8 + 6 = 14. 1 önlikni keyingi xonaga ötkazing: 40 + 10 + 10.", "Töğri: 48 + 16 = 64."),
                new("72 − 35 = ?", "37", null, "72 dan avval 30 ni ayiring, 42 çiqadi. Keyin 5 ni ayiring.", "Töğri: 72 − 35 = 37."),
                new("Kutubxonada 45 ta kitob bor edi. 28 tasi öquvçilarga berildi. Neçta kitob qoldi?", "17", "ta", "Qolganini topamiz: 45 − 28. Avval 20 ni, keyin 8 ni ayiring.", "Töğri: 45 − 28 = 17 ta kitob."),
            ]);

        MathLesson(course, "3-ga-kopaytirish", "3 ga köpaytiriş", "2-sinf: 3 sonini köpaytiriş va 3 ga köpaytiriş.",
            "HAeBYRA6iYc", "2-sinf matematikasi. 97-dars. 3 sonini köpaytiriş va 3 ga köpaytiriş",
            [
                new("3 × 4 = ?", "12", null, "3 ni 4 marta qöşing: 3 + 3 + 3 + 3.", "Töğri: 3 × 4 = 12."),
                new("3 × 7 = ?", "21", null, "3 × 6 = 18. Unga yana 3 ni qöşing.", "Töğri: 3 × 7 = 21."),
                new("6 × 3 = ?", "18", null, "Köpaytuvçilar örnini almaştirsa ham natija özgarmaydi: 3 × 6.", "Töğri: 6 × 3 = 18."),
                new("3 × 9 = ?", "27", null, "3 × 10 = 30. Undan 3 ni ayiring.", "Töğri: 3 × 9 = 27."),
                new("5 ta tovoqning har birida 3 tadan olma bor. Jami neçta olma bor?", "15", "ta", "Teng guruhlar: 5 × 3.", "Töğri: 5 × 3 = 15 ta olma."),
            ]);

        MathLesson(course, "masalalar-2-sinf", "Masalalar yeçiş: 2-sinf", "Masala şartini öqib, qaysi amal kerakligini aniqlaşni örganamiz.",
            "CxqeSozlzA8", "2-sinf matematikasi. 116-dars. Masalalar yeçiş",
            [
                new("Zalda 4 qator stul bor. Har bir qatorda 6 tadan stul. Zalda jami neçta stul bor?", "24", "ta", "Teng qatorlar köpaytiriş bilan topiladi: 4 × 6.", "Töğri: 4 × 6 = 24 ta stul."),
                new("24 ta qalamni 3 ta bolaga teng bölib berişdi. Har bir bolaga neçtadan qalam tegdi?", "8", "ta", "“Teng bölib” sözi bölişni bildiradi: 24 : 3.", "Töğri: 24 : 3 = 8 ta qalam."),
                new("Dökonda 30 kg olma bor edi. 12 kg sotildi, keyin yana 5 kg keltirildi. Dökonda neça kg olma böldi?", "23", "kg", "Ikki amal: avval 30 − 12, keyin natijaga 5 ni qöşing.", "Töğri: 30 − 12 + 5 = 23 kg."),
                new("Kitob 40 betdan iborat. Bola birinçi kuni 15 bet, ikkinçi kuni 10 bet öqidi. Neça bet qoldi?", "15", "bet", "Avval öqilganini toping: 15 + 10. Keyin 40 dan ayiring.", "Töğri: 40 − 25 = 15 bet."),
                new("Bir daftar 5 söm turadi. 7 ta daftar neça söm turadi?", "35", "söm", "Bitta narxni soniga köpaytiring: 5 × 7.", "Töğri: 5 × 7 = 35 söm."),
            ]);

        MathLesson(course, "uch-xonali-kopaytirish", "Uç xonali sonni bir xonali songa köpaytiriş", "3-sinf: xonalar böyiça köpaytiriş.",
            "R_11dapZziE", "3-SINF MATEMATIKA 27-DARS",
            [
                new("123 × 2 = ?", "246", null, "Har bir xonani 2 ga köpaytiring: 3 × 2, 20 × 2, 100 × 2.", "Töğri: 123 × 2 = 246."),
                new("214 × 3 = ?", "642", null, "4 × 3 = 12, 10 × 3 = 30, 200 × 3 = 600. Natijalarni qöşing.", "Töğri: 214 × 3 = 642."),
                new("125 × 4 = ?", "500", null, "5 × 4 = 20, 20 × 4 = 80, 100 × 4 = 400. Natijalarni qöşing.", "Töğri: 125 × 4 = 500."),
                new("308 × 3 = ?", "924", null, "8 × 3 = 24, 300 × 3 = 900. Önliklar xonasida 0 borligiga e’tibor bering.", "Töğri: 308 × 3 = 924."),
                new("Har bir qutida 215 ta qalam bor. 4 ta qutida neçta qalam bor?", "860", "ta", "Teng guruhlar: 215 × 4. Avval 200 × 4, keyin 15 × 4.", "Töğri: 215 × 4 = 860 ta qalam."),
            ]);

        MathLesson(course, "masalalar-4-sinf", "Masalalar yeçiş: 4-sinf", "Tezlik, böliş va perimetrga oid masalalarni işlaymiz.",
            "A0ysuYexSSI", "4-sinf matematika 45-dars. Masalalar yeçiş",
            [
                new("Poyezd soatiga 80 km tezlik bilan 4 soat yurdi. U qança masofani bosib ötdi?", "320", "km", "Masofa = tezlik × vaqt.", "Töğri: 80 × 4 = 320 km."),
                new("Boğda 240 tup olma daraxti bor. Nok daraxtlari undan 3 marta kam. Boğda neça tup nok daraxti bor?", "80", "tup", "“Marta kam” bölişni bildiradi: 240 : 3.", "Töğri: 240 : 3 = 80 tup."),
                new("Zavod birinçi kuni 350 ta, ikkinçi kuni 420 ta detal işlab çiqardi. Ikki kunda jami neçta detal işlab çiqarildi?", "770", "ta", "“Jami” qöşişni bildiradi: 350 + 420.", "Töğri: 350 + 420 = 770 ta detal."),
                new("960 kg unni 8 ta qopga teng bölib solişdi. Har bir qopda neça kg un bor?", "120", "kg", "Teng böliş: 960 : 8. Avval 800 : 8, keyin 160 : 8.", "Töğri: 960 : 8 = 120 kg."),
                new("Töğri törtburçakning böyi 12 sm, eni 5 sm. Uning perimetri neça sm?", "34", "sm", "Perimetr = (böyi + eni) × 2.", "Töğri: (12 + 5) × 2 = 34 sm."),
            ]);

        await db.SaveChangesAsync(ct);
    }

    private sealed record MathProblem(string Prompt, string Answer, string? Unit, string Hint, string Explanation);

    private static void MathLesson(Course course, string slug, string title, string summary, string youtubeId, string videoTitle, MathProblem[] problems)
    {
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(slug, title, summary, 20);

        lesson.AddStep(LessonStepKind.Video, "Video darsni köring", StepContentJson.Serialize(new VideoContent(
            youtubeId,
            videoTitle,
            "Avval darsni oxirigaça köring. Kerak bölsa, töxtatib, qaytadan tinglang. Keyin misol va masalalarni özingiz işlaysiz.")));

        for (var i = 0; i < problems.Length; i++)
        {
            var problem = problems[i];
            lesson.AddStep(LessonStepKind.Exercise, $"{i + 1}-maşq", StepContentJson.Serialize(new ExerciseContent(
                problem.Prompt, problem.Answer, problem.Unit, problem.Hint, problem.Explanation)));
        }
    }
}
