using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ViewAccessRequirement : IAuthorizationRequirement
    {
        public ViewAccessRequirement(string accessViewName)
        {
            AccessViewName = accessViewName;
        }

        public string AccessViewName { get; set; }
    }
}
