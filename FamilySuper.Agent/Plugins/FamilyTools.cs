using System.ComponentModel;
using System.Text.Json;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace FamilySuper.Agent.Plugins;

public class FamilyTools
{
    private readonly IServiceProvider _sp;

    public FamilyTools(IServiceProvider sp)
    {
        _sp = sp;
    }

    [Description("获取家庭成员列表")]
    [KernelFunction("get_family_members")]
    public async Task<string> GetFamilyMembersAsync()
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FamilyMember>>();
        var members = await repo.GetAllAsync();

        var result = members.Select(m => new
        {
            m.Id,
            m.Name,
            m.Role,
            m.Gender,
            m.BirthDate,
            m.Phone,
            m.Address
        });
        return JsonSerializer.Serialize(result);
    }

    [Description("根据姓名查询家庭成员详情")]
    [KernelFunction("get_member_by_name")]
    public async Task<string> GetMemberByNameAsync([Description("成员姓名")] string name)
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FamilyMember>>();
        var members = await repo.FindAsync(m => m.Name == name);

        var member = members.FirstOrDefault();
        if (member is null)
        {
            return JsonSerializer.Serialize(new { Name = name, Found = false, Message = "未找到该成员" });
        }

        return JsonSerializer.Serialize(new
        {
            member.Id,
            member.Name,
            member.Role,
            member.Gender,
            member.BirthDate,
            member.Phone,
            member.Address,
            member.Preferences,
            Found = true
        });
    }
}
