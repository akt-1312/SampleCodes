using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.Enums;
using HMS.Models.Nurse;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationVitalSignsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DrConsultationVitalSignsViewModel DrConsultationVitalSignsVM{ get; set; }
        public DrConsultationVitalSignsController(ApplicationDbContext context)
        {
            this._context = context;
            DrConsultationVitalSignsViewModel DrConsultationVitalSignsVM = new DrConsultationVitalSignsViewModel()
            {
                VitalSignSetups = _context.VitalSignSetups.ToList(),
                PatientVitalInfos = new List<PatientVitalInfo>(),
                totalPVS=new List<PatientVitalInfo>(),
                DoctorAppointmentDataList = new List<DoctorAppointmentData>(),
            };
        }
        [HttpGet]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> DrConsultationVitalSigns(int appointmentId)
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            DrConsultationVitalSignsViewModel model = new DrConsultationVitalSignsViewModel();
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await _context.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.VitalSigns;
            PatientVitalInfo pinfo = new PatientVitalInfo();
            model.VitalSignSetups = await _context.VitalSignSetups.OrderBy(x => x.Description).ToListAsync();
            var totalPVS = _context.PatientVitalInfos.Where(x => x.AppointmentId == appointmentId && x.PatientVitalInfoId != 0)
             .OrderBy(x => x.CreatedDate).ToList();
            foreach (var item in model.VitalSignSetups)
            {
                int currentVsId = item.VitalSignSetupId;
                int currentUomId = item.UnitOfMeasurementId;
                string uomDes = _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId == currentUomId)
                    .Select(x => x.Description).Single();
                pinfo = new PatientVitalInfo
                {
                    UnitOfMeasurementDescription = uomDes,
                };
                model.AppointmentId = appointmentId;
                model.PatientVitalInfos.Add(pinfo);
            }
            model.totalPVS = totalPVS;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DrConsultationVitalSigns(DrConsultationVitalSignsViewModel model,int appointmentId)
        {
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await _context.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();
            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.VitalSigns;
            DateTime VitalSignDate = DateTime.UtcNow.AddMinutes(390);
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalVInfo = _context.PatientVitalInfos.ToList();
            //var patient = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == model.AppointmentId)
            //             .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
            //             .Include(a => a.Department).FirstOrDefaultAsync();
            //model.DoctorAppointmentData = patient;
            model.VitalSignSetups = await _context.VitalSignSetups.OrderBy(x => x.Description).ToListAsync();
            List<PatientVitalInfo> PatientVInfos = new List<PatientVitalInfo>();
            PatientVitalInfo pvinfo = new PatientVitalInfo();
            foreach (var patientVInfo in model.VitalSignSetups)
            {
                int VSId = patientVInfo.VitalSignSetupId;
                int uomId = patientVInfo.UnitOfMeasurementId;
                pvinfo = new PatientVitalInfo
                {
                    VitalSignDescription = patientVInfo.Description,
                    AppointmentId = model.AppointmentId,
                    UnitOfMeasurementDescription = _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId == uomId).Select(x => x.Description).SingleOrDefault(),
                    CreatedDate = CreatedDate,
                    UpdatedDate = UpdatedDate,
                    VitalSignDate = VitalSignDate,
                    CurrentValue = model.PatientVitalInfo.CurrentValue,
                    Remarks = model.PatientVitalInfo.Remarks,
                };
                PatientVInfos.Add(pvinfo);

            }

            try
            {
                foreach (var item in PatientVInfos)
                {
                    await _context.PatientVitalInfos.AddAsync(item);
                }
                await _context.SaveChangesAsync();
                TempData["SuccessedAlertMessage"] = "Save Successful";
                return RedirectToAction(nameof(DrConsultationVitalSigns),new { appointmentId=appointmentId});
                //return View(model);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (model.PatientVitalInfo.AppointmentId != 0)
                {
                    return NotFound();
                }
                else
                {
                    return View(model);
                }
            }
        }
    }
}
