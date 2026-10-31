namespace Tasvir.Modules.Classroom.Application;

public sealed record RubricCriterion(string Key, string Title, string Description);
public sealed record SurveyQuestion(string Key, string Text);

/// <summary>Baholash vositalari. Rubrika dissertatsiyadagi 1.4-jadval mezonlari bilan bir xil: ularni birga o‘zgartiring.</summary>
public static class ClassroomCatalog
{
    public const int MaxCriterionScore = 3;
    public const int MinLikert = 1;
    public const int MaxLikert = 5;

    public static readonly IReadOnlyList<RubricCriterion> Rubric =
    [
        new("accuracy", "Ilmiy to‘g‘rilik", "Axborot to‘g‘ri, atamalar aniq qo‘llanilgan, soddalashtirish mazmunni buzmagan"),
        new("purpose", "Didaktik maqsadga muvofiqlik", "Material aniq o‘quv maqsadiga xizmat qiladi, ortiqcha elementlar yo‘q"),
        new("structure", "Tuzilmaning aniqligi", "Materialning mantiqiy tuzilishi va elementlar o‘rtasidagi aloqalar aniq ko‘rinadi"),
        new("form", "Vizual shaklning mosligi", "Tanlangan shakl (sxema, jadval, diagramma va boshq.) mazmun xususiyatiga mos"),
        new("load", "Kognitiv yuklama", "Bir vaqtda taqdim etilgan elementlar soni maqbul, muhim joylar ajratilgan"),
        new("design", "Vizual dizayn sifati", "Ierarxiya, tekislash, rang va shrift tizimi izchil"),
        new("technical", "Texnik sifat va qulaylik", "Turli qurilmalarda to‘g‘ri aks etadi, kontrast yetarli, matn o‘qiladi"),
        new("activation", "Interaktivlik va faollashtirish", "Material o‘quvchini faol bilish faoliyatiga undaydi"),
    ];

    public static int MaxTotalScore => Rubric.Count * MaxCriterionScore;

    public static readonly IReadOnlyList<SurveyQuestion> Survey =
    [
        new("usability", "Platforma interfeysi tushunarli va qulay edi."),
        new("learning", "Darslar vizual material yaratishni o‘rganishga yordam berdi."),
        new("creating", "Studiyada material yaratish oson edi."),
        new("transfer", "Yaratgan materiallarimni o‘z darslarimda ishlatgim keladi."),
        new("motivation", "Mashg‘ulotlar menda vizuallashtirishga qiziqish uyg‘otdi."),
        new("feedback", "O‘qituvchimning bahosi va mezonlar ishimni yaxshilashga yordam berdi."),
    ];
}
