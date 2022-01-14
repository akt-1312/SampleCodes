using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationOrderDrugController : Controller
    {
        private readonly ApplicationDbContext db;
        public DrConsultationOrderDrugViewModel DrConsultationOrderDrugVM { get; set; }
        public DrConsultationOrderDrugController(ApplicationDbContext db)
        {
            this.db = db;
            DrConsultationOrderDrugViewModel DrConsultationOrderDrugVM = new DrConsultationOrderDrugViewModel()
            {
                DoctorAppointmentDataList = new List<DoctorAppointmentData>(),
            };
        }

        [HttpGet]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DrConsultationOrderDrug(int appointmentId)
        {
            DrConsultationOrderDrugViewModel model = new DrConsultationOrderDrugViewModel();
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await db.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.OrderSubFormName = Models.Enums.OrderActionSubFormEnum.Drugs;
            return View(model);
        }
    }
}