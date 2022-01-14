using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Enums;
using HMS.Models.Reception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Reception.Controllers
{
    [Authorize]
    [Area("Reception")]
    public class OnlinePatientController : Controller
    {
        private readonly ApplicationDbContext db;

        public OnlinePatientController(ApplicationDbContext db)
        {
            this.db = db;
        }

        [HttpGet]
        public async Task<IActionResult> OnlinePatient()
        {
            //List<NewPatientBookingAppointment> onlinePatientList = new List<NewPatientBookingAppointment>();
            //List<DoctorAppointmentData> doctorAppointmentDatas = new List<DoctorAppointmentData>();
            var doctorAppointmentDatas = await db.DoctorAppointmentDatas
                .Where(x => (x.NewBookingPatientId.HasValue || x.PatientRegId.HasValue) && x.AppointmentStatus == AppointmentStatus.Booked 
                && x.AppointmentDate.Date >= DateTime.UtcNow.AddMinutes(390).Date)
                .Include(x => x.NewPatientBookingAppointment).Include(x=> x.PatientRegistration).Include(x => x.DoctorInfo)
                .Include(x => x.Unit).Include(x => x.Department).OrderByDescending(x => x.AppointmentDate).ThenBy(x=> x.VisitNo).ToListAsync();
            //doctorAppointmentDatas = doctorAppointmentDatas != null
            //    ?
            //    doctorAppointmentDatas.OrderBy(x => x.NewPatientBookingAppointment.FullName).ToList()
            //    :
            //    new List<DoctorAppointmentData>();
            //doctorAppointmentDatas = doctorAppointmentDatas != null ? doctorAppointmentDatas.OrderBy(x=> x.VisitNo).ToList() : new List<DoctorAppointmentData>();
            doctorAppointmentDatas = doctorAppointmentDatas ?? new List<DoctorAppointmentData>();
            return View(doctorAppointmentDatas);
        }
    }
}
