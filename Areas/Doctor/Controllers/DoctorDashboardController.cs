using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DoctorDashboardController : Controller
    {
        [Authorize(Policy = "DoctorModule")]
        public IActionResult DoctorDashboard()
        {
            return View();
        }
    }
}
