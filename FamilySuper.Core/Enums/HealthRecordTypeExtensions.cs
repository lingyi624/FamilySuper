namespace FamilySuper.Core.Enums;

public static class HealthRecordTypeExtensions
{
    public static string ToChinese(this HealthRecordType type) => type switch
    {
        HealthRecordType.Examination => "体检记录",
        HealthRecordType.MedicalHistory => "病史记录",
        HealthRecordType.Allergy => "过敏记录",
        HealthRecordType.Medication => "用药记录",
        HealthRecordType.VitalSigns => "生命体征",
        HealthRecordType.Vaccination => "疫苗接种",
        HealthRecordType.Other => "其他",
        _ => type.ToString()
    };
}