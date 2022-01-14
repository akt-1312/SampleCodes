using HMS.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class RoleAccessHandler : AuthorizationHandler<RoleAccessRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleAccessRequirement requirement)
        {
            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            var loginRole = httpContextAccessor.HttpContext.Request.Cookies["LoginRole"];

            if (!context.User.Identity.IsAuthenticated || loginRole == null)
            {
                await Task.FromResult(0);
            }
            else
            {
                try
                {                   
                    if (loginRole.StringCompareFormat() != requirement.AccessRoleName.StringCompareFormat())
                    {
                        await Task.FromResult(0);
                    }
                    else
                    {
                        context.Succeed(requirement);
                    }
                }
                catch (Exception)
                {
                    await Task.FromResult(0);
                }

            }
            await Task.FromResult(0);

        }
    }
}
