using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.Data.Context;

public class EducationDbContext : DbContext
{
    public EducationDbContext(DbContextOptions<EducationDbContext> options) : base(options)
    {
    }

    public DbSet<EducationRecord> EducationRecords => Set<EducationRecord>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyLog> StudyLogs => Set<StudyLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EducationRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Subject).HasMaxLength(30);
            e.Property(x => x.Question).IsRequired().HasMaxLength(int.MaxValue);
            e.Property(x => x.Answer).HasMaxLength(int.MaxValue);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.RecordDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<StudyPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Subject).IsRequired().HasMaxLength(30);
            e.Property(x => x.Goal).HasMaxLength(500);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.StartDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<StudyLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Completion).HasMaxLength(20);
            e.Property(x => x.Content).HasMaxLength(int.MaxValue);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.StudyPlanId);
            e.HasIndex(x => x.StudyDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        base.OnModelCreating(modelBuilder);
    }
}
