using System.ComponentModel;
using System.Text.Json;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace FamilySuper.Agent.Plugins;

public class HealthTools
{
    private readonly IServiceProvider _sp;

    public HealthTools(IServiceProvider sp)
    {
        _sp = sp;
    }

    [Description("根据家庭成员姓名查询其健康档案")]
    [KernelFunction("get_health_records_by_member")]
    public async Task<string> GetHealthRecordsByMemberAsync(
        [Description("家庭成员姓名")] string memberName)
    {
        using var scope = _sp.CreateScope();
        var memberRepo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FamilyMember>>();
        var healthRepo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.HealthRecord>>();

        var members = await memberRepo.FindAsync(m => m.Name == memberName);
        var member = members.FirstOrDefault();
        if (member is null)
        {
            return JsonSerializer.Serialize(new { MemberName = memberName, Found = false, Message = "未找到该成员" });
        }

        var records = await healthRepo.FindAsync(r => r.MemberId == member.Id);
        var result = records.OrderByDescending(r => r.RecordDate).Select(r => new
        {
            r.Id,
            r.Title,
            Type = r.RecordType.ToString(),
            r.Content,
            r.RecordDate,
            r.Doctor,
            r.Hospital
        });

        return JsonSerializer.Serialize(new
        {
            MemberName = member.Name,
            Found = true,
            Records = result
        });
    }

    [Description("获取所有健康档案,可选按类型筛选")]
    [KernelFunction("get_all_health_records")]
    public async Task<string> GetAllHealthRecordsAsync(
        [Description("档案类型:Examination、MedicalHistory、Allergy、Medication、VitalSigns、Vaccination")] string? recordType = null)
    {
        using var scope = _sp.CreateScope();
        var healthRepo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.HealthRecord>>();
        var memberRepo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FamilyMember>>();

        var records = await healthRepo.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(recordType) && Enum.TryParse<Core.Enums.HealthRecordType>(recordType, true, out var rt))
        {
            records = records.Where(r => r.RecordType == rt).ToList();
        }

        var members = await memberRepo.GetAllAsync();
        var memberMap = members.ToDictionary(m => m.Id, m => m.Name);

        var result = records.OrderByDescending(r => r.RecordDate).Select(r => new
        {
            r.Id,
            r.Title,
            Type = r.RecordType.ToString(),
            r.Content,
            r.RecordDate,
            r.Doctor,
            r.Hospital,
            MemberName = r.MemberId.HasValue && memberMap.TryGetValue(r.MemberId.Value, out var name) ? name : null
        });

        return JsonSerializer.Serialize(result);
    }
}
