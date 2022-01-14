using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationPatientHistoryAndExamController : Controller
    {
        private readonly ApplicationDbContext db;

        public DrConsultationPatientHistoryAndExamController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public async Task<IActionResult> DrConsultationPatientHistoryAndExam(int appointmentId)
        {
            DrConsultationPatientHistoryAndExamViewModel model = new DrConsultationPatientHistoryAndExamViewModel();
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await db.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.PatientHistoryAndExam;

            return View(model);
        }
    }
}
