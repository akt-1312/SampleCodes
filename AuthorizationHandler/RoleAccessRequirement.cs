using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class RoleAccessRequirement : IAuthorizationRequirement
    {
        public RoleAccessRequirement(string accessRoleName)
        {
            AccessRoleName = accessRoleName;
        }

        public string AccessRoleName { get; set; }
    }
}
