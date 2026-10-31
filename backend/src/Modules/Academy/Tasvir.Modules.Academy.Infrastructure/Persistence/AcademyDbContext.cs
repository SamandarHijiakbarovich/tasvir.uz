using Microsoft.EntityFrameworkCore;
using Tasvir.Modules.Academy.Application;
using Tasvir.Modules.Academy.Domain;

namespace Tasvir.Modules.Academy.Infrastructure.Persistence;

public sealed class AcademyDbContext(DbContextOptions<AcademyDbContext> options) : DbContext(options)
{
    public const string Schema = "academy";

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
    public DbSet<StepCompletion> StepCompletions => Set<StepCompletion>();
    public DbSet<StepAttempt> StepAttempts => Set<StepAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.Entity<Course>(course =>
        {
            course.ToTable("courses");
            course.Property(c => c.Id).ValueGeneratedNever();
            course.Property(c => c.Slug).HasMaxLength(100).IsRequired();
            course.Property(c => c.Title).HasMaxLength(200).IsRequired();
            course.Property(c => c.Description).HasMaxLength(1000).IsRequired();
            course.HasIndex(c => c.Slug).IsUnique();
            course.HasMany(c => c.Lessons).WithOne().HasForeignKey(l => l.CourseId).OnDelete(DeleteBehavior.Cascade);
            course.Navigation(c => c.Lessons).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Lesson>(lesson =>
        {
            lesson.ToTable("lessons");
            lesson.Property(l => l.Id).ValueGeneratedNever();
            lesson.Property(l => l.Slug).HasMaxLength(100).IsRequired();
            lesson.Property(l => l.Title).HasMaxLength(200).IsRequired();
            lesson.Property(l => l.Summary).HasMaxLength(500).IsRequired();
            lesson.HasIndex(l => l.Slug).IsUnique();
            lesson.HasMany(l => l.Steps).WithOne().HasForeignKey(s => s.LessonId).OnDelete(DeleteBehavior.Cascade);
            lesson.Navigation(l => l.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<LessonStep>(step =>
        {
            step.ToTable("lesson_steps");
            step.Property(s => s.Id).ValueGeneratedNever();
            step.Property(s => s.Title).HasMaxLength(200).IsRequired();
            step.Property(s => s.Kind).HasConversion<int>();
            step.Property(s => s.ContentJson).IsRequired();
        });

        modelBuilder.Entity<LessonProgress>(progress =>
        {
            progress.ToTable("lesson_progress");
            progress.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        });

        modelBuilder.Entity<StepCompletion>(completion =>
        {
            completion.ToTable("step_completions");
            completion.HasIndex(c => new { c.UserId, c.StepId }).IsUnique();
        });

        modelBuilder.Entity<StepAttempt>(attempt =>
        {
            attempt.ToTable("step_attempts");
            attempt.Property(a => a.Answer).HasMaxLength(StepAttempt.MaxAnswerLength).IsRequired();
            attempt.HasIndex(a => new { a.UserId, a.StepId });
        });
    }
}

internal sealed class AcademyRepository(AcademyDbContext db) : IAcademyRepository
{
    private IQueryable<Course> Courses => db.Courses
        .AsSplitQuery()
        .Include(c => c.Lessons.OrderBy(l => l.SortOrder))
        .ThenInclude(l => l.Steps.OrderBy(s => s.SortOrder));

    public Task<Course?> FindCourseBySlugAsync(string slug, CancellationToken ct) =>
        Courses.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public Task<Course?> FindCourseByLessonSlugAsync(string lessonSlug, CancellationToken ct) =>
        Courses.FirstOrDefaultAsync(c => c.Lessons.Any(l => l.Slug == lessonSlug), ct);

    public Task<List<Course>> GetCoursesAsync(CancellationToken ct) => Courses.OrderBy(c => c.Slug).ToListAsync(ct);

    public Task<List<LessonProgress>> GetLessonProgressAsync(Guid userId, IReadOnlyCollection<Guid> lessonIds, CancellationToken ct) =>
        db.LessonProgress.Where(p => p.UserId == userId && lessonIds.Contains(p.LessonId)).ToListAsync(ct);

    public Task<List<StepCompletion>> GetStepCompletionsAsync(Guid userId, IReadOnlyCollection<Guid> stepIds, CancellationToken ct) =>
        db.StepCompletions.Where(c => c.UserId == userId && stepIds.Contains(c.StepId)).ToListAsync(ct);

    public void Add(LessonProgress progress) => db.LessonProgress.Add(progress);

    public void Add(StepCompletion completion) => db.StepCompletions.Add(completion);

    public void Add(StepAttempt attempt) => db.StepAttempts.Add(attempt);

    public Task<List<StepCompletion>> GetCompletionsOfUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct) =>
        db.StepCompletions.AsNoTracking().Where(c => userIds.Contains(c.UserId)).ToListAsync(ct);

    public Task<List<StepAttempt>> GetAttemptsOfUsersAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct) =>
        db.StepAttempts.AsNoTracking().Where(a => userIds.Contains(a.UserId)).ToListAsync(ct);

    public async Task<bool> TrySaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // Noyob indeks ikkinchi nusxani ushlab qoldi: birinchi so‘rov allaqachon yozgan
            db.ChangeTracker.Clear();
            return false;
        }
    }
}
