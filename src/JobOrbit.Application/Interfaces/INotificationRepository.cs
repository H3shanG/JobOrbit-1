using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Interfaces;

public interface INotificationRepository
{
    Task<PagedResultDto<NotificationListItemDto>> ListAsync(int userId, NotificationQuery query, CancellationToken token);
    Task<int> UnreadCountAsync(int userId, CancellationToken token);
    Task<bool> MarkReadAsync(int userId, int notificationId, CancellationToken token);
    Task<int> MarkAllReadAsync(int userId, CancellationToken token);
    Task<bool> DeleteAsync(int userId, int notificationId, CancellationToken token);
    Task<bool> RecipientIsActiveAsync(int userId, CancellationToken token);
    Task<bool> EventExistsAsync(int userId, string eventKey, CancellationToken token);
    Task AddAsync(Notification notification, CancellationToken token);
}
