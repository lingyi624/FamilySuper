using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class ConversationMessage : EntityBase
{
    public long SessionId { get; set; }
    public MessageRole Role { get; set; } = MessageRole.User;
    public string Content { get; set; } = string.Empty;
    public int? TokenCount { get; set; }
    public string? ModelId { get; set; }
}
