using System.Text.Json;
using JobOrbit.Application.Authorization;
using JobOrbit.Application.DTOs.AdminRoles;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class AdminRoleRepository(JobOrbitDbContext db) : IAdminRoleRepository
{
    public async Task<IReadOnlyList<AdminRoleListItemDto>> ListRolesAsync(CancellationToken token=default)
    {
        var userCounts = await db.Users.AsNoTracking().GroupBy(x=>x.Role).Select(x=>new{x.Key,Count=x.Count()}).ToDictionaryAsync(x=>x.Key,x=>x.Count,token);
        var permissionCounts = await db.RolePermissions.AsNoTracking().GroupBy(x=>x.Role).Select(x=>new{x.Key,Count=x.Count()}).ToDictionaryAsync(x=>x.Key,x=>x.Count,token);
        return Enum.GetValues<UserRole>().Select(role=>new AdminRoleListItemDto(
            PermissionConstants.PublicRole(role), PermissionConstants.PublicRole(role), Description(role),
            userCounts.GetValueOrDefault(role),permissionCounts.GetValueOrDefault(role),true)).ToList();
    }

    public Task<int> UserCountAsync(UserRole role,CancellationToken token=default) => db.Users.AsNoTracking().CountAsync(x=>x.Role==role,token);

    public async Task<IReadOnlySet<string>> AssignedCodesAsync(UserRole role,CancellationToken token=default) =>
        (await db.RolePermissions.AsNoTracking().Where(x=>x.Role==role).Select(x=>x.Permission.Code).ToListAsync(token)).ToHashSet(StringComparer.Ordinal);

    public async Task ReplaceAsync(int adminUserId,UserRole role,IReadOnlyCollection<string> codes,string action,CancellationToken token=default)
    {
        await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(token) : null;
        var old = await db.RolePermissions.Where(x=>x.Role==role).ToListAsync(token);
        db.RolePermissions.RemoveRange(old);
        var permissions = await db.Permissions.Where(x=>codes.Contains(x.Code)).ToListAsync(token);
        db.RolePermissions.AddRange(permissions.Select(x=>new RolePermission{Role=role,PermissionId=x.Id}));
        db.AuditLogs.Add(new AuditLog{UserId=adminUserId,EntityName="RolePermission",Action=action,OldValues=JsonSerializer.Serialize(old.Select(x=>x.PermissionId)),NewValues=JsonSerializer.Serialize(codes.Order())});
        await db.SaveChangesAsync(token);
        if(transaction is not null) await transaction.CommitAsync(token);
    }

    private static string Description(UserRole role)=>role switch
    {
        UserRole.Candidate=>"Applies for jobs and manages their profile.",
        UserRole.Recruiter=>"Creates jobs and manages candidates.",
        UserRole.HiringManager=>"Reviews candidates and makes hiring decisions.",
        _=>"Manages users, roles, and platform configuration."
    };
}
