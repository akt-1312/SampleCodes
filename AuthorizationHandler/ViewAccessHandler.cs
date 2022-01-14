using HMS.Data;
using HMS.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ViewAccessHandler : AuthorizationHandler<ViewAccessRequirement>
    {
        private readonly ApplicationDbContext db;

        public ViewAccessHandler(ApplicationDbContext db)
        {
            this.db = db;
        }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewAccessRequirement requirement)
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
                    var pagesInRole = await db.PagesInRoles.Include(x => x.IdentityRole).Include(x => x.PageNamesInModule)
                    .Where(x => x.IdentityRole.Name.Trim().Replace(" ", string.Empty).ToLower() == loginRole.StringCompareFormat()
                    && x.PageNamesInModule.PageName.Trim().Replace(" ", string.Empty).ToLower() == requirement.AccessViewName.StringCompareFormat()
                    && x.Status).FirstOrDefaultAsync();
                    if (pagesInRole == null)
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
