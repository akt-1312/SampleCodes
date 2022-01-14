using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using HMS.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationDiagnosisAndPlanController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment hostingEnvironment;

        public DrConsultationDiagnosisAndPlanController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.hostingEnvironment = hostingEnvironment;

        }
        [HttpGet]
        public async Task<IActionResult> DrConsultationDiagnosisAndPlan(int appointmentId)
        {


            DrConsultationDiagnosisAndPlanViewModel model = new DrConsultationDiagnosisAndPlanViewModel();


            string uploadFolders = Path.Combine(hostingEnvironment.WebRootPath, SD.RemarkTextFileFolder);
            string uniqueFileName = ".txt";
            string filePath = Path.Combine(uploadFolders, uniqueFileName);
            string remarkText = System.IO.File.Exists(filePath) ? System.IO.File.ReadAllText(filePath) : "";
            model.Context_1 = remarkText;

            model.Content = remarkText;

            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await db.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.DiagnosisAndPlan;

            return View(model);
        }

        private string UploadedFile(DrConsultationDiagnosisAndPlanViewModel BsteVm)
        {
            string uniqueFileName = null;
            if (BsteVm.TextFile != null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + "_" + BsteVm.TextFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\remarkFiles", uniqueFileName);
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    BsteVm.TextFile.CopyTo(filestream);
                }
                //patientRegistrationVm.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            return uniqueFileName;
        }
    }
}
