using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class ProactivePushWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notifications;
    private readonly ILogger<ProactivePushWorker> _logger;
    private readonly HashSet<string> _pushedToday = new();
    private DateTime _pushedDate = DateTime.UtcNow.Date;

    public ProactivePushWorker(
        IServiceProvider serviceProvider,
        INotificationService notifications,
        ILogger<ProactivePushWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _notifications = notifications;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("主动推送后台服务已启动");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndPushAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "主动推送检查失败");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CheckAndPushAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow.Date != _pushedDate)
        {
            _pushedToday.Clear();
            _pushedDate = DateTime.UtcNow.Date;
        }

        using var scope = _serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;

        await PushBillRemindersAsync(sp, ct);
        await PushBudgetAlertsAsync(sp, ct);
    }

    private async Task PushBillRemindersAsync(IServiceProvider sp, CancellationToken ct)
    {
        var billSvc = sp.GetRequiredService<IBillService>();
        var dueSoon = await billSvc.GetDueSoonAsync(3, ct);
        foreach (var bill in dueSoon)
        {
            var key = $"bill_{bill.Id}_{_pushedDate:yyyyMMdd}";
            if (_pushedToday.Contains(key)) continue;
            _pushedToday.Add(key);

            var days = (bill.NextDueDate - DateTime.UtcNow).Days;
            var msg = days < 0
                ? $"账单「{bill.Name}」已逾期 {-days} 天,金额 ¥{bill.Amount:N2},请尽快处理。"
                : days == 0
                    ? $"账单「{bill.Name}」今天到期,金额 ¥{bill.Amount:N2}。"
                    : $"账单「{bill.Name}」将在 {days} 天后到期,金额 ¥{bill.Amount:N2}。";

            await _notifications.PushAsync(
                title: $"账单提醒:{bill.Name}",
                message: msg,
                category: "bill",
                memberId: bill.MemberId,
                cancellationToken: ct);
        }
    }

    private async Task PushBudgetAlertsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var budgetSvc = sp.GetRequiredService<IBudgetService>();
        var now = DateTime.UtcNow;
        var statuses = await budgetSvc.GetMonthlyStatusAsync(now.Year, now.Month, ct);

        foreach (var (budget, status) in statuses)
        {
            if (status.Remaining < 0)
            {
                var key = $"budget_over_{budget.Id}_{_pushedDate:yyyyMMdd}";
                if (_pushedToday.Contains(key)) continue;
                _pushedToday.Add(key);
                await _notifications.PushAsync(
                    title: $"预算超支:{(string.IsNullOrEmpty(budget.Category) ? "总预算" : budget.Category)}",
                    message: $"{budget.Year}年{budget.Month}月「{(string.IsNullOrEmpty(budget.Category) ? "总预算" : budget.Category)}」已超支 ¥{-status.Remaining:N2},实际支出 ¥{status.ActualExpense:N2}(预算 ¥{status.BudgetAmount:N2})。",
                    category: "budget",
                    memberId: budget.MemberId,
                    cancellationToken: ct);
            }
            else if (status.UsagePercent >= 80m)
            {
                var key = $"budget_warn_{budget.Id}_{_pushedDate:yyyyMMdd}";
                if (_pushedToday.Contains(key)) continue;
                _pushedToday.Add(key);
                await _notifications.PushAsync(
                    title: $"预算警戒:{(string.IsNullOrEmpty(budget.Category) ? "总预算" : budget.Category)}",
                    message: $"{budget.Year}年{budget.Month}月「{(string.IsNullOrEmpty(budget.Category) ? "总预算" : budget.Category)}」使用率已达 {status.UsagePercent:F1}%,剩余 ¥{status.Remaining:N2},请注意控制支出。",
                    category: "budget",
                    memberId: budget.MemberId,
                    cancellationToken: ct);
            }
        }
    }
}
