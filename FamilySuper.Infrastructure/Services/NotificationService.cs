using System.Collections.Concurrent;
using FamilySuper.Core.Entities;
using FamilySuper.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FamilySuper.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ConcurrentDictionary<long, NotificationItem> _store = new();
    private readonly ILogger<NotificationService> _logger;
    private long _idCounter;

    public event Action<NotificationItem>? OnNotification;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task<List<NotificationItem>> GetPendingAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        var items = _store.Values
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToList();
        return Task.FromResult(items);
    }

    public Task PushAsync(string title, string message, string category, long? memberId = null, CancellationToken cancellationToken = default)
    {
        var item = new NotificationItem
        {
            Id = Interlocked.Increment(ref _idCounter),
            Title = title,
            Message = message,
            Category = category,
            MemberId = memberId,
            CreatedAt = DateTime.UtcNow
        };
        _store[item.Id] = item;
        _logger.LogInformation("推送通知 [{Category}] {Title}: {Message}", category, title, message);
        OnNotification?.Invoke(item);
        return Task.CompletedTask;
    }

    public Task MarkReadAsync(long id, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(id, out var item))
        {
            item.IsRead = true;
        }
        return Task.CompletedTask;
    }

    public Task MarkAllReadAsync(CancellationToken cancellationToken = default)
    {
        foreach (var item in _store.Values) item.IsRead = true;
        return Task.CompletedTask;
    }
}
