using System.ComponentModel;
using System.Text.Json;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace FamilySuper.Agent.Plugins;

public class FinanceTools
{
    private readonly IServiceProvider _sp;

    public FinanceTools(IServiceProvider sp)
    {
        _sp = sp;
    }

    [Description("获取家庭财务记录,可选按类型(Income/Expense)和类别筛选")]
    [KernelFunction("get_finance_records")]
    public async Task<string> GetFinanceRecordsAsync(
        [Description("收支类型:Income 或 Expense,留空查全部")] string? type = null,
        [Description("类别关键词,如:餐饮、交通、工资")] string? category = null)
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FinanceRecord>>();

        var records = await repo.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<FinanceType>(type, true, out var ft))
        {
            records = records.Where(r => r.Type == ft).ToList();
        }
        if (!string.IsNullOrWhiteSpace(category))
        {
            records = records.Where(r => r.Category != null && r.Category.Contains(category)).ToList();
        }

        var result = records.OrderByDescending(r => r.RecordDate).Select(r => new
        {
            r.Id,
            r.Title,
            Type = r.Type.ToString(),
            r.Amount,
            r.Category,
            r.RecordDate,
            r.Remark
        });
        return JsonSerializer.Serialize(result);
    }

    [Description("添加一条财务记录")]
    [KernelFunction("add_finance_record")]
    public async Task<string> AddFinanceRecordAsync(
        [Description("记录标题")] string title,
        [Description("金额")] decimal amount,
        [Description("收支类型:Income 或 Expense")] string type,
        [Description("类别,如:餐饮、交通、工资")] string? category = null,
        [Description("备注")] string? remark = null)
    {
        if (!Enum.TryParse<FinanceType>(type, true, out var ft))
        {
            return JsonSerializer.Serialize(new { Success = false, Message = "类型无效,应为 Income 或 Expense" });
        }

        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FinanceRecord>>();

        var record = new Core.Entities.FinanceRecord
        {
            Title = title,
            Amount = amount,
            Type = ft,
            Category = category ?? string.Empty,
            Remark = remark ?? string.Empty,
            RecordDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(record);
        await repo.SaveChangesAsync();

        return JsonSerializer.Serialize(new
        {
            record.Id,
            record.Title,
            record.Amount,
            Type = record.Type.ToString(),
            record.Category,
            Success = true
        });
    }

    [Description("获取财务汇总:总收入、总支出、余额")]
    [KernelFunction("get_finance_summary")]
    public async Task<string> GetFinanceSummaryAsync()
    {
        using var scope = _sp.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepository<Core.Entities.FinanceRecord>>();
        var records = await repo.GetAllAsync();

        var income = records.Where(r => r.Type == FinanceType.Income).Sum(r => r.Amount);
        var expense = records.Where(r => r.Type == FinanceType.Expense).Sum(r => r.Amount);

        return JsonSerializer.Serialize(new
        {
            TotalIncome = income,
            TotalExpense = expense,
            Balance = income - expense,
            RecordCount = records.Count
        });
    }
}
