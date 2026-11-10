using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Infrastructure.Persistence;

/// <summary>Kursning 4–8-darslari. Mazmun: tushunchalar xaritasi, diagrammalar, infografika, dizayn, darsga tatbiq etish.</summary>
internal static partial class AcademySeeder
{
    private static void AddMaps(Course course)
    {
        const string slug = "aqliy-xarita-va-tushunchalar-xaritasi";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Aqliy xarita va tuşunçalar xaritasi",
            "Ikki öxşaş şakl örtasidagi farqni va ularni qaçon işlatişni bilib oling.",
            10);

        Reading(lesson, "Ikki xarita, ikki vazifa", new ReadingContent(
            [
                "**Aqliy xarita** markazdagi bitta mavzudan tarmoqlanadi. U ğoyalarni yiğiş, mavzuni rejalaştiriş va takrorlaş uçun qulay. T.Buzan tomonidan ommalaştirilgan.",
                "**Tuşunçalar xaritasi** bir neçta tuşunçani va ular orasidagi munosabatni körsatadi. Boğlovçi çiziqlarda munosabat söz bilan yoziladi: “hosil qiladi”, “tarkibiga kiradi”. Bu usulni J.Novak işlab çiqqan.",
                "Asosiy farq şunda: aqliy xaritada tarmoqlar mavzuga tegişli, tuşunçalar xaritasida esa har bir boğliqlik aniq nomlanadi.",
            ],
            Callout: "Tuşunçalar xaritasini öquvçining öziga tuzdiriş ayniqsa foydali: boğlovçi sözni topa olmasa, tuşunçalar aloqasini hali anglab yetmagan böladi."));

        Quiz(lesson, "Qaysi birini tanlaysiz?", new QuizContent(
            "Yangi mavzudan oldin öquvçilarning ğoyalarini tez yiğmoqçisiz. Qaysi şakl qulayroq?",
            [
                new QuizOption("a", "Aqliy xarita"),
                new QuizOption("b", "Tuşunçalar xaritasi"),
                new QuizOption("c", "Çiziqli grafik"),
            ],
            "a",
            "Töğri. Aqliy xarita ğoyalarni erkin yiğiş uçun qulay: boğliqliklarni aniq nomlaş şart emas.",
            "Bu vaziyatda hali aniq munosabatlar emas, tez yiğiladigan ğoyalar kerak. Qaysi şakl bunga mos?"));

        Quiz(lesson, "Asosiy farq", new QuizContent(
            "Tuşunçalar xaritasining öziga xos belgisi nima?",
            [
                new QuizOption("a", "Har doim bitta markaziy sözdan boşlanadi"),
                new QuizOption("b", "Boğlovçi çiziqlarda munosabat söz bilan yoziladi"),
                new QuizOption("c", "Faqat raqamli ma’lumotlar uçun işlatiladi"),
            ],
            "b",
            "Töğri. Nomlangan boğlovçilar bilimning tuzilişini oçib beradi.",
            "Yana bir bor öylang: bu xaritada çiziqlarning özi nimani bildiradi?"));

