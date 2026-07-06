using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.Data.Context;

public class HealthDbContext : DbContext
{
    private readonly IModeManager _modeManager;

    public HealthDbContext(DbContextOptions<HealthDbContext> options, IModeManager modeManager) : base(options)
    {
        _modeManager = modeManager;
    }

    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<MedicationPlan> MedicationPlans => Set<MedicationPlan>();
    public DbSet<MedicationRecord> MedicationRecords => Set<MedicationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var currentMode = _modeManager.CurrentMode.ToString().ToLower();

        modelBuilder.Entity<HealthRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.RecordType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Content).HasMaxLength(int.MaxValue);
            e.Property(x => x.Doctor).HasMaxLength(50);
            e.Property(x => x.Hospital).HasMaxLength(100);
            e.Property(x => x.AttachmentPath).HasMaxLength(500);
            e.Property(x => x.Remark).HasMaxLength(1000);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.RecordDate);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<MedicationPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DrugName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Dosage).HasMaxLength(50);
            e.Property(x => x.Frequency).HasMaxLength(100);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.StartDate);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<MedicationRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.Property(x => x.Note).HasMaxLength(500);
            e.HasIndex(x => x.MedicationPlanId);
            e.HasIndex(x => x.TakenAt);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        base.OnModelCreating(modelBuilder);
    }
}
