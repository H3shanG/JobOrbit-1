using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace JobOrbit.API.Middleware;

public sealed class MaintenanceModeMiddleware(RequestDelegate next)
{
 public async Task InvokeAsync(HttpContext context,ISystemSettingsProvider settings)
 {
  var path=context.Request.Path;
  if(path.StartsWithSegments("/api/health")||path.StartsWithSegments("/swagger")||path.StartsWithSegments("/api/auth/login")){await next(context);return;}
  var m=(await settings.GetAsync(context.RequestAborted)).Maintenance;
  if(m.MaintenanceModeEnabled&&context.User.Identity?.IsAuthenticated==true&&!context.User.IsInRole(nameof(UserRole.Administrator)))
  {context.Response.StatusCode=503;context.Response.ContentType="application/problem+json";context.Response.Headers.RetryAfter="300";await context.Response.WriteAsJsonAsync(new ProblemDetails{Status=503,Title="Platform maintenance",Detail=m.MaintenanceMessage},context.RequestAborted);return;}
  await next(context);
 }
}
