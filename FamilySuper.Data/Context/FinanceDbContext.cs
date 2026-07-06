using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.Data.Context;

public class FinanceDbContext : DbContext
{
    private readonly IModeManager _modeManager;

    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, IModeManager modeManager) : base(options)
    {
        _modeManager = modeManager;
    }

    public DbSet<FinanceRecord> FinanceRecords => Set<FinanceRecord>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BillReminder> BillReminders => Set<BillReminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FinanceRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(100);
            e.Property(x => x.Type).HasMaxLength(20);
            e.Property(x => x.Category).HasMaxLength(50);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == _modeManager.CurrentMode.ToString().ToLower());
        });

        modelBuilder.Entity<Budget>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => new { x.Year, x.Month });
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == _modeManager.CurrentMode.ToString().ToLower());
        });

        modelBuilder.Entity<BillReminder>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Notes).HasMaxLength(500);
            e.HasIndex(x => x.NextDueDate);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == _modeManager.CurrentMode.ToString().ToLower());
        });

        base.OnModelCreating(modelBuilder);
    }
}
