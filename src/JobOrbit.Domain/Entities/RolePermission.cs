using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class RolePermission
{
    public UserRole Role { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
