namespace FamilySuper.Core.Entities;

public class MemoryEntry : EntityBase
{
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public long? MemberId { get; set; }
    public string? EmbeddingJson { get; set; }
}
