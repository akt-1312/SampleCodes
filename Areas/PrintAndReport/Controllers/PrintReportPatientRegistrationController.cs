using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models;
using HMS.Models.Reception;
using HMS.Models.ViewModels;
using HMS.Models.ViewModels.Reception;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.PrintAndReport.Controllers
{
    [Area("PrintAndReport")]
    public class PrintReportPatientRegistrationController : Controller
    {
        private readonly ApplicationDbContext db;
        //private readonly PatientRegistrationAKTViewModel patientRegistrationVM;

        [BindProperty]
        public PatientRegistrationAKTViewModel patientRegistrationVM { get; set; }

        public PrintReportPatientRegistrationController(ApplicationDbContext db)
        {
            this.db = db;
            patientRegistrationVM = new PatientRegistrationAKTViewModel()
            {
                Prefixes = db.Prefixes.ToList(),
                Countries = db.Countries.ToList(),
                Genders = db.Genders.ToList(),
                MaritalStatuses = db.MaritalStatuses.ToList(),
                Nationalities = db.Nationalities.ToList(),
                Occupations = db.Occupations.ToList(),
                Relationships = db.Relationships.ToList(),
                Services = db.Services.ToList(),
                States = db.States.ToList(),
                Townships = db.Townships.ToList(),
                PatientRegistration = new PatientRegistration(),
                //RegistrationFee = new RegistrationFee(),
                //RegistrationFees = db.RegistrationFees.ToList(),
                //RegistrationTypes = db.RegistrationTypes.ToList(),
            };
        }

        public IActionResult PrintPatientBlankForm()
        {
            return View();
        }

        public async Task<IActionResult> PrintPatientDetailsForm(int? id)
        {
            if (id == null)
            {
                return View("Error");
            }

            patientRegistrationVM.PatientRegistration = (await db.PatientRegistrations.FindAsync(id));
            if (patientRegistrationVM.PatientRegistration == null)
            {
                Response.StatusCode = 404;
                return View("IdNotFound", id.Value);
            }
            ViewBag.State = patientRegistrationVM.PatientRegistration.State;
            ViewBag.Township = patientRegistrationVM.PatientRegistration.Township;
            //ViewBag.RegName = patientRegistrationVM.PatientRegistration.PatientRegistrationType;
            //GetRegTypes(RegName);            
            return View(patientRegistrationVM);
        }

        public IActionResult PrintPatientIdentityCard(int id)
        {
            patientRegistrationVM.PatientRegistration = db.PatientRegistrations.Where(p => p.Reg_Id == id).FirstOrDefault();
            return View(patientRegistrationVM);

            //PatientRegistration patientReg = new PatientRegistration();
            //patientReg = db.PatientRegistrations.Where(p => p.Reg_Id == id).FirstOrDefault();
            //return View(patientReg);
        }

        public IActionResult PrintKinIdentityCard(int id)
        {
            patientRegistrationVM.PatientRegistration = db.PatientRegistrations.Where(p => p.Reg_Id == id).FirstOrDefault();
            return View(patientRegistrationVM);
            //PatientRegistration patientReg = new PatientRegistration();
            //patientReg = db.PatientRegistrations.Where(p => p.Reg_Id == id).FirstOrDefault();
            //return View(patientReg);
        }
    }
}