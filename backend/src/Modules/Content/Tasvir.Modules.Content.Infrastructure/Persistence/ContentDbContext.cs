using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Content.Domain;

namespace Tasvir.Modules.Content.Infrastructure.Persistence;

/// <summary>
/// Content modulining o‘z bazasi (kontekst). Boshqa modullar bu jadvallarga to‘g‘ridan-to‘g‘ri murojaat qilmaydi —
/// shu sababli modul keyinchalik alohida mikroservisga osongina ajratiladi.
/// </summary>
public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
{
    public const string Schema = "content";

    public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<ResultCriterion> ResultCriteria => Set<ResultCriterion>();
    public DbSet<ResultStat> ResultStats => Set<ResultStat>();
    public DbSet<Partner> Partners => Set<Partner>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi; PostgreSQL da modul jadvallari "content" sxemasida turadi.
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly);
    }
}
