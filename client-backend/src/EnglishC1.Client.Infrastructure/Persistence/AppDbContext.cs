using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EnglishC1.Client.Domain.PlacementTest;
using EnglishC1.Client.Infrastructure.Identity;

namespace EnglishC1.Client.Infrastructure.Persistence;

// IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> brings in
// the Identity tables (AspNetUsers, AspNetRoles, etc.). The placement
// test tables were added 2026-08-27 - see docs/PENDING_IDEAS.md for what
// still isn't scoped beyond it (reading/listening skills, adaptive
// scoring, etc.).
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<TestAttempt> TestAttempts => Set<TestAttempt>();
    public DbSet<TestAnswer> TestAnswers => Set<TestAnswer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Question>(entity =>
        {
            entity.Property(q => q.Text).HasMaxLength(500).IsRequired();
            entity.Property(q => q.Explanation).HasMaxLength(500);
            // Explicit default so adding this column to the already-populated
            // Questions table (64 rows, shared dev+prod DB) doesn't need a
            // separate backfill step - every existing row becomes false.
            entity.Property(q => q.IsAiGenerated).HasDefaultValue(false);
            entity.HasMany(q => q.Options)
                .WithOne()
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.Property(o => o.Text).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<TestAttempt>(entity =>
        {
            entity.HasMany(a => a.Answers)
                .WithOne()
                .HasForeignKey(a => a.TestAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