        Practice(lesson, "Amaliy topşiriq", new PracticeContent(
            PracticeModes.Text,
            "Öz mavzuingizdan uçta tuşunçani tanlang. Ular orasidagi munosabatni “… → (boğlovçi söz) → …” körinişida yozing.",
            "Masalan: “Xloroplast → (tarkibida bor) → xlorofill; xlorofill → (yutadi) → yoruğlik.”"));
    }

    private static void AddCharts(Course course)
    {
        const string slug = "diagrammani-togri-tanlash";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Diagrammani töğri tanlaş",
            "Raqamli ma’lumot uçun mos diagrammani tanlang va ma’lumotni buzib körsatmang.",
            10);

        Reading(lesson, "Savoldan boşlang", new ReadingContent(
            [
                "**Taqqoslaş** (qaysi biri köp?): ustunli diagramma. **Özgariş** (vaqt ötişi bilan qanday özgardi?): çiziqli grafik. **Uluş** (butunning qança qismi?): doiraviy diagramma, lekin bölaklar kam bölsa.",
                "**Halollik qoidasi.** Ustunli diagrammada öq noldan boşlansin. Aks holda kiçik farq katta bölib körinadi va öquvçi notöğri xulosa çiqaradi.",
                "**Ortiqçasini olib taşlang.** Uç ölçamli effekt, soya va ortiqça çiziqlar ma’lumotga heç narsa qöşmaydi. E.Tafti buni ma’lumot körsatmaydigan “siyoh”ni kamaytiriş deb tuşuntiradi.",
            ]));

        Quiz(lesson, "Mos diagramma", new QuizContent(
            "Viloyat aholisining sönggi 10 yildagi özgarişini körsatmoqçisiz. Qaysi diagramma eng mos?",
            [
                new QuizOption("a", "Doiraviy diagramma"),
                new QuizOption("b", "Çiziqli grafik"),
                new QuizOption("c", "Aqliy xarita"),
            ],
            "b",
            "Töğri. Vaqt böyiça özgariş çiziqli grafikda eng yaxşi körinadi.",
            "Bu yerda asosiy savol: vaqt ötişi bilan nima özgardi? Qaysi şakl vaqtni körsatadi?"));

        Quiz(lesson, "Halollik", new QuizContent(
            "Ustunli diagrammada öq 95 dan boşlangan, ustunlar 96 va 98 ni körsatadi. Nima muammo?",
            [
                new QuizOption("a", "Heç qanday muammo yöq, joy tejaladi"),
                new QuizOption("b", "Ustunlar juda rangli bölib qolgan"),
                new QuizOption("c", "Kiçik farq bir neça baravar katta bölib körinadi"),
            ],
            "c",
            "Töğri. Ustun uzunligi qiymatga mutanosib bölişi kerak, şuning uçun öq noldan boşlanadi.",
            "Ustunlarning uzunligiga qarang: ular haqiqiy farqni töğri aks ettiryaptimi?"));

        Practice(lesson, "Amaliy topşiriq", new PracticeContent(
            PracticeModes.Text,
            "Öz faningizdan raqamli ma’lumot talab qiladigan bitta mavzuni yozing. Qaysi savolga javob berişini va qaysi diagrammani tanlaganingizni tuşuntiring.",
            "Masalan: “Mavzu: Dunyo aholisi. Savol: qaysi qit’ada aholi köp? Diagramma: ustunli, çunki qiymatlar taqqoslanadi.”"));
    }

    private static void AddInfographic(Course course)
    {
        const string slug = "infografika-rejasi";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Infografika: bitta sahifa, bitta ğoya",
            "Infografikani çizişdan oldin uning rejasini tuzişni örganing.",
            15);

        Reading(lesson, "Yaxşi infografika tuzilişi", new ReadingContent(
            [
                "Infografika şunçaki rasmli matn emas. U bitta asosiy ğoyani qisqa va tartibli vizual hikoya sifatida yetkazadi.",
                "**Sarlavha** asosiy ğoyani aytadi. **Asosiy qism** 3–5 ta blokdan iborat böladi. **Yakun** xulosa yoki savol beradi. **Manba** raqam va faktlar qayerdan olinganini körsatadi.",
                "Öquvçi közi yuqoridan pastga yoki çapdan öngga yuradi. Bloklarni şu tartibda joylaştiring va raqamlang.",
            ],
            Callout: "Infografikani dastur oçişdan oldin qoğozda rejalang. Reja aniq bölsa, dizayn ança tez böladi."));

        Quiz(lesson, "Tekşiramiz", new QuizContent(
            "Infografikaga barça faktlarni siğdirişga uriniş nimaga olib keladi?",
            [
                new QuizOption("a", "Öquvçi asosiy ğoyani yöqotadi va kognitiv yuklama oşadi"),
                new QuizOption("b", "Material albatta töliqroq özlaştiriladi"),
                new QuizOption("c", "Sahifa yanada işonçli körinadi"),
            ],
            "a",
            "Töğri. Bitta sahifa, bitta ğoya. Qolgan faktlarni oğzaki tuşuntiriş yoki boşqa materialda beriş mumkin.",
            "Kognitiv yuklama haqidagi birinçi darsni eslang: işçi xotira siğimi çeklangan."));

        Practice(lesson, "Infografika rejangiz", new PracticeContent(
            PracticeModes.Process,
            "Öz mavzuingiz uçun infografika rejasini tuzing. Birinçi qatorga asosiy ğoyani, keyingilariga bloklarni, oxirgisiga xulosani yozing.",
            "Blok nomi",
            MinItems: 3,
            MaxItems: 6));
    }

    private static void AddDesign(Course course)
    {
        const string slug = "rang-shrift-va-urgu";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Rang, şrift va urğu",
            "Dizaynçi bölmasdan ham materialingizni öqişga qulay qiling.",
            10);

        Reading(lesson, "Törtta oddiy qoida", new ReadingContent(
            [
                "**Vizual ierarxiya.** Eng muhim narsa eng katta yoki eng ajralib turgan bölsin. Öquvçi bir qaraşda qayerdan boşlaşni bilişi kerak.",
                "**Rang ma’no bildirsin.** Ikki-uç rang yetarli. Har bir rang biror narsani bildirsin: masalan, bitta guruh yoki eng muhim element.",
                "**Öqilişi oson şrift.** Bitta materialda bir-ikki şrift. Slaydda matn orqa qatordan ham körinişi kerak.",
                "**Kontrast va böş joy.** Oç fonda töq matn işlating. Elementlar orasida joy qoldiring: böş joy ham guruhlaş vositasi.",
            ],
            Callout: "Axborotni faqat rang bilan bildirmang. Rangni ajrata olmaydigan öquvçi uçun yozuv yoki belgi ham qöşing."));

        Quiz(lesson, "Vaziyat", new QuizContent(
            "Sxemada “töğri” va “notöğri” javoblar faqat yaşil va qizil rang bilan ajratilgan. Nima qiliş kerak?",
            [
                new QuizOption("a", "Ranglarni yanada yorqinroq qiliş"),
                new QuizOption("b", "Rang bilan birga belgi yoki yozuv ham qöşiş"),
                new QuizOption("c", "Heç narsa: rang yetarli"),
            ],
            "b",
            "Töğri. Belgi yoki yozuv bölsa, axborot rangni ajrata olmaydigan öquvçiga ham yetib boradi.",
            "Rangni ajrata olmaydigan öquvçi bu sxemani qanday öqiydi? Şuni öylab köring."));

        Practice(lesson, "Materialingizni tekşiring", new PracticeContent(
            PracticeModes.Checklist,
            "Oldingi darslarda tayyorlagan materialingizni yoki yaqinda işlatgan slaydingizni oçing va har bir bandni tekşiring.",
            Checklist:
            [
                "Bir qaraşda eng muhim element körinadi",
                "Rang ikki-uçtadan oşmaydi va ma’no bildiradi",
                "Matn orqa qatordan ham öqiladi",
                "Axborot faqat rang bilan berilmagan",
            ]));
    }

    private static void AddClassroom(Course course)
    {
        const string slug = "materialni-darsga-olib-kirish";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Materialni darsga olib kiriş",
            "Vizual materialni dars bosqiçiga moslang va sun’iy intellekt natijasini tekşirişni örganing.",
            15);

        Reading(lesson, "Qaçon va qanday körsatiş kerak?", new ReadingContent(
            [
                "**Dars boşida** savol qöyuvçi infografika qiziqiş uyğotadi. **Yangi mavzuda** sxemani bosqiçma-bosqiç oçib boring: hammasini birdaniga körsatmang.",
                "**Mustahkamlaşda** böş joyli sxema bering. **Yakunda** öquvçilar özlari aqliy xarita yoki sxema tuzsin: eng katta foyda aynan şu yerda, çunki ular materialni qayta tuzadi.",
                "**Sun’iy intellekt** sxema va matnni tez tayyorlaydi, lekin xato qilişi mumkin. Ilmiy töğrilikni, öquvçi yoşiga mosligini va manbani har doim özingiz tekşiring. Javobgarlik öqituvçida qoladi.",
            ]));

        Quiz(lesson, "Sun’iy intellekt", new QuizContent(
            "Sun’iy intellekt fotosintez sxemasini yaratdi. Darsda körsatişdan oldin nima qiliş kerak?",
            [
                new QuizOption("a", "Heç narsa: u har doim töğri işlaydi"),
                new QuizOption("b", "Faqat ranglarini özgartiriş"),
                new QuizOption("c", "Ilmiy töğriligini va öquvçilarga mosligini tekşiriş"),
            ],
            "c",
            "Töğri. Vosita tez işlaydi, baholovçi esa öqituvçi.",
            "Sxemada ilmiy xato bölsa, u öquvçilarda notöğri bilim hosil qiladi. Unda birinçi navbatda nima muhim?"));

        Quiz(lesson, "Faol örganiş", new QuizContent(
            "Öquvçilar qaçon materialni çuqurroq qayta işlaydi?",
            [
                new QuizOption("a", "Tayyor sxemani körib ötirganda"),
                new QuizOption("b", "Matn asosida sxemani özlari tuzganda"),
                new QuizOption("c", "Sxemani daftarga aynan köçirganda"),
            ],
            "b",
            "Töğri. Sxema tuziş uçun asosiy fikrni ajratiş va boğliqlikni topiş kerak böladi.",
            "Qaysi holatda öquvçi tahlil qilişi va özi qaror qabul qilişi kerak?"));

        Practice(lesson, "Yakuniy topşiriq", new PracticeContent(
            PracticeModes.Text,
            "Tayyorlagan materialingizni qaysi sinfda, dars qaysi bosqiçida va qanday işlatişingizni yozing. Keyin öquvçilarga qanday topşiriq berasiz?",
            "Masalan: “8-sinf, yangi mavzu bosqiçi. Sxemani bosqiçma-bosqiç oçaman. Söng öquvçilar böş joylarni töldiradi.”"));
    }
}
