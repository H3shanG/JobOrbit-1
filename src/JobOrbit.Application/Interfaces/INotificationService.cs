using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;

namespace JobOrbit.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResultDto<NotificationListItemDto>> ListAsync(int userId, NotificationQuery query, CancellationToken token = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken token = default);
    Task<bool> MarkReadAsync(int userId, int notificationId, CancellationToken token = default);
    Task<int> MarkAllReadAsync(int userId, CancellationToken token = default);
    Task<bool> DeleteAsync(int userId, int notificationId, CancellationToken token = default);
    Task<bool> CreateAsync(NotificationCreateRequest request, CancellationToken token = default);
    Task<int> CreateManyAsync(IEnumerable<NotificationCreateRequest> requests, CancellationToken token = default);
}
