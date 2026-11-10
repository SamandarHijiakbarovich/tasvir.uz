using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Infrastructure.Persistence;

/// <summary>Birinchi kurs mazmuni. Nazariy asosi: dissertatsiyaning I bobi (ikki kanalli kodlash, kognitiv yuklama, vizual shakllar).</summary>
internal static partial class AcademySeeder
{
    public const string FirstCourseSlug = "vizuallashtirish-asoslari";

    public static async Task SeedAsync(AcademyDbContext db, CancellationToken ct)
    {
        var course = await db.Courses
            .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
            .FirstOrDefaultAsync(c => c.Slug == FirstCourseSlug, ct);

        if (course is null)
        {
            course = new Course(
                FirstCourseSlug,
                "Vizuallaştiriş asoslari",
                "Öquv materialini tuşunarli sxemaga aylantirişni qisqa, amaliy darslarda örganing.");
            db.Courses.Add(course);
        }

        // Faqat yetishmayotgan darslar oxiriga qo‘shiladi: mavjud darslar va foydalanuvchilar progressi o‘zgarmaydi
        AddWhyVisualsWork(course);
        AddChoosingTheForm(course);
        AddFirstDiagram(course);
        AddMaps(course);
        AddCharts(course);
        AddInfographic(course);
        AddDesign(course);
        AddClassroom(course);

        await db.SaveChangesAsync(ct);
        await SeedMathCourseAsync(db, ct);
    }

    private static bool Exists(Course course, string slug) => course.Lessons.Any(l => l.Slug == slug);

    private static void Reading(Lesson lesson, string title, ReadingContent content) =>
        lesson.AddStep(LessonStepKind.Reading, title, StepContentJson.Serialize(content));

    private static void Quiz(Lesson lesson, string title, QuizContent content) =>
        lesson.AddStep(LessonStepKind.Quiz, title, StepContentJson.Serialize(content));

    private static void Practice(Lesson lesson, string title, PracticeContent content) =>
        lesson.AddStep(LessonStepKind.Practice, title, StepContentJson.Serialize(content));

