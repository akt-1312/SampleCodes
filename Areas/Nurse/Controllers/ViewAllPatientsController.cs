using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Areas.Administration.Controllers;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Reception;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.Util;

namespace HMS.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    public class ViewAllPatientsController : Controller
    {
        private readonly ApplicationDbContext db;

        public ViewAllPatientsController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> ViewAllPatients()
        {
            //var allEncounteredPatients = await db.DoctorAppointmentDatas.Include(x => x.PatientRegistration)
            //    .Where(x => x.PatientRegId.HasValue && x.AppointmentSituation == EncounterEnum.Encounter)
            //    .OrderByDescending(x=> x.AppointmentTime).ToListAsync();
            //var allPatients = new List<ViewAllPatientsViewModel>();
            //if (allEncounteredPatients != null || allEncounteredPatients.Count > 0)
            //{
            //    allPatients = allEncounteredPatients.GroupBy(x => x.PatientRegId).Select(patients => new ViewAllPatientsViewModel()
            //                    {
            //                        PatientRegistration = patients.ToList().FirstOrDefault().PatientRegistration,
            //                        DoctorAppointmentData = patients.ToList().LastOrDefault()
            //                    }).ToList();
            //}
            //return View(allPatients);

            //var test = await db.DoctorAppointmentDatas.Include(x => x.PatientRegistration)
            //    .Include(x => x.DoctorInfo).Include(x => x.Department).Include(x => x.Unit)
            //    .Where(x => x.PatientRegId.HasValue && x.AppointmentSituation == EncounterEnum.Encounter && x.AppointmentDate.Date >= DateTime.UtcNow.AddMinutes(390).Date)
            //    .OrderBy(x => x.Department.DepartmentName).ThenByDescending(x => x.AppointmentTime).Select(x=> new ViewAllPatientsViewModel() 
            //    { 
            //        DoctorAppointmentData = x
            //    }).ToListAsync();

            try
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
            catch
            {
                return View("Error");
            }
        }
    }   
}
