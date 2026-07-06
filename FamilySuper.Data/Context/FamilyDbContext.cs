using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.Data.Context;

public class FamilyDbContext : DbContext
{
    private readonly IModeManager _modeManager;

    public FamilyDbContext(DbContextOptions<FamilyDbContext> options, IModeManager modeManager) : base(options)
    {
        _modeManager = modeManager;
    }

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var currentMode = _modeManager.CurrentMode.ToString().ToLower();

        modelBuilder.Entity<FamilyMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Role).HasMaxLength(20);
            e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.IsDeleted);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<SystemConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Key).IsRequired().HasMaxLength(100);
            e.Property(x => x.Category).HasMaxLength(50);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Certificate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Number).HasMaxLength(50);
            e.Property(x => x.FileName).HasMaxLength(200);
            e.Property(x => x.Remark).HasMaxLength(500);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<WorkTask>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Category).HasMaxLength(50);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.Status);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<Survey>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasIndex(x => x.IsActive);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<SurveyResponse>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SurveyId);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        base.OnModelCreating(modelBuilder);
    }
}
