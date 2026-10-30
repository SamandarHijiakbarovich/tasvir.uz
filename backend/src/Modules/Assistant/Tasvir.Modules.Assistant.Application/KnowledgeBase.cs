namespace Tasvir.Modules.Assistant.Application;

public sealed record KnowledgeEntry(
    string Question,
    string Answer,
    string[] Strong,
    string[] Weak,
    AssistantLink? Link = null,
    IReadOnlyList<string>? RelatedQuestions = null)
{
    public IReadOnlyList<string> Related => RelatedQuestions ?? [];
}

/// <summary>
/// Yordamchi bilimi. Strong: bitta o‘zi javobni tanlashga yetadigan kalit so‘z. Weak: boshqa so‘z bilan birga ishlaydi.
/// Kalitlar so‘z o‘zagi sifatida yoziladi. Faqat haqiqatan mavjud narsalar haqida yozing.
/// </summary>
internal static class KnowledgeBase
{
    public const string Fallback =
        "Bu savolga hozircha javob bera olmayman. Men Tasvir.uz, darslar va vizuallashtirish asoslari haqidagi oddiy savollarga javob beraman. Quyidagilardan birini tanlab ko‘ring.";

    private const string WhatIs = "Tasvir.uz nima?";
    private const string HowStart = "Qanday boshlayman?";
    private const string ForgotPassword = "Parolimni unutdim";
    private const string Price = "Narxi qancha?";
    private const string WhichShape = "Qaysi shaklni tanlayman?";
    private const string DualCoding = "Ikki kanalli kodlash nima?";
    private const string Infographic = "Infografika nima?";
    private const string MindMap = "Aqliy xarita nima?";
    private const string LessonLocked = "Dars nega yopiq?";

    public static readonly IReadOnlyList<string> DefaultSuggestions = [WhatIs, HowStart, WhichShape, ForgotPassword];

