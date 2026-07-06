using FamilySuper.Core.Entities;
using FamilySuper.Core.Enums;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class BillService : IBillService
{
    private readonly IRepository<BillReminder> _repo;
    private readonly INotificationService _notifications;
    private readonly ILogger<BillService> _logger;

    public BillService(
        IRepository<BillReminder> repo,
        INotificationService notifications,
        ILogger<BillService> logger)
    {
        _repo = repo;
        _notifications = notifications;
        _logger = logger;
    }

    public async Task<List<BillReminder>> GetBillsAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var bills = await _repo.GetAllAsync(cancellationToken);
        if (activeOnly == true) bills = bills.Where(b => b.IsActive).ToList();
        return bills.OrderBy(b => b.NextDueDate).ToList();
    }

    public async Task<BillReminder?> GetBillByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _repo.GetByIdAsync(id, cancellationToken);
    }

    public async Task<BillReminder> AddBillAsync(BillReminder bill, CancellationToken cancellationToken = default)
    {
        bill.NextDueDate = ComputeNextDue(bill.StartDate, bill.CycleType, bill.LastPaidDate);
        await _repo.AddAsync(bill, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增账单:{Name} 金额 {Amount} 下次到期 {Due}", bill.Name, bill.Amount, bill.NextDueDate);
        return bill;
    }

    public async Task UpdateBillAsync(BillReminder bill, CancellationToken cancellationToken = default)
    {
        bill.NextDueDate = ComputeNextDue(bill.StartDate, bill.CycleType, bill.LastPaidDate);
        bill.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(bill, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteBillAsync(long id, CancellationToken cancellationToken = default)
    {
        var bill = await _repo.GetByIdAsync(id, cancellationToken);
        if (bill is null) return;
        bill.IsDeleted = true;
        bill.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkPaidAsync(long id, CancellationToken cancellationToken = default)
    {
        var bill = await _repo.GetByIdAsync(id, cancellationToken);
        if (bill is null) return;
        bill.LastPaidDate = DateTime.UtcNow;
        bill.IsPaid = true;
        if (bill.CycleType != BillCycleType.Once)
        {
            bill.NextDueDate = ComputeNextDue(bill.StartDate, bill.CycleType, bill.LastPaidDate);
            bill.IsPaid = false;
        }
        bill.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("账单已支付:{Name}", bill.Name);
    }

    public async Task<List<BillReminder>> GetDueSoonAsync(int days = 7, CancellationToken cancellationToken = default)
    {
        var bills = await _repo.GetAllAsync(cancellationToken);
        var threshold = DateTime.UtcNow.AddDays(days);
        return bills
            .Where(b => b.IsActive && !b.IsPaid && b.NextDueDate <= threshold)
            .OrderBy(b => b.NextDueDate)
            .ToList();
    }

    private static DateTime ComputeNextDue(DateTime start, BillCycleType cycle, DateTime? lastPaid)
    {
        var baseDate = lastPaid ?? start;
        return cycle switch
        {
            BillCycleType.Once => start,
            BillCycleType.Monthly => baseDate.AddMonths(1),
            BillCycleType.Quarterly => baseDate.AddMonths(3),
            BillCycleType.Yearly => baseDate.AddYears(1),
            _ => baseDate.AddMonths(1)
        };
    }
}
