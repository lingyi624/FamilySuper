namespace FamilySuper.Core.Entities;

public class SystemConfig : EntityBase
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
}
