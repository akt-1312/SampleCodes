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
    public class DrConsultationOrdersController : Controller
    {
        private readonly ApplicationDbContext db;

        public DrConsultationOrdersController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public async Task<IActionResult> DrConsultationOrders(int appointmentId)
        {
            DrConsultationOrdersViewModel model = new DrConsultationOrdersViewModel();
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await db.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.Orders;

            return View(model);
        }
       
    }
}
