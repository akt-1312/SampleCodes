using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ActionAccessRequirement : IAuthorizationRequirement
    {
        public ActionAccessRequirement(string accessViewName, string accessActionName)
        {
            AccessViewName = accessViewName;
            AccessActionName = accessActionName;
        }

        public string AccessViewName { get; set; }

        public string AccessActionName { get; set; }
    }
}
