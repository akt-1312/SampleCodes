using HMS.Data;
using HMS.Extensions;
using HMS.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ActionAccessHandler : AuthorizationHandler<ActionAccessRequirement>
    {
        private readonly ApplicationDbContext db;

        public ActionAccessHandler(ApplicationDbContext db)
        {
            this.db = db;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ActionAccessRequirement requirement)
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
                    var actionOfpagesInRole = new PagesInRole();
                    if(requirement.AccessActionName.StringCompareFormat() == "create")
                    {
                        actionOfpagesInRole = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule)
                    .Where(x => x.IdentityRole.Name.Trim().Replace(" ", string.Empty).ToLower() == loginRole.StringCompareFormat()
                    && x.PageNamesInModule.PageName.Trim().Replace(" ", string.Empty).ToLower() == requirement.AccessViewName.StringCompareFormat()
                    && x.Status && x.IsCreateAccess).FirstOrDefaultAsync();
                    }else if(requirement.AccessActionName.StringCompareFormat() == "update")
                    {
                        actionOfpagesInRole = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule)
                    .Where(x => x.IdentityRole.Name.Trim().Replace(" ", string.Empty).ToLower() == loginRole.StringCompareFormat()
                    && x.PageNamesInModule.PageName.Trim().Replace(" ", string.Empty).ToLower() == requirement.AccessViewName.StringCompareFormat()
                    && x.Status && x.IsUpdateAccess).FirstOrDefaultAsync();
                    }
                    else
                    {
                        actionOfpagesInRole = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule)
                    .Where(x => x.IdentityRole.Name.Trim().Replace(" ", string.Empty).ToLower() == loginRole.StringCompareFormat()
                    && x.PageNamesInModule.PageName.Trim().Replace(" ", string.Empty).ToLower() == requirement.AccessViewName.StringCompareFormat()
                    && x.Status && x.IsDeleteAccess).FirstOrDefaultAsync();
                    }
                    if (actionOfpagesInRole == null)
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
