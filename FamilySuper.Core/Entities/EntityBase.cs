using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public abstract class EntityBase
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public string Mode { get; set; } = AppMode.Adult.ToString().ToLower();
}
