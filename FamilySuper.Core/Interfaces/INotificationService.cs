using FamilySuper.Core.Entities;

namespace FamilySuper.Core.Interfaces;

public interface INotificationService
{
    event Action<NotificationItem>? OnNotification;

    Task<List<NotificationItem>> GetPendingAsync(int limit = 50, CancellationToken cancellationToken = default);

    Task PushAsync(string title, string message, string category, long? memberId = null, CancellationToken cancellationToken = default);

    Task MarkReadAsync(long id, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(CancellationToken cancellationToken = default);
}
