namespace FamilySuper.Core.Entities;

public class EntertainmentRecord : EntityBase
{
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public long? MemberId { get; set; }
    public FamilyMember? Member { get; set; }
}