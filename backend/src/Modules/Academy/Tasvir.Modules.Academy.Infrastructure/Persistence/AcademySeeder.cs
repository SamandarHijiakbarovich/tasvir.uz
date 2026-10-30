using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Infrastructure.Persistence;

/// <summary>Birinchi kurs mazmuni. Nazariy asosi: dissertatsiyaning I bobi (ikki kanalli kodlash, kognitiv yuklama, vizual shakllar).</summary>
internal static class AcademySeeder
{
    public const string FirstCourseSlug = "vizuallashtirish-asoslari";

    public static async Task SeedAsync(AcademyDbContext db, CancellationToken ct)
    {
        if (await db.Courses.AnyAsync(ct))
        {
            return;
        }

        var course = new Course(
            FirstCourseSlug,
            "Vizuallashtirish asoslari",
            "O‘quv materialini tushunarli sxemaga aylantirishni qisqa, amaliy darslarda o‘rganing.");

        AddWhyVisualsWork(course);
        AddChoosingTheForm(course);
        AddFirstDiagram(course);

        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
    }

    private static void Reading(Lesson lesson, string title, ReadingContent content) =>
        lesson.AddStep(LessonStepKind.Reading, title, StepContentJson.Serialize(content));

    private static void Quiz(Lesson lesson, string title, QuizContent content) =>
        lesson.AddStep(LessonStepKind.Quiz, title, StepContentJson.Serialize(content));

    private static void Practice(Lesson lesson, string title, PracticeContent content) =>
        lesson.AddStep(LessonStepKind.Practice, title, StepContentJson.Serialize(content));

