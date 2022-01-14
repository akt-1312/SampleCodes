﻿using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.AuthorizationHandler
{
    public class ModuleAccessRequirement : IAuthorizationRequirement
    {
        public ModuleAccessRequirement(string accessModuleName)
        {
            AccessModuleName = accessModuleName;
        }

        public string AccessModuleName { get; set; }
    }
}
