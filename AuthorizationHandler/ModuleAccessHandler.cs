using HMS.Data;
using HMS.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ModuleAccessHandler : AuthorizationHandler<ModuleAccessRequirement>
    {
        private readonly ApplicationDbContext db;

        public ModuleAccessHandler(ApplicationDbContext db)
        {
            this.db = db;
        }

        protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, ModuleAccessRequirement requirement)
        {
            HttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            var loginRole = httpContextAccessor.HttpContext.Request.Cookies["LoginRole"];

            if (!context.User.Identity.IsAuthenticated || loginRole == null)
            {
                await Task.FromResult(0);
            }
            else
            {
                var accessModule = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule).ThenInclude(x=> x.Module)
                    .Where(x => x.IdentityRole.Name.Trim().Replace(" ", string.Empty).ToLower() == loginRole.StringCompareFormat()
                    && x.PageNamesInModule.Module.ModuleName.Trim().Replace(" ", string.Empty).ToLower() == requirement.AccessModuleName.StringCompareFormat()
                    && x.Status).FirstOrDefaultAsync();
                if (accessModule == null)
                {
                    await Task.FromResult(0);
                }
                else
                {
                    context.Succeed(requirement);
                }
            }
            await Task.FromResult(0);
        }
    }
}
