using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    public class NursingDashboardController : Controller
    {
        [Authorize(Policy = "NursingModule")]
        public IActionResult NursingDashboard()
        {
            return View();
        }
    }
}
