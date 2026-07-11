using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class CareReminderService : ICareReminderService
{
    private readonly IRepository<CareReminder> _repo;
    private readonly ILogger<CareReminderService> _logger;

    public CareReminderService(IRepository<CareReminder> repo, ILogger<CareReminderService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<CareReminder>> GetAllAsync(bool? isCompleted = null, CancellationToken cancellationToken = default)
    {
        var reminders = await _repo.GetAllAsync(cancellationToken);
        if (isCompleted.HasValue)
            reminders = reminders.Where(r => r.IsCompleted == isCompleted.Value).ToList();
        return reminders.OrderBy(r => r.RemindDate ?? r.CreatedAt).ToList();
    }

    public async Task<CareReminder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => await _repo.GetByIdAsync(id, cancellationToken);

    public async Task<CareReminder> AddAsync(CareReminder reminder, CancellationToken cancellationToken = default)
    {
        await _repo.AddAsync(reminder, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("新增关怀提醒: {Title} {Category}", reminder.Title, reminder.Category);
        return reminder;
    }

    public async Task UpdateAsync(CareReminder reminder, CancellationToken cancellationToken = default)
    {
        reminder.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(reminder, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var reminder = await _repo.GetByIdAsync(id, cancellationToken);
        if (reminder is null) return;
        reminder.IsDeleted = true;
        reminder.UpdatedAt = DateTime.UtcNow;
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkCompletedAsync(long id, CancellationToken cancellationToken = default)
    {
        var reminder = await _repo.GetByIdAsync(id, cancellationToken);
        if (reminder is null) return;
        reminder.IsCompleted = true;
        reminder.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(reminder, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<CareReminder>> GetDueSoonAsync(int days = 3, CancellationToken cancellationToken = default)
    {
        var reminders = await _repo.GetAllAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var deadline = now.AddDays(days);
        return reminders
            .Where(r => !r.IsCompleted && r.RemindDate.HasValue && r.RemindDate <= deadline)
            .OrderBy(r => r.RemindDate)
            .ToList();
    }
}