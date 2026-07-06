namespace FamilySuper.Core.Entities;

public class ConversationSession : EntityBase
{
    public string? Title { get; set; }
    public long? MemberId { get; set; }
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public int MessageCount { get; set; }
}
