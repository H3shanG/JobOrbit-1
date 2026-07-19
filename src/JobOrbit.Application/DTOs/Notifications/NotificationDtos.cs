using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.Notifications;

public sealed class NotificationQuery
{
    public bool? IsRead { get; set; }
    public string? Type { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = "newest";
}

public sealed record NotificationListItemDto(int NotificationId, string Type, string Title,
    string Message, string Priority, bool IsRead, string? ActionUrl, DateTime CreatedAt);
public sealed record UnreadNotificationCountDto(int UnreadCount);
public sealed record MarkAllNotificationsReadDto(int UpdatedCount);
public sealed record NotificationCreateRequest(int RecipientUserId, string Type, string Title,
    string Message, string? RelatedEntityType = null, int? RelatedEntityId = null,
    string? ActionUrl = null, Domain.Enums.NotificationPriority Priority = Domain.Enums.NotificationPriority.Normal,
    string? EventKey = null);
