using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class AdminSettingDashboardController : Controller
    {
        [Authorize(Policy = "SetupSettingModule")]
        public IActionResult AdminSettingDashboard()
        {
            return View();
        }
    }
}