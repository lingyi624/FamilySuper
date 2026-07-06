namespace FamilySuper.Core.Entities;

public class NotificationItem
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public long? MemberId { get; set; }
}
