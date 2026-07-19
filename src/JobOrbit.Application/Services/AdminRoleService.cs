using JobOrbit.Application.Authorization;
using JobOrbit.Application.DTOs.AdminRoles;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Services;

public sealed class AdminRoleService(IAdminRoleRepository repository) : IAdminRoleService
{
    public Task<IReadOnlyList<AdminRoleListItemDto>> ListAsync(CancellationToken token=default)=>repository.ListRolesAsync(token);
    public async Task<AdminRoleDetailsDto?> DetailsAsync(string roleName,CancellationToken token=default)
    {
        if(!PermissionConstants.TryRole(roleName,out var role)) return null;
        var assigned=await repository.AssignedCodesAsync(role,token);
        var permissions=PermissionConstants.All.Where(x=>x.Role==role).Select(x=>new AdminRolePermissionDto(x.Code,x.DisplayName,x.Description,x.Category,assigned.Contains(x.Code),x.IsRequired,true,null)).ToList();
        return new(PermissionConstants.PublicRole(role),PermissionConstants.PublicRole(role),Description(role),true,await repository.UserCountAsync(role,token),permissions);
    }
    public Task<IReadOnlyList<PermissionDefinitionDto>> PermissionsAsync(string? category,string? search,CancellationToken token=default)
    {
        IEnumerable<PermissionDefinition> result=PermissionConstants.All;
        if(!string.IsNullOrWhiteSpace(category)) result=result.Where(x=>x.Category.Equals(category.Trim(),StringComparison.OrdinalIgnoreCase));
        if(!string.IsNullOrWhiteSpace(search)){var q=search.Trim();result=result.Where(x=>x.Code.Contains(q,StringComparison.OrdinalIgnoreCase)||x.DisplayName.Contains(q,StringComparison.OrdinalIgnoreCase)||x.Description.Contains(q,StringComparison.OrdinalIgnoreCase));}
        return Task.FromResult<IReadOnlyList<PermissionDefinitionDto>>(result.OrderBy(x=>x.Category).ThenBy(x=>x.DisplayName).Select(x=>new PermissionDefinitionDto(x.Code,x.DisplayName,x.Description,x.Category)).ToList());
    }
    public async Task<AdminRoleUpdateResult> UpdateAsync(int admin,string name,UpdateRolePermissionsRequest request,CancellationToken token=default)
    {
        if(!PermissionConstants.TryRole(name,out var role)) return new(AdminRoleUpdateOutcome.UnknownRole);
        var codes=request.PermissionCodes??[];
        if(codes.Count!=codes.Distinct(StringComparer.Ordinal).Count()) return new(AdminRoleUpdateOutcome.DuplicatePermission);
        var known=PermissionConstants.All.Select(x=>x.Code).ToHashSet(StringComparer.Ordinal);
        var unknown=codes.FirstOrDefault(x=>!known.Contains(x)); if(unknown is not null)return new(AdminRoleUpdateOutcome.UnknownPermission,null,unknown);
        var compatible=PermissionConstants.Defaults[role]; var incompatible=codes.FirstOrDefault(x=>!compatible.Contains(x)); if(incompatible is not null)return new(AdminRoleUpdateOutcome.IncompatiblePermission,null,incompatible);
        var required=PermissionConstants.All.Where(x=>x.Role==role&&x.IsRequired).Select(x=>x.Code).ToHashSet();
        if(!required.IsSubsetOf(codes))return new(AdminRoleUpdateOutcome.MandatoryPermissionMissing);
        await repository.ReplaceAsync(admin,role,codes,"AdminUpdateRolePermissions",token);
        return new(AdminRoleUpdateOutcome.Success,await DetailsAsync(name,token));
    }
    public async Task<AdminRoleUpdateResult> ResetAsync(int admin,string name,CancellationToken token=default)
    {
        if(!PermissionConstants.TryRole(name,out var role))return new(AdminRoleUpdateOutcome.UnknownRole);
        await repository.ReplaceAsync(admin,role,PermissionConstants.Defaults[role].ToList(),"AdminResetRolePermissions",token);
        return new(AdminRoleUpdateOutcome.Success,await DetailsAsync(name,token));
    }
    private static string Description(UserRole role)=>role switch{UserRole.Candidate=>"Applies for jobs and manages their profile.",UserRole.Recruiter=>"Creates jobs and manages candidates.",UserRole.HiringManager=>"Reviews candidates and makes hiring decisions.",_=>"Manages users, roles, and platform configuration."};
}