    private static void AddWhyVisualsWork(Course course)
    {
        if (Exists(course, "nega-korgazmali-material-ishlaydi")) return;

        var lesson = course.AddLesson(
            "nega-korgazmali-material-ishlaydi",
            "Nega körgazmali material yaxşiroq işlaydi?",
            "Bir xil mazmun matnda va sxemada nega turliça qabul qilinişini bilib oling.",
            10);

        Reading(lesson, "Bitta mazmun, ikki xil köriniş", new ReadingContent(
            [
                "Öquvçi matnni öqiganda jarayon bosqiçlarini va ular orasidagi boğliqlikni öz tasavvurida yiğib olişi kerak. Sxemada esa bu boğliqlik tayyor holda körinib turadi.",
                "Quyidagi misolda mazmun bir xil. Farqi faqat şaklda: qaysi birida jarayon ketma-ketligi tezroq közga taşlanişiga e’tibor bering.",
            ],
            Example: new ReadingExample(
                "Matn",
                "Quyoş issiqligi ta’sirida suv buğlanadi. Havoga kötarilgan buğ soviydi va bulutlarni hosil qiladi. Bulutlardan yomğir yoki qor yoğadi. Söng suv daryolar orqali yana dengizga oqib boradi.",
                "Sxema",
                ["Buğlaniş", "Kondensatsiya", "Yoğin", "Oqim"])));

        Reading(lesson, "Miyamiz axborotni qanday qabul qiladi?", new ReadingContent(
            [
                "**Ikki kanalli kodlaş.** A.Paivio nazariyasiga köra, inson söz va tasvirni ikki alohida, lekin boğliq tizimda qayta işlaydi. Axborot ikkala tizimda ham qayd etilsa, uni eslab qoliş osonlaşadi.",
                "**Kognitiv yuklama.** J.Sveller nazariyasiga köra, işçi xotira siğimi çeklangan. Material notöğri tuzilsa, xotira mazmunni tuşunişga emas, materialning özini “yeçişga” sarflanadi.",
                "Yaxşi sxema ortiqça yuklamani kamaytiradi: öquvçi kuçini boğliqliklarni qidirişga emas, mazmunni tuşunişga sarflaydi.",
            ],
            Callout: "Muhim: çiroyli rasm qöşiş vizuallaştiriş emas. Mazmunga aloqasi yöq bezak öquvçini çalğitadi va foydadan köra zarar keltirişi mumkin."));

        Quiz(lesson, "Tekşirib köramiz", new QuizContent(
            "Ikki kanalli kodlaş nazariyasi nima deydi?",
            [
                new QuizOption("a", "Tasvir har doim matndan yaxşiroq, şuning uçun matndan voz keçiş kerak"),
                new QuizOption("b", "Söz va tasvir birga töğri berilsa, axborotni eslab qoliş osonlaşadi"),
                new QuizOption("c", "Har bir öquvçi faqat bitta kanal orqali örganadi"),
            ],
            "b",
            "Töğri. Nazariya matndan voz keçişni emas, söz va tasvirni uyğunlaştirişni tavsiya qiladi.",
            "Unçalik emas. Nazariya ikki kanalning birgalikda işlaşi haqida. Yana bir bor öylab köring."));

        Quiz(lesson, "Yana bir savol", new QuizContent(
            "Slaydga mavzuga aloqasi yöq çiroyli rasm qöşilsa, nima bölişi mumkin?",
            [
                new QuizOption("a", "Öquvçi diqqati çalğiydi va ortiqça kognitiv yuklama paydo böladi"),
                new QuizOption("b", "Mavzu albatta yaxşiroq özlaştiriladi"),
                new QuizOption("c", "Işçi xotira siğimi kengayadi"),
            ],
            "a",
            "Töğri. Har bir vizual element aniq vazifani bajarişi kerak, aks holda u faqat yuklama qöşadi.",
            "Bu javob töğri emas. Kognitiv yuklama haqidagi qismni eslang: xotira siğimi çeklangan."));

        Practice(lesson, "Amaliy topşiriq", new PracticeContent(
            PracticeModes.Text,
            "Öz faningizdan öquvçilar eng qiynaladigan bitta mavzuni yozing. Nima uçun aynan şu mavzu qiyin ekanini qisqaça tuşuntiring. Keyingi darslarda şu mavzuga qaytamiz.",
            "Masalan: “Fotosintez bosqiçlari. Öquvçilar qaysi modda qayerda hosil bölişini adaştiradi.”"));
    }

