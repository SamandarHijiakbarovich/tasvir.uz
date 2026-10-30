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
                "Boshlang‘ich matematika",
                "1–4-sinf mavzulari: video darsni ko‘ring, keyin misol va masalalarni o‘zingiz ishlang.");
            db.Courses.Add(course);
        }

        MathLesson(course, "ayirish-hadlari", "Ayirish hadlari", "1-sinf: kamayuvchi, ayriluvchi va ayirma nima?",
            "RM3JmJPRlow", "1-sinf Matematika 33-dars: Ayirish hadlari",
            [
                new("9 − 4 = ?", "5", null,"9 tadan 4 tasini olib tashlang va qolganini sanang.", "To‘g‘ri: 9 − 4 = 5."),
                new("15 − 7 = ?", "8", null, "Avval 15 dan 5 ni ayiring, 10 chiqadi. Yana 2 ni ayiring.", "To‘g‘ri: 15 − 7 = 8."),
                new("12 − 5 = 7 tengligida kamayuvchi qaysi son?", "12", null, "Kamayuvchi: birinchi turadigan, undan ayriladigan son.", "To‘g‘ri: kamayuvchi 12."),
                new("18 − 6 = 12 tengligida ayriluvchi qaysi son?", "6", null, "Ayriluvchi: kamayuvchidan olib tashlanadigan son.", "To‘g‘ri: ayriluvchi 6."),
                new("Savatda 14 ta olma bor edi. 6 tasi yeyildi. Savatda nechta olma qoldi?", "8", "ta", "Qolganini topish uchun ayiramiz: 14 − 6.", "To‘g‘ri: 14 − 6 = 8 ta olma."),
            ]);

        MathLesson(course, "20-ichida-qoshish-ayirish", "20 ichida qo‘shish va ayirish", "2-sinf: 20 gacha sonlarni qo‘shish va ayirishni takrorlaymiz.",
            "0mG288VPwDY", "2-SINF MATEMATIKA 1-DARS",
            [
                new("8 + 7 = ?", "15", null, "8 ga 2 ni qo‘shsak 10 bo‘ladi. Yana 5 ni qo‘shing.", "To‘g‘ri: 8 + 7 = 15."),
                new("13 + 6 = ?", "19", null, "Birliklarni qo‘shing: 3 + 6. O‘nlik o‘zgarmaydi.", "To‘g‘ri: 13 + 6 = 19."),
                new("17 − 9 = ?", "8", null, "17 dan 7 ni ayiring, 10 chiqadi. Yana 2 ni ayiring.", "To‘g‘ri: 17 − 9 = 8."),
                new("20 − 12 = ?", "8", null, "Avval 10 ni ayiring, keyin 2 ni.", "To‘g‘ri: 20 − 12 = 8."),
                new("Alida 9 ta, Valida 8 ta daftar bor. Ikkalasida jami nechta daftar bor?", "17", "ta", "“Jami” so‘zi qo‘shishni bildiradi: 9 + 8.", "To‘g‘ri: 9 + 8 = 17 ta daftar."),
            ]);

        MathLesson(course, "yigindi-va-ayirma", "Yig‘indi va ayirmani topish", "2-sinf: ikki xonali sonlarni qo‘shish va ayirish.",
            "SamHEJXa1sQ", "3-dars / Yig‘indi va ayirmani topish / 2-sinf matematika",
            [
                new("34 + 25 = ?", "59", null, "Birliklarni alohida (4 + 5), o‘nliklarni alohida (30 + 20) qo‘shing.", "To‘g‘ri: 34 + 25 = 59."),
                new("67 − 23 = ?", "44", null, "Birliklarni (7 − 3), keyin o‘nliklarni (60 − 20) ayiring.", "To‘g‘ri: 67 − 23 = 44."),
                new("48 + 16 = ?", "64", null, "8 + 6 = 14. 1 o‘nlikni keyingi xonaga o‘tkazing: 40 + 10 + 10.", "To‘g‘ri: 48 + 16 = 64."),
                new("72 − 35 = ?", "37", null, "72 dan avval 30 ni ayiring, 42 chiqadi. Keyin 5 ni ayiring.", "To‘g‘ri: 72 − 35 = 37."),
                new("Kutubxonada 45 ta kitob bor edi. 28 tasi o‘quvchilarga berildi. Nechta kitob qoldi?", "17", "ta", "Qolganini topamiz: 45 − 28. Avval 20 ni, keyin 8 ni ayiring.", "To‘g‘ri: 45 − 28 = 17 ta kitob."),
            ]);

        MathLesson(course, "3-ga-kopaytirish", "3 ga ko‘paytirish", "2-sinf: 3 sonini ko‘paytirish va 3 ga ko‘paytirish.",
            "HAeBYRA6iYc", "2-sinf matematikasi. 97-dars. 3 sonini ko‘paytirish va 3 ga ko‘paytirish",
            [
                new("3 × 4 = ?", "12", null, "3 ni 4 marta qo‘shing: 3 + 3 + 3 + 3.", "To‘g‘ri: 3 × 4 = 12."),
                new("3 × 7 = ?", "21", null, "3 × 6 = 18. Unga yana 3 ni qo‘shing.", "To‘g‘ri: 3 × 7 = 21."),
                new("6 × 3 = ?", "18", null, "Ko‘paytuvchilar o‘rnini almashtirsa ham natija o‘zgarmaydi: 3 × 6.", "To‘g‘ri: 6 × 3 = 18."),
                new("3 × 9 = ?", "27", null, "3 × 10 = 30. Undan 3 ni ayiring.", "To‘g‘ri: 3 × 9 = 27."),
                new("5 ta tovoqning har birida 3 tadan olma bor. Jami nechta olma bor?", "15", "ta", "Teng guruhlar: 5 × 3.", "To‘g‘ri: 5 × 3 = 15 ta olma."),
            ]);

        MathLesson(course, "masalalar-2-sinf", "Masalalar yechish: 2-sinf", "Masala shartini o‘qib, qaysi amal kerakligini aniqlashni o‘rganamiz.",
            "CxqeSozlzA8", "2-sinf matematikasi. 116-dars. Masalalar yechish",
            [
                new("Zalda 4 qator stul bor. Har bir qatorda 6 tadan stul. Zalda jami nechta stul bor?", "24", "ta", "Teng qatorlar ko‘paytirish bilan topiladi: 4 × 6.", "To‘g‘ri: 4 × 6 = 24 ta stul."),
                new("24 ta qalamni 3 ta bolaga teng bo‘lib berishdi. Har bir bolaga nechtadan qalam tegdi?", "8", "ta", "“Teng bo‘lib” so‘zi bo‘lishni bildiradi: 24 : 3.", "To‘g‘ri: 24 : 3 = 8 ta qalam."),
                new("Do‘konda 30 kg olma bor edi. 12 kg sotildi, keyin yana 5 kg keltirildi. Do‘konda necha kg olma bo‘ldi?", "23", "kg", "Ikki amal: avval 30 − 12, keyin natijaga 5 ni qo‘shing.", "To‘g‘ri: 30 − 12 + 5 = 23 kg."),
                new("Kitob 40 betdan iborat. Bola birinchi kuni 15 bet, ikkinchi kuni 10 bet o‘qidi. Necha bet qoldi?", "15", "bet", "Avval o‘qilganini toping: 15 + 10. Keyin 40 dan ayiring.", "To‘g‘ri: 40 − 25 = 15 bet."),
                new("Bir daftar 5 so‘m turadi. 7 ta daftar necha so‘m turadi?", "35", "so‘m", "Bitta narxni soniga ko‘paytiring: 5 × 7.", "To‘g‘ri: 5 × 7 = 35 so‘m."),
            ]);

        MathLesson(course, "uch-xonali-kopaytirish", "Uch xonali sonni bir xonali songa ko‘paytirish", "3-sinf: xonalar bo‘yicha ko‘paytirish.",
            "R_11dapZziE", "3-SINF MATEMATIKA 27-DARS",
            [
                new("123 × 2 = ?", "246", null, "Har bir xonani 2 ga ko‘paytiring: 3 × 2, 20 × 2, 100 × 2.", "To‘g‘ri: 123 × 2 = 246."),
                new("214 × 3 = ?", "642", null, "4 × 3 = 12, 10 × 3 = 30, 200 × 3 = 600. Natijalarni qo‘shing.", "To‘g‘ri: 214 × 3 = 642."),
                new("125 × 4 = ?", "500", null, "5 × 4 = 20, 20 × 4 = 80, 100 × 4 = 400. Natijalarni qo‘shing.", "To‘g‘ri: 125 × 4 = 500."),
                new("308 × 3 = ?", "924", null, "8 × 3 = 24, 300 × 3 = 900. O‘nliklar xonasida 0 borligiga e’tibor bering.", "To‘g‘ri: 308 × 3 = 924."),
                new("Har bir qutida 215 ta qalam bor. 4 ta qutida nechta qalam bor?", "860", "ta", "Teng guruhlar: 215 × 4. Avval 200 × 4, keyin 15 × 4.", "To‘g‘ri: 215 × 4 = 860 ta qalam."),
            ]);

        MathLesson(course, "masalalar-4-sinf", "Masalalar yechish: 4-sinf", "Tezlik, bo‘lish va perimetrga oid masalalarni ishlaymiz.",
            "A0ysuYexSSI", "4-sinf matematika 45-dars. Masalalar yechish",
            [
                new("Poyezd soatiga 80 km tezlik bilan 4 soat yurdi. U qancha masofani bosib o‘tdi?", "320", "km", "Masofa = tezlik × vaqt.", "To‘g‘ri: 80 × 4 = 320 km."),
                new("Bog‘da 240 tup olma daraxti bor. Nok daraxtlari undan 3 marta kam. Bog‘da necha tup nok daraxti bor?", "80", "tup", "“Marta kam” bo‘lishni bildiradi: 240 : 3.", "To‘g‘ri: 240 : 3 = 80 tup."),
                new("Zavod birinchi kuni 350 ta, ikkinchi kuni 420 ta detal ishlab chiqardi. Ikki kunda jami nechta detal ishlab chiqarildi?", "770", "ta", "“Jami” qo‘shishni bildiradi: 350 + 420.", "To‘g‘ri: 350 + 420 = 770 ta detal."),
                new("960 kg unni 8 ta qopga teng bo‘lib solishdi. Har bir qopda necha kg un bor?", "120", "kg", "Teng bo‘lish: 960 : 8. Avval 800 : 8, keyin 160 : 8.", "To‘g‘ri: 960 : 8 = 120 kg."),
                new("To‘g‘ri to‘rtburchakning bo‘yi 12 sm, eni 5 sm. Uning perimetri necha sm?", "34", "sm", "Perimetr = (bo‘yi + eni) × 2.", "To‘g‘ri: (12 + 5) × 2 = 34 sm."),
            ]);

        await db.SaveChangesAsync(ct);
    }

    private sealed record MathProblem(string Prompt, string Answer, string? Unit, string Hint, string Explanation);

    private static void MathLesson(Course course, string slug, string title, string summary, string youtubeId, string videoTitle, MathProblem[] problems)
    {
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(slug, title, summary, 20);

        lesson.AddStep(LessonStepKind.Video, "Video darsni ko‘ring", StepContentJson.Serialize(new VideoContent(
            youtubeId,
            videoTitle,
            "Avval darsni oxirigacha ko‘ring. Kerak bo‘lsa, to‘xtatib, qaytadan tinglang. Keyin misol va masalalarni o‘zingiz ishlaysiz.")));

        for (var i = 0; i < problems.Length; i++)
        {
            var problem = problems[i];
            lesson.AddStep(LessonStepKind.Exercise, $"{i + 1}-mashq", StepContentJson.Serialize(new ExerciseContent(
                problem.Prompt, problem.Answer, problem.Unit, problem.Hint, problem.Explanation)));
        }
    }
}
