using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.DTOs.Notifications;
using JobOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Controllers;

[ApiController, Route("api/notifications"), Authorize]
public sealed class NotificationsController(INotificationService service) : ControllerBase
{
    private bool UserId(out int id) => int.TryParse(User.FindFirst("UserId")?.Value, out id);
    [HttpGet] public async Task<ActionResult<PagedResultDto<NotificationListItemDto>>> List([FromQuery] NotificationQuery query, CancellationToken token) => UserId(out var id) ? Ok(await service.ListAsync(id, query, token)) : Unauthorized();
    [HttpGet("unread-count")] public async Task<ActionResult<UnreadNotificationCountDto>> UnreadCount(CancellationToken token) => UserId(out var id) ? Ok(new UnreadNotificationCountDto(await service.GetUnreadCountAsync(id, token))) : Unauthorized();
    [HttpPatch("{notificationId:int}/read")] public async Task<IActionResult> Read(int notificationId, CancellationToken token) => UserId(out var id) ? await service.MarkReadAsync(id, notificationId, token) ? NoContent() : NotFound() : Unauthorized();
    [HttpPatch("read-all")] public async Task<ActionResult<MarkAllNotificationsReadDto>> ReadAll(CancellationToken token) => UserId(out var id) ? Ok(new MarkAllNotificationsReadDto(await service.MarkAllReadAsync(id, token))) : Unauthorized();
    [HttpDelete("{notificationId:int}")] public async Task<IActionResult> Delete(int notificationId, CancellationToken token) => UserId(out var id) ? await service.DeleteAsync(id, notificationId, token) ? NoContent() : NotFound() : Unauthorized();
}
