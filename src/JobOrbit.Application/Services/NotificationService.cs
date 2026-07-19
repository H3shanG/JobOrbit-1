using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Services;

public sealed class NotificationService(INotificationRepository repository, ISystemSettingsProvider settings) : INotificationService
{
    private static readonly string[] AllowedPrefixes = ["/candidate/", "/recruiter/", "/manager/", "/admin/"];

    public Task<PagedResultDto<NotificationListItemDto>> ListAsync(int userId, NotificationQuery query, CancellationToken token = default)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        return repository.ListAsync(userId, query, token);
    }
    public Task<int> GetUnreadCountAsync(int userId, CancellationToken token = default) => repository.UnreadCountAsync(userId, token);
    public Task<bool> MarkReadAsync(int userId, int notificationId, CancellationToken token = default) => repository.MarkReadAsync(userId, notificationId, token);
    public Task<int> MarkAllReadAsync(int userId, CancellationToken token = default) => repository.MarkAllReadAsync(userId, token);
    public Task<bool> DeleteAsync(int userId, int notificationId, CancellationToken token = default) => repository.DeleteAsync(userId, notificationId, token);

    public async Task<bool> CreateAsync(NotificationCreateRequest request, CancellationToken token = default)
    {
        var flags = (await settings.GetAsync(token)).Notifications;
        if (!flags.EnableNotifications || !EventEnabled(request.Type, flags) || !NotificationTypes.All.Contains(request.Type)) return false;
        if (!await repository.RecipientIsActiveAsync(request.RecipientUserId, token)) return false;
        if (!string.IsNullOrWhiteSpace(request.EventKey) && await repository.EventExistsAsync(request.RecipientUserId, request.EventKey, token)) return false;
        var actionUrl = NormalizeActionUrl(request.ActionUrl);
        await repository.AddAsync(new Notification
        {
            RecipientUserId = request.RecipientUserId, Type = request.Type,
            Title = request.Title.Trim()[..Math.Min(request.Title.Trim().Length, 150)],
            Message = request.Message.Trim()[..Math.Min(request.Message.Trim().Length, 500)],
            RelatedEntityType = request.RelatedEntityType, RelatedEntityId = request.RelatedEntityId,
            ActionUrl = actionUrl, Priority = request.Priority, EventKey = request.EventKey
        }, token);
        return true;
    }

    public async Task<int> CreateManyAsync(IEnumerable<NotificationCreateRequest> requests, CancellationToken token = default)
    { var count = 0; foreach (var request in requests) if (await CreateAsync(request, token)) count++; return count; }

    private static string? NormalizeActionUrl(string? url) => string.IsNullOrWhiteSpace(url) ? null :
        AllowedPrefixes.Any(prefix => url.StartsWith(prefix, StringComparison.Ordinal)) ? url : null;
    private static bool EventEnabled(string type, DTOs.AdminSystemSettings.NotificationSettingsDto flags) => type switch
    {
        NotificationTypes.ApplicationStatusChanged or NotificationTypes.HiringDecisionUpdated => flags.NotifyCandidateOnStatusChange,
        NotificationTypes.NewApplicationReceived => flags.NotifyRecruiterOnNewApplication,
        NotificationTypes.CandidateReadyForReview or NotificationTypes.EvaluationRequired => flags.NotifyManagerOnEvaluationRequired,
        NotificationTypes.InterviewScheduled or NotificationTypes.InterviewRescheduled or NotificationTypes.InterviewCancelled or NotificationTypes.InterviewStatusChanged => flags.NotifyParticipantsOnInterviewChange,
        _ => true
    };
}
