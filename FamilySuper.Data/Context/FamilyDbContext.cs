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
    public DbSet<ShoppingItem> ShoppingItems => Set<ShoppingItem>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();
    public DbSet<EntertainmentRecord> EntertainmentRecords => Set<EntertainmentRecord>();
    public DbSet<CareReminder> CareReminders => Set<CareReminder>();
    public DbSet<HomeItem> HomeItems => Set<HomeItem>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<MedicalGuide> MedicalGuides => Set<MedicalGuide>();
    public DbSet<MeetingNote> MeetingNotes => Set<MeetingNote>();
    public DbSet<PriceCompare> PriceCompares => Set<PriceCompare>();
    public DbSet<VirtualScene> VirtualScenes => Set<VirtualScene>();
    public DbSet<Annotation> Annotations => Set<Annotation>();
    public DbSet<PhotoTimeline> PhotoTimelines => Set<PhotoTimeline>();

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
            e.HasIndex(x => x.FatherId);
            e.HasIndex(x => x.MotherId);
            e.HasQueryFilter(x => !x.IsDeleted);
            e.HasOne(x => x.Father)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.FatherId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Mother)
                .WithMany()
                .HasForeignKey(x => x.MotherId)
                .OnDelete(DeleteBehavior.Restrict);
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

        modelBuilder.Entity<ShoppingItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<EmergencyContact>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.Property(x => x.Relationship).HasMaxLength(30);
            e.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
            e.Property(x => x.AlternatePhone).HasMaxLength(30);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<HealthMetric>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.RecordDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<EntertainmentRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(30);
            e.Property(x => x.Type).HasMaxLength(50);
            e.Property(x => x.Source).HasMaxLength(100);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<CareReminder>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Content).HasMaxLength(1000);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.IsCompleted);
            e.HasIndex(x => x.RemindDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<HomeItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.Location).HasMaxLength(100);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.Brand).HasMaxLength(100);
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<MediaItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            e.Property(x => x.MediaType).HasMaxLength(20);
            e.Property(x => x.Category).HasMaxLength(50);
            e.HasIndex(x => x.MediaType);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<MedicalGuide>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.HospitalName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Department).HasMaxLength(100);
            e.Property(x => x.DoctorName).HasMaxLength(50);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.Category).HasMaxLength(50);
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.Department);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<MeetingNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).HasMaxLength(20);
            e.Property(x => x.Location).HasMaxLength(200);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.MeetingDate);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PriceCompare>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(50);
            e.Property(x => x.StoreName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Unit).HasMaxLength(20);
            e.HasIndex(x => x.ProductName);
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<VirtualScene>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Location).IsRequired().HasMaxLength(300);
            e.Property(x => x.SceneType).HasMaxLength(30);
            e.HasIndex(x => x.SceneType);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Annotation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Text).IsRequired().HasMaxLength(500);
            e.Property(x => x.Color).HasMaxLength(20);
            e.Property(x => x.ImageUrl).HasMaxLength(500);
            e.Property(x => x.RelatedMediaPath).HasMaxLength(500);
            e.HasIndex(x => x.SceneId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PhotoTimeline>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.FilePath).IsRequired().HasMaxLength(500);
            e.Property(x => x.ThumbnailPath).HasMaxLength(500);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Location).HasMaxLength(300);
            e.Property(x => x.PeopleTags).HasMaxLength(200);
            e.Property(x => x.SceneTags).HasMaxLength(200);
            e.Property(x => x.Category).HasMaxLength(30);
            e.HasIndex(x => x.Category);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.TakenDate);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        base.OnModelCreating(modelBuilder);
    }
}
