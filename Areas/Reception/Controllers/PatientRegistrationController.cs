using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Extensions;
using HMS.GlobalDependencyData;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Reception;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace HMS.Areas.Reception.Controllers
{
    [Authorize]
    [Area("Reception")]
    public class PatientRegistrationController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IAuthorizationService authorizationService;

        [BindProperty]
        public PatientRegistrationAKTViewModel PatientRegistrationVM { get; set; }

        public PatientRegistrationController(ApplicationDbContext _db,
                                                IWebHostEnvironment hostingEnvironment,
                                                IAuthorizationService authorizationService)
        {
            db = _db;
            this.hostingEnvironment = hostingEnvironment;
            this.authorizationService = authorizationService;
            PatientRegistrationVM = new PatientRegistrationAKTViewModel()
            {
                Prefixes = TheWholeGlobalSettingData.Prefixes,
                Countries = TheWholeGlobalSettingData.Countries,
                Genders = TheWholeGlobalSettingData.Genders,
                MaritalStatuses = TheWholeGlobalSettingData.MaritalStatuses,
                Nationalities = TheWholeGlobalSettingData.Nationalities,
                Occupations = TheWholeGlobalSettingData.Occupations,
                Relationships = TheWholeGlobalSettingData.Relationships,
                //Services = db.Services.ToList(),
                States = TheWholeGlobalSettingData.States,
                Townships = TheWholeGlobalSettingData.Townships,
                PatientRegistration = new PatientRegistration(),
                //RegistrationFee = new RegistrationFee(),
                //RegistrationFees = db.RegistrationFees.ToList(),
                //RegistrationTypes = db.RegistrationTypes.ToList(),
                ReferalHospitals = TheWholeGlobalSettingData.ReferalHospitals,
            };
        }

        [Authorize(Policy = "PatientRegistrationCreate")]
        [HttpGet]
        public async Task<IActionResult> PatientRegistrationCreate(string regName, int? bookedPatientId)
        {
            ViewBag.RegName = regName ?? RegistrationType.Normal.ToString();
            ViewBag.TodayDate = DateTime.UtcNow.AddMinutes(390).FormattedDateString();
            PatientRegistrationVM.PatientRegistration = new PatientRegistration();
            PatientRegistrationVM.PatientRegistration.RegDate = DateTime.UtcNow.AddMinutes(390);
            PatientRegistrationVM.PatientRegistration.RegistrationType = regName == null ? RegistrationType.Normal : RegistrationType.Emergency;
            if (bookedPatientId != null)
            {
                var bookedPatient = await db.NewPatientBookingAppointments.FindAsync(bookedPatientId.Value);
                if (bookedPatient != null)
                {
                    ViewBag.BookedPatientId = bookedPatientId.Value;
                    ViewBag.Age = bookedPatient.Age;

                    PatientRegistrationVM.PatientRegistration.PatientPrefix = bookedPatient.Prefix;
                    PatientRegistrationVM.PatientRegistration.PatientFirstName = bookedPatient.FirstName;
                    PatientRegistrationVM.PatientRegistration.PatientMiddleName = bookedPatient.MiddleName;
                    PatientRegistrationVM.PatientRegistration.PatientLastName = bookedPatient.LastName;
                    PatientRegistrationVM.PatientRegistration.Gender = bookedPatient.Gender;
                    PatientRegistrationVM.PatientRegistration.DOB = bookedPatient.DateOfBirth;
                    PatientRegistrationVM.PatientRegistration.Age = bookedPatient.Age;
                    PatientRegistrationVM.PatientRegistration.PatientMobile1 = bookedPatient.PhoneNo;
                }
            }
            //GetRegTypes(RegName);
            return View(PatientRegistrationVM);
        }

        [Authorize(Policy = "PatientRegistrationCreate")]
        [HttpPost, ActionName("PatientRegistrationCreate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientRegistrationCreate(string registrationFees, PatientRegistrationAKTViewModel patientRegistrationVm, string rdoMRNo, int? bookedPatientId)
        {
            PatientRegistrationVM = new PatientRegistrationAKTViewModel()
            {
                Prefixes = TheWholeGlobalSettingData.Prefixes,
                Countries = TheWholeGlobalSettingData.Countries,
                Genders = TheWholeGlobalSettingData.Genders,
                MaritalStatuses = TheWholeGlobalSettingData.MaritalStatuses,
                Nationalities = TheWholeGlobalSettingData.Nationalities,
                Occupations = TheWholeGlobalSettingData.Occupations,
                Relationships = TheWholeGlobalSettingData.Relationships,
                //Services = db.Services.ToList(),
                States = TheWholeGlobalSettingData.States,
                Townships = TheWholeGlobalSettingData.Townships,
                ReferalHospitals = TheWholeGlobalSettingData.ReferalHospitals,
                PatientRegistration = patientRegistrationVm.PatientRegistration,
                //RegistrationFee = new RegistrationFee(),
            };
            //string abc = registrationFees;
            ViewBag.rdoMRNo = rdoMRNo;
            ViewBag.RegName = patientRegistrationVm.PatientRegistration.RegistrationType.ToString();
            ViewBag.PatientIdentityType = patientRegistrationVm.PatientRegistration.PatientIdentityType;
            ViewBag.Age = patientRegistrationVm.PatientRegistration.Age;
            ViewBag.State = patientRegistrationVm.PatientRegistration.State;
            ViewBag.Township = patientRegistrationVm.PatientRegistration.Township;
            ViewBag.TodayDate = patientRegistrationVm.PatientRegistration.RegDate.FormattedDateString();
            ViewBag.BookedPatientId = bookedPatientId;

            if (ModelState.IsValid)
            {
                if (patientRegistrationVm.PatientRegistration.RegistrationType == RegistrationType.Normal)
                {
                    if (string.IsNullOrEmpty(patientRegistrationVm.PatientRegistration.PatientMobile1) ||
                        patientRegistrationVm.PatientRegistration.DOB == null ||
                        patientRegistrationVm.PatientRegistration.Age == null)
                    {
                        if (string.IsNullOrEmpty(patientRegistrationVm.PatientRegistration.PatientMobile1))
                        {
                            ModelState.AddModelError("PatientRegistration.PatientMobile1", "Mobile field is required");
                        }
                        if (patientRegistrationVm.PatientRegistration.DOB == null)
                        {
                            ModelState.AddModelError("PatientRegistration.DOB", "Date of Birth field is required");
                        }
                        if (patientRegistrationVm.PatientRegistration.Age == null)
                        {
                            ModelState.AddModelError("PatientRegistration.Age", "Age field is required");
                        }

                        return View(PatientRegistrationVM);
                    }
                }

                string finalRegMRNo = "";
                string regMRNo = "";
                string lastId = "";
                if (rdoMRNo == "Auto" /*"Defined"*/)
                {
                    string regDate = (DateTime.UtcNow.AddMinutes(390)).ToString("MM/dd/yy");
                    string[] regDateArr = regDate.Split('-', '/', '.', '_');
                    foreach (var item in regDateArr)
                    {
                        regMRNo += item;
                    }

                    var todayDate = DateTime.Parse(DateTime.UtcNow.AddMinutes(390).ToShortDateString());
                    var todayMRNos = (from data in db.PatientRegistrations where data.CreatedDate.Date == todayDate.Date select data).ToList().LastOrDefault();

                    if (todayMRNos == null)
                    {
                        finalRegMRNo = SD.constDefinedMRNo + regMRNo + 1.ToString();
                    }
                    else
                    {
                        lastId = todayMRNos.Reg_Id.ToString();
                        finalRegMRNo = SD.constDefinedMRNo + regMRNo + (int.Parse(lastId) + 1).ToString();
                    }
                }

                else if (rdoMRNo == "Manual" && string.IsNullOrEmpty(patientRegistrationVm.PatientRegistration.MRNo))
                {
                    ViewBag.NullMRNoError = "MRNo Field is required!";
                    return View(PatientRegistrationVM);
                }
                else if ((rdoMRNo == "Manual" && !string.IsNullOrEmpty(patientRegistrationVm.PatientRegistration.MRNo)) /*|| rdoMRNo == "Auto"*/)
                {
                    finalRegMRNo = patientRegistrationVm.PatientRegistration.MRNo;
                }

                try
                {
                    string uniqueFileName = ProcessUploadedFile(patientRegistrationVm);

                    patientRegistrationVm.PatientRegistration.MRNo = finalRegMRNo;
                    patientRegistrationVm.PatientRegistration.CreatedDate = DateTime.UtcNow.AddMinutes(390);
                    patientRegistrationVm.PatientRegistration.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                    patientRegistrationVm.PatientRegistration.PhotoPath = uniqueFileName;
                    patientRegistrationVm.PatientRegistration.CreatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    db.PatientRegistrations.Add(patientRegistrationVm.PatientRegistration);
                    await db.SaveChangesAsync();

                    PatientCase patientCase = new PatientCase();
                    patientCase.PatientRegId = patientRegistrationVm.PatientRegistration.Reg_Id;
                    patientCase.RegistrationType = patientRegistrationVm.PatientRegistration.RegistrationType;
                    patientCase.OptIpt = OptIpt.OutPatient;
                    patientCase.IsActive = true;
                    db.PatientCases.Add(patientCase);
                    await db.SaveChangesAsync();

                    List<PatientServiceFees> patientServiceFee = new List<PatientServiceFees>();

                    patientServiceFee = JsonConvert.DeserializeObject<List<PatientServiceFees>>(registrationFees);
                    foreach (PatientServiceFees regFee in patientServiceFee)
                    {
                        regFee.PatientCaseId = patientCase.PatientCaseId;
                        regFee.IsCashed = true;
                        regFee.CreatedDate = DateTime.UtcNow.AddMinutes(390);
                        //regFee.CurrencyCode = ;
                        db.PatientServiceFees.Add(regFee);
                    }
                    await db.SaveChangesAsync();

                    if (bookedPatientId != null)
                    {
                        var drAppointmentList = await db.DoctorAppointmentDatas.Where(x => x.AppointmentStatus == AppointmentStatus.Booked &&
                        x.NewBookingPatientId.HasValue && x.NewBookingPatientId == bookedPatientId).ToListAsync();
                        if (drAppointmentList != null && drAppointmentList.Count > 0)
                        {
                            var drAppointment = drAppointmentList.LastOrDefault();
                            drAppointment.PatientRegId = patientRegistrationVm.PatientRegistration.Reg_Id;

                            db.DoctorAppointmentDatas.Update(drAppointment);
                            await db.SaveChangesAsync();
                        }

                        //return RedirectToAction("PatientRegistrationUpdate", new { patientRegId = patientRegistrationVm.PatientRegistration.Reg_Id, isSaveSuccess = true, bookedPatientId = bookedPatientId });
                    }

                    TempData["SuccessedAlertMessage"] = "Patient Successfully Registered.";
                    return RedirectToAction("PatientRegistrationUpdate", new { patientRegId = patientRegistrationVm.PatientRegistration.Reg_Id, bookedPatientId = bookedPatientId });
                    //return RedirectToAction(nameof(PatientRegList));
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.ErrorTitle = "Saving patient registration error!";
                    ViewBag.ErrorMessage = "An error is occured when saving patient registration data. If you want to know " +
                        "details please contact us to " + SD.companyEmail;
                    return View("Error");
                }
            }
            else
            {
                return View(PatientRegistrationVM);
            }
        }

        private string ProcessUploadedFile(PatientRegistrationAKTViewModel patientRegistrationVm)
        {
            string uniqueFileName = null;
            if (patientRegistrationVm.Photo != null)
            {
                string uploadFolders = Path.Combine(hostingEnvironment.WebRootPath, SD.PatientRegImagesFolder);
                uniqueFileName = Guid.NewGuid().ToString() + "_" + patientRegistrationVm.Photo.FileName;
                string filePath = Path.Combine(uploadFolders, uniqueFileName);
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    patientRegistrationVm.Photo.CopyTo(filestream);
                }
                //patientRegistrationVm.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            return uniqueFileName;
        }

        [HttpGet]
        [Authorize(Policy = "PatientRegistrationUpdate")]
        public async Task<IActionResult> PatientRegistrationUpdate(int patientRegId, int? bookedPatientId)
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            ViewBag.IsBookedPatient = bookedPatientId == null ? false : true;

            var patientRegistration = await db.PatientRegistrations.FindAsync(patientRegId);
            if (patientRegistration == null)
            {
                return View("Error");
            }
            else
            {
                ViewBag.RegName = patientRegistration.RegistrationType.ToString();
                ViewBag.State = patientRegistration.State;
                ViewBag.Township = patientRegistration.Township;
                PatientRegistrationVM.PatientRegistration = patientRegistration;
                return View(PatientRegistrationVM);
            }
        }

        [HttpPost]
        [Authorize(Policy = "PatientRegistrationUpdate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PatientRegistrationUpdate(PatientRegistrationAKTViewModel model, int? bookedPatientId)
        {
            PatientRegistrationVM = new PatientRegistrationAKTViewModel()
            {
                Prefixes = TheWholeGlobalSettingData.Prefixes,
                Countries = TheWholeGlobalSettingData.Countries,
                Genders = TheWholeGlobalSettingData.Genders,
                MaritalStatuses = TheWholeGlobalSettingData.MaritalStatuses,
                Nationalities = TheWholeGlobalSettingData.Nationalities,
                Occupations = TheWholeGlobalSettingData.Occupations,
                Relationships = TheWholeGlobalSettingData.Relationships,
                //Services = db.Services.ToList(),
                States = TheWholeGlobalSettingData.States,
                Townships = TheWholeGlobalSettingData.Townships,
                ReferalHospitals = TheWholeGlobalSettingData.ReferalHospitals,
                //RegistrationFee = new RegistrationFee(),
                PatientRegistration = model.PatientRegistration,
            };

            ViewBag.IsBookedPatient = bookedPatientId == null ? false : true;
            //PatientRegistrationVM.PatientRegistration = model.PatientRegistration;

            ViewBag.Age = model.PatientRegistration.Age;
            ViewBag.State = model.PatientRegistration.State;
            ViewBag.Township = model.PatientRegistration.Township;
            if (ModelState.IsValid)
            {
                //string abc = registrationFees; 
                //ViewBag.PatientIdentityType = patientRegistrationVm.PatientRegistration.PatientIdentityType;



                try
                {
                    model.PatientRegistration.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                    if (model.Photo != null)
                    {
                        if (model.PatientRegistration.PhotoPath != null)
                        {
                            var filePath = Path.Combine(hostingEnvironment.WebRootPath, SD.PatientRegImagesFolder, model.PatientRegistration.PhotoPath);
                            System.IO.File.Delete(filePath);
                        }
                        model.PatientRegistration.PhotoPath = ProcessUploadedFile(model);
                    }
                    db.PatientRegistrations.Update(model.PatientRegistration);

                    //List<RegistrationFee> lstRegFeeInDb = (from data in db.RegistrationFees
                    //                                       where
                    //      data.MRNo == patientRegistrationVm.PatientRegistration.MRNo
                    //                                       select data).ToList();
                    //foreach (RegistrationFee regFeeInDb in lstRegFeeInDb)
                    //{
                    //    //db.RegistrationFees.Remove(regFeeInDb);
                    //}

                    //var lstRegFee = JsonConvert.DeserializeObject<List<RegistrationFee>>(registrationFees);
                    //foreach (RegistrationFee regFee in lstRegFee)
                    //{
                    //    regFee.MRNo = patientRegistrationVm.PatientRegistration.MRNo;
                    //    //db.RegistrationFees.Add(regFee);
                    //}
                    await db.SaveChangesAsync();

                    ViewBag.SuccessedAlertMessage = "PatientRegistration Successfully Updated";
                    return View(PatientRegistrationVM);
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.ErrorTitle = "Updating patient registration error!";
                    ViewBag.ErrorMessage = "An error is occured when updating patient registration data. If you want to know " +
                        "details please contact us to " + SD.companyEmail;
                    return View("Error");
                }
            }
            else
            {
                return View(PatientRegistrationVM);
            }
        }

        [HttpGet]
        [Authorize(Policy = "PatientRegistrationView")]
        public async Task<IActionResult> PatientRegistrationDetails(int patientRegId)
        {
            var patientRegistration = await db.PatientRegistrations.FindAsync(patientRegId);
            if (patientRegistration == null)
            {
                return View("Error");
            }
            else
            {
                ViewBag.RegName = patientRegistration.RegistrationType.ToString();
                ViewBag.State = patientRegistration.State;
                ViewBag.Township = patientRegistration.Township;
                PatientRegistrationVM.PatientRegistration = patientRegistration;
                return View(PatientRegistrationVM);
            }
        }

        [HttpPost]
        [Authorize(Policy = "PatientRegistrationDelete")]
        public async Task<IActionResult> ViewAllRegisteredPatients(int patientRegId)
        {
            var toDeletePaitent = await db.PatientRegistrations.FindAsync(patientRegId);
            if (toDeletePaitent == null)
            {
                return View("Error");
            }

            try
            {
                db.PatientRegistrations.Remove(toDeletePaitent);
                await db.SaveChangesAsync();

                TempData["SuccessedAlertMessage"] = "Patient Info Successfully Deleted";
                return RedirectToAction(nameof(ViewAllRegisteredPatients));
            }
            catch (Exception)
            {
                ViewBag.ErrorTitle = "Remove Patient Info Not Success!";
                ViewBag.ErrorMessage = "This Patient cannot be removed because of other reference exist this data";
                return View("Error");
            }
        }


        [HttpGet]
        [Authorize(Policy = "PatientRegistrationView")]
        public async Task<IActionResult> ViewAllRegisteredPatients()
        {
            List<PatientRegistration> patients = new List<PatientRegistration>();
            patients = await db.PatientRegistrations.ToListAsync();
            patients = patients.OrderBy(x => x.FullName).ThenBy(x => x.MRNo).ThenByDescending(x => x.RegDate).ToList();
            return View(patients);
        }
    }
}