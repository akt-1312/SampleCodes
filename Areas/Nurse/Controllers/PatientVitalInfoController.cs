using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Nurse;
using Microsoft.AspNetCore.Authorization;
using HMS.Models.ViewModels.Nurse;
using HMS.Models.Reception;

namespace HMS.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    public class PatientVitalInfoController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PatientVitalInfoViewModel PatientVitalInfoVM { get; set; }
        public PatientVitalInfoController(ApplicationDbContext context)
        {
            _context = context;
            PatientVitalInfoViewModel PatientVitalInfoVM = new PatientVitalInfoViewModel()
            {
                VitalSignSetups = _context.VitalSignSetups.ToList(),
                PatientVitalInfos = new List<PatientVitalInfo>(),
                DoctorAppointmentDataList=new List<DoctorAppointmentData>(),
            };

        }
        // GET: CommonAjaxCall/Township
        [HttpGet]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> PatientVitalInfo(int? PatientVitalInfoId,int appointmentId)
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            PatientVitalInfoViewModel model = new PatientVitalInfoViewModel();
            model.PatientVitalInfos = new List<PatientVitalInfo>();
            PatientVitalInfo pinfo = new PatientVitalInfo();
            var patient = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == appointmentId)
                        .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                        .Include(a => a.Department).FirstOrDefaultAsync();
            model.DoctorAppointmentData = patient;
            model.VitalSignSetups = await _context.VitalSignSetups.OrderBy(x => x.Description).ToListAsync();
            foreach (var item in model.VitalSignSetups)
            {
                int currentVsId = item.VitalSignSetupId;
                int currentUomId = item.UnitOfMeasurementId;
                string uomDes = _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId == currentUomId).Select(x => x.Description).Single();
                pinfo = new PatientVitalInfo
                {
                    UnitOfMeasurementDescription = uomDes,
                };
                model.AppointmentId = appointmentId;
                model.PatientVitalInfos.Add(pinfo);
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientVitalInfo(PatientVitalInfoViewModel model,int appointmentId)
        {
            DateTime VitalSignDate = DateTime.UtcNow.AddMinutes(390);
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalVInfo = _context.PatientVitalInfos.ToList();
            var patient = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == model.AppointmentId)
                         .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                         .Include(a => a.Department).FirstOrDefaultAsync();
            model.DoctorAppointmentData = patient;
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
                    UnitOfMeasurementDescription = _context.UnitOfMeasurements.Where(x => x.UnitOfMeasurementId == uomId).Select(x => x.Code).SingleOrDefault(),
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
                return RedirectToAction(nameof(PatientVitalInfo), new { appointmentId = appointmentId });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (model.PatientVitalInfo.AppointmentId!=0)
                {
                    return NotFound();
                }
                else
                {
                    return View(model);
                }
            }
        }
        public IActionResult Index(int appointmentId)
        {
            return View();
        } 


    }
}


