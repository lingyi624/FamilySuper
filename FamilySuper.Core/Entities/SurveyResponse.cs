namespace FamilySuper.Core.Entities;

public class SurveyResponse : EntityBase
{
    public long SurveyId { get; set; }
    public long? MemberId { get; set; }
    public string ValuesJson { get; set; } = "{}";
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
