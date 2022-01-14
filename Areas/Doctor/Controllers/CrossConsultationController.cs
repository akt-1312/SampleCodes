using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Doctor;
using HMS.Models.ViewModels.Doctor;
using HMS.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using HMS.Models.Reception;
//using HMS.Migrations;



namespace HMS.Areas.Doctor
{
    [Area("Doctor")]
    public class CrossConsultationController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public CrossConsultationViewModel CrossConsultationViewModel { get; set; }

        public CrossConsultationController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            CrossConsultationViewModel = new CrossConsultationViewModel()
            {
                DepartmentList = new List<Department>(),
                DoctorInfoList = new List<DoctorInfo>(),

                CrossConsultationList = new List<CrossConsultation>(),
                DoctorAppointmentDataList = new List<DoctorAppointmentData>()
            };

        }





        [Authorize(Policy = "CrossConsultationView")]

        [HttpGet]
        public async Task<IActionResult> CrossConsultation(int? CrossConsultationId, string btnActionName,int appointmentId)
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            CrossConsultationViewModel model = new CrossConsultationViewModel();
            var consultations = await _context.CrossConsultations.Where(v => v.AppointmentId == appointmentId).ToListAsync();

            var totalCrossConsultation =await _context.CrossConsultations.Include(x => x.DoctorAppointmentData)
                                         .Include(x => x.CrossConsultationDoctorDepartment)
                                         .Include(a => a.CrossConlultationDoctorInfo).ToListAsync();
            var current = totalCrossConsultation.Where(a => a.CrossConsultationId == CrossConsultationId).FirstOrDefault();
            var patient = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == appointmentId)
                        .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                        .Include(a => a.Department).FirstOrDefaultAsync();

            model.DepartmentList = await _context.Departments.ToListAsync();
            model.DoctorInfoList = await _context.DoctorInfos.ToListAsync();
            model.CrossConsultationList = consultations;
            model.DoctorAppointmentData = patient;
            if (CrossConsultationId == null || CrossConsultationId.Value == 0)
            {
                model.CrossConsultation = new CrossConsultation();
                model.BtnActionName = "Create";
                return View(model);
            }
            else
            {
                if (!((await authorizationService.AuthorizeAsync(User, "CrossConsultationUpdate")).Succeeded ||
                  (await authorizationService.AuthorizeAsync(User, "CrossConsultationDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                }
                var toUpdateCrossConsultation = await _context.CrossConsultations.Include(x => x.CrossConsultationDoctorDepartment)
                        .Include(x => x.DoctorAppointmentData).Include(a=>a.CrossConlultationDoctorInfo)
                        .Where(x => x.CrossConsultationId == CrossConsultationId.Value).FirstOrDefaultAsync();

                List<CrossConsultation> lst = new List<CrossConsultation>();
                lst = await _context.CrossConsultations.Where(a => a.CrossConsultationId == CrossConsultationId).ToListAsync();
                CrossConsultationViewModel moduleVm = new CrossConsultationViewModel()
                {
                    CrossConsultationList = totalCrossConsultation,
                    CrossConsultation = lst.FirstOrDefault(),
                    BtnActionName = btnActionName,
                };
                model.CrossConsultationDoctorId = toUpdateCrossConsultation.CrossConsultationDoctorId;
                model.DoctorName = toUpdateCrossConsultation.CrossConlultationDoctorInfo.DoctorName;
                model.CrossConsultationDepartmentId = toUpdateCrossConsultation.CrossConsultationDepartmentId;
                model.DepartmentName = toUpdateCrossConsultation.CrossConsultationDoctorDepartment.DepartmentName;
                model.Reason = toUpdateCrossConsultation.Reason;
                model.HospitalName = toUpdateCrossConsultation.HospitalName;
                model.CrossConsultationRequestType = toUpdateCrossConsultation.CrossConsultationRequestType;
                model.CrossConsultationType = toUpdateCrossConsultation.CrossConsultationType;
                model.Priority = toUpdateCrossConsultation.Priority;
                model.DoctorAppointmentData = toUpdateCrossConsultation.DoctorAppointmentData;

                if (btnActionName.ToLower().Trim() == "edit")
                {
                    model.BtnActionName = "Edit";
                }
                else
                {
                    model.BtnActionName = "Delete";
                }
                return View(model);
            }

        }
        [Authorize(Policy = "CrossConsultationView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrossConsultation(int? CrossConsultationId, CrossConsultationViewModel model, string btnSubmit)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var crossConsultation = await _context.CrossConsultations.Where(v => v.AppointmentId == model.DoctorAppointmentData.AppointmentId).
                                     Include(a => a.CrossConlultationDoctorInfo).Include(a => a.CrossConsultationDoctorDepartment).ToListAsync();

            var patient = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == model.DoctorAppointmentData.AppointmentId)
                        .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                        .Include(a => a.Department).FirstOrDefaultAsync();

            model.DoctorAppointmentData = patient;
            model.DoctorInfoList = await _context.DoctorInfos.ToListAsync();
            model.DepartmentList = await _context.Departments.ToListAsync();

            model.CrossConsultationList = crossConsultation;
            CrossConsultation Consultation = new CrossConsultation();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (ModelState.IsValid)
            {
                if (btnSubmit == "edit")
                {
                    if (!(await authorizationService.AuthorizeAsync(User, "CrossConsultationUpdate")).Succeeded)
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                    }
                    try
                    {
                        if (model.CrossConsultationId == null || model.CrossConsultationId.Value == 0)
                        {
                            return View("Error");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(model.HospitalName) ||
                                  model.CrossConsultationDepartmentId == 0 ||
                                  model.CrossConsultationDoctorId == 0 ||
                                  model.CrossConsultationType == null)
                      
                            {
                                if (string.IsNullOrEmpty(model.HospitalName))
                                {

                                    ModelState.AddModelError("HospitalName", "HospitalName field is required");
                                }
                                if (model.CrossConsultationType == null)
                                {
                                    ModelState.AddModelError("CrossConsultationType", "CrossConsultationType field is required");

                                }

                                if (model.CrossConsultationDepartmentId == 0)
                                {
                                    ModelState.AddModelError("CrossConsultationDepartmentId", "CrossConsultationDepartmentId field is required");

                                }
                                if (model.CrossConsultationDoctorId == 0)
                                {
                                    ModelState.AddModelError("CrossConsultationDoctorId", "CrossConsultationDoctorId field is required");

                                }
                                return View(model);

                            }
                            else
                            {
                                var updatedConsultation = await _context.CrossConsultations.FindAsync(model.CrossConsultationId.Value);
                                if (updatedConsultation != null)
                                {
                                    updatedConsultation.HospitalName = model.HospitalName;
                                    updatedConsultation.Reason = model.Reason;
                                    updatedConsultation.CrossConsultationRequestType = model.CrossConsultationRequestType;
                                    updatedConsultation.Priority = model.Priority;
                                    updatedConsultation.CrossConsultationType = model.CrossConsultationType;
                                    updatedConsultation.CrossConsultationDepartmentId = model.CrossConsultationDepartmentId;
                                    updatedConsultation.CrossConsultationDoctorId = model.CrossConsultationDoctorId;
                                    updatedConsultation.AppointmentId = model.DoctorAppointmentData.AppointmentId;
                                    updatedConsultation.CurrentConsultationDoctorId = model.DoctorAppointmentData.DoctorId;
                                    updatedConsultation.UpdatedDate = UpdatedDate;
                                    _context.CrossConsultations.Update(updatedConsultation);
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(CrossConsultation), new { appointmentId = model.DoctorAppointmentData.AppointmentId });
                                }
                                else
                                {
                                    return View("Error");
                                }
                            }

                            

                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }

                }
                else if (btnSubmit == "delete")
                {
                    if (!(await authorizationService.AuthorizeAsync(User, "CrossConsultationDelete")).Succeeded)
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                    }
                    try
                    {
                        if (model.CrossConsultationId == null || model.CrossConsultationId.Value == 0)
                        {
                            return View("Error");
                        }
                        else
                        {
                            var updatedConsultation= await _context.CrossConsultations.FindAsync(model.CrossConsultationId.Value);
                            if (updatedConsultation != null)
                            {
                                _context.CrossConsultations.Remove(updatedConsultation);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Delete Successful";
                                return RedirectToAction(nameof(CrossConsultation), new { appointmentId = model.DoctorAppointmentData.AppointmentId });


                            }
                            else
                            {
                               
                               return View("Error");
                            }
                        }
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        throw;
                    }


                }
                else
                {
                    if (!(await authorizationService.AuthorizeAsync(User, "CrossConsultationCreate")).Succeeded)
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                    }
                    if (string.IsNullOrEmpty(model.HospitalName) ||
                        model.CrossConsultationDepartmentId == 0 ||
                        model.CrossConsultationDoctorId == 0 ||
                        model.CrossConsultationType == null 
                       )


                    {
                        if (string.IsNullOrEmpty(model.HospitalName))
                        {

                            ModelState.AddModelError("HospitalName", "HospitalName field is required");
                        }
                        if (model.CrossConsultationType == null)
                        {
                            ModelState.AddModelError("CrossConsultationType", "CrossConsultationType field is required");

                        }
                       
                        if (model.CrossConsultationDepartmentId == 0)
                        {
                            ModelState.AddModelError("CrossConsultationDepartmentId", "CrossConsultationDepartmentId field is required");

                        }
                        if (model.CrossConsultationDoctorId == 0)
                        {
                            ModelState.AddModelError("CrossConsultationDoctorId", "CrossConsultationDoctorId field is required");

                        }
                        return View(model);

                    }
                    else
                    {
                        Consultation.Reason = model.Reason;
                        Consultation.CrossConsultationRequestType = model.CrossConsultationRequestType;
                        Consultation.HospitalName = model.HospitalName;
                        Consultation.CrossConsultationDepartmentId = model.CrossConsultationDepartmentId;
                        Consultation.AppointmentId = model.DoctorAppointmentData.AppointmentId;
                        Consultation.CurrentConsultationDoctorId = model.DoctorAppointmentData.DoctorId;
                        Consultation.Priority = model.Priority;
                        Consultation.CrossConsultationType = model.CrossConsultationType;
                        Consultation.CrossConsultationDoctorId = model.CrossConsultationDoctorId;
                        Consultation.CreatedDate = CreatedDate;
                        await _context.CrossConsultations.AddAsync(Consultation);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Save Successful";
                    }
                         
                   
                }
                return RedirectToAction(nameof(CrossConsultation), new { appointmentId = model.DoctorAppointmentData.AppointmentId });
            }
            else
            {
                
                return View("Error");

            }



        }
       




    }
}
