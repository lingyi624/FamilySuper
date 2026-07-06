using FamilySuper.Core.Enums;

namespace FamilySuper.Core.Entities;

public class Certificate : EntityBase
{
    public long? MemberId { get; set; }
    public string Name { get; set; } = string.Empty;
    public CertificateType Type { get; set; } = CertificateType.Other;
    public string? Number { get; set; }
    public byte[]? EncryptedData { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Remark { get; set; }
}
