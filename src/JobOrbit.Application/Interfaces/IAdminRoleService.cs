using JobOrbit.Application.DTOs.AdminRoles;

namespace JobOrbit.Application.Interfaces;

public interface IAdminRoleService
{
    Task<IReadOnlyList<AdminRoleListItemDto>> ListAsync(CancellationToken token=default);
    Task<AdminRoleDetailsDto?> DetailsAsync(string roleName,CancellationToken token=default);
    Task<IReadOnlyList<PermissionDefinitionDto>> PermissionsAsync(string? category,string? search,CancellationToken token=default);
    Task<AdminRoleUpdateResult> UpdateAsync(int adminUserId,string roleName,UpdateRolePermissionsRequest request,CancellationToken token=default);
    Task<AdminRoleUpdateResult> ResetAsync(int adminUserId,string roleName,CancellationToken token=default);
}
public interface ICurrentUserPermissionService { Task<bool> HasAsync(System.Security.Claims.ClaimsPrincipal user,string code,CancellationToken token=default); }
