using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class FamilyMember : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? IdCardNumber { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? BirthDate { get; set; }
    public Gender? Gender { get; set; }
    public string? AvatarPath { get; set; }
    public string Role { get; set; } = "成年人";
    public string? Preferences { get; set; }
}
