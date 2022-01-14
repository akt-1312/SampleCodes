using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.Reception.Controllers
{
    [Area("Reception")]
    public class ReceptionDashboardController : Controller
    {
        [Authorize(Policy = "ReceptionModule")]
        public IActionResult ReceptionDashboard()
        {
            return View();
        }
    }
}