    private static void AddWhyVisualsWork(Course course)
    {
        var lesson = course.AddLesson(
            "nega-korgazmali-material-ishlaydi",
            "Nega ko‘rgazmali material yaxshiroq ishlaydi?",
            "Bir xil mazmun matnda va sxemada nega turlicha qabul qilinishini bilib oling.",
            10);

        Reading(lesson, "Bitta mazmun, ikki xil ko‘rinish", new ReadingContent(
            [
                "O‘quvchi matnni o‘qiganda jarayon bosqichlarini va ular orasidagi bog‘liqlikni o‘z tasavvurida yig‘ib olishi kerak. Sxemada esa bu bog‘liqlik tayyor holda ko‘rinib turadi.",
                "Quyidagi misolda mazmun bir xil. Farqi faqat shaklda: qaysi birida jarayon ketma-ketligi tezroq ko‘zga tashlanishiga e’tibor bering.",
            ],
            Example: new ReadingExample(
                "Matn",
                "Quyosh issiqligi ta’sirida suv bug‘lanadi. Havoga ko‘tarilgan bug‘ soviydi va bulutlarni hosil qiladi. Bulutlardan yomg‘ir yoki qor yog‘adi. So‘ng suv daryolar orqali yana dengizga oqib boradi.",
                "Sxema",
                ["Bug‘lanish", "Kondensatsiya", "Yog‘in", "Oqim"])));

        Reading(lesson, "Miyamiz axborotni qanday qabul qiladi?", new ReadingContent(
            [
                "**Ikki kanalli kodlash.** A.Paivio nazariyasiga ko‘ra, inson so‘z va tasvirni ikki alohida, lekin bog‘liq tizimda qayta ishlaydi. Axborot ikkala tizimda ham qayd etilsa, uni eslab qolish osonlashadi.",
                "**Kognitiv yuklama.** J.Sveller nazariyasiga ko‘ra, ishchi xotira sig‘imi cheklangan. Material noto‘g‘ri tuzilsa, xotira mazmunni tushunishga emas, materialning o‘zini “yechishga” sarflanadi.",
                "Yaxshi sxema ortiqcha yuklamani kamaytiradi: o‘quvchi kuchini bog‘liqliklarni qidirishga emas, mazmunni tushunishga sarflaydi.",
            ],
            Callout: "Muhim: chiroyli rasm qo‘shish vizuallashtirish emas. Mazmunga aloqasi yo‘q bezak o‘quvchini chalg‘itadi va foydadan ko‘ra zarar keltirishi mumkin."));

        Quiz(lesson, "Tekshirib ko‘ramiz", new QuizContent(
            "Ikki kanalli kodlash nazariyasi nima deydi?",
            [
                new QuizOption("a", "Tasvir har doim matndan yaxshiroq, shuning uchun matndan voz kechish kerak"),
                new QuizOption("b", "So‘z va tasvir birga to‘g‘ri berilsa, axborotni eslab qolish osonlashadi"),
                new QuizOption("c", "Har bir o‘quvchi faqat bitta kanal orqali o‘rganadi"),
            ],
            "b",
            "To‘g‘ri. Nazariya matndan voz kechishni emas, so‘z va tasvirni uyg‘unlashtirishni tavsiya qiladi.",
            "Unchalik emas. Nazariya ikki kanalning birgalikda ishlashi haqida. Yana bir bor o‘ylab ko‘ring."));

        Quiz(lesson, "Yana bir savol", new QuizContent(
            "Slaydga mavzuga aloqasi yo‘q chiroyli rasm qo‘shilsa, nima bo‘lishi mumkin?",
            [
                new QuizOption("a", "O‘quvchi diqqati chalg‘iydi va ortiqcha kognitiv yuklama paydo bo‘ladi"),
                new QuizOption("b", "Mavzu albatta yaxshiroq o‘zlashtiriladi"),
                new QuizOption("c", "Ishchi xotira sig‘imi kengayadi"),
            ],
            "a",
            "To‘g‘ri. Har bir vizual element aniq vazifani bajarishi kerak, aks holda u faqat yuklama qo‘shadi.",
            "Bu javob to‘g‘ri emas. Kognitiv yuklama haqidagi qismni eslang: xotira sig‘imi cheklangan."));

        Practice(lesson, "Amaliy topshiriq", new PracticeContent(
            PracticeModes.Text,
            "O‘z faningizdan o‘quvchilar eng qiynaladigan bitta mavzuni yozing. Nima uchun aynan shu mavzu qiyin ekanini qisqacha tushuntiring. Keyingi darslarda shu mavzuga qaytamiz.",
            "Masalan: “Fotosintez bosqichlari. O‘quvchilar qaysi modda qayerda hosil bo‘lishini adashtiradi.”"));
    }

