using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FamilySuper.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        await EnsureCreatedAsync<FamilyDbContext>(sp);
        await EnsureCreatedAsync<FinanceDbContext>(sp);
        await EnsureCreatedAsync<HealthDbContext>(sp);
        await EnsureCreatedAsync<EducationDbContext>(sp);
        await EnsureCreatedAsync<KnowledgeDbContext>(sp);

        await EnsureNewTablesAsync(sp);

        await SeedFamilyDataAsync(sp);
    }

    private static async Task EnsureCreatedAsync<T>(IServiceProvider sp) where T : DbContext
    {
        var ctx = sp.GetRequiredService<T>();
        await ctx.Database.EnsureCreatedAsync();
    }

    private static async Task EnsureNewTablesAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<FamilyDbContext>();
        
      

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS ShoppingItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Category TEXT,
    Quantity INTEGER NOT NULL DEFAULT 1,
    Unit TEXT,
    EstimatedPrice REAL,
    Status TEXT NOT NULL DEFAULT 'Pending',
    PurchasedDate TEXT,
    Notes TEXT,
    MemberId INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS EmergencyContacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Relationship TEXT,
    PhoneNumber TEXT NOT NULL,
    AlternatePhone TEXT,
    IsPrimary INTEGER NOT NULL DEFAULT 0,
    Notes TEXT,
    MemberId INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS HealthMetrics (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    MemberId INTEGER,
    RecordDate TEXT NOT NULL,
    HeartRate REAL,
    SystolicPressure REAL,
    DiastolicPressure REAL,
    BloodSugar REAL,
    Weight REAL,
    Temperature REAL,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS EntertainmentRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Category TEXT,
    Title TEXT NOT NULL,
    Type TEXT,
    Source TEXT,
    Notes TEXT,
    MemberId INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");

        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ShoppingItems_Status ON ShoppingItems(Status)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_ShoppingItems_MemberId ON ShoppingItems(MemberId)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_EmergencyContacts_MemberId ON EmergencyContacts(MemberId)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_HealthMetrics_MemberId ON HealthMetrics(MemberId)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_HealthMetrics_RecordDate ON HealthMetrics(RecordDate)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_EntertainmentRecords_MemberId ON EntertainmentRecords(MemberId)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS CareReminders (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Category TEXT,
    Type TEXT,
    Content TEXT,
    RemindDate TEXT,
    IsRecurring INTEGER NOT NULL DEFAULT 0,
    RecurrencePattern TEXT,
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    MemberId INTEGER,
    Notes TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult'
)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CareReminders_MemberId ON CareReminders(MemberId)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CareReminders_IsCompleted ON CareReminders(IsCompleted)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_CareReminders_RemindDate ON CareReminders(RemindDate)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS VirtualScenes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Location TEXT NOT NULL,
    Latitude REAL,
    Longitude REAL,
    Description TEXT,
    ImagePaths TEXT,
    SceneType TEXT,
    PanoramaPath TEXT,
    ModelPath TEXT,
    Notes TEXT,
    MemberId INTEGER,
    PhotoCount INTEGER,
    PhotoDirectory TEXT,
    ReconstructionStatus TEXT,
    ReconstructedAt TEXT,
    ReconstructionProgress INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_VirtualScenes_SceneType ON VirtualScenes(SceneType)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_VirtualScenes_MemberId ON VirtualScenes(MemberId)");

        try
        {
            await ctx.Database.ExecuteSqlRawAsync("ALTER TABLE VirtualScenes ADD COLUMN PhotoCount INTEGER");
        }
        catch { }
        try
        {
            await ctx.Database.ExecuteSqlRawAsync("ALTER TABLE VirtualScenes ADD COLUMN PhotoDirectory TEXT");
        }
        catch { }
        try
        {
            await ctx.Database.ExecuteSqlRawAsync("ALTER TABLE VirtualScenes ADD COLUMN ReconstructionStatus TEXT");
        }
        catch { }
        try
        {
            await ctx.Database.ExecuteSqlRawAsync("ALTER TABLE VirtualScenes ADD COLUMN ReconstructedAt TEXT");
        }
        catch { }
        try
        {
            await ctx.Database.ExecuteSqlRawAsync("ALTER TABLE VirtualScenes ADD COLUMN ReconstructionProgress INTEGER");
        }
        catch { }

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS Annotations (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SceneId INTEGER NOT NULL,
    PositionX REAL NOT NULL,
    PositionY REAL NOT NULL,
    PositionZ REAL NOT NULL,
    Text TEXT NOT NULL,
    ImageUrl TEXT,
    Color TEXT,
    MemoryText TEXT,
    MemoryDate TEXT,
    RelatedMediaPath TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (SceneId) REFERENCES VirtualScenes(Id)
)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Annotations_SceneId ON Annotations(SceneId)");

        await ctx.Database.ExecuteSqlRawAsync(@"
CREATE TABLE IF NOT EXISTS PhotoTimelines (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    ThumbnailPath TEXT,
    Description TEXT,
    TakenDate TEXT,
    Location TEXT,
    Latitude REAL,
    Longitude REAL,
    PeopleTags TEXT,
    SceneTags TEXT,
    Category TEXT,
    MemberId INTEGER,
    IsCover INTEGER NOT NULL DEFAULT 0,
    Width INTEGER,
    Height INTEGER,
    FileSize INTEGER,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    IsDeleted INTEGER NOT NULL DEFAULT 0,
    Mode TEXT NOT NULL DEFAULT 'adult',
    FOREIGN KEY (MemberId) REFERENCES FamilyMembers(Id)
)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_PhotoTimelines_Category ON PhotoTimelines(Category)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_PhotoTimelines_MemberId ON PhotoTimelines(MemberId)");
        await ctx.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_PhotoTimelines_TakenDate ON PhotoTimelines(TakenDate)");
    }

    private static async Task SeedFamilyDataAsync(IServiceProvider sp)
    {
        var ctx = sp.GetRequiredService<FamilyDbContext>();

        if (await ctx.FamilyMembers.AnyAsync()) return;

        ctx.FamilyMembers.Add(new FamilyMember
        {
            Name = "家庭管理员",
            Role = "家庭管理员",
            Gender = Gender.Unknown,
            Mode = AppMode.Adult.ToString().ToLower(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        ctx.SystemConfigs.Add(new SystemConfig
        {
            Key = "system.initialized",
            Value = DateTime.UtcNow.ToString("O"),
            Description = "系统初始化时间",
            Category = "System",
            Mode = AppMode.Adult.ToString().ToLower(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await ctx.SaveChangesAsync();
    }
}
