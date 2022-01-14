using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using HMS.Data;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class AdministrationDashboardController : Controller
    {
        public AdministrationDashboardViewModel AdminVM { get; set; }
        private readonly ApplicationDbContext _context;
        public AdministrationDashboardController(ApplicationDbContext context)
        {
            _context = context;
            AdminVM = new AdministrationDashboardViewModel()
            {

                DoctorInfos = new List<DoctorInfo>(),
                Departments = new List<Department>(),
                Currencies = new List<Currency>(),

            };
        }

        [Authorize(Policy = "AdminRoleMenu")]
        public IActionResult AdministrationDashboard()
        {
            ViewBag.TotalUsers = _context.Users.ToList();
            ViewBag.TotalRoles = _context.Roles.ToList();
            var totalDepartments = _context.Departments.ToList();
            var totalDoctorInfos = _context.DoctorInfos.ToList();
            var totalCurrencies = _context.Currencies.ToList();
            AdministrationDashboardViewModel adminvm = new AdministrationDashboardViewModel();
            adminvm.Departments = totalDepartments ?? new List<Department>();
            adminvm.DoctorInfos = totalDoctorInfos ?? new List<DoctorInfo>();
            adminvm.Currencies = totalCurrencies ?? new List<Currency>();
            return View(adminvm);
        }
    }
}