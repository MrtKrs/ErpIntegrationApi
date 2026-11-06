using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler()
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Instantly reject anonymous requests before anything else runs.
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            context.Fail();
            return;
        }

        // Check if the authenticated identity possesses the required permission claim string
        // extracted from the database during the authentication phase
        HashSet<string> assignedPermissions = context.User.Claims
            .Where(c => c.Type == "permissions")
            .Select(c => c.Value)
            .ToHashSet();

        if (assignedPermissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}
