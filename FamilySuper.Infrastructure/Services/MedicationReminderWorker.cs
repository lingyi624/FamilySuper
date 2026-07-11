using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class MedicationReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MedicationReminderWorker> _logger;
    private readonly HashSet<string> _pushedToday = new();
    private DateTime _pushedDate = DateTime.UtcNow.Date;

    public MedicationReminderWorker(
        IServiceProvider serviceProvider,
        INotificationService notificationService,
        ILogger<MedicationReminderWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("用药提醒后台服务已启动");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckRemindersAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用药提醒检查失败");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task CheckRemindersAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow.Date != _pushedDate)
        {
            _pushedToday.Clear();
            _pushedDate = DateTime.UtcNow.Date;
        }

        using var scope = _serviceProvider.CreateScope();
        var medSvc = scope.ServiceProvider.GetRequiredService<IMedicationService>();
        var due = await medSvc.GetDueRemindersAsync(ct);

        foreach (var plan in due)
        {
            var key = $"{plan.Id}_{_pushedDate:yyyyMMdd}";
            if (_pushedToday.Contains(key)) continue;
            _pushedToday.Add(key);

            var member = plan.MemberId == null ? "家庭成员" : $"成员#{plan.MemberId}";
            await _notificationService.PushAsync(
                title: $"用药提醒:{plan.DrugName}",
                message: $"{member} 今日需服用 {plan.DrugName} {(plan.Dosage ?? "")} 共 {plan.TimesPerDay} 次{(string.IsNullOrEmpty(plan.Frequency) ? "" : $"({plan.Frequency})")}。当前尚未记录服药。",
                category: "medication",
                memberId: plan.MemberId,
                cancellationToken: ct);
        }
    }
}
