using JobOrbit.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace JobOrbit.API.Authorization;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement;

public sealed class PermissionAuthorizationHandler(ICurrentUserPermissionService permissions)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,PermissionRequirement requirement)
    {
        if(await permissions.HasAsync(context.User,requirement.Code)) context.Succeed(requirement);
    }
}
