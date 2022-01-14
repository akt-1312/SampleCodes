using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Extensions;
using HMS.GlobalDependencyData;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.CommonAjaxCall.Controllers
{
    [Authorize]
    [Area("CommonAjaxCall")]
    public class PatientRegistrationAjaxCallController : Controller
    {
        private readonly ApplicationDbContext db;

        public PatientRegistrationAjaxCallController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JsonResult GetStates(string CountryName)
        {
            List<State> lstState = new List<State>();
            lstState = TheWholeGlobalSettingData.States.Where(x => x.Country.Cty_name == CountryName).ToList(); 
            //(from data in db.States where data.Country.Cty_name == CountryId select data).ToList();

            return Json(new SelectList(lstState, "State_name", "State_name"));
        }

        public JsonResult GetTownships(string StateName)
        {
            List<Township> lstTownship = new List<Township>();
            lstTownship = TheWholeGlobalSettingData.Townships.Where(x => x.State.State_name == StateName).ToList();
                //(from data in db.Townships where data.State.State_name == StateName select data).ToList();

            return Json(new SelectList(lstTownship, "Tsp_name", "Tsp_name"));
        }

        public JsonResult MRNoSerial(int CountOfDigit = 9)
        {
            var saveCountOfDigit = CountOfDigit;
            StringBuilder lstMRNoSerial = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < CountOfDigit; i++)
            {
                var rdm = random.Next(9);
                lstMRNoSerial.Append(rdm.ToString());
            }
            string finalMRNo = SD.constAutoMRNo + lstMRNoSerial.ToString();

            var checkUsedMRNo = (from data in db.PatientRegistrations
                                 where
data.MRNo == finalMRNo
                                 select data).FirstOrDefault();
            if (checkUsedMRNo != null)
            {
                MRNoSerial(saveCountOfDigit);
            }
            return Json(finalMRNo);
        }

        private class ServiceFee
        {           
            public string ServiceName { get; set; }
            public string ServiceAmmount { get; set; }
            public string CurrencyCode { get; set; }
            public bool IsFoc { get; set; }
        }

        public async Task<JsonResult> GetServiceFees(string Nationality, string RegName)
        {
            var registrationFees = new List<RegistrationService>();
            List<ServiceFee> serviceFees = new List<ServiceFee>();
            if (RegName == Enum.GetName(typeof(RegistrationType), RegistrationType.Emergency))
            {
                registrationFees = await db.RegistrationServices.Where(x => x.IsActive && x.IsForEmergency).ToListAsync();
            }
            else
            {
                registrationFees = await db.RegistrationServices.Where(x => x.IsActive && x.IsForNormal).ToListAsync();
            }
            if(registrationFees == null)
            {
                return Json(serviceFees);
            }
            else
            {
                if (string.IsNullOrEmpty(Nationality) || Nationality.ToLower().Trim() == TheWholeGlobalSettingData.LocalNationalityName.ToLower().Trim())
                {
                    foreach (var item in registrationFees)
                    {
                        ServiceFee serviceFee = new ServiceFee();
                        serviceFee.ServiceName = item.RegistrationServiceDescription;
                        serviceFee.ServiceAmmount = await GetServiceAmmount(item.RegistrationServicePrice, true);
                        serviceFee.IsFoc = false;
                        serviceFee.CurrencyCode = TheWholeGlobalSettingData.LocalCurrencyCode;

                        serviceFees.Add(serviceFee);
                    }
                    return Json(serviceFees);
                }
                else
                {
                    foreach (var item in registrationFees)
                    {
                        ServiceFee serviceFee = new ServiceFee();
                        serviceFee.ServiceName = item.RegistrationServiceDescription;
                        serviceFee.ServiceAmmount = await GetServiceAmmount(item.RegistrationServicePrice, false);
                        serviceFee.IsFoc = false;
                        serviceFee.CurrencyCode = TheWholeGlobalSettingData.ForeignerCurrencyCode;

                        serviceFees.Add(serviceFee);
                    }
                    return Json(serviceFees);
                }
            }
        }

        public async Task<string> GetServiceAmmount(float localServiceAmmount, bool isLocal)
        {
            float serviceAmmount = 0f;
            if (isLocal)
            {
                localServiceAmmount = localServiceAmmount * TheWholeGlobalSettingData.LocalMultiplyRate;
                serviceAmmount = localServiceAmmount;
            }
            else
            {
                localServiceAmmount = localServiceAmmount * TheWholeGlobalSettingData.ForeignerMultiplyRate;
                if(TheWholeGlobalSettingData.LocalCurrencyCode == TheWholeGlobalSettingData.ForeignerCurrencyCode)
                {
                    serviceAmmount = localServiceAmmount;
                }
                else
                {
                    var currencyRate = await db.CurrencyRates.Where(x => x.CurrencyId == TheWholeGlobalSettingData.ForeignerCurrencyId)
                        .Include(x=> x.Currency).FirstOrDefaultAsync();
                    if (currencyRate != null)
                    {
                        if (currencyRate.FormulaSign == CurrencyFormulaSign.Multipy)
                        {
                            serviceAmmount = localServiceAmmount * currencyRate.FormulaRate.Value;
                        }
                        else
                        {
                            serviceAmmount = localServiceAmmount / currencyRate.FormulaRate.Value;
                        }
                    }
                    else
                    {
                        serviceAmmount = localServiceAmmount;
                    }
                }
            }
            var formattedServiceAmmount = serviceAmmount.ToString("0.0000");
            //var test = float.Parse(serviceAmmount.ToString("0.0000"));
            return formattedServiceAmmount;
        }

        public async Task<JsonResult> GetServiceFeesAfterRegistration(int regId)
        {
            int lastCaseId = await db.PatientCases.Where(x => x.PatientRegId == regId).Select(x => x.PatientCaseId).OrderByDescending(x=> x.ToString()).FirstOrDefaultAsync();

            //var patientRegService = await db.PatientServiceFees.Where(x=> x.ServiceName == )

            return Json(true);
        }

        public async Task<JsonResult> GetGenderByPrefix(string prefixName)
        {
            var genderName = await db.Prefixes.Include(x=> x.Gender).Where(x => x.PrefixName.ToLower().Trim() == prefixName.ToLower().Trim())
                                    .Select(x=> x.Gender.GenderName).FirstOrDefaultAsync();
            return Json(genderName ?? "");

        }

        //public async Task<JsonResult> GetRegTypes(string Nationality, string RegName)
        //{
        //    List<ServiceFee> ServiceFees = new List<ServiceFee>();
        //    List<Service> serviceGroupTypes = new List<Service>();
        //    if (RegName == Enum.GetName(typeof(RegistrationType), RegistrationType.Emergency))
        //    {
        //        serviceGroupTypes = await db.Services.Include(x => x.ServiceGroupType)
        //                        .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.IsForEmergency).ToListAsync();
        //    }
        //    else
        //    {
        //        serviceGroupTypes = await db.Services.Include(x => x.ServiceGroupType)
        //                        .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.IsForNormal).ToListAsync();
        //    }

        //    foreach (var item in serviceGroupTypes)
        //    {
        //        ServiceFee serviceFee = new ServiceFee()
        //        {
        //            //ServiceGroupTypeName = item.ServiceGroupType.ServiceGroupTypeName,
        //            ServiceName = item.ServiceName,
        //            ServiceAmmount = item.ServiceRateAmount,
        //            IsFoc = false
        //        };
        //        ServiceFees.Add(serviceFee);
        //    }

        //    return Json(ServiceFees);

        //    //List<RegistrationType> registrationTypes = (from data in db.RegistrationTypes where data.RegistrationTypeName == RegName select data).ToList();
        //    //List<RegistrationFee> registrationFees = new List<RegistrationFee>();

        //    //foreach (var item in registrationTypes)
        //    //{
        //    //    //var serviceAmount = db.ServiceRates.Find(item.ServiceRateId);
        //    //    //registrationFees.Add(new RegistrationFee
        //    //    //{
        //    //    //    ServiceName = serviceAmount.Service.ServiceName,
        //    //    //    RegistrationAmmount = item.ServiceRate.ServiceRateAmount,
        //    //    //});
        //    //}

        //    //return Json(registrationFees);

        //    //return Json(true);
        //}

        //public async Task<JsonResult> GetRegTypesForForeigner(string RegName)
        //{
        //    List<ServiceFee> ServiceFees = new List<ServiceFee>();
        //    List<Service> serviceGroupTypes = new List<Service>();
        //    if (RegName == Enum.GetName(typeof(RegistrationType), RegistrationType.Emergency))
        //    {
        //        serviceGroupTypes = await db.Services.Include(x => x.ServiceGroupType)
        //                        .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.IsForEmergency).ToListAsync();
        //    }
        //    else
        //    {
        //        serviceGroupTypes = await db.Services.Include(x => x.ServiceGroupType)
        //                        .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.IsForNormal).ToListAsync();
        //    }

        //    float foreignerAmountMultiply = await db.HomeLocalForeignerCurrencyTypes.Where(x => x.IsActive).Select(x => x.MultiplyHomeToForeigner).FirstOrDefaultAsync();

        //    foreach (var item in serviceGroupTypes)
        //    {
        //        ServiceFee serviceFee = new ServiceFee()
        //        {
        //            //ServiceGroupTypeName = item.ServiceGroupType.ServiceGroupTypeName,
        //            ServiceName = item.ServiceName,
        //            ServiceAmmount = item.ServiceRateAmount * foreignerAmountMultiply,
        //            IsFoc = false
        //        };
        //        ServiceFees.Add(serviceFee);
        //    }

        //    return Json(ServiceFees);

        //    //List<RegistrationType> registrationTypes = (from data in db.RegistrationTypes where data.RegistrationTypeName == RegName select data).ToList();
        //    //List<RegistrationFee> registrationFees = new List<RegistrationFee>();

        //    //foreach (var item in registrationTypes)
        //    //{
        //    //    //var serviceAmount = db.ServiceRates.Find(item.ServiceRateId);
        //    //    //registrationFees.Add(new RegistrationFee
        //    //    //{
        //    //    //    ServiceName = serviceAmount.Service.ServiceName,
        //    //    //    RegistrationAmmount = item.ServiceRate.ForeignerRateAmount,
        //    //    //});
        //    //}

        //    //return Json(registrationFees);
        //}

        //public async Task<JsonResult> GetRegTypesUpdate(int PatientRegId)
        //{
        //    List<ServiceFee> ServiceFees = new List<ServiceFee>();
        //    int CaseId = 1; //await db.PatientCases.Where(x => x.PatientRegId == PatientRegId && x.IsActive).Select(x=> x.PatientCaseId).LastOrDefaultAsync();
        //    var PatientRegServiceFees = await db.PatientServiceFees
        //        .Where(x => x.PatientCaseId == CaseId && x.ServiceGroupTypeName.ToLower() == "registration").ToListAsync();
        //    //if (PatientCase.RegistrationType == RegistrationType.Emergency)
        //    //{
        //    //    serviceGroupTypes = await db.ServiceGroupTypeMapServices.Include(x => x.ServiceGroupType).Include(x => x.Service)
        //    //                    .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.Service.IsForEmergency).ToListAsync();
        //    //}
        //    //else
        //    //{
        //    //    serviceGroupTypes = await db.ServiceGroupTypeMapServices.Include(x => x.ServiceGroupType).Include(x => x.Service)
        //    //                    .Where(x => x.ServiceGroupType.ServiceGroupTypeName.ToLower() == "registration").Where(x => x.Service.IsForNormal).ToListAsync();
        //    //}

        //    foreach (var item in PatientRegServiceFees)
        //    {
        //        ServiceFee serviceFee = new ServiceFee()
        //        {
        //            //ServiceGroupTypeName = item.ServiceGroupTypeName,
        //            ServiceName = item.ServiceName,
        //            ServiceAmmount = item.ServiceAmmount,
        //            IsFoc = item.IsFoc
        //        };
        //        ServiceFees.Add(serviceFee);
        //    }

        //    return Json(ServiceFees);

        //    //List<RegistrationType> registrationTypes = (from data in db.RegistrationTypes where data.RegistrationTypeName == RegName select data).ToList();
        //    //List<RegistrationFee> registrationFees = new List<RegistrationFee>();
        //    //List<RegistrationFee> lst = (from data in db.RegistrationFees where data.MRNo == MRNo select data).ToList();
        //    //foreach (var item in lst)
        //    //{
        //    //    registrationFees.Add(new RegistrationFee
        //    //    {
        //    //        ServiceName = item.ServiceName,
        //    //        RegistrationAmmount = item.RegistrationAmmount,
        //    //        FOC = item.FOC
        //    //    });
        //    //}

        //    //return Json(registrationFees);
        //}
    }
}