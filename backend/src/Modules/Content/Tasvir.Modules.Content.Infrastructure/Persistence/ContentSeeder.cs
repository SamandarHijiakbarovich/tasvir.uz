using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Content.Domain;

namespace Tasvir.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Boshlang‘ich ma’lumotlar (dizayn handoffidagi matnlar). Raqamlar, narxlar, sharhlar va hamkorlar
/// real ma’lumot kelguncha bo‘sh (null) yoki namuna belgisi bilan qoladi — o‘ylab topilgan qiymat yozilmaydi.
/// </summary>
internal static class ContentSeeder
{
    public static async Task SeedAsync(ContentDbContext db, CancellationToken cancellationToken)
    {
        if (await db.PricingPlans.AnyAsync(cancellationToken))
        {
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
            new ResultStat("[Ko‘rsatkich izohi]", null, "%", 1),
            new ResultStat("[Ko‘rsatkich izohi]", null, "%", 2),
            new ResultStat("Tajriba-sinov ishtirokchilari", null, null, 3),
            new ResultStat("Ishtirokchi ta’lim muassasalari", null, null, 4));

        db.PricingPlans.AddRange(
            new PricingPlan(
                "Bepul", "Platformani sinab ko‘rish uchun", 0m, "so‘m",
                ["Akademiyadan kirish darslari", "Studiyada asosiy shablonlar", "Hamjamiyatga kirish"],
                isFeatured: false, "Bepul boshlash", "#boshlash", 1),
            new PricingPlan(
                "Pro", "O‘qituvchi uchun", null, "so‘m / oy",
                ["Barcha kurslar va amaliy topshiriqlar", "Barcha shablonlar", "Sertifikat", "Progress kuzatuvi"],
                isFeatured: true, "Pro ni tanlash", "#pro", 2),
            new PricingPlan(
                "Tashkilot", "Maktab, kollej va OTM uchun", null, "so‘m",
                ["Pro dagi barcha imkoniyatlar", "O‘qituvchilar jamoasi uchun hisob", "Jamoa progressini kuzatish"],
                isFeatured: false, "Bog‘lanish", "#boglanish", 3));

        db.Testimonials.AddRange(
            new Testimonial("[O‘qituvchi sharhi shu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Fan o‘qituvchisi, maktab]", null, isVerified: false, 1),
            new Testimonial("[Talaba sharhi shu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Talaba, OTM nomi]", null, isVerified: false, 2),
            new Testimonial("[Muassasa vakili sharhi shu yerga joylanadi — real fikr, egasining ruxsati bilan.]",
                "[Ism Familiya]", "[Lavozim, muassasa]", null, isVerified: false, 3));

        db.FaqItems.AddRange(
            new FaqItem("Foydalanish uchun dizayn ko‘nikmasi kerakmi?",
                "Yo‘q. Akademiya kurslari vizual material yaratishni noldan, bosqichma-bosqich o‘rgatadi. Studiyadagi tayyor shablonlar esa ishni yanada osonlashtiradi.", 1),
            new FaqItem("Akademiya va Studiyaning farqi nimada?",
                "Akademiya — o‘rgatadi: kurslar, amaliy topshiriqlar va sertifikat. Studiya — yaratadi: tayyor shablon asosida mavzuingizni infografika yoki sxemaga aylantiradi.", 2),
            new FaqItem("Qaysi fanlar uchun mos?",
                "Galereyada matematika, biologiya, tarix, informatika va ona tili bo‘yicha namunalar bor. Metodika har qanday fan mavzusini vizual shaklga keltirishga yo‘naltirilgan.", 3),
            new FaqItem("Sertifikat beriladimi?",
                "Ha, Akademiya kursini yakunlaganingizdan so‘ng sertifikat olasiz.", 4),
            new FaqItem("Platformaning ilmiy asosi nimada?",
                "Metodika pedagogika fanlari bo‘yicha dissertatsiya tadqiqoti doirasida ishlab chiqilgan va tajriba-sinovdan o‘tkazilgan. Batafsil — “Tadqiqot” sahifasida.", 5),
            new FaqItem("Maktab yoki OTM uchun alohida tarif bormi?",
                "Ha, “Tashkilot” tarifi ta’lim muassasalari uchun mo‘ljallangan. Shartlar bo‘yicha biz bilan bog‘laning.", 6));

        await db.SaveChangesAsync(cancellationToken);
    }
}
