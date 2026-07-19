using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.AdminRoles;

public sealed record PermissionDefinitionDto(string Code,string DisplayName,string Description,string Category);
public sealed record AdminRoleListItemDto(string RoleName,string DisplayName,string Description,int UserCount,int PermissionCount,bool IsSystemRole);
public sealed record AdminRolePermissionDto(string Code,string DisplayName,string Description,string Category,bool IsAssigned,bool IsRequired,bool IsCompatible,string? DisabledReason);
public sealed record AdminRoleDetailsDto(string RoleName,string DisplayName,string Description,bool IsSystemRole,int UserCount,IReadOnlyList<AdminRolePermissionDto> Permissions);
public sealed class UpdateRolePermissionsRequest
{
    [Required] public List<string> PermissionCodes { get; set; } = [];
}
public enum AdminRoleUpdateOutcome { Success, UnknownRole, UnknownPermission, DuplicatePermission, IncompatiblePermission, MandatoryPermissionMissing }
public sealed record AdminRoleUpdateResult(AdminRoleUpdateOutcome Outcome,AdminRoleDetailsDto? Role=null,string? InvalidCode=null);
