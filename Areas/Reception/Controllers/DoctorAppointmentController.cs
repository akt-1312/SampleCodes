using HMS.Data;
using HMS.GlobalDependencyData;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Models.Reception;
using HMS.Models.ViewModels.CommonViewModal;
using HMS.Models.ViewModels.Reception;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Areas.Reception.Controllers
{
    [Authorize]
    [Area("Reception")]
    public class DoctorAppointmentController : Controller
    {
        private readonly ApplicationDbContext db;

        public DoctorAppointmentController(ApplicationDbContext db)
        {
            this.db = db;
        }
        [HttpGet]
        public async Task<IActionResult> DoctorAppointment(int? patientRegId, bool isBookedPatient)
        {
            PatientRegistration appointmentPatient = new PatientRegistration();

            if (patientRegId != null || patientRegId > 0)
            {
                appointmentPatient = await db.PatientRegistrations.FindAsync(patientRegId.Value);

                if (appointmentPatient == null)
                {
                    return View("Error");
                }
            }

            //ViewBag.UpdateAppointment = "Error";
            var patientRegList = await db.PatientRegistrations.ToListAsync();
            CreateDoctorAppointmentViewModel model = new CreateDoctorAppointmentViewModel()
            {
                Prefixes = TheWholeGlobalSettingData.Prefixes,
                AppointmentDate = DateTime.UtcNow.AddMinutes(390),
                //AppointmentTime = DateTime.UtcNow.AddMinutes(390),
                PatientRegId = patientRegId,
                PatientRegistration = appointmentPatient,
                DoctorDuty = new DoctorDuty(),
                DrDeptUnitForPartialViewModel = new DrDeptUnitForPartialViewModel()
                {
                    Departments = await db.Departments.OrderBy(x => x.DepartmentName).ToListAsync(),
                },
                PatientRegistrations = patientRegList.OrderBy(x => x.FullName).ThenBy(x => x.MRNo).ThenByDescending(x => x.RegDate).ToList(),
            };

            if (isBookedPatient)
            {
                if (patientRegId != null || patientRegId > 0)
                {
                    var bookedAppointment = await db.DoctorAppointmentDatas.Where(x => x.AppointmentStatus == AppointmentStatus.Booked
                    && x.PatientRegId.HasValue && x.PatientRegId == patientRegId.Value)
                        .Include(x => x.DoctorInfo).Include(x => x.Department).Include(x => x.Unit).Include(x => x.PatientRegistration).OrderByDescending(x => x.AppointmentId).FirstOrDefaultAsync();

                    if (bookedAppointment != null)
                    {
                        var doctorDuty = await db.DoctorDuties.Where(x => x.DutyDay.ToLower() == bookedAppointment.AppointmentDate.DayOfWeek
                        .ToString().ToLower() && x.DepartmentId == bookedAppointment.DepartmentId
                        && x.UnitId == bookedAppointment.UnitId
                        && x.DoctorId == bookedAppointment.DoctorId).FirstOrDefaultAsync();

                        model.AppointmentDate = bookedAppointment.AppointmentDate;
                        model.AppointmentTime = bookedAppointment.AppointmentTime;
                        model.DoctorDuty = doctorDuty ?? new DoctorDuty();
                        model.VisitType = bookedAppointment.VisitType;
                        model.AppointmentStatus = AppointmentStatus.Confirm;
                        model.DrDeptUnitForPartialViewModel.DepartmentId = bookedAppointment.DepartmentId;
                        model.DrDeptUnitForPartialViewModel.DepartmentName = bookedAppointment.Department.DepartmentName;
                        model.DrDeptUnitForPartialViewModel.UnitId = bookedAppointment.UnitId;
                        model.DrDeptUnitForPartialViewModel.UnitName = bookedAppointment.Unit.UnitName;
                        model.DrDeptUnitForPartialViewModel.DoctorId = bookedAppointment.DoctorId;
                        model.DrDeptUnitForPartialViewModel.DoctorName = bookedAppointment.DoctorInfo.DoctorName;
                        model.DoctorAppointmentDatas = await db.DoctorAppointmentDatas
                                                        .Include(x => x.PatientRegistration).Include(x => x.NewPatientBookingAppointment)
                                                        .Where(x => x.AppointmentSituation != EncounterEnum.Finished && x.AppointmentDate.Date == bookedAppointment.AppointmentDate.Date && x.DepartmentId == bookedAppointment.DepartmentId
                                                           && x.UnitId == bookedAppointment.UnitId && x.DoctorId == bookedAppointment.DoctorId).ToListAsync() ?? new List<DoctorAppointmentData>();
                        model.IsFirstAssign = bookedAppointment.IsFirstAssign;
                        model.TokenNo = bookedAppointment.TokenNo;
                        model.DoctorAppointmentId = bookedAppointment.AppointmentId;
                    }
                }

            }
            ViewBag.IsBookedPatient = isBookedPatient;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> DoctorAppointment(CreateDoctorAppointmentViewModel model, string btnSubmit, bool isBookedPatient)
        {

            //if (btnSubmit == "GetPatient")
            //{
            //    var tempDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            //    TempData["AppointmentData"] = tempDataJson;
            //    return RedirectToAction("PatientSearchForm", "PatientRegistrationAKT", new { Area = "Registration" });
            //}
            model.Prefixes = TheWholeGlobalSettingData.Prefixes;

            var todayDate = DateTime.UtcNow.AddMinutes(390);

            ViewBag.IsBookedPatient = isBookedPatient;

            model.PatientRegistrations = await db.PatientRegistrations.ToListAsync();
            model.PatientRegistrations = model.PatientRegistrations.OrderBy(x => x.FullName).ThenBy(x => x.MRNo).ThenByDescending(x => x.RegDate).ToList();

            var drDuty = model.DrDeptUnitForPartialViewModel == null ? new DoctorDuty() : await db.DoctorDuties
               .Where(x => x.DutyDay.ToLower() == model.AppointmentDate.Value.DayOfWeek.ToString().ToLower() &&
               x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId &&
               x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).FirstOrDefaultAsync();
            if (drDuty != null)
            {
                model.DoctorDuty = drDuty;
            }
            else
            {
                model.DoctorDuty = new DoctorDuty();
            }

            model.PatientRegistration = model.PatientRegId == null || model.PatientRegId.Value <= 0 ? new PatientRegistration()
                : await db.PatientRegistrations.FindAsync(model.PatientRegId);

            model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                .Include(x => x.PatientRegistration).Include(x => x.NewPatientBookingAppointment).Where(x => x.AppointmentSituation != EncounterEnum.Finished && x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                   && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();

            model.DrDeptUnitForPartialViewModel.Departments = await db.Departments.OrderBy(x => x.DepartmentName).ToListAsync();

            if (btnSubmit == "CreateAppointment")
            {
                if (model.IsForExternalPatient)
                {
                    if (string.IsNullOrEmpty(model.FirstName) || string.IsNullOrEmpty(model.Gender) || model.Age == null ||
                        model.Age.Value <= 0 || string.IsNullOrEmpty(model.PhoneNo) || model.DrDeptUnitForPartialViewModel.DoctorId <= 0
                    || model.AppointmentTime == null || !model.VisitType.HasValue || !model.AppointmentStatus.HasValue)
                    {
                        if (string.IsNullOrEmpty(model.FirstName))
                        {
                            ModelState.AddModelError("", "FirstName is required");
                        }
                        if (string.IsNullOrEmpty(model.Gender))
                        {
                            ModelState.AddModelError("", "Gender is required");
                        }
                        if (model.Age == null || model.Age.Value <= 0)
                        {
                            if (model.Age == null)
                            {
                                ModelState.AddModelError("", "Age is required");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Age doesn't allow zero or less than zero");
                            }
                        }
                        if (string.IsNullOrEmpty(model.PhoneNo))
                        {
                            ModelState.AddModelError("", "Phone No is required");
                        }
                        if (model.DrDeptUnitForPartialViewModel.DoctorId <= 0)
                        {
                            ModelState.AddModelError("", "Choose Doctor");
                        }
                        if (model.AppointmentTime == null)
                        {
                            ModelState.AddModelError("", "Choose Appointment Time");
                        }
                        if (!model.AppointmentStatus.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Appointment Status");
                        }
                        if (!model.VisitType.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Visit Type");
                        }
                        return View(model);
                    }
                    else
                    {
                        if (model.AppointmentStatus != AppointmentStatus.Booked || model.VisitType != VisitType.FirstVisit)
                        {
                            if (model.AppointmentStatus != AppointmentStatus.Booked)
                            {
                                ModelState.AddModelError("", "Appoint status not valid!");
                            }
                            if (model.VisitType != VisitType.FirstVisit)
                            {
                                ModelState.AddModelError("", "Visit type not valid!");
                            }
                            return View(model);
                        }
                        else
                        {
                            NewPatientBookingAppointment bookingPatientData = new NewPatientBookingAppointment()
                            {
                                Prefix = model.Prefix,
                                FirstName = model.FirstName,
                                MiddleName = model.MiddleName,
                                LastName = model.LastName,
                                Gender = model.Gender,
                                DateOfBirth = model.DateOfBirth,
                                Age = model.Age.Value,
                                PhoneNo = model.PhoneNo,
                                CreatedDate = todayDate,
                                UpdatedDate = todayDate
                            };

                            try
                            {
                                await db.NewPatientBookingAppointments.AddAsync(bookingPatientData);
                                await db.SaveChangesAsync();
                            }
                            catch
                            {
                                return View("Error");
                            }
                            DoctorAppointmentData doctorAppointment = new DoctorAppointmentData()
                            {
                                VisitNo = DefinedVisitNoFormat(model.DrDeptUnitForPartialViewModel.DoctorId, model.AppointmentDate.Value),
                                TokenNo = model.TokenNo,
                                PatientRegId = null,
                                NewBookingPatientId = bookingPatientData.NewPatientBookingAppointmentId,
                                AppointmentDate = model.AppointmentDate.Value,
                                AppointmentTime = model.AppointmentTime.Value,
                                DoctorId = model.DrDeptUnitForPartialViewModel.DoctorId,
                                DepartmentId = model.DrDeptUnitForPartialViewModel.DepartmentId,
                                UnitId = model.DrDeptUnitForPartialViewModel.UnitId,
                                VisitType = model.VisitType.Value,
                                AppointmentStatus = model.AppointmentStatus.Value,
                                IsFirstAssign = model.IsFirstAssign,
                                AppointmentSituation = EncounterEnum.NotEncounter,
                                //IsFinishedAppointment = false,
                                UpdateRemarks = null,
                                CreatedDate = todayDate,
                                UpdatedDate = todayDate,
                            };

                            try
                            {
                                await db.DoctorAppointmentDatas.AddAsync(doctorAppointment);
                                await db.SaveChangesAsync();
                            }
                            catch
                            {
                                return View("Error");
                            }


                        }

                        model.PatientRegistration = new PatientRegistration();
                        model.PatientRegId = null;
                        model.AppointmentTime = null;
                        model.TokenNo = null;
                        model.VisitType = null;
                        model.AppointmentStatus = null;

                        model.Prefix = string.Empty;
                        model.FirstName = string.Empty;
                        model.MiddleName = string.Empty;
                        model.LastName = string.Empty;
                        model.Gender = string.Empty;
                        model.DateOfBirth = null;
                        model.Age = null;
                        model.PhoneNo = string.Empty;

                        ModelState.Clear();

                        ViewBag.SuccessedAlertMessage = "Appointment is Successfully Booked";

                    }
                }
                else
                {
                    if (model.PatientRegId == null || model.PatientRegId <= 0 || model.DrDeptUnitForPartialViewModel.DoctorId <= 0
                    || model.AppointmentTime == null || !model.VisitType.HasValue || !model.AppointmentStatus.HasValue)
                    {
                        if (model.PatientRegId == null || model.PatientRegId <= 0)
                        {
                            ModelState.AddModelError("", "Choose Patient");
                        }
                        if (model.DrDeptUnitForPartialViewModel.DoctorId <= 0)
                        {
                            ModelState.AddModelError("", "Choose Doctor");
                        }
                        if (model.AppointmentTime == null)
                        {
                            ModelState.AddModelError("", "Choose Appointment Time");
                        }
                        if (!model.AppointmentStatus.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Appointment Status");
                        }
                        if (!model.VisitType.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Visit Type");
                        }

                        return View(model);
                    }
                    else
                    {
                        DoctorAppointmentData drApointmentData = new DoctorAppointmentData()
                        {
                            VisitNo = DefinedVisitNoFormat(model.DrDeptUnitForPartialViewModel.DoctorId, model.AppointmentDate.Value),
                            TokenNo = model.TokenNo,
                            PatientRegId = model.PatientRegId.Value,
                            AppointmentDate = model.AppointmentDate.Value,
                            AppointmentTime = model.AppointmentTime.Value,
                            DoctorId = model.DrDeptUnitForPartialViewModel.DoctorId,
                            DepartmentId = model.DrDeptUnitForPartialViewModel.DepartmentId,
                            UnitId = model.DrDeptUnitForPartialViewModel.UnitId,
                            VisitType = model.VisitType.Value,
                            AppointmentStatus = model.AppointmentStatus.Value,
                            IsFirstAssign = model.IsFirstAssign,
                            AppointmentSituation = EncounterEnum.NotEncounter,
                            CreatedDate = todayDate,
                            UpdatedDate = todayDate,
                            UpdateRemarks = null
                        };
                        try
                        {
                            await db.DoctorAppointmentDatas.AddAsync(drApointmentData);
                            await db.SaveChangesAsync();
                        }
                        catch (Exception)
                        {
                            return View("Error");
                        }
                        model.PatientRegistration = new PatientRegistration();
                        //    model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                        //.Include(x => x.PatientRegistration).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                        //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();
                        model.PatientRegId = null;
                        model.AppointmentTime = null;
                        model.TokenNo = null;
                        model.VisitType = null;
                        model.AppointmentStatus = null;

                        ViewBag.SuccessedAlertMessage = "Appointment is Successfully Created";
                    }
                }
            }

            if (btnSubmit == "UpdateAppointment")
            {
                try
                {
                    DoctorAppointmentData drAppointmentDataToUpdate = new DoctorAppointmentData();
                    drAppointmentDataToUpdate = await db.DoctorAppointmentDatas.Where(x => x.IsFirstAssign == model.IsFirstAssign && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId &&
                    x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId && x.AppointmentDate.Date == model.AppointmentDate.Value.Date &&
                    x.AppointmentTime == model.AppointmentTime.Value).FirstOrDefaultAsync();

                    if ((string.IsNullOrEmpty(model.Remarks) && !isBookedPatient) || !model.VisitType.HasValue
                        || !model.AppointmentStatus.HasValue)
                    {
                        if (string.IsNullOrEmpty(model.Remarks) && !isBookedPatient)
                        {
                            ModelState.AddModelError("", "Remarks is required");
                        }
                        if (!model.AppointmentStatus.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Appointment Status");
                        }
                        if (!model.VisitType.HasValue)
                        {
                            ModelState.AddModelError("", "Choose Visit Type");
                        }
                        model.PatientRegistration = drAppointmentDataToUpdate.PatientRegistration;
                        ViewBag.UpdateAppointment = "Error";
                        return View(model);
                    }


                    drAppointmentDataToUpdate.VisitType = model.VisitType.Value;
                    drAppointmentDataToUpdate.AppointmentStatus = model.AppointmentStatus.Value;
                    drAppointmentDataToUpdate.UpdateRemarks = model.Remarks;
                    drAppointmentDataToUpdate.UpdatedDate = todayDate;

                    db.DoctorAppointmentDatas.Update(drAppointmentDataToUpdate);
                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return View("Error");
                }

                model.PatientRegistration = new PatientRegistration();
                //    model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                //.Include(x => x.PatientRegistration).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();
                model.PatientRegId = null;
                model.AppointmentTime = null;
                model.TokenNo = null;
                model.VisitType = null;
                model.AppointmentStatus = null;
                model.Remarks = null;

                ViewBag.SuccessedAlertMessage = "Appointment is Successfully Updated";
                ViewBag.IsBookedPatient = false;
            }

            #region DeleteAppointment
            //if (btnSubmit == "DeleteAppointment")
            //{
            //    try
            //    {
            //        DoctorAppointmentData drAppointmentDataToDelete = new DoctorAppointmentData();
            //        drAppointmentDataToDelete = await db.DoctorAppointmentDatas.Where(x => x.IsFirstAssign == model.IsFirstAssign && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId &&
            //        x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId && x.AppointmentDate.Date == model.AppointmentDate.Value.Date &&
            //        x.AppointmentTime == model.AppointmentTime.Value).FirstOrDefaultAsync();

            //        db.DoctorAppointmentDatas.Remove(drAppointmentDataToDelete);
            //        await db.SaveChangesAsync();
            //    }
            //    catch (Exception)
            //    {
            //        return View("Error");
            //    }

            //    model.PatientRegistration = new PatientRegistration();
            //    //    model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
            //    //.Include(x => x.PatientRegistration).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
            //    //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();
            //    model.PatientRegId = null;
            //    model.AppointmentTime = null;
            //    model.TokenNo = null;
            //    model.VisitType = null;
            //    model.AppointmentStatus = null;

            //    ViewBag.SuccessedAlertMessage = "Appointment is Successfully Deleted";

            //}
            #endregion

            if (btnSubmit == "EncounteredAppointment")
            {
                try
                {
                    DoctorAppointmentData drAppointmentDataToEncountered = new DoctorAppointmentData();
                    drAppointmentDataToEncountered = await db.DoctorAppointmentDatas.Where(x => x.IsFirstAssign == model.IsFirstAssign && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId &&
                    x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId && x.AppointmentDate.Date == model.AppointmentDate.Value.Date &&
                    x.AppointmentTime == model.AppointmentTime.Value).FirstOrDefaultAsync();

                    drAppointmentDataToEncountered.AppointmentSituation = EncounterEnum.Encounter;
                    drAppointmentDataToEncountered.UpdatedDate = todayDate;
                    db.DoctorAppointmentDatas.Update(drAppointmentDataToEncountered);
                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return View("Error");
                }

                model.PatientRegistration = new PatientRegistration();
                //    model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                //.Include(x => x.PatientRegistration).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();
                model.PatientRegId = null;
                model.AppointmentTime = null;
                model.TokenNo = null;
                model.VisitType = null;
                model.AppointmentStatus = null;

                ViewBag.SuccessedAlertMessage = "Appointment is Successfully Encountered";

            }

            if (btnSubmit == "DragAndDrop")
            {
                DoctorAppointmentData changeDrAppointmentData = await db.DoctorAppointmentDatas.FindAsync(model.DoctorAppointmentId);
                List<DoctorAppointmentData> listDrAppData = new List<DoctorAppointmentData>();
                if (changeDrAppointmentData.AppointmentTime > model.AppointmentTime)
                {
                    listDrAppData = await db.DoctorAppointmentDatas.Where(x => x.AppointmentSituation != EncounterEnum.Finished && x.IsFirstAssign == changeDrAppointmentData.IsFirstAssign && x.AppointmentTime >= model.AppointmentTime && x.AppointmentTime < changeDrAppointmentData.AppointmentTime).ToListAsync();

                    foreach (var item in listDrAppData)
                    {
                        item.TokenNo = item.TokenNo + 1;
                        item.AppointmentTime = item.AppointmentTime.AddMinutes(model.DoctorDuty.IntervalGapForPatient);
                    }

                    changeDrAppointmentData.AppointmentTime = model.AppointmentTime.Value;
                    changeDrAppointmentData.TokenNo = model.TokenNo;

                    listDrAppData.Add(changeDrAppointmentData);
                }
                else
                {
                    listDrAppData = await db.DoctorAppointmentDatas.Where(x => x.AppointmentSituation != EncounterEnum.Finished && x.IsFirstAssign == changeDrAppointmentData.IsFirstAssign && x.AppointmentTime <= model.AppointmentTime && x.AppointmentTime > changeDrAppointmentData.AppointmentTime).ToListAsync();

                    foreach (var item in listDrAppData)
                    {
                        item.TokenNo = item.TokenNo - 1;
                        item.AppointmentTime = item.AppointmentTime.AddMinutes(-(model.DoctorDuty.IntervalGapForPatient));
                    }

                    changeDrAppointmentData.AppointmentTime = model.AppointmentTime.Value;
                    changeDrAppointmentData.TokenNo = model.TokenNo;
                    changeDrAppointmentData.UpdatedDate = todayDate;

                    listDrAppData.Add(changeDrAppointmentData);

                    //listDrAppData = await db.DoctorAppointmentDatas.Where(x => x.AppointmentTime >= model.AppointmentTime).ToListAsync();

                    //foreach (var item in listDrAppData)
                    //{
                    //    item.TokenNo = item.TokenNo + 1;
                    //    item.AppointmentTime = item.AppointmentTime.AddMinutes(model.DoctorDuty.IntervalGapForPatient);
                    //}

                    //changeDrAppointmentData.AppointmentTime = model.AppointmentTime.Value;
                    //changeDrAppointmentData.TokenNo = model.TokenNo;

                    //listDrAppData.Add(changeDrAppointmentData);
                }

                try { 
                db.DoctorAppointmentDatas.UpdateRange(listDrAppData);
                await db.SaveChangesAsync();
                }
                catch
                {
                    return View("Error");
                }
                //    model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                //.Include(x => x.PatientRegistration).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();

                model.AppointmentTime = null;
                model.TokenNo = null;
            }

            if (model.DrDeptUnitForPartialViewModel != null && model.DrDeptUnitForPartialViewModel.DepartmentId > 0 && model.DoctorDuty.DutyStartTime1 != null)
            {
                var lastAppointment1 = await db.DoctorAppointmentDatas.Where(x => x.IsFirstAssign == true && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId &&
               x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId &&
               x.AppointmentDate.Date == model.AppointmentDate.Value.Date).OrderByDescending(x => x.AppointmentTime).FirstOrDefaultAsync();

                if (lastAppointment1 != null)
                {
                    model.DoctorDuty.DutyStartTime1 = DateTime.Parse(model.AppointmentDate.Value.Date.ToShortDateString() + " " + model.DoctorDuty.DutyStartTime1.Value.ToShortTimeString());
                    DateTime dutyEndTime = DateTime.Parse(model.AppointmentDate.Value.Date.ToShortDateString() + " " + model.DoctorDuty.DutyEndTime1.Value.ToShortTimeString());
                    if (dutyEndTime <= lastAppointment1.AppointmentTime)
                    {
                        model.DoctorDuty.DutyEndTime1 = lastAppointment1.AppointmentTime.AddMinutes(model.DoctorDuty.IntervalGapForPatient);
                    }
                    else
                    {
                        model.DoctorDuty.DutyEndTime1 = dutyEndTime;
                    }
                }

                var lastAppointment2 = await db.DoctorAppointmentDatas.Where(x => x.IsFirstAssign == false && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId &&
               x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId &&
               x.AppointmentDate.Date == model.AppointmentDate.Value.Date).OrderByDescending(x => x.AppointmentTime).FirstOrDefaultAsync();

                if (lastAppointment2 != null)
                {
                    model.DoctorDuty.DutyStartTime2 = DateTime.Parse(model.AppointmentDate.Value.Date.ToShortDateString() + " " + model.DoctorDuty.DutyStartTime2.Value.ToShortTimeString());
                    DateTime dutyEndTime = DateTime.Parse(model.AppointmentDate.Value.Date.ToShortDateString() + " " + model.DoctorDuty.DutyEndTime2.Value.ToShortTimeString());
                    if (dutyEndTime <= lastAppointment2.AppointmentTime)
                    {
                        model.DoctorDuty.DutyEndTime2 = lastAppointment2.AppointmentTime.AddMinutes(model.DoctorDuty.IntervalGapForPatient);
                    }
                    else
                    {
                        model.DoctorDuty.DutyEndTime2 = dutyEndTime;
                    }
                }
            }

            //if (btnSubmit == "GetDoctorDuty")
            //{               
            //    model.AppointmentTime = null;
            //    model.TokenNo = null;
            //    model.VisitType = "First Visit";
            //    model.AppointmentStatus = "No Confirm";
            //}

            if (btnSubmit == "RejectAppointment")
            {
                model.PatientRegId = null;
                model.AppointmentTime = null;
                model.TokenNo = null;
                model.VisitType = null;
                model.AppointmentStatus = null;
                model.PatientRegistration = new PatientRegistration();

                model.Prefix = string.Empty;
                model.FirstName = string.Empty;
                model.MiddleName = string.Empty;
                model.LastName = string.Empty;
                model.Gender = string.Empty;
                model.DateOfBirth = null;
                model.Age = null;
                model.PhoneNo = string.Empty;

                ModelState.Clear();

                ViewBag.IsBookedPatient = false;
            }

            if (btnSubmit != "GetDoctorDuty")
            {
                model.PatientRegId = null;
                model.AppointmentTime = null;
                model.TokenNo = null;
                model.VisitType = null;
                model.AppointmentStatus = null;
            }

            model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
                .Include(x => x.PatientRegistration).Include(x => x.NewPatientBookingAppointment).Where(x => x.AppointmentSituation != EncounterEnum.Finished && x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
                   && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();

            model.DrDeptUnitForPartialViewModel.Departments = await db.Departments.OrderBy(x => x.DepartmentName).ToListAsync();

            //model.DoctorAppointmentDatas = model.DrDeptUnitForPartialViewModel == null ? new List<DoctorAppointmentData>() : await db.DoctorAppointmentDatas
            //.Include(x => x.PatientRegistration).Include(x=> x.NewPatientBookingAppointment).Where(x => x.AppointmentDate.Date == model.AppointmentDate.Value.Date && x.DepartmentId == model.DrDeptUnitForPartialViewModel.DepartmentId
            //  && x.UnitId == model.DrDeptUnitForPartialViewModel.UnitId && x.DoctorId == model.DrDeptUnitForPartialViewModel.DoctorId).ToListAsync();

            return View(model);
        }


        private string DefinedVisitNoFormat(int DoctorId, DateTime appointmentDate)
        {
            string CalculatedVisitNo = null;
            int todaySpecificDoctorAppointmentLastVisitNo;
            List<DoctorAppointmentData> todaySpecificDoctorAppointmentList = db.DoctorAppointmentDatas.Where(x => x.DoctorId == DoctorId
            && x.AppointmentDate.Date == appointmentDate.Date).ToList();
            if (todaySpecificDoctorAppointmentList.Count == 0)
            {
                todaySpecificDoctorAppointmentLastVisitNo = 1;
            }
            else
            {
                string lastVistiNoString = todaySpecificDoctorAppointmentList.LastOrDefault().VisitNo;
                string[] splitLastVisitNo = lastVistiNoString.Split('-');
                int.TryParse(splitLastVisitNo[1], out todaySpecificDoctorAppointmentLastVisitNo);
                todaySpecificDoctorAppointmentLastVisitNo = todaySpecificDoctorAppointmentLastVisitNo + 1;
            }
            string doctorAppointmentDateShortStringFormat = appointmentDate.ToString("yyyyMMdd");

            CalculatedVisitNo = doctorAppointmentDateShortStringFormat + DoctorId.ToString() + "-" +
                todaySpecificDoctorAppointmentLastVisitNo.ToString();
            return CalculatedVisitNo;
        }
        //[HttpPost]
        //public IActionResult DoctorAppointment()
        //{

        //}
    }
}