namespace Tasvir.Modules.Classroom.Application;

public sealed record RubricCriterion(string Key, string Title, string Description);
public sealed record SurveyQuestion(string Key, string Text);

/// <summary>Baholash vositalari. Rubrika dissertatsiyadagi 2.1.2-jadval mezonlari bilan bir xil: ularni birga o‘zgartiring.</summary>
public static class ClassroomCatalog
{
    public const int MaxCriterionScore = 3;
    public const int MinLikert = 1;
    public const int MaxLikert = 5;

    public static readonly IReadOnlyList<RubricCriterion> Rubric =
    [
        new("accuracy", "Ilmiy töğrilik", "Axborot töğri, atamalar aniq qöllanilgan, soddalaştiriş mazmunni buzmagan"),
        new("purpose", "Didaktik maqsadga muvofiqlik", "Material aniq öquv maqsadiga xizmat qiladi, ortiqça elementlar yöq"),
        new("structure", "Tuzilmaning aniqligi", "Materialning mantiqiy tuzilişi va elementlar örtasidagi aloqalar aniq körinadi"),
        new("form", "Vizual şaklning mosligi", "Tanlangan şakl (sxema, jadval, diagramma va boşq.) mazmun xususiyatiga mos"),
        new("load", "Kognitiv yuklama", "Bir vaqtda taqdim etilgan elementlar soni maqbul, muhim joylar ajratilgan"),
        new("design", "Vizual dizayn sifati", "Ierarxiya, tekislaş, rang va şrift tizimi izçil"),
        new("technical", "Texnik sifat va qulaylik", "Turli qurilmalarda töğri aks etadi, kontrast yetarli, matn öqiladi"),
        new("activation", "Interaktivlik va faollaştiriş", "Material öquvçini faol biliş faoliyatiga undaydi"),
    ];

    public static int MaxTotalScore => Rubric.Count * MaxCriterionScore;

    public static readonly IReadOnlyList<SurveyQuestion> Survey =
    [
        new("usability", "Platforma interfeysi tuşunarli va qulay edi."),
        new("learning", "Darslar vizual material yaratişni örganişga yordam berdi."),
        new("creating", "Studiyada material yaratiş oson edi."),
        new("transfer", "Yaratgan materiallarimni öz darslarimda işlatgim keladi."),
        new("motivation", "Maşğulotlar menda vizuallaştirişga qiziqiş uyğotdi."),
        new("feedback", "Öqituvçimning bahosi va mezonlar işimni yaxşilaşga yordam berdi."),
    ];
}