    public static readonly IReadOnlyList<KnowledgeEntry> Entries =
    [
        new("Salom",
            "Assalomu alaykum! Men Tasvir.uz yordamchisiman. Sayt, darslar va vizuallashtirish asoslari haqida so‘rashingiz mumkin.",
            ["salom", "assalomu", "hayrli"], []),

        new("Rahmat",
            "Arziydi! Yana savolingiz bo‘lsa, bemalol so‘rang.",
            ["rahmat", "tashakkur"], []),

        new("Sen kimsan?",
            "Men Tasvir.uz yordamchisiman. Tayyor bilim bazasi asosida oddiy savollarga avtomatik javob beraman, inson emasman. Bilmagan narsamni to‘qib aytmayman.",
            ["kimsan", "robot", "bot", "yordamchi"], ["sen", "siz"]),

        new(WhatIs,
            "Tasvir.uz o‘qituvchilarga o‘quv materialini sxema, infografika, diagramma va aqliy xarita ko‘rinishiga keltirishni o‘rgatadigan platforma. Akademiya o‘rgatadi, Studiya esa tayyor shablonlardan material yaratadi.",
            ["tasvir"], ["nima", "haqida", "platforma", "sayt"],
            new AssistantLink("Akademiyaga o‘tish", "/akademiya"),
            [HowStart, Price]),

        new(HowStart,
            "Ro‘yxatdan o‘ting, keyin kabinetda “Birinchi darsni boshlash” tugmasini bosing. Har bir dars o‘qish, test va amaliy topshiriqdan iborat.",
            ["boshla", "ishlatish"], ["qanday", "royxat", "qayerdan"],
            new AssistantLink("Ro‘yxatdan o‘tish", "/royxatdan-otish"),
            [LessonLocked, Price]),

        new("Qanday ro‘yxatdan o‘taman?",
            "Ism, email va parolingizni kiritib, hisob yarating. Karta talab qilinmaydi.",
            ["royxat", "registr", "hisob", "akkaunt", "akaunt"], [],
            new AssistantLink("Ro‘yxatdan o‘tish", "/royxatdan-otish")),

        new("Qanday kiraman?",
            "Yuqoridagi “Kirish” havolasini bosing va email bilan parolingizni kiriting. 5 marta xato kiritilsa, hisob 15 daqiqaga qulflanadi.",
            ["kira", "kirish", "login", "kirolmay", "qulf", "bloklan"], [],
            new AssistantLink("Kirish sahifasi", "/kirish"),
            [ForgotPassword]),

        new(ForgotPassword,
            "“Parolni tiklash” sahifasida emailingizni kiriting. Unga yangi parol o‘rnatish havolasi yuboriladi. Havola 60 daqiqa amal qiladi.",
            ["parol", "unut"], [],
            new AssistantLink("Parolni tiklash", "/parolni-tiklash")),

        new("Tasdiqlash xati kelmadi",
            "Avval “Spam” bo‘limini tekshiring. Kelmagan bo‘lsa, kabinetdagi “Xatni qayta yuborish” tugmasini bosing.",
            ["tasdiq", "xat", "pochta", "email"], ["kelmadi", "kelmayapti"],
            new AssistantLink("Kabinetga o‘tish", "/kabinet")),

        new(Price,
            "Bepul tarif bor va boshlash uchun karta talab qilinmaydi. Pro va Tashkilot tariflarining narxi hali e’lon qilinmagan.",
            ["narx", "tolov", "pul", "qancha", "tarif", "obuna"], ["turadi"],
            new AssistantLink("Narxlarni ko‘rish", "/#narxlar")),

        new("Sertifikat beriladimi?",
            "Sertifikat hozircha ishlab chiqilmoqda. Kursdagi natijalaringiz saqlanadi.",
            ["sertifikat", "guvohnoma", "diplom"], []),

        new("Studiya nima?",
            "Studiyada shablon tanlab, o‘z mavzuingiz bo‘yicha blok-sxema, vaqt chizig‘i, aqliy xarita, qiyosiy jadval yoki infografika yaratasiz. Material saqlanadi, PNG, SVG yoki PDF qilib olinadi va darsda bosqichma-bosqich ko‘rsatiladi.",
            ["studiya", "shablon", "material", "yaratish"], ["qachon"],
            new AssistantLink("Studiyaga o‘tish", "/studiya")),

        new("Materialni darsda qanday ko‘rsataman?",
            "Materialni ochib “Darsda ko‘rsatish” tugmasini bosing. O‘ng strelka yoki bo‘sh joy tugmasi keyingi elementni ochadi, chap strelka orqaga qaytaradi, F to‘liq ekran qiladi.",
            ["korsatish", "doska", "ekran", "proyektor"], ["darsda"],
            new AssistantLink("Materiallarim", "/studiya")),

        new("Sxemamni qanday yuklab olaman?",
            "Studiyadagi materialni ochib, “PNG”, “SVG” yoki “Chop etish / PDF” tugmasini bosing. Darslarda tuzgan sxemalar esa “Mening ishlarim” sahifasida.",
            ["yukla", "saqla", "png", "svg", "ishlarim", "skachat"], ["sxema"],
            new AssistantLink("Mening ishlarim", "/akademiya/ishlarim")),

        new(LessonLocked,
            "Darslar ketma-ket ochiladi, chunki har biri oldingisiga tayanadi. Oldingi darsning barcha bosqichlarini yakunlang, keyingisi o‘zi ochiladi.",
            ["yopiq", "ochilmay", "qulfli", "ochilmadi"], ["dars"],
            new AssistantLink("Darslar ro‘yxati", "/akademiya")),

        new("Testda xato qilsam nima bo‘ladi?",
            "Hech narsa yo‘qotmaysiz. Maslahatni o‘qib, qayta urinib ko‘rishingiz mumkin. To‘g‘ri javob topilgach, keyingi bosqich ochiladi.",
            ["test", "xato", "notogri"], ["javob"],
            null,
            [LessonLocked]),

        new("Natijam saqlanadimi?",
            "Ha. Har bir yakunlangan bosqich saqlanadi. Keyinroq kirsangiz, to‘xtagan joyingizdan davom etasiz.",
            ["saqlanadi", "progress"], ["natija", "davom"],
            new AssistantLink("Akademiyaga o‘tish", "/akademiya")),

        new("Nechta dars bor?",
            "Ikkita kurs bor: “Vizuallashtirish asoslari” (8 ta dars) va “Boshlang‘ich matematika” (7 ta dars). Darslar taxminan 10–20 daqiqa davom etadi.",
            ["nechta", "davomiylik"], ["dars", "kurs", "akademiya"],
            new AssistantLink("Darslar ro‘yxati", "/akademiya")),

        new("Akademiyada nimalar o‘rgatiladi?",
            "Vizual materialning ilmiy asoslari, mazmunga mos shaklni tanlash, sxema, aqliy xarita, diagramma va infografika tuzish, rang va shrift, materialni darsga olib kirish.",
            ["akademiya", "kurs", "orgatiladi", "darslar"], ["nima"],
            new AssistantLink("Darslar ro‘yxati", "/akademiya")),

        new("Matematika darslari bormi?",
            "Ha, “Boshlang‘ich matematika” kursida 1–4-sinf mavzulari bor: YouTube’dagi video darsni ko‘rasiz, keyin misol va masalalarni ishlaysiz. Javob darhol tekshiriladi, xato bo‘lsa maslahat chiqadi.",
            ["matematika", "misol", "masala"], ["dars"],
            new AssistantLink("Kurslarni ko‘rish", "/akademiya")),

        new("Video ochilmayapti",
            "Video YouTube orqali ko‘rsatiladi. Ochilmasa, internetni tekshiring yoki video ostidagi “YouTube’da ochish” havolasini bosing. Keyin mashqlarni davom ettirishingiz mumkin.",
            ["video", "yotub", "youtube", "ochilmayapti"], [],
            new AssistantLink("Kurslarni ko‘rish", "/akademiya")),

        new("Telefonda ishlaydimi?",
            "Ha, sayt telefon, planshet va kompyuterda ishlaydi. Alohida ilova kerak emas.",
            ["telefon", "mobil", "planshet", "ilova"], []),

        new("Kimlar uchun?",
            "O‘qituvchilar va bo‘lajak o‘qituvchilar uchun. Dasturlash yoki dizayn tajribasi talab qilinmaydi.",
            ["kimlar", "talaba", "oqituvchi"], ["uchun"]),

        new("Tadqiqot haqida",
            "Tasvir.uz metodikasi bo‘lajak o‘qituvchilarning vizuallashtirish kompetentligini rivojlantirishga oid dissertatsiya tadqiqotiga tayanadi. Tajriba-sinov natijalari tayyor bo‘lgach e’lon qilinadi.",
            ["tadqiqot", "dissertatsiya", "ilmiy", "metodika"], [],
            new AssistantLink("Tadqiqot bo‘limi", "/#tadqiqot")),

        new("Siz bilan qanday bog‘lanaman?",
            "Aloqa ma’lumotlari tez orada saytga joylanadi. Hozircha bu yerda yozilgan savollar hech kimga yuborilmaydi.",
            ["boglan", "aloqa", "kontakt", "murojaat"], []),

        new(WhichShape,
            "Avval bog‘liqlik turini aniqlang. Ketma-ketlik: blok-sxema. Xronologiya: vaqt chizig‘i. Tasnif: daraxtsimon sxema yoki aqliy xarita. Taqqoslash: jadval. Sabab-oqibat: strelkali sxema. Raqamlar: diagramma.",
            ["shakl", "tanla", "qaysi"], ["sxema", "mos"],
            new AssistantLink("2-dars: Mazmunga mos shaklni tanlash", "/akademiya"),
            [Infographic, MindMap]),

        new(DualCoding,
            "A.Paivio nazariyasi: inson so‘z va tasvirni ikki alohida, lekin bog‘liq tizimda qayta ishlaydi. Axborot ikkalasida ham qayd etilsa, eslab qolish osonlashadi.",
            ["kanal", "kodlash", "paivio"], [],
            null,
            ["Kognitiv yuklama nima?"]),

        new("Kognitiv yuklama nima?",
            "J.Sveller nazariyasi: ishchi xotira sig‘imi cheklangan. Material noto‘g‘ri tuzilsa, kuch mazmunni tushunishga emas, materialning o‘zini “yechishga” ketadi. Ortiqcha bezak va zich matn yuklamani oshiradi.",
            ["kognitiv", "yuklama", "sveller"], [],
            null,
            [DualCoding]),

        new(Infographic,
            "Infografika bitta asosiy g‘oyani matn, raqam va tasvir yordamida qisqa vizual hikoya qilib beradi. Tuzilishi: sarlavha, 3–5 ta blok, xulosa va manba.",
            ["infografika"], [],
            new AssistantLink("Darslar ro‘yxati", "/akademiya")),

        new(MindMap,
            "Aqliy xarita markazdagi bitta mavzudan tarmoqlanadigan tuzilma. G‘oyalarni yig‘ish, mavzuni rejalashtirish va takrorlash uchun qulay.",
            ["aqliy"], ["xarita"],
            null,
            ["Tushunchalar xaritasi nima?"]),

        new("Tushunchalar xaritasi nima?",
            "Tushunchalar orasidagi munosabatni ko‘rsatuvchi tuzilma. Bog‘lovchi chiziqlarda munosabat so‘z bilan yoziladi: “hosil qiladi”, “tarkibiga kiradi”. J.Novak ishlab chiqqan.",
            ["tushuncha"], ["xarita"]),

        new("Qaysi diagramma kerak?",
            "Taqqoslash uchun ustunli diagramma, vaqt bo‘yicha o‘zgarish uchun chiziqli grafik, butunning ulushi uchun doiraviy diagramma (bo‘laklar kam bo‘lsa). Ustunli diagrammada o‘q noldan boshlansin.",
            ["diagramma", "grafik"], []),

        new("Yaxshi sxema qanday bo‘ladi?",
            "Uchta qoida: elementlar kam, yozuvlar qisqa (ikki-uch so‘z), urg‘u bitta. O‘quvchi birinchi nimaga qarashni bir qarashda bilishi kerak.",
            ["sxema"], ["yaxshi", "qanday", "tuzish"],
            new AssistantLink("Darslar ro‘yxati", "/akademiya")),

        new("Qaysi rang va shriftni tanlay?",
            "Ikki-uch rang yetarli, har biri ma’no bildirsin. Bir-ikki o‘qilishi oson shrift ishlating. Fon bilan matn orasida kontrast bo‘lsin. Axborotni faqat rang bilan bildirmang.",
            ["rang", "shrift", "dizayn", "kontrast"], []),

        new("Sun’iy intellektdan foydalansam bo‘ladimi?",
            "Ha, tez tayyorlash uchun foydali. Lekin u xato qilishi mumkin: ilmiy to‘g‘rilikni va o‘quvchilarga mosligini har doim o‘zingiz tekshiring.",
            ["intellekt", "chatgpt", "gemini", "claude"], ["suniy"]),
    ];
}
