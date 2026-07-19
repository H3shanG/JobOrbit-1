using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsSystemPermission { get; set; } = true;
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