    private static void AddChoosingTheForm(Course course)
    {
        var lesson = course.AddLesson(
            "mazmunga-mos-shaklni-tanlash",
            "Mazmunga mos shaklni tanlash",
            "Mavzudagi bog‘liqlik turini aniqlab, unga mos vizual shaklni tanlashni o‘rganing.",
            10);

        Reading(lesson, "Avval bog‘liqlikni aniqlang", new ReadingContent(
            [
                "Shaklni tanlashdan oldin o‘zingizga bitta savol bering: bu mavzuda tushunchalar bir-biri bilan qanday bog‘langan?",
                "**Ketma-ketlik** (jarayon, algoritm): blok-sxema. **Xronologiya** (tarixiy voqealar): vaqt chizig‘i.",
                "**Ierarxiya** (tasnif, tuzilma): daraxtsimon sxema yoki aqliy xarita. **Taqqoslash**: jadval yoki Venn diagrammasi.",
                "**Sabab-oqibat**: strelkali sxema. **Miqdoriy bog‘liqlik** (raqamlar): grafik yoki diagramma.",
            ],
            Callout: "Eng ko‘p uchraydigan xato: mazmun tuzilishini tahlil qilmasdan, chiroyli ko‘ringan shablonni tanlash."));

        Quiz(lesson, "Qaysi shakl mos?", new QuizContent(
            "“Buyuk geografik kashfiyotlar” mavzusida voqealar qaysi tartibda bo‘lganini ko‘rsatmoqchisiz. Qaysi shakl eng mos?",
            [
                new QuizOption("a", "Aqliy xarita"),
                new QuizOption("b", "Vaqt chizig‘i"),
                new QuizOption("c", "Doiraviy diagramma"),
            ],
            "b",
            "To‘g‘ri. Voqealarning xronologik ketma-ketligi vaqt chizig‘ida eng aniq ko‘rinadi.",
            "Bu shakl boshqa bog‘liqlik uchun qulayroq. Bu yerda asosiy narsa nima: voqealar tartibi."));

        Quiz(lesson, "Yana bir vaziyat", new QuizContent(
            "“So‘z turkumlari” mavzusida mustaqil va yordamchi so‘zlar tasnifini ko‘rsatmoqchisiz. Qaysi shakl mos?",
            [
                new QuizOption("a", "Vaqt chizig‘i"),
                new QuizOption("b", "Chiziqli grafik"),
                new QuizOption("c", "Daraxtsimon sxema"),
            ],
            "c",
            "To‘g‘ri. Tasnif ierarxik bog‘liqlikdir, uni daraxtsimon sxema yaxshi ko‘rsatadi.",
            "Bu mavzuda vaqt yoki raqam emas, “turlarga bo‘linish” muhim. Bu qaysi bog‘liqlik turiga kiradi?"));

        Practice(lesson, "Amaliy topshiriq", new PracticeContent(
            PracticeModes.Text,
            "Birinchi darsda tanlagan mavzuingizga qayting. Undagi asosiy bog‘liqlik turini va unga mos shaklni yozing. Nima uchun aynan shu shaklni tanlaganingizni bir-ikki gap bilan asoslang.",
            "Masalan: “Bog‘liqlik turi: ketma-ketlik. Shakl: blok-sxema, chunki fotosintez bosqichlari birin-ketin keladi.”"));
    }

    private static void AddFirstDiagram(Course course)
    {
        var lesson = course.AddLesson(
            "birinchi-sxemangiz",
            "Birinchi sxemangiz",
            "O‘z mavzuingiz bo‘yicha jarayon sxemasini tuzing va uni mezonlar bo‘yicha tekshiring.",
            15);

        Reading(lesson, "Yaxshi sxemaning uchta qoidasi", new ReadingContent(
            [
                "**Kam element.** Bir sxemada faqat shu dars uchun kerakli bosqichlarni qoldiring. Elementlar ko‘paysa, ularni guruhlarga ajrating.",
                "**Qisqa yozuv.** Har bir elementda to‘liq gap emas, ikki-uch so‘z bo‘lsin. Tushuntirishni o‘zingiz og‘zaki aytasiz.",
                "**Bitta urg‘u.** O‘quvchi birinchi nimaga qarashi kerakligini rang yoki o‘lcham bilan ajrating. Hamma narsa ajratilsa, hech narsa ajralib turmaydi.",
            ]));

        Practice(lesson, "Sxemangizni tuzing", new PracticeContent(
            PracticeModes.Process,
            "Tanlagan mavzuingizdagi jarayonni bosqichlarga ajrating. Har bir bosqichga qisqa nom bering. Sxema yozganingiz sari chizilib boradi.",
            "Bosqich nomi",
            MinItems: 3,
            MaxItems: 6));

        Practice(lesson, "O‘zingizni tekshiring", new PracticeContent(
            PracticeModes.Checklist,
            "Sxemangizga qarang va har bir bandni halol tekshiring. Biror band bajarilmagan bo‘lsa, orqaga qaytib tuzating.",
            Checklist:
            [
                "Bosqichlar to‘g‘ri tartibda va ilmiy jihatdan to‘g‘ri",
                "Har bir yozuv qisqa: ikki-uch so‘zdan oshmaydi",
                "Ortiqcha element yo‘q: har biri aniq vazifani bajaradi",
                "Bu sxema mavzuni matndan ko‘ra tushunarliroq ko‘rsatadi",
            ]));
    }
}