    private static void AddChoosingTheForm(Course course)
    {
        if (Exists(course, "mazmunga-mos-shaklni-tanlash")) return;

        var lesson = course.AddLesson(
            "mazmunga-mos-shaklni-tanlash",
            "Mazmunga mos şaklni tanlaş",
            "Mavzudagi boğliqlik turini aniqlab, unga mos vizual şaklni tanlaşni örganing.",
            10);

        Reading(lesson, "Avval boğliqlikni aniqlang", new ReadingContent(
            [
                "Şaklni tanlaşdan oldin özingizga bitta savol bering: bu mavzuda tuşunçalar bir-biri bilan qanday boğlangan?",
                "**Ketma-ketlik** (jarayon, algoritm): blok-sxema. **Xronologiya** (tarixiy voqealar): vaqt çiziği.",
                "**Ierarxiya** (tasnif, tuzilma): daraxtsimon sxema yoki aqliy xarita. **Taqqoslaş**: jadval yoki Venn diagrammasi.",
                "**Sabab-oqibat**: strelkali sxema. **Miqdoriy boğliqlik** (raqamlar): grafik yoki diagramma.",
            ],
            Callout: "Eng köp uçraydigan xato: mazmun tuzilişini tahlil qilmasdan, çiroyli köringan şablonni tanlaş."));

        Quiz(lesson, "Qaysi şakl mos?", new QuizContent(
            "“Buyuk geografik kaşfiyotlar” mavzusida voqealar qaysi tartibda bölganini körsatmoqçisiz. Qaysi şakl eng mos?",
            [
                new QuizOption("a", "Aqliy xarita"),
                new QuizOption("b", "Vaqt çiziği"),
                new QuizOption("c", "Doiraviy diagramma"),
            ],
            "b",
            "Töğri. Voqealarning xronologik ketma-ketligi vaqt çiziğida eng aniq körinadi.",
            "Bu şakl boşqa boğliqlik uçun qulayroq. Bu yerda asosiy narsa nima: voqealar tartibi."));

        Quiz(lesson, "Yana bir vaziyat", new QuizContent(
            "“Söz turkumlari” mavzusida mustaqil va yordamçi sözlar tasnifini körsatmoqçisiz. Qaysi şakl mos?",
            [
                new QuizOption("a", "Vaqt çiziği"),
                new QuizOption("b", "Çiziqli grafik"),
                new QuizOption("c", "Daraxtsimon sxema"),
            ],
            "c",
            "Töğri. Tasnif ierarxik boğliqlikdir, uni daraxtsimon sxema yaxşi körsatadi.",
            "Bu mavzuda vaqt yoki raqam emas, “turlarga böliniş” muhim. Bu qaysi boğliqlik turiga kiradi?"));

        Practice(lesson, "Amaliy topşiriq", new PracticeContent(
            PracticeModes.Text,
            "Birinçi darsda tanlagan mavzuingizga qayting. Undagi asosiy boğliqlik turini va unga mos şaklni yozing. Nima uçun aynan şu şaklni tanlaganingizni bir-ikki gap bilan asoslang.",
            "Masalan: “Boğliqlik turi: ketma-ketlik. Şakl: blok-sxema, çunki fotosintez bosqiçlari birin-ketin keladi.”"));
    }

    private static void AddFirstDiagram(Course course)
    {
        if (Exists(course, "birinchi-sxemangiz")) return;

        var lesson = course.AddLesson(
            "birinchi-sxemangiz",
            "Birinçi sxemangiz",
            "Öz mavzuingiz böyiça jarayon sxemasini tuzing va uni mezonlar böyiça tekşiring.",
            15);

        Reading(lesson, "Yaxşi sxemaning uçta qoidasi", new ReadingContent(
            [
                "**Kam element.** Bir sxemada faqat şu dars uçun kerakli bosqiçlarni qoldiring. Elementlar köpaysa, ularni guruhlarga ajrating.",
                "**Qisqa yozuv.** Har bir elementda töliq gap emas, ikki-uç söz bölsin. Tuşuntirişni özingiz oğzaki aytasiz.",
                "**Bitta urğu.** Öquvçi birinçi nimaga qaraşi kerakligini rang yoki ölçam bilan ajrating. Hamma narsa ajratilsa, heç narsa ajralib turmaydi.",
            ]));

        Practice(lesson, "Sxemangizni tuzing", new PracticeContent(
            PracticeModes.Process,
            "Tanlagan mavzuingizdagi jarayonni bosqiçlarga ajrating. Har bir bosqiçga qisqa nom bering. Sxema yozganingiz sari çizilib boradi.",
            "Bosqiç nomi",
            MinItems: 3,
            MaxItems: 6));

        Practice(lesson, "Özingizni tekşiring", new PracticeContent(
            PracticeModes.Checklist,
            "Sxemangizga qarang va har bir bandni halol tekşiring. Biror band bajarilmagan bölsa, orqaga qaytib tuzating.",
            Checklist:
            [
                "Bosqiçlar töğri tartibda va ilmiy jihatdan töğri",
                "Har bir yozuv qisqa: ikki-uç sözdan oşmaydi",
                "Ortiqça element yöq: har biri aniq vazifani bajaradi",
                "Bu sxema mavzuni matndan köra tuşunarliroq körsatadi",
            ]));
    }
}
