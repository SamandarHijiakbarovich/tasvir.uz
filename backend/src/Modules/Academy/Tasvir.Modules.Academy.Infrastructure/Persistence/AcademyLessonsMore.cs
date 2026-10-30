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
            "Aqliy xarita va tushunchalar xaritasi",
            "Ikki o‘xshash shakl o‘rtasidagi farqni va ularni qachon ishlatishni bilib oling.",
            10);

        Reading(lesson, "Ikki xarita, ikki vazifa", new ReadingContent(
            [
                "**Aqliy xarita** markazdagi bitta mavzudan tarmoqlanadi. U g‘oyalarni yig‘ish, mavzuni rejalashtirish va takrorlash uchun qulay. T.Buzan tomonidan ommalashtirilgan.",
                "**Tushunchalar xaritasi** bir nechta tushunchani va ular orasidagi munosabatni ko‘rsatadi. Bog‘lovchi chiziqlarda munosabat so‘z bilan yoziladi: “hosil qiladi”, “tarkibiga kiradi”. Bu usulni J.Novak ishlab chiqqan.",
                "Asosiy farq shunda: aqliy xaritada tarmoqlar mavzuga tegishli, tushunchalar xaritasida esa har bir bog‘liqlik aniq nomlanadi.",
            ],
            Callout: "Tushunchalar xaritasini o‘quvchining o‘ziga tuzdirish ayniqsa foydali: bog‘lovchi so‘zni topa olmasa, tushunchalar aloqasini hali anglab yetmagan bo‘ladi."));

        Quiz(lesson, "Qaysi birini tanlaysiz?", new QuizContent(
            "Yangi mavzudan oldin o‘quvchilarning g‘oyalarini tez yig‘moqchisiz. Qaysi shakl qulayroq?",
            [
                new QuizOption("a", "Aqliy xarita"),
                new QuizOption("b", "Tushunchalar xaritasi"),
                new QuizOption("c", "Chiziqli grafik"),
            ],
            "a",
            "To‘g‘ri. Aqliy xarita g‘oyalarni erkin yig‘ish uchun qulay: bog‘liqliklarni aniq nomlash shart emas.",
            "Bu vaziyatda hali aniq munosabatlar emas, tez yig‘iladigan g‘oyalar kerak. Qaysi shakl bunga mos?"));

        Quiz(lesson, "Asosiy farq", new QuizContent(
            "Tushunchalar xaritasining o‘ziga xos belgisi nima?",
            [
                new QuizOption("a", "Har doim bitta markaziy so‘zdan boshlanadi"),
                new QuizOption("b", "Bog‘lovchi chiziqlarda munosabat so‘z bilan yoziladi"),
                new QuizOption("c", "Faqat raqamli ma’lumotlar uchun ishlatiladi"),
            ],
            "b",
            "To‘g‘ri. Nomlangan bog‘lovchilar bilimning tuzilishini ochib beradi.",
            "Yana bir bor o‘ylang: bu xaritada chiziqlarning o‘zi nimani bildiradi?"));

        Practice(lesson, "Amaliy topshiriq", new PracticeContent(
            PracticeModes.Text,
            "O‘z mavzuingizdan uchta tushunchani tanlang. Ular orasidagi munosabatni “… → (bog‘lovchi so‘z) → …” ko‘rinishida yozing.",
            "Masalan: “Xloroplast → (tarkibida bor) → xlorofill; xlorofill → (yutadi) → yorug‘lik.”"));
    }

    private static void AddCharts(Course course)
    {
        const string slug = "diagrammani-togri-tanlash";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Diagrammani to‘g‘ri tanlash",
            "Raqamli ma’lumot uchun mos diagrammani tanlang va ma’lumotni buzib ko‘rsatmang.",
            10);

        Reading(lesson, "Savoldan boshlang", new ReadingContent(
            [
                "**Taqqoslash** (qaysi biri ko‘p?): ustunli diagramma. **O‘zgarish** (vaqt o‘tishi bilan qanday o‘zgardi?): chiziqli grafik. **Ulush** (butunning qancha qismi?): doiraviy diagramma, lekin bo‘laklar kam bo‘lsa.",
                "**Halollik qoidasi.** Ustunli diagrammada o‘q noldan boshlansin. Aks holda kichik farq katta bo‘lib ko‘rinadi va o‘quvchi noto‘g‘ri xulosa chiqaradi.",
                "**Ortiqchasini olib tashlang.** Uch o‘lchamli effekt, soya va ortiqcha chiziqlar ma’lumotga hech narsa qo‘shmaydi. E.Tafti buni ma’lumot ko‘rsatmaydigan “siyoh”ni kamaytirish deb tushuntiradi.",
            ]));

        Quiz(lesson, "Mos diagramma", new QuizContent(
            "Viloyat aholisining so‘nggi 10 yildagi o‘zgarishini ko‘rsatmoqchisiz. Qaysi diagramma eng mos?",
            [
                new QuizOption("a", "Doiraviy diagramma"),
                new QuizOption("b", "Chiziqli grafik"),
                new QuizOption("c", "Aqliy xarita"),
            ],
            "b",
            "To‘g‘ri. Vaqt bo‘yicha o‘zgarish chiziqli grafikda eng yaxshi ko‘rinadi.",
            "Bu yerda asosiy savol: vaqt o‘tishi bilan nima o‘zgardi? Qaysi shakl vaqtni ko‘rsatadi?"));

        Quiz(lesson, "Halollik", new QuizContent(
            "Ustunli diagrammada o‘q 95 dan boshlangan, ustunlar 96 va 98 ni ko‘rsatadi. Nima muammo?",
            [
                new QuizOption("a", "Hech qanday muammo yo‘q, joy tejaladi"),
                new QuizOption("b", "Ustunlar juda rangli bo‘lib qolgan"),
                new QuizOption("c", "Kichik farq bir necha baravar katta bo‘lib ko‘rinadi"),
            ],
            "c",
            "To‘g‘ri. Ustun uzunligi qiymatga mutanosib bo‘lishi kerak, shuning uchun o‘q noldan boshlanadi.",
            "Ustunlarning uzunligiga qarang: ular haqiqiy farqni to‘g‘ri aks ettiryaptimi?"));

        Practice(lesson, "Amaliy topshiriq", new PracticeContent(
            PracticeModes.Text,
            "O‘z faningizdan raqamli ma’lumot talab qiladigan bitta mavzuni yozing. Qaysi savolga javob berishini va qaysi diagrammani tanlaganingizni tushuntiring.",
            "Masalan: “Mavzu: Dunyo aholisi. Savol: qaysi qit’ada aholi ko‘p? Diagramma: ustunli, chunki qiymatlar taqqoslanadi.”"));
    }

    private static void AddInfographic(Course course)
    {
        const string slug = "infografika-rejasi";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Infografika: bitta sahifa, bitta g‘oya",
            "Infografikani chizishdan oldin uning rejasini tuzishni o‘rganing.",
            15);

        Reading(lesson, "Yaxshi infografika tuzilishi", new ReadingContent(
            [
                "Infografika shunchaki rasmli matn emas. U bitta asosiy g‘oyani qisqa va tartibli vizual hikoya sifatida yetkazadi.",
                "**Sarlavha** asosiy g‘oyani aytadi. **Asosiy qism** 3–5 ta blokdan iborat bo‘ladi. **Yakun** xulosa yoki savol beradi. **Manba** raqam va faktlar qayerdan olinganini ko‘rsatadi.",
                "O‘quvchi ko‘zi yuqoridan pastga yoki chapdan o‘ngga yuradi. Bloklarni shu tartibda joylashtiring va raqamlang.",
            ],
            Callout: "Infografikani dastur ochishdan oldin qog‘ozda rejalang. Reja aniq bo‘lsa, dizayn ancha tez bo‘ladi."));

        Quiz(lesson, "Tekshiramiz", new QuizContent(
            "Infografikaga barcha faktlarni sig‘dirishga urinish nimaga olib keladi?",
            [
                new QuizOption("a", "O‘quvchi asosiy g‘oyani yo‘qotadi va kognitiv yuklama oshadi"),
                new QuizOption("b", "Material albatta to‘liqroq o‘zlashtiriladi"),
                new QuizOption("c", "Sahifa yanada ishonchli ko‘rinadi"),
            ],
            "a",
            "To‘g‘ri. Bitta sahifa, bitta g‘oya. Qolgan faktlarni og‘zaki tushuntirish yoki boshqa materialda berish mumkin.",
            "Kognitiv yuklama haqidagi birinchi darsni eslang: ishchi xotira sig‘imi cheklangan."));

        Practice(lesson, "Infografika rejangiz", new PracticeContent(
            PracticeModes.Process,
            "O‘z mavzuingiz uchun infografika rejasini tuzing. Birinchi qatorga asosiy g‘oyani, keyingilariga bloklarni, oxirgisiga xulosani yozing.",
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
            "Rang, shrift va urg‘u",
            "Dizaynchi bo‘lmasdan ham materialingizni o‘qishga qulay qiling.",
            10);

        Reading(lesson, "To‘rtta oddiy qoida", new ReadingContent(
            [
                "**Vizual ierarxiya.** Eng muhim narsa eng katta yoki eng ajralib turgan bo‘lsin. O‘quvchi bir qarashda qayerdan boshlashni bilishi kerak.",
                "**Rang ma’no bildirsin.** Ikki-uch rang yetarli. Har bir rang biror narsani bildirsin: masalan, bitta guruh yoki eng muhim element.",
                "**O‘qilishi oson shrift.** Bitta materialda bir-ikki shrift. Slaydda matn orqa qatordan ham ko‘rinishi kerak.",
                "**Kontrast va bo‘sh joy.** Och fonda to‘q matn ishlating. Elementlar orasida joy qoldiring: bo‘sh joy ham guruhlash vositasi.",
            ],
            Callout: "Axborotni faqat rang bilan bildirmang. Rangni ajrata olmaydigan o‘quvchi uchun yozuv yoki belgi ham qo‘shing."));

        Quiz(lesson, "Vaziyat", new QuizContent(
            "Sxemada “to‘g‘ri” va “noto‘g‘ri” javoblar faqat yashil va qizil rang bilan ajratilgan. Nima qilish kerak?",
            [
                new QuizOption("a", "Ranglarni yanada yorqinroq qilish"),
                new QuizOption("b", "Rang bilan birga belgi yoki yozuv ham qo‘shish"),
                new QuizOption("c", "Hech narsa: rang yetarli"),
            ],
            "b",
            "To‘g‘ri. Belgi yoki yozuv bo‘lsa, axborot rangni ajrata olmaydigan o‘quvchiga ham yetib boradi.",
            "Rangni ajrata olmaydigan o‘quvchi bu sxemani qanday o‘qiydi? Shuni o‘ylab ko‘ring."));

        Practice(lesson, "Materialingizni tekshiring", new PracticeContent(
            PracticeModes.Checklist,
            "Oldingi darslarda tayyorlagan materialingizni yoki yaqinda ishlatgan slaydingizni oching va har bir bandni tekshiring.",
            Checklist:
            [
                "Bir qarashda eng muhim element ko‘rinadi",
                "Rang ikki-uchtadan oshmaydi va ma’no bildiradi",
                "Matn orqa qatordan ham o‘qiladi",
                "Axborot faqat rang bilan berilmagan",
            ]));
    }

    private static void AddClassroom(Course course)
    {
        const string slug = "materialni-darsga-olib-kirish";
        if (Exists(course, slug)) return;

        var lesson = course.AddLesson(
            slug,
            "Materialni darsga olib kirish",
            "Vizual materialni dars bosqichiga moslang va sun’iy intellekt natijasini tekshirishni o‘rganing.",
            15);

        Reading(lesson, "Qachon va qanday ko‘rsatish kerak?", new ReadingContent(
            [
                "**Dars boshida** savol qo‘yuvchi infografika qiziqish uyg‘otadi. **Yangi mavzuda** sxemani bosqichma-bosqich ochib boring: hammasini birdaniga ko‘rsatmang.",
                "**Mustahkamlashda** bo‘sh joyli sxema bering. **Yakunda** o‘quvchilar o‘zlari aqliy xarita yoki sxema tuzsin: eng katta foyda aynan shu yerda, chunki ular materialni qayta tuzadi.",
                "**Sun’iy intellekt** sxema va matnni tez tayyorlaydi, lekin xato qilishi mumkin. Ilmiy to‘g‘rilikni, o‘quvchi yoshiga mosligini va manbani har doim o‘zingiz tekshiring. Javobgarlik o‘qituvchida qoladi.",
            ]));

        Quiz(lesson, "Sun’iy intellekt", new QuizContent(
            "Sun’iy intellekt fotosintez sxemasini yaratdi. Darsda ko‘rsatishdan oldin nima qilish kerak?",
            [
                new QuizOption("a", "Hech narsa: u har doim to‘g‘ri ishlaydi"),
                new QuizOption("b", "Faqat ranglarini o‘zgartirish"),
                new QuizOption("c", "Ilmiy to‘g‘riligini va o‘quvchilarga mosligini tekshirish"),
            ],
            "c",
            "To‘g‘ri. Vosita tez ishlaydi, baholovchi esa o‘qituvchi.",
            "Sxemada ilmiy xato bo‘lsa, u o‘quvchilarda noto‘g‘ri bilim hosil qiladi. Unda birinchi navbatda nima muhim?"));

        Quiz(lesson, "Faol o‘rganish", new QuizContent(
            "O‘quvchilar qachon materialni chuqurroq qayta ishlaydi?",
            [
                new QuizOption("a", "Tayyor sxemani ko‘rib o‘tirganda"),
                new QuizOption("b", "Matn asosida sxemani o‘zlari tuzganda"),
                new QuizOption("c", "Sxemani daftarga aynan ko‘chirganda"),
            ],
            "b",
            "To‘g‘ri. Sxema tuzish uchun asosiy fikrni ajratish va bog‘liqlikni topish kerak bo‘ladi.",
            "Qaysi holatda o‘quvchi tahlil qilishi va o‘zi qaror qabul qilishi kerak?"));

        Practice(lesson, "Yakuniy topshiriq", new PracticeContent(
            PracticeModes.Text,
            "Tayyorlagan materialingizni qaysi sinfda, dars qaysi bosqichida va qanday ishlatishingizni yozing. Keyin o‘quvchilarga qanday topshiriq berasiz?",
            "Masalan: “8-sinf, yangi mavzu bosqichi. Sxemani bosqichma-bosqich ochaman. So‘ng o‘quvchilar bo‘sh joylarni to‘ldiradi.”"));
    }
}
