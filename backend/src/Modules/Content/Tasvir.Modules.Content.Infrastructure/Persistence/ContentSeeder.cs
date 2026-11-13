using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Content.Domain;

namespace Tasvir.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Boshlang‘ich ma’lumotlar (dizayn handoffidagi matnlar). Raqamlar, narxlar, sharhlar va hamkorlar
/// real ma’lumot kelguncha bo‘sh (null) yoki namuna belgisi bilan qoladi — o‘ylab topilgan qiymat yozilmaydi.
/// </summary>
internal static class ContentSeeder
{
    private const string ScientificBasisQuestion = "Platformaning ilmiy asosi nimada?";
    private const string ScientificBasisAnswer = "Metodika pedagogika fanlari böyiça dissertatsiya tadqiqoti doirasida işlab çiqilgan. Tajriba-sinov natijalari yakunlangaç e’lon qilinadi.";
    /// <summary>Pro tarifi tugmasi tölov sahifasiga olib boradi (Payments moduli).</summary>
    private const string ProPlanHref = "/tariflar";
    private const string AcademyVsStudioQuestion = "Akademiya va Studiyaning farqi nimada?";
    private const string AcademyVsStudioAnswer = "Akademiya — örgatadi: kurslar va amaliy topşiriqlar. Studiya — yaratadi: tayyor şablon asosida mavzuingizni infografika yoki sxemaga aylantiradi.";

    public static async Task SeedAsync(ContentDbContext db, CancellationToken cancellationToken)
    {
        if (await db.PricingPlans.AnyAsync(cancellationToken))
        {
            // Mavjud bazalardagi tasdiqlanmagan da’volarni ham tuzatadi: tajriba-sinov hali o‘tkazilmagan, sertifikat hali yo‘q
            await db.FaqItems.Where(f => f.Question == ScientificBasisQuestion)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.Answer, ScientificBasisAnswer), cancellationToken);
            await db.FaqItems.Where(f => f.Question == AcademyVsStudioQuestion)
                .ExecuteUpdateAsync(s => s.SetProperty(f => f.Answer, AcademyVsStudioAnswer), cancellationToken);
            await db.PricingPlans.Where(p => p.Name == "Pro" && p.CtaHref == "/royxatdan-otish")
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.CtaHref, ProPlanHref), cancellationToken);
            return;
        }

        db.Partners.AddRange(
            new Partner("Hamkor OTM logotipi", null, 1),
            new Partner("Hamkor maktab logotipi", null, 2),
            new Partner("Hamkor logotipi", null, 3),
            new Partner("Hamkor logotipi", null, 4));

        db.ResultCriteria.AddRange(
            new ResultCriterion("[1-mezon]", null, null, 1),
            new ResultCriterion("[2-mezon]", null, null, 2),
            new ResultCriterion("[3-mezon]", null, null, 3),
            new ResultCriterion("[4-mezon]", null, null, 4));

        db.ResultStats.AddRange(
            new ResultStat("[Körsatkiç izohi]", null, "%", 1),
            new ResultStat("[Körsatkiç izohi]", null, "%", 2),
            new ResultStat("Tajriba-sinov iştirokçilari", null, null, 3),
            new ResultStat("Iştirokçi ta’lim muassasalari", null, null, 4));

        db.PricingPlans.AddRange(
            new PricingPlan(
                "Bepul", "Platformani sinab köriş uçun", 0m, "söm",
                ["Akademiyadan kiriş darslari", "Studiyada asosiy şablonlar", "Hamjamiyatga kiriş"],
                isFeatured: false, "Bepul boşlaş", "/royxatdan-otish", 1),
            new PricingPlan(
                "Pro", "Öqituvçi uçun", null, "söm / oy",
                ["Barça kurslar va amaliy topşiriqlar", "Barça şablonlar", "Sertifikat", "Progress kuzatuvi"],
                isFeatured: true, "Pro ni tanlaş", ProPlanHref, 2),
            new PricingPlan(
                "Taşkilot", "Maktab, kollej va OTM uçun", null, "söm",
                ["Pro dagi barça imkoniyatlar", "Öqituvçilar jamoasi uçun hisob", "Jamoa progressini kuzatiş"],
                isFeatured: false, "Boğlaniş", "#boglanish", 3));

        db.Testimonials.AddRange(
            new Testimonial("[Öqituvçi şarhi şu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Fan öqituvçisi, maktab]", null, isVerified: false, 1),
            new Testimonial("[Talaba şarhi şu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Talaba, OTM nomi]", null, isVerified: false, 2),
            new Testimonial("[Muassasa vakili şarhi şu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Lavozim, muassasa]", null, isVerified: false, 3));

        db.FaqItems.AddRange(
            new FaqItem("Foydalaniş uçun dizayn könikmasi kerakmi?",
                "Yöq. Akademiya kurslari vizual material yaratişni noldan, bosqiçma-bosqiç örgatadi. Studiyadagi tayyor şablonlar esa işni yanada osonlaştiradi.", 1),
            new FaqItem(AcademyVsStudioQuestion,
                AcademyVsStudioAnswer, 2),
            new FaqItem("Qaysi fanlar uçun mos?",
                "Galereyada matematika, biologiya, tarix, informatika va ona tili böyiça namunalar bor. Metodika har qanday fan mavzusini vizual şaklga keltirişga yönaltirilgan.", 3),
            new FaqItem("Sertifikat beriladimi?",
                "Sertifikat hozirça işlab çiqilmoqda. Kursdagi natijalaringiz saqlanadi, sertifikat işga tuşgaç e’lon qilamiz.", 4),
            new FaqItem(ScientificBasisQuestion,
                ScientificBasisAnswer, 5),
            new FaqItem("Maktab yoki OTM uçun alohida tarif bormi?",
                "Ha, “Taşkilot” tarifi ta’lim muassasalari uçun möljallangan. Şartlar böyiça biz bilan boğlaning.", 6));

        await db.SaveChangesAsync(cancellationToken);
    }
}
