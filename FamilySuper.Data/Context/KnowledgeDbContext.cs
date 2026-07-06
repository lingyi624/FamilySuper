using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FamilySuper.Data.Context;

public class KnowledgeDbContext : DbContext
{
    private readonly IModeManager _modeManager;

    public KnowledgeDbContext(DbContextOptions<KnowledgeDbContext> options, IModeManager modeManager) : base(options)
    {
        _modeManager = modeManager;
    }

    public DbSet<ConversationSession> ConversationSessions => Set<ConversationSession>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<MemoryEntry> MemoryEntries => Set<MemoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var currentMode = _modeManager.CurrentMode.ToString().ToLower();

        modelBuilder.Entity<ConversationSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.LastMessageAt);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<ConversationMessage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Content).IsRequired().HasMaxLength(int.MaxValue);
            e.Property(x => x.ModelId).HasMaxLength(50);
            e.HasIndex(x => x.SessionId);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        modelBuilder.Entity<MemoryEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Content).IsRequired().HasMaxLength(int.MaxValue);
            e.Property(x => x.Category).IsRequired().HasMaxLength(50);
            e.Property(x => x.EmbeddingJson).HasMaxLength(int.MaxValue);
            e.HasIndex(x => x.MemberId);
            e.HasIndex(x => x.Category);
            e.HasQueryFilter(x => !x.IsDeleted && x.Mode == currentMode);
        });

        base.OnModelCreating(modelBuilder);
    }
}
