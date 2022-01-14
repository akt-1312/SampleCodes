using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class SuperAdminDashboardController : Controller
    {
        [HttpGet]
        public IActionResult SuperAdminDashboard()
        {
            return View();
        }
    }
}