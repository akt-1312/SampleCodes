using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using HMS.Models.ViewModels.Reception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DoctorWorkBenchController : Controller
    {
        private readonly ApplicationDbContext db;

        public DoctorWorkBenchController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllPatientsForDoctor()
        {
            var allEncounteredPatients = await db.DoctorAppointmentDatas.Include(x => x.PatientRegistration)
                .Include(x => x.DoctorInfo).Include(x => x.Department).Include(x => x.Unit)
                .Where(x => x.PatientRegId.HasValue && x.AppointmentSituation == EncounterEnum.Encounter && x.AppointmentDate.Date >= DateTime.UtcNow.AddMinutes(390).Date)
                .OrderBy(x => x.Department.DepartmentName).ThenByDescending(x => x.AppointmentTime).ToListAsync();

            List<ViewAllPatientsViewModel> model = allEncounteredPatients != null ? allEncounteredPatients.Select(x => new ViewAllPatientsViewModel()
            {
                DoctorAppointmentData = x
            }).ToList() : new List<ViewAllPatientsViewModel>();

            //allEncounteredPatients = allEncounteredPatients ?? new List<DoctorAppointmentData>();

            return View(model);
        }       
    }
}
