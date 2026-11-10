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
        "Bu savolga hozirça javob bera olmayman. Men Tasvir.uz, darslar va vizuallaştiriş asoslari haqidagi oddiy savollarga javob beraman. Quyidagilardan birini tanlab köring.";

    private const string WhatIs = "Tasvir.uz nima?";
    private const string HowStart = "Qanday boşlayman?";
    private const string ForgotPassword = "Parolimni unutdim";
    private const string Price = "Narxi qança?";
    private const string WhichShape = "Qaysi şaklni tanlayman?";
    private const string DualCoding = "Ikki kanalli kodlaş nima?";
    private const string Infographic = "Infografika nima?";
    private const string MindMap = "Aqliy xarita nima?";
    private const string LessonLocked = "Dars nega yopiq?";
    private const string Visualization = "Vizualizatsiya nima?";
    private const string Tools = "Qanday dasturlar bor?";

    public static readonly IReadOnlyList<string> DefaultSuggestions = [WhatIs, HowStart, WhichShape, ForgotPassword];

    public static readonly IReadOnlyList<KnowledgeEntry> Entries =
    [
        new("Salom",
            "Assalomu alaykum! Men Tasvir.uz yordamçisiman. Sayt, darslar va vizuallaştiriş asoslari haqida söraşingiz mumkin.",
            ["salom", "assalomu", "hayrli"], []),

        new("Rahmat",
            "Arziydi! Yana savolingiz bölsa, bemalol sörang.",
            ["rahmat", "tashakkur"], []),

        new("Sen kimsan?",
            "Men Tasvir.uz yordamçisiman. Tayyor bilim bazasi asosida oddiy savollarga avtomatik javob beraman, inson emasman. Bilmagan narsamni töqib aytmayman.",
            ["kimsan", "robot", "bot", "yordamchi"], ["sen", "siz"]),

        new(WhatIs,
            "Tasvir.uz öqituvçilarga öquv materialini sxema, infografika, diagramma va aqliy xarita körinişiga keltirişni örgatadigan platforma. Akademiya örgatadi, Studiya esa tayyor şablonlardan material yaratadi.",
            ["tasvir"], ["nima", "haqida", "platforma", "sayt"],
            new AssistantLink("Akademiyaga ötiş", "/akademiya"),
            [HowStart, Price]),

        new(HowStart,
            "Röyxatdan öting, keyin kabinetda “Birinçi darsni boşlaş” tugmasini bosing. Har bir dars öqiş, test va amaliy topşiriqdan iborat.",
            ["boshla", "ishlatish"], ["qanday", "royxat", "qayerdan"],
            new AssistantLink("Röyxatdan ötiş", "/royxatdan-otish"),
            [LessonLocked, Price]),

        new("Qanday röyxatdan ötaman?",
            "Ism, email va parolingizni kiritib, hisob yarating. Karta talab qilinmaydi.",
            ["royxat", "registr", "hisob", "akkaunt", "akaunt"], [],
            new AssistantLink("Röyxatdan ötiş", "/royxatdan-otish")),

        new("Qanday kiraman?",
            "Yuqoridagi “Kiriş” havolasini bosing va email bilan parolingizni kiriting. 5 marta xato kiritilsa, hisob 15 daqiqaga qulflanadi.",
            ["kira", "kirish", "login", "kirolmay", "qulf", "bloklan"], [],
            new AssistantLink("Kiriş sahifasi", "/kirish"),
            [ForgotPassword]),

        new(ForgotPassword,
            "“Parolni tiklaş” sahifasida emailingizni kiriting. Unga yangi parol örnatiş havolasi yuboriladi. Havola 60 daqiqa amal qiladi.",
            ["parol", "unut"], [],
            new AssistantLink("Parolni tiklaş", "/parolni-tiklash")),

        new("Tasdiqlaş xati kelmadi",
            "Avval “Spam” bölimini tekşiring. Kelmagan bölsa, kabinetdagi “Xatni qayta yuboriş” tugmasini bosing.",
            ["tasdiq", "xat", "pochta", "email"], ["kelmadi", "kelmayapti"],
            new AssistantLink("Kabinetga ötiş", "/kabinet")),

        new(Price,
            "Bepul tarif bor va boşlaş uçun karta talab qilinmaydi. Pro va Taşkilot tariflarining narxi hali e’lon qilinmagan.",
            ["narx", "tolov", "pul", "qancha", "tarif", "obuna"], ["turadi"],
            new AssistantLink("Narxlarni köriş", "/#narxlar")),

        new("Sertifikat beriladimi?",
            "Sertifikat hozirça işlab çiqilmoqda. Kursdagi natijalaringiz saqlanadi.",
            ["sertifikat", "guvohnoma", "diplom"], []),

        new("Studiya nima?",
            "Studiyada şablon tanlab, öz mavzuingiz böyiça blok-sxema, vaqt çiziği, aqliy xarita, qiyosiy jadval yoki infografika yaratasiz. Material saqlanadi, PNG, SVG yoki PDF qilib olinadi va darsda bosqiçma-bosqiç körsatiladi.",
            ["studiya", "shablon", "material", "yaratish"], ["qachon"],
            new AssistantLink("Studiyaga ötiş", "/studiya")),

        new("Materialni darsda qanday körsataman?",
            "Materialni oçib “Darsda körsatiş” tugmasini bosing. Öng strelka yoki böş joy tugmasi keyingi elementni oçadi, çap strelka orqaga qaytaradi, F töliq ekran qiladi.",
            ["korsatish", "doska", "ekran", "proyektor"], ["darsda"],
            new AssistantLink("Materiallarim", "/studiya")),

        new("Sxemamni qanday yuklab olaman?",
            "Studiyadagi materialni oçib, “PNG”, “SVG” yoki “Çop etiş / PDF” tugmasini bosing. Darslarda tuzgan sxemalar esa “Mening işlarim” sahifasida.",
            ["yukla", "saqla", "png", "svg", "ishlarim", "skachat"], ["sxema"],
            new AssistantLink("Mening işlarim", "/akademiya/ishlarim")),

        new(LessonLocked,
            "Darslar ketma-ket oçiladi, çunki har biri oldingisiga tayanadi. Oldingi darsning barça bosqiçlarini yakunlang, keyingisi özi oçiladi.",
            ["yopiq", "ochilmay", "qulfli", "ochilmadi"], ["dars"],
            new AssistantLink("Darslar röyxati", "/akademiya")),

        new("Testda xato qilsam nima böladi?",
            "Heç narsa yöqotmaysiz. Maslahatni öqib, qayta urinib körişingiz mumkin. Töğri javob topilgaç, keyingi bosqiç oçiladi.",
            ["test", "xato", "notogri"], ["javob"],
            null,
            [LessonLocked]),

        new("Natijam saqlanadimi?",
            "Ha. Har bir yakunlangan bosqiç saqlanadi. Keyinroq kirsangiz, töxtagan joyingizdan davom etasiz.",
            ["saqlanadi", "progress"], ["natija", "davom"],
            new AssistantLink("Akademiyaga ötiş", "/akademiya")),

        new("Neçta dars bor?",
            "Ikkita kurs bor: “Vizuallaştiriş asoslari” (8 ta dars) va “Boşlanğiç matematika” (7 ta dars). Darslar taxminan 10–20 daqiqa davom etadi.",
            ["nechta", "davomiylik"], ["dars", "kurs", "akademiya"],
            new AssistantLink("Darslar röyxati", "/akademiya")),

        new("Akademiyada nimalar örgatiladi?",
            "Vizual materialning ilmiy asoslari, mazmunga mos şaklni tanlaş, sxema, aqliy xarita, diagramma va infografika tuziş, rang va şrift, materialni darsga olib kiriş.",
            ["akademiya", "kurs", "orgatiladi", "darslar"], ["nima"],
            new AssistantLink("Darslar röyxati", "/akademiya")),

        new("Matematika darslari bormi?",
            "Ha, “Boşlanğiç matematika” kursida 1–4-sinf mavzulari bor: YouTube’dagi video darsni körasiz, keyin misol va masalalarni işlaysiz. Javob darhol tekşiriladi, xato bölsa maslahat çiqadi.",
            ["matematika", "misol", "masala"], ["dars"],
            new AssistantLink("Kurslarni köriş", "/akademiya")),

        new("Video oçilmayapti",
            "Video YouTube orqali körsatiladi. Oçilmasa, internetni tekşiring yoki video ostidagi “YouTube’da oçiş” havolasini bosing. Keyin maşqlarni davom ettirişingiz mumkin.",
            ["video", "yotub", "youtube", "ochilmayapti"], [],
            new AssistantLink("Kurslarni köriş", "/akademiya")),

        new("Telefonda işlaydimi?",
            "Ha, sayt telefon, planşet va kompyuterda işlaydi. Alohida ilova kerak emas.",
            ["telefon", "mobil", "planshet", "ilova"], []),

        new("Kimlar uçun?",
            "Öqituvçilar va bölajak öqituvçilar uçun. Dasturlaş yoki dizayn tajribasi talab qilinmaydi.",
            ["kimlar", "talaba", "oqituvchi"], ["uchun"]),

        new("Tadqiqot haqida",
            "Tasvir.uz metodikasi bölajak öqituvçilarning vizuallaştiriş kompetentligini rivojlantirişga oid dissertatsiya tadqiqotiga tayanadi. Tajriba-sinov natijalari tayyor bölgaç e’lon qilinadi.",
            ["tadqiqot", "dissertatsiya", "ilmiy", "metodika"], [],
            new AssistantLink("Tadqiqot bölimi", "/#tadqiqot")),

        new("Siz bilan qanday boğlanaman?",
            "Aloqa ma’lumotlari tez orada saytga joylanadi. Hozirça bu yerda yozilgan savollar heç kimga yuborilmaydi.",
            ["boglan", "aloqa", "kontakt", "murojaat"], []),

        new(WhichShape,
            "Avval boğliqlik turini aniqlang. Ketma-ketlik: blok-sxema. Xronologiya: vaqt çiziği. Tasnif: daraxtsimon sxema yoki aqliy xarita. Taqqoslaş: jadval. Sabab-oqibat: strelkali sxema. Raqamlar: diagramma.",
            ["shakl", "tanla", "qaysi"], ["sxema", "mos"],
            new AssistantLink("2-dars: Mazmunga mos şaklni tanlaş", "/akademiya"),
            [Infographic, MindMap]),

        new(DualCoding,
            "A.Paivio nazariyasi: inson söz va tasvirni ikki alohida, lekin boğliq tizimda qayta işlaydi. Axborot ikkalasida ham qayd etilsa, eslab qoliş osonlaşadi.",
            ["kanal", "kodlash", "paivio"], [],
            null,
            ["Kognitiv yuklama nima?"]),

        new("Kognitiv yuklama nima?",
            "J.Sveller nazariyasi: işçi xotira siğimi çeklangan. Material notöğri tuzilsa, kuç mazmunni tuşunişga emas, materialning özini “yeçişga” ketadi. Ortiqça bezak va ziç matn yuklamani oşiradi.",
            ["kognitiv", "yuklama", "sveller"], [],
            null,
            [DualCoding]),

        new(Infographic,
            "Infografika bitta asosiy ğoyani matn, raqam va tasvir yordamida qisqa vizual hikoya qilib beradi. Tuzilişi: sarlavha, 3–5 ta blok, xulosa va manba.",
            ["infografika"], [],
            new AssistantLink("Darslar röyxati", "/akademiya")),

        new(MindMap,
            "Aqliy xarita markazdagi bitta mavzudan tarmoqlanadigan tuzilma. Ğoyalarni yiğiş, mavzuni rejalaştiriş va takrorlaş uçun qulay.",
            ["aqliy"], ["xarita"],
            null,
            ["Tuşunçalar xaritasi nima?"]),

        new("Tuşunçalar xaritasi nima?",
            "Tuşunçalar orasidagi munosabatni körsatuvçi tuzilma. Boğlovçi çiziqlarda munosabat söz bilan yoziladi: “hosil qiladi”, “tarkibiga kiradi”. J.Novak işlab çiqqan.",
            ["tushuncha"], ["xarita"]),

        new("Qaysi diagramma kerak?",
            "Taqqoslaş uçun ustunli diagramma, vaqt böyiça özgariş uçun çiziqli grafik, butunning uluşi uçun doiraviy diagramma (bölaklar kam bölsa). Ustunli diagrammada öq noldan boşlansin.",
            ["diagramma", "grafik"], []),

        new("Yaxşi sxema qanday böladi?",
            "Uçta qoida: elementlar kam, yozuvlar qisqa (ikki-uç söz), urğu bitta. Öquvçi birinçi nimaga qaraşni bir qaraşda bilişi kerak.",
            ["sxema"], ["yaxshi", "qanday", "tuzish"],
            new AssistantLink("Darslar röyxati", "/akademiya")),

        new("Qaysi rang va şriftni tanlay?",
            "Ikki-uç rang yetarli, har biri ma’no bildirsin. Bir-ikki öqilişi oson şrift işlating. Fon bilan matn orasida kontrast bölsin. Axborotni faqat rang bilan bildirmang.",
            ["rang", "shrift", "dizayn", "kontrast"], []),

        new(Visualization,
            "Vizualizatsiya axborotni köz bilan idrok etiladigan şaklga keltiriş: matn, son va tuşunçalar orasidagi boğliqlik sxema, diagramma, jadval yoki xaritada körsatiladi. Dasturda vizualizatsiya qilganda bu işni dastur osonlaştiradi: şablon beradi, ma’lumotdan diagrammani özi çizadi, materialni saqlaş va darsda körsatişga yordam beradi. Qaysi şakl kerakligini esa öqituvçi hal qiladi.",
            ["vizualizats"], ["vizuallashtirish", "nima", "dastur"],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            [Tools, WhichShape]),

        new("Canva nima?",
            "Canva onlayn grafik dizayn xizmati: tayyor şablonlardan infografika, plakat, slayd va iş varaği yasaladi. Bepul tarifi bor. U har qanday dizayn uçun universal vosita, Tasvir.uz Studiyasi esa mazmunga mos şaklni tanlaşga yönaltiradi.",
            ["canva"], [],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            [Tools]),

        new("GeoGebra nima?",
            "GeoGebra va Desmos matematika uçun bepul dasturlar: formula yozilsa grafik çiziladi, parametrni özgartirib çizma qanday özgarişini kuzatiş mumkin. GeoGebrada geometrik yasaş va 3D şakllar ham bor.",
            ["geogebra", "desmos"], [],
            new AssistantLink("Matematika vositalari", "/vositalar?fan=matematika"),
            [Tools]),

        new("Power BI va Tableau nima?",
            "Bular ma’lumotlarni tahlil qilib, interaktiv diagramma va hisobotlar (dashboard) yaratadigan dasturlar. Oddiyroq diagramma uçun Excel yetarli. Power BI Desktop va Tableau Public bepul, lekin Tableau Public’dagi işlar oçiq e’lon qilinadi.",
            ["tableau", "powerbi", "excel", "dashboard"], ["power", "bi"],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            ["Qaysi diagramma kerak?"]),

        new("Simulyatsiya uçun qaysi dastur?",
            "Fizika, kimyo va biologiya uçun PhET interaktiv simulyatsiyalari bepul: öquvçi hodisani özi boşqarib kuzatadi. Matematikada GeoGebra şunday işlaydi.",
            ["phet", "simulyats", "laboratoriya"], [],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            [Tools]),

        new("Taqdimot uçun qaysi dastur?",
            "PowerPoint va Prezi taqdimot dasturlari. Tasvir.uz Studiyasidagi materialni ham “Darsda körsatiş” rejimida bosqiçma-bosqiç oçib körsatiş mumkin.",
            ["powerpoint", "prezi", "taqdimot", "slayd"], [],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            [Tools]),

        new("AR va VR bormi?",
            "Tasvir.uz hozir sxema, jadval va infografika kabi tekis (2D) şakllar bilan işlaydi, AR va VR yöq. 3D va VR uçun CoSpaces Edu, Tinkercad va BioDigital Human kabi vositalar bor, ular “Vositalar” sahifasida.",
            ["vr", "virtual", "3d", "augmented"], ["ar"],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            [Tools]),

        // Ro‘yxat oxirida: aniq dastur nomi bor savolda teng ball bo‘lsa, o‘sha dasturning javobi tanlanadi
        new(Tools,
            "Vizuallaştiriş uçun köplab dasturlar bor: infografika va dizayn uçun Canva, diagramma uçun Excel, Power BI va Tableau, sxema va aqliy xarita uçun draw.io, Miro va XMind, matematika uçun GeoGebra va Desmos, tabiiy fanlar uçun PhET simulyatsiyalari. Har birining vazifasi va qaysi fanga mosligi “Vositalar” sahifasida yozilgan.",
            ["dasturlar", "vositalar", "programma"], ["dastur", "vosita", "boshqa", "qanday"],
            new AssistantLink("Vositalar sahifasi", "/vositalar"),
            ["Canva nima?", "GeoGebra nima?"]),

        new("Sun’iy intellektdan foydalansam böladimi?",
            "Ha, tez tayyorlaş uçun foydali. Lekin u xato qilişi mumkin: ilmiy töğrilikni va öquvçilarga mosligini har doim özingiz tekşiring.",
            ["intellekt", "chatgpt", "gemini", "claude"], ["suniy"]),
    ];
}
