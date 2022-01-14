using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationCostEstimationController : Controller
    {
        private readonly ApplicationDbContext db;

        public DrConsultationCostEstimationController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DrConsultationCostEstimation(int appointmentId)
        {
            DrConsultationCostEstimationViewModel model = new DrConsultationCostEstimationViewModel();
            var consultationPatient = await db.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();
            model.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            return View(model);
        }
    }
}
