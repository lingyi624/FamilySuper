using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class HealthRecord : EntityBase
{
    public long? MemberId { get; set; }
    public HealthRecordType RecordType { get; set; } = HealthRecordType.Other;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime RecordDate { get; set; } = DateTime.UtcNow;
    public string? Doctor { get; set; }
    public string? Hospital { get; set; }
    public string? AttachmentPath { get; set; }
    public string? Remark { get; set; }
}
