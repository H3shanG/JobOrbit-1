using JobOrbit.Application.DTOs.AdminRoles;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Interfaces;

public interface IAdminRoleRepository
{
    Task<IReadOnlyList<AdminRoleListItemDto>> ListRolesAsync(CancellationToken token=default);
    Task<int> UserCountAsync(UserRole role,CancellationToken token=default);
    Task<IReadOnlySet<string>> AssignedCodesAsync(UserRole role,CancellationToken token=default);
    Task ReplaceAsync(int adminUserId,UserRole role,IReadOnlyCollection<string> codes,string action,CancellationToken token=default);
}
