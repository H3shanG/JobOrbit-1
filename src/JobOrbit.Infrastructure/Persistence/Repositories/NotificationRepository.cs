using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class NotificationRepository(JobOrbitDbContext db) : INotificationRepository
{
    private IQueryable<Notification> Active(int userId) => db.Notifications.Where(x => x.RecipientUserId == userId && !x.IsDeleted && (!x.ExpiresAtUtc.HasValue || x.ExpiresAtUtc > DateTime.UtcNow));
    public async Task<PagedResultDto<NotificationListItemDto>> ListAsync(int userId, NotificationQuery query, CancellationToken token)
    {
        var q = Active(userId).AsNoTracking();
        if (query.IsRead.HasValue) q = q.Where(x => x.IsRead == query.IsRead.Value);
        if (!string.IsNullOrWhiteSpace(query.Type)) q = q.Where(x => x.Type == query.Type);
        if (!string.IsNullOrWhiteSpace(query.Priority) && Enum.TryParse<NotificationPriority>(query.Priority, true, out var priority)) q = q.Where(x => x.Priority == priority);
        q = query.Sort.Equals("oldest", StringComparison.OrdinalIgnoreCase) ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt);
        var total = await q.CountAsync(token);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new NotificationListItemDto(x.Id, x.Type, x.Title, x.Message, x.Priority.ToString(), x.IsRead, x.ActionUrl, x.CreatedAt)).ToListAsync(token);
        return new() { Items = items, Page = query.Page, PageSize = query.PageSize, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)query.PageSize) };
    }
    public Task<int> UnreadCountAsync(int userId, CancellationToken token) => Active(userId).CountAsync(x => !x.IsRead, token);
    public async Task<bool> MarkReadAsync(int userId, int notificationId, CancellationToken token)
    { var x = await Active(userId).SingleOrDefaultAsync(x => x.Id == notificationId, token); if (x is null) return false; if (!x.IsRead) { x.IsRead = true; x.ReadAtUtc = DateTime.UtcNow; await db.SaveChangesAsync(token); } return true; }
    public async Task<int> MarkAllReadAsync(int userId, CancellationToken token) => await Active(userId).Where(x => !x.IsRead).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true).SetProperty(x => x.ReadAtUtc, DateTime.UtcNow).SetProperty(x => x.UpdatedAt, DateTime.UtcNow), token);
    public async Task<bool> DeleteAsync(int userId, int notificationId, CancellationToken token)
    { var x = await Active(userId).SingleOrDefaultAsync(x => x.Id == notificationId, token); if (x is null) return false; x.IsDeleted = true; await db.SaveChangesAsync(token); return true; }
    public Task<bool> RecipientIsActiveAsync(int userId, CancellationToken token) => db.Users.AsNoTracking().AnyAsync(x => x.Id == userId && x.IsActive, token);
    public Task<bool> EventExistsAsync(int userId, string eventKey, CancellationToken token) => db.Notifications.AsNoTracking().AnyAsync(x => x.RecipientUserId == userId && x.EventKey == eventKey, token);
    public async Task AddAsync(Notification notification, CancellationToken token) { db.Notifications.Add(notification); await db.SaveChangesAsync(token); }
}
