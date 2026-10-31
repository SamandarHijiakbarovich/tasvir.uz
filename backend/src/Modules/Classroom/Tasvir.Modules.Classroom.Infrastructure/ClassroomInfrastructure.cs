using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Tasvir.Modules.Classroom.Application;
using Tasvir.Modules.Classroom.Domain;

namespace Tasvir.Modules.Classroom.Infrastructure;

public sealed class ClassroomDbContext(DbContextOptions<ClassroomDbContext> options) : DbContext(options)
{
    public const string Schema = "classroom";

    public DbSet<StudyGroup> Groups => Set<StudyGroup>();
    public DbSet<GroupMember> Members => Set<GroupMember>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionReview> Reviews => Set<SubmissionReview>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite sxemalarni qo‘llab-quvvatlamaydi
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema(Schema);
        }

        modelBuilder.Entity<StudyGroup>(group =>
        {
            group.ToTable("study_groups");
            group.Property(g => g.Id).ValueGeneratedNever();
            group.Property(g => g.Name).HasMaxLength(80).IsRequired();
            group.Property(g => g.OwnerName).HasMaxLength(100).IsRequired();
            group.Property(g => g.JoinCode).HasMaxLength(8).IsRequired();
            group.HasIndex(g => g.JoinCode).IsUnique();
            group.HasIndex(g => g.OwnerUserId);
        });

        modelBuilder.Entity<GroupMember>(member =>
        {
            member.ToTable("group_members");
            member.Property(m => m.Id).ValueGeneratedNever();
            member.Property(m => m.DisplayName).HasMaxLength(100).IsRequired();
            member.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();
            member.HasIndex(m => m.UserId);
            member.HasOne<StudyGroup>().WithMany().HasForeignKey(m => m.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Submission>(submission =>
        {
            submission.ToTable("submissions");
            submission.Property(s => s.Id).ValueGeneratedNever();
            submission.Property(s => s.TemplateKey).HasMaxLength(40).IsRequired();
            submission.Property(s => s.Title).HasMaxLength(120).IsRequired();
            submission.Property(s => s.Subject).HasMaxLength(40);
            submission.Property(s => s.ContentJson).IsRequired();
            submission.HasIndex(s => new { s.GroupId, s.UserId, s.MaterialId }).IsUnique();
            submission.HasOne<StudyGroup>().WithMany().HasForeignKey(s => s.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SubmissionReview>(review =>
        {
            review.ToTable("submission_reviews");
            review.Property(r => r.Id).ValueGeneratedNever();
            review.Property(r => r.ScoresJson).IsRequired();
            review.Property(r => r.Comment).HasMaxLength(1000);
            review.HasIndex(r => new { r.SubmissionId, r.Version }).IsUnique();
            review.HasOne<Submission>().WithMany().HasForeignKey(r => r.SubmissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SurveyResponse>(response =>
        {
            response.ToTable("survey_responses");
            response.Property(r => r.Id).ValueGeneratedNever();
            response.Property(r => r.AnswersJson).IsRequired();
            response.Property(r => r.Comment).HasMaxLength(1000);
            response.HasIndex(r => new { r.GroupId, r.UserId }).IsUnique();
            response.HasOne<StudyGroup>().WithMany().HasForeignKey(r => r.GroupId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}

internal sealed class ClassroomRepository(ClassroomDbContext db) : IClassroomRepository
{
    public Task<List<StudyGroup>> ListOwnedAsync(Guid userId, CancellationToken ct) =>
        db.Groups.AsNoTracking().Where(g => g.OwnerUserId == userId).OrderByDescending(g => g.CreatedAt).ToListAsync(ct);

    public Task<List<StudyGroup>> ListJoinedAsync(Guid userId, CancellationToken ct) =>
        db.Groups.AsNoTracking()
            .Where(g => db.Members.Any(m => m.GroupId == g.Id && m.UserId == userId))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync(ct);

    public Task<StudyGroup?> FindGroupAsync(Guid id, CancellationToken ct) =>
        db.Groups.FirstOrDefaultAsync(g => g.Id == id, ct);

    public Task<StudyGroup?> FindGroupByCodeAsync(string code, CancellationToken ct) =>
        db.Groups.FirstOrDefaultAsync(g => g.JoinCode == code, ct);

    public Task<int> CountOwnedAsync(Guid userId, CancellationToken ct) =>
        db.Groups.CountAsync(g => g.OwnerUserId == userId, ct);

    public Task<Dictionary<Guid, int>> CountMembersAsync(IReadOnlyCollection<Guid> groupIds, CancellationToken ct) =>
        db.Members.Where(m => groupIds.Contains(m.GroupId))
            .GroupBy(m => m.GroupId)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);

    public async Task<List<GroupMember>> ListMembersAsync(Guid groupId, CancellationToken ct)
    {
        var members = await db.Members.AsNoTracking().Where(m => m.GroupId == groupId).ToListAsync(ct);
        return members.OrderBy(m => m.DisplayName, StringComparer.CurrentCulture).ToList();
    }

    public Task<GroupMember?> FindMemberAsync(Guid groupId, Guid userId, CancellationToken ct) =>
        db.Members.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

    public async Task<List<Submission>> ListSubmissionsAsync(Guid groupId, CancellationToken ct)
    {
        var submissions = await db.Submissions.AsNoTracking().Where(s => s.GroupId == groupId).ToListAsync(ct);
        return submissions.OrderByDescending(s => s.SubmittedAt).ToList();
    }

    public async Task<List<Submission>> ListSubmissionsOfAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        var submissions = await db.Submissions.AsNoTracking().Where(s => s.GroupId == groupId && s.UserId == userId).ToListAsync(ct);
        return submissions.OrderByDescending(s => s.SubmittedAt).ToList();
    }

    public Task<Submission?> FindSubmissionAsync(Guid id, CancellationToken ct) =>
        db.Submissions.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<Submission?> FindSubmissionAsync(Guid groupId, Guid userId, Guid materialId, CancellationToken ct) =>
        db.Submissions.FirstOrDefaultAsync(s => s.GroupId == groupId && s.UserId == userId && s.MaterialId == materialId, ct);

    public Task<List<SubmissionReview>> ListReviewsAsync(IReadOnlyCollection<Guid> submissionIds, CancellationToken ct) =>
        db.Reviews.AsNoTracking().Where(r => submissionIds.Contains(r.SubmissionId)).ToListAsync(ct);

    public Task<SubmissionReview?> FindReviewAsync(Guid submissionId, int version, CancellationToken ct) =>
        db.Reviews.FirstOrDefaultAsync(r => r.SubmissionId == submissionId && r.Version == version, ct);

    public Task<List<SurveyResponse>> ListSurveyResponsesAsync(Guid groupId, CancellationToken ct) =>
        db.SurveyResponses.AsNoTracking().Where(r => r.GroupId == groupId).ToListAsync(ct);

    public Task<SurveyResponse?> FindSurveyResponseAsync(Guid groupId, Guid userId, CancellationToken ct) =>
        db.SurveyResponses.FirstOrDefaultAsync(r => r.GroupId == groupId && r.UserId == userId, ct);

    public void Add(StudyGroup group) => db.Groups.Add(group);

    public void Add(GroupMember member) => db.Members.Add(member);

    public void Add(Submission submission) => db.Submissions.Add(submission);

    public void Add(SubmissionReview review) => db.Reviews.Add(review);

    public void Add(SurveyResponse response) => db.SurveyResponses.Add(response);

    public void Remove(GroupMember member) => db.Members.Remove(member);

    public async Task RemoveGroupAsync(StudyGroup group, CancellationToken ct)
    {
        db.Groups.Remove(group);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberDataAsync(Guid groupId, Guid userId, CancellationToken ct)
    {
        await db.Submissions.Where(s => s.GroupId == groupId && s.UserId == userId).ExecuteDeleteAsync(ct);
        await db.SurveyResponses.Where(r => r.GroupId == groupId && r.UserId == userId).ExecuteDeleteAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public async Task<bool> TrySaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
            return false;
        }
    }
}

public static class DependencyInjection
{
    /// <summary>Sozlamalar: "ConnectionStrings:Classroom", "Database:Provider".</summary>
    public static IServiceCollection AddClassroomInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Classroom")
            ?? throw new InvalidOperationException("ConnectionStrings:Classroom topilmadi.");

        services.AddDbContext<ClassroomDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "postgres":
                    options.UseNpgsql(connectionString,
                        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations", ClassroomDbContext.Schema));
                    break;
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Noma’lum baza provayderi: {provider}");
            }
        });

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IClassroomRepository, ClassroomRepository>();
        return services;
    }

    public static async Task InitializeClassroomDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();

        // TODO: ishlab chiqarishga chiqishdan oldin EnsureCreated o‘rniga EF Core migratsiyalariga o‘tish.
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
