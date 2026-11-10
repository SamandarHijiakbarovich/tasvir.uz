namespace Tasvir.Modules.Classroom.Application;

public sealed record DiagnosticOption(string Id, string Text);

/// <summary>Topic: kompetentlikning bilimlilik mezoni bo‘yicha qaysi bilim tekshirilishi (savollar tahlili uchun).</summary>
public sealed record DiagnosticQuestion(string Key, string Topic, string Text, IReadOnlyList<DiagnosticOption> Options, string CorrectOptionId);

/// <summary>
/// Boshlang‘ich va yakuniy diagnostika testi: dissertatsiyadagi bilimlilik mezoni bo‘yicha. Ikkala bosqichda bir xil
/// savollar beriladi, shuning uchun natijalarni to‘g‘ridan-to‘g‘ri solishtirish mumkin. Savolni o‘zgartirsangiz,
/// avval to‘plangan natijalar bilan solishtirib bo‘lmaydi.
/// </summary>
public static class DiagnosticCatalog
{
    public static string Title(string stage) => stage == Domain.DiagnosticStage.Pre ? "Boşlanğiç diagnostika" : "Yakuniy diagnostika";

    private static List<DiagnosticOption> Options(params string[] texts) =>
        texts.Select((text, index) => new DiagnosticOption(((char)('a' + index)).ToString(), text)).ToList();

    public static readonly IReadOnlyList<DiagnosticQuestion> Questions =
    [
        new("q1", "Nazariy asoslar",
            "Ikki kanalli kodlaş nazariyasi (A.Paivio) nimani asoslaydi?",
            Options(
                "Söz va tasvir birga berilganda axborot yaxşiroq esda qoladi",
                "Tasvir har doim matndan afzal, matnni qisqartiriş kerak",
                "Har bir öquvçi faqat bitta kanal orqali örganadi",
                "Rang qança köp bölsa, tuşuniş şunça oşadi"),
            "a"),
        new("q2", "Nazariy asoslar",
            "Kognitiv yuklama nazariyasiga köra, slaydga mavzuga aloqasiz bezak rasmlar qöşilsa nima böladi?",
            Options(
                "Öquvçi qiziqib, mavzuni yaxşiroq özlaştiradi",
                "Ortiqça yuklama paydo bölib, asosiy mazmunni özlaştiriş qiyinlaşadi",
                "Hech qanday ta’sir qilmaydi",
                "Işçi xotira hajmi kengayadi"),
            "b"),
        new("q3", "Nazariy asoslar",
            "Signal (urğu) tamoyili nimani talab qiladi?",
            Options(
                "Barça elementlarni bir xil rangda beriş",
                "Materialga ovozli signal qöşiş",
                "Muhim joylarni rang, ştrix yoki strelka bilan ajratib körsatiş",
                "Har bir slaydga kamida uçta rasm qöyiş"),
            "c"),
        new("q4", "Nazariy asoslar",
            "Segmentlaş tamoyili nimani anglatadi?",
            Options(
                "Murakkab jarayonni bosqiçlarga bölib, ketma-ket körsatiş",
                "Materialni bir neça faylga bölib saqlaş",
                "Har bir mavzuni alohida rangda beriş",
                "Matnni ikki ustunga bölib yoziş"),
            "a"),
        new("q5", "Vizual şakllar",
            "Tarixiy voqealarning ketma-ketligini körsatiş uçun qaysi şakl eng mos?",
            Options("Doiraviy diagramma", "Vaqt çiziği", "Aqliy xarita", "Qiyosiy jadval"),
            "b"),
        new("q6", "Vizual şakllar",
            "“Söz turkumlari” kabi tasnif va ierarxiyani körsatiş uçun qaysi şakl mos?",
            Options("Çiziqli grafik", "Vaqt çiziği", "Daraxtsimon sxema yoki aqliy xarita", "Doiraviy diagramma"),
            "c"),
        new("q7", "Vizual şakllar",
            "Ikki-uç obyektni bir neça belgi böyiça taqqoslaş uçun qaysi şakl qulay?",
            Options("Qiyosiy jadval", "Blok-sxema", "Vaqt çiziği", "Animatsiya"),
            "a"),
        new("q8", "Vizual şakllar",
            "Bir körsatkiçning yillar böyiça özgarişini körsatiş uçun qaysi diagramma mos?",
            Options("Doiraviy diagramma", "Aqliy xarita", "Çiziqli grafik", "Qiyosiy jadval"),
            "c"),
        new("q9", "Vizual şakllar",
            "Butunning qismlari uluşini (bölaklar kam bölganda) körsatiş uçun qaysi diagramma mos?",
            Options("Çiziqli grafik", "Doiraviy diagramma", "Vaqt çiziği", "Blok-sxema"),
            "b"),
        new("q10", "Dizayn tamoyillari",
            "Ustunli diagrammada qiymatlar öqi noldan boşlanmasa, qanday xato yuzaga keladi?",
            Options(
                "Diagramma çiroylilaşadi, xato yöq",
                "Ustunlar orasidagi farq haqiqatdagidan katta körinadi",
                "Diagramma umuman körinmaydi",
                "Ranglar almaşib ketadi"),
            "b"),
        new("q11", "Dizayn tamoyillari",
            "Axborotni faqat rang bilan bildirmaslik kerak. Nega?",
            Options(
                "Ranglar çop etişda qimmat",
                "Rangni farqlay olmaydigan öquvçilar ham tuşunişi uçun",
                "Rangli material darsga mos emas",
                "Ranglar faqat sarlavhada işlatiladi"),
            "b"),
        new("q12", "Dizayn tamoyillari",
            "Vizual materialda matn va fon orasidagi kontrast nima uçun muhim?",
            Options(
                "Matn turli ekran va yoritiş şaroitida öqilişi uçun",
                "Material kamroq joy egallaşi uçun",
                "Dastur tezroq işlaşi uçun",
                "Kontrast muhim emas, faqat şrift muhim"),
            "a"),
        new("q13", "Dasturiy vositalar",
            "Funksiya grafigini parametr özgarişi bilan dinamik körsatiş uçun qaysi dastur mos?",
            Options("Canva", "Powtoon", "GeoGebra", "XMind"),
            "c"),
        new("q14", "Dasturiy vositalar",
            "Tayyor şablonlardan infografika va plakat yasaş uçun möljallangan onlayn dastur qaysi?",
            Options("Canva", "Flowgorithm", "Desmos", "PhET"),
            "a"),
        new("q15", "Loyihalaş",
            "Öquv matnini vizuallaştirişda birinçi qadam qaysi?",
            Options(
                "Dasturni örnatiş",
                "Mazmundagi boğliqlik turini aniqlaş: ketma-ketlik, ierarxiya yoki taqqoslaş",
                "Rang va şriftni tanlaş",
                "Tayyor rasmlarni qidiriş"),
            "b"),
    ];

    public static int MaxScore => Questions.Count;
}
