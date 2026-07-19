using System.Security.Claims;
using JobOrbit.Application.Authorization;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence;

public sealed class CurrentUserPermissionService(JobOrbitDbContext db) : ICurrentUserPermissionService
{
    public async Task<bool> HasAsync(ClaimsPrincipal user,string code,CancellationToken token=default)
    {
        var value=user.FindFirst("Role")?.Value;
        if(value is null || !Enum.TryParse<UserRole>(value,true,out var role)) return false;
        return await db.RolePermissions.AsNoTracking().AnyAsync(x=>x.Role==role&&x.Permission.Code==code,token);
    }
}
