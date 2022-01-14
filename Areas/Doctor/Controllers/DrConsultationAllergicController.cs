using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Extensions;
using HMS.Models.Administration;
using HMS.Models.Enums;
using HMS.Models.Nurse;
using HMS.Models.Reception;
using HMS.Models.ViewModels.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]
    public class DrConsultationAllergicController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public DrConsultationAllergicViewModel drConsultationAllergicViewModel { get; set; }
        public DrConsultationAllergicController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            drConsultationAllergicViewModel = new DrConsultationAllergicViewModel()
            {


                AllergicTypes = new List<AllergicType>(),
                OnSetTypes = new List<OnSetType>(),
                OnSets = new List<OnSet>(),
                Reactions = new List<Reaction>(),
                ReactionList = new List<Reaction>(),
                AllergicPatientReactionList = new List<AllergicPatientReaction>(),
                //AllergicPatientReactions = new List<AllergicPatientReaction>(),
                Allergics = new List<AllergicReactionViewModel>(),
                //DoctorAppointmentDataList = new List<DoctorAppointmentData>(),
            };
            _context = context;
            this.authorizationService = authorizationService;

        }

       


        [Authorize(Policy = "PatientAllergicView")]

        [HttpGet]
        public async Task<IActionResult> DrConsultationAllergic(int appointmentId)
        {
            DrConsultationAllergicViewModel model = new DrConsultationAllergicViewModel();
           
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];

            var allergic = new List<AllergicReactionViewModel>();
            var noKnownAllergicList = await _context.Allergics.Where(x => x.AppointmentId == appointmentId && x.IsNoKnownAllergy).ToListAsync();
            bool isNoKnownAllergyPatient = noKnownAllergicList.Count > 0;
            if (!isNoKnownAllergyPatient)
            {
                var allReactionsOfAllergic = await _context.AllergicPatientReactions.Include(x => x.Reaction).OrderBy(x => x.Reaction.ReactionName).ToListAsync();
                var allergicsOfAppointmentPatient = await _context.Allergics.Where(x => x.AppointmentId == appointmentId).Include(x => x.DoctorAppointmentData)
                    .ThenInclude(x => x.PatientRegistration).Include(x => x.AllergicType).Include(x => x.OnSet).Include(x => x.OnSetType).ToListAsync();
                allergic = allergicsOfAppointmentPatient.Select(d => new AllergicReactionViewModel()
                {
                    Allergic = d,
                    ReactionsOfAllergic = allReactionsOfAllergic.Where(x => x.AllergicId == d.AllergicId).Select(x => x.Reaction).ToList(),
                }).ToList();

                model.IsAllergy = true;
            }
            else
            {
                model.IsAllergy = false;
                allergic = noKnownAllergicList.Select(x => new AllergicReactionViewModel()
                {
                    Allergic = x,
                    ReactionsOfAllergic = new List<Reaction>()
                }).ToList();
            }

            model.AllergicTypes = await _context.AllergicTypes.OrderBy(v => v.AllergicTypeName).ToListAsync();
            model.OnSets = await _context.OnSets.OrderBy(v => v.OnSetName).ToListAsync();
            model.OnSetTypes = await _context.OnSetTypes.OrderBy(v => v.OnSetTypeName).ToListAsync();
            model.Reactions = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
            model.Allergics = allergic;


            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await _context.DoctorAppointmentDatas.Where(x => x.AppointmentId == appointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();
            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.Allergic;

           
            return View(model);
          
        }


        [Authorize(Policy = "PatientAllergicView")]


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DrConsultationAllergic(DrConsultationAllergicViewModel model, string btnSubmit, int? toDeleteAllergicId)
        {
            model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
            var consultationPatient = await _context.DoctorAppointmentDatas.Where(x => x.AppointmentId == model.AppointmentId)
                .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

            model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
            model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.Allergic;

            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!(await authorizationService.AuthorizeAsync(User, "PatientAllergicDelete")).Succeeded)
                {
                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                }
                ModelState.Clear();
                if (toDeleteAllergicId == null)
                {
                    return View("Error");
                }
                try
                {
                    var toDeleteAllergic = await _context.Allergics.FindAsync(toDeleteAllergicId);
                    if (toDeleteAllergic == null)
                    {
                        return View("Error");
                    }
                    var toDeleteReactions = await _context.AllergicPatientReactions.Where(x => x.AllergicId == toDeleteAllergic.AllergicId).ToListAsync();
                    if (toDeleteReactions != null && toDeleteReactions.Count > 0)
                    {
                        _context.AllergicPatientReactions.RemoveRange(toDeleteReactions);
                    }
                    _context.Allergics.Remove(toDeleteAllergic);
                    await _context.SaveChangesAsync();

                    TempData["SuccessedAlertMessage"] = "Allergic data successfully removed";
                    return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = toDeleteAllergic.AppointmentId });
                }
                catch (Exception)
                {
                    ViewBag.ErrorTitle = "Error when removing allergic data";
                    ViewBag.ErrorMessage = "Allergic data cannot be removed because some processing issue occur! Please try again after sometime.";
                    return View("Error");
                }

            }
            else
            {
                var checkedReactions = model.Reactions.Where(x => x.IsChecked).ToList();
                var allergic = new List<AllergicReactionViewModel>();
                var noKnownAllergicList = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId && x.IsNoKnownAllergy).ToListAsync();
                bool isNoKnownAllergyPatient = noKnownAllergicList.Count > 0;
                if (!isNoKnownAllergyPatient)
                {
                    var allReactionsOfAllergic = await _context.AllergicPatientReactions.Include(x => x.Reaction).OrderBy(x => x.Reaction.ReactionName).ToListAsync();
                    var allergicsOfAppointmentPatient = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).Include(x => x.DoctorAppointmentData)
                        .ThenInclude(x => x.PatientRegistration).Include(x => x.AllergicType).Include(x => x.OnSet).Include(x => x.OnSetType).ToListAsync();
                    allergic = allergicsOfAppointmentPatient.Select(d => new AllergicReactionViewModel()
                    {
                        Allergic = d,
                        ReactionsOfAllergic = allReactionsOfAllergic.Where(x => x.AllergicId == d.AllergicId).Select(x => x.Reaction).ToList(),
                    }).ToList();

                    model.IsAllergy = true;
                }
                else
                {
                    model.IsAllergy = false;
                    allergic = noKnownAllergicList.Select(x => new AllergicReactionViewModel()
                    {
                        Allergic = x,
                        ReactionsOfAllergic = new List<Reaction>()
                    }).ToList();
                }

                //var appointmentData = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == model.DoctorAppointmentData.AppointmentId)
                //             .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                //             .Include(a => a.Department).FirstOrDefaultAsync();

                model.AllergicTypes = await _context.AllergicTypes.OrderBy(v => v.AllergicTypeName).ToListAsync();
                model.OnSets = await _context.OnSets.OrderBy(v => v.OnSetName).ToListAsync();
                model.OnSetTypes = await _context.OnSetTypes.OrderBy(v => v.OnSetTypeName).ToListAsync();
                model.Reactions = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
                model.Allergics = allergic;

                //model.DoctorAppointmentData = appointmentData;
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "create")
                    {
                        try
                        {
                            var createAllergic = new Allergic();
                            if (model.IsNoKnownAllergy)
                            {
                                var alreadyExistPatientReactions = await _context.AllergicPatientReactions.Include(x => x.Allergic).Where(x => x.Allergic.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistPatientReactions != null && alreadyExistPatientReactions.Count > 0)
                                {
                                    _context.AllergicPatientReactions.RemoveRange(alreadyExistPatientReactions);
                                }

                                var alreadyExistAllergics = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistAllergics != null && alreadyExistAllergics.Count > 0)
                                {
                                    _context.Allergics.RemoveRange(alreadyExistAllergics);
                                }

                                createAllergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
                                createAllergic.AppointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId;
                                createAllergic.Remark = model.Remark;
                                createAllergic.CreatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                            }
                            else
                            {
                                var noKnownAllergy = await _context.Allergics.Where(x => x.IsNoKnownAllergy && x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                _context.Allergics.RemoveRange(noKnownAllergy);

                                createAllergic.AppointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId;
                                createAllergic.AllergicTypeId = model.AllergicTypeId;
                                createAllergic.AllergicTo = model.AllergicTo;
                                createAllergic.AllergicStatus = model.AllergicStatus;
                                createAllergic.OnSetDate = model.OnSetDate;
                                createAllergic.ResolvedDate = model.ResolvedDate;
                                createAllergic.Remark = model.Remark;
                                createAllergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
                                createAllergic.CreatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.OnSetTypeId = model.OnSetTypeId;
                                createAllergic.OnSetId = model.OnSetId;
                            }
                            await _context.Allergics.AddAsync(createAllergic);
                            await _context.SaveChangesAsync();

                            if (!model.IsNoKnownAllergy)
                            {
                                foreach (var reaction in checkedReactions)
                                {
                                    var patientReaction = new AllergicPatientReaction()
                                    {
                                        AllergicId = createAllergic.AllergicId,
                                        ReactionId = reaction.ReactionId
                                    };
                                    await _context.AllergicPatientReactions.AddAsync(patientReaction);
                                }
                            }
                            await _context.SaveChangesAsync();

                            TempData["SuccessedAlertMessage"] = "Allergic data successfully saved.";
                            return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });
                        }
                        catch (DbUpdateException)
                        {
                            return View("Error");
                        }
                    }
                    else
                    {
                        if (model.AllergicId == null)
                        {
                            return View("Error");
                        }
                        try
                        {
                            var createAllergic = await _context.Allergics.FindAsync(model.AllergicId.Value);
                            if (model.IsNoKnownAllergy)
                            {
                                var alreadyExistPatientReactions = await _context.AllergicPatientReactions.Include(x => x.Allergic).Where(x => x.Allergic.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistPatientReactions != null && alreadyExistPatientReactions.Count > 0)
                                {
                                    _context.AllergicPatientReactions.RemoveRange(alreadyExistPatientReactions);
                                }

                                var alreadyExistAllergics = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistAllergics != null && alreadyExistAllergics.Count > 0)
                                {
                                    _context.Allergics.RemoveRange(alreadyExistAllergics);
                                }

                                createAllergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
                                createAllergic.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.AllergicTypeId = null;
                                createAllergic.AllergicTo = null;
                                createAllergic.AllergicStatus = null;
                                createAllergic.OnSetDate = null;
                                createAllergic.ResolvedDate = null;
                                createAllergic.Remark = model.Remark;
                                createAllergic.OnSetTypeId = null;
                                createAllergic.OnSetId = null;
                            }
                            else
                            {
                                var noKnownAllergy = await _context.Allergics.Where(x => x.IsNoKnownAllergy && x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
                                _context.Allergics.RemoveRange(noKnownAllergy);

                                createAllergic.AllergicTypeId = model.AllergicTypeId;
                                createAllergic.AllergicTo = model.AllergicTo;
                                createAllergic.AllergicStatus = model.AllergicStatus;
                                createAllergic.OnSetDate = model.OnSetDate;
                                createAllergic.ResolvedDate = model.ResolvedDate;
                                createAllergic.Remark = model.Remark;
                                createAllergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
                                createAllergic.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.OnSetTypeId = model.OnSetTypeId;
                                createAllergic.OnSetId = model.OnSetId;
                            }
                            _context.Allergics.Update(createAllergic);

                            if (!model.IsNoKnownAllergy)
                            {
                                var alreadyExistUpdatePatientReactions = await _context.AllergicPatientReactions.Where(x => x.AllergicId == createAllergic.AllergicId).ToListAsync();
                                if (alreadyExistUpdatePatientReactions != null && alreadyExistUpdatePatientReactions.Count > 0)
                                {
                                    _context.AllergicPatientReactions.RemoveRange(alreadyExistUpdatePatientReactions);
                                }

                                foreach (var reaction in checkedReactions)
                                {
                                    var patientReaction = new AllergicPatientReaction()
                                    {
                                        AllergicId = createAllergic.AllergicId,
                                        ReactionId = reaction.ReactionId
                                    };
                                    await _context.AllergicPatientReactions.AddAsync(patientReaction);
                                }
                            }
                            await _context.SaveChangesAsync();

                            TempData["SuccessedAlertMessage"] = "Allergic data successfully updated.";
                            return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });
                        }
                        catch (DbUpdateException)
                        {
                            return View("Error");
                        }
                    }
                }
                else
                {
                    return View(model);
                }
            }
        }

        //public async Task<IActionResult> DrConsultationAllergic(DrConsultationAllergicViewModel model, string btnSubmit, List<string> CheckData,List<Reaction> lang,int ToDeleteId, int appointmentId)
        //{

        //    model.DoctorConsultationViewModel = new DoctorConsultationViewModel();
        //    var consultationPatient = await _context.DoctorAppointmentDatas.Where(x => x.AppointmentId == model.AppointmentId)
        //        .Include(x => x.DoctorInfo).Include(x => x.PatientRegistration).Include(x => x.Department).Include(x => x.Unit).FirstOrDefaultAsync();

        //    model.DoctorConsultationViewModel.DoctorAppointmentData = consultationPatient ?? new DoctorAppointmentData();
        //    model.DoctorConsultationViewModel.SubFormName = DoctorConsultationSubFormEnum.Allergic;

        //    var allergyyyy = await _context.Allergics.Where(v => v.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).Include(a => a.DoctorAppointmentData.PatientRegistration).Include(a => a.AllergicType).
        //                       Include(a => a.OnSet).Include(a => a.OnSetType).ToListAsync();

        //    DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
        //    DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
        //    var totalAllergic = await _context.Allergics.Include(x => x.OnSet).Include(x => x.OnSetType).Include(x => x.AllergicType).Include(c => c.DoctorAppointmentData).ToListAsync();
        //    var total = await _context.Allergics.ToListAsync();
        //    model.AllergicTypes = await _context.AllergicTypes.OrderBy(v => v.AllergicTypeName).ToListAsync();
        //    model.OnSets = await _context.OnSets.OrderBy(v => v.OnSetName).ToListAsync();
        //    model.OnSetTypes = await _context.OnSetTypes.OrderBy(v => v.OnSetTypeName).ToListAsync();
        //    model.ReactionList = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
        //    model.Allergics = allergyyyy;
        //    Allergic allergic = new Allergic();
        //    btnSubmit = btnSubmit.ToLower().Trim();

        //    if (btnSubmit == "delete")
        //    {
        //        if (!((await authorizationService.AuthorizeAsync(User, "PatientAllergicDelete")).Succeeded))
        //        {
        //            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
        //        }

        //        ModelState.Clear();
        //        var delete = await _context.Allergics.FindAsync(ToDeleteId) ;
        //        try
        //        {
        //            if (delete != null)
        //            {
                        
        //                var allReact = await _context.AllergicPatientReactions.Where(m => m.AllergicId ==ToDeleteId).ToListAsync();

        //                _context.AllergicPatientReactions.RemoveRange(allReact);
        //                await _context.SaveChangesAsync();
        //                //var remove = await _context.Allergics.FindAsync();
        //                _context.Allergics.Remove(delete);
        //                await _context.SaveChangesAsync();
        //                TempData["SuccessedAlertMessage"] = "Delete Successful";

        //                return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = delete.AppointmentId });
        //            }
        //            else
        //            {
        //                return View("Error");
        //            }
        //        }
        //        catch (DbUpdateException ex)
        //        {
        //            ModelState.AddModelError("", $"Allergic Name can't delete!.These table are use in another Table");
        //            return View(model);
        //        }
        //    }
        //    else
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            if (btnSubmit == "update")
        //            {
        //                if (!(await authorizationService.AuthorizeAsync(User, "PatientAllergicUpdate")).Succeeded)
        //                {
        //                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

        //                }
        //                try
        //                {
        //                    if (model.AllergicId == null || model.AllergicId.Value == 0)
        //                    {
        //                        return View("Error");
        //                    }
        //                    else
        //                    {
        //                        //var updated = await _context.Allergics.Include(x => x.DoctorAppointmentData).FindAsync(model.AllergicId.Value);

        //                        var allergicRecord = await _context.Allergics.Include(x => x.DoctorAppointmentData).Where(x => x.AllergicId ==model.AllergicId.Value).FirstOrDefaultAsync();

        //                        var appointment = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
        //                        if (appointment.Count != 0)
        //                        {
        //                            var exist = appointment.Where(m => m.IsNoKnownAllergy == model.IsNoKnownAllergy).ToList();

        //                            if (exist.Count != 0)
        //                            {
        //                                if (model.IsNoKnownAllergy == true)
        //                                {
        //                                    var True = appointment.Where(a => a.AllergicId == model.AllergicId).FirstOrDefault();
        //                                    _context.Allergics.RemoveRange(True);
        //                                    _context.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    var ReactionLists = _context.AllergicPatientReactions.Where(a => a.AllergicId == model.AllergicId).ToList();
        //                                    _context.AllergicPatientReactions.RemoveRange(ReactionLists);
        //                                    var False = appointment.Where(a => a.AllergicId == model.AllergicId).FirstOrDefault();
        //                                    _context.Allergics.RemoveRange(False);
        //                                    _context.SaveChanges();

        //                                }

        //                            }
        //                            var noexist = appointment.Where(m => m.IsNoKnownAllergy != model.IsNoKnownAllergy).ToList();

        //                            if (noexist.Count != 0)
        //                            {

        //                                var allergicPatientReactionList = await _context.AllergicPatientReactions.Include(x => x.Allergic).Where(a => a.Allergic.AppointmentId == allergicRecord.AppointmentId).ToListAsync();
        //                                _context.AllergicPatientReactions.RemoveRange(allergicPatientReactionList);
        //                                await _context.SaveChangesAsync();
        //                                _context.Allergics.RemoveRange(appointment);
        //                                _context.SaveChanges();
        //                            }

        //                        }



        //                        if (allergic != null)
        //                        {
        //                            allergic.AppointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId;
        //                            allergic.AllergicTypeId = model.AllergicTypeId;
        //                            allergic.OnSetId = model.OnSetId;
        //                            allergic.OnSetTypeId = model.OnSetTypeId;
        //                            allergic.AllergicTo = model.AllergicTo;
        //                            allergic.Remark = model.Remark;
        //                            allergic.ResolvedDate = model.ResolvedDate;
        //                            allergic.OnSetDate = model.OnSetDate;
        //                            allergic.AllergicStatus = model.AllergicStatus;
        //                            allergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
        //                            allergic.UpdatedDate = UpdatedDate;
        //                            _context.Allergics.Update(allergic);
        //                            await _context.SaveChangesAsync();


        //                            //var allergicId = await _context.Allergics.FindAsync(model.AllergicId.Value);

        //                            if (CheckData != null)
        //                            {
        //                                var allReact = await _context.AllergicPatientReactions.Where(m => m.AllergicId == allergic.AllergicId).ToListAsync();

        //                                _context.AllergicPatientReactions.RemoveRange(allReact);
        //                                _context.SaveChanges();
        //                                foreach (var check in CheckData)
        //                                {
        //                                    var allreaction = new AllergicPatientReaction
        //                                    {
        //                                        AllergicId = allergic.AllergicId,
        //                                        ReactionId = int.Parse(check)
        //                                    };

        //                                    await _context.AllergicPatientReactions.AddAsync(allreaction);
        //                                }
        //                                _context.SaveChanges();
        //                            }
        //                            TempData["SuccessedAlertMessage"] = "Update Successful";

        //                            return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });


        //                        }
        //                        else
        //                        {
        //                            return View("Error");
        //                        }

        //                    }
        //                }
        //                catch (DbUpdateConcurrencyException)
        //                {
        //                    throw;
        //                }

        //            }
        //            //else if (btnSubmit == "delete")
        //            //{
        //            //    if (!(await authorizationService.AuthorizeAsync(User, "PatientAllergicDelete")).Succeeded)
        //            //    {
        //            //        return RedirectToAction("AccessDenied", "UserAccount", "Administration");
        //            //    }
        //            //    try
        //            //    {

        //            //        if (model.AllergicId == null || model.AllergicId.Value == 0)
        //            //        {
        //            //            return View("Error");
        //            //        }
        //            //        else
        //            //        {

        //            //            var updatedAllergics = await _context.Allergics.FindAsync(model.AllergicId.Value);
        //            //            if (updatedAllergics != null)
        //            //            {
        //            //                var allReact = await _context.AllergicPatientReactions.Where(m => m.AllergicId == model.AllergicId.Value).ToListAsync();

        //            //                _context.AllergicPatientReactions.RemoveRange(allReact);
        //            //                await _context.SaveChangesAsync();
        //            //                var remove = await _context.Allergics.FindAsync(model.AllergicId.Value);
        //            //                _context.Allergics.Remove(remove);
        //            //                await _context.SaveChangesAsync();
        //            //                TempData["SuccessedAlertMessage"] = "Delete Successful";

        //            //                return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });


        //            //            }
        //            //            else
        //            //            {
        //            //                return View("Error");
        //            //            }


        //            //        }
        //            //    }
        //            //    catch (DbUpdateConcurrencyException)
        //            //    {
        //            //        throw;
        //            //    }


        //            //}
        //            else
        //            {
        //                if (!(await authorizationService.AuthorizeAsync(User, "PatientAllergicCreate")).Succeeded)
        //                {
        //                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

        //                }

        //                if (model.IsNoKnownAllergy == false)
        //                {

        //                    if (string.IsNullOrEmpty(model.AllergicTo) ||

        //                            model.AllergicTypeId == null ||

        //                             model.AllergicStatus == null)

        //                    {
        //                        var allergy = await _context.Allergics.Where(v => v.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
        //                        if (allergy == null || allergy.Count == 0)
        //                        {
        //                            model.IsAllergy = true;
        //                        }
        //                        else
        //                        {
        //                            var aa = allergy.Where(a => a.IsNoKnownAllergy).FirstOrDefault();


        //                            if (aa == null)
        //                            {
        //                                model.IsAllergy = true;
        //                            }
        //                            else
        //                            {
        //                                model.IsAllergy = false;
        //                            }
        //                        }
        //                        if (string.IsNullOrEmpty(model.AllergicTo))
        //                        {

        //                            ModelState.AddModelError("AllergicTo", "AllergicTo field is required");
        //                        }
        //                        if (model.AllergicTypeId == null)
        //                        {

        //                            ModelState.AddModelError("AllergicTypeId", "AllergicTypeName field is required");
        //                        }

        //                        if (model.AllergicStatus == null)
        //                        {
        //                            ModelState.AddModelError("AllergicStatus", "AllergicStatus field is required");
        //                        }


        //                        return View(model);

        //                    }



        //                    else
        //                    {
        //                        var allergy = await _context.Allergics.Where(v => v.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
        //                        if (allergy == null || allergy.Count == 0)
        //                        {
        //                            model.IsAllergy = true;
        //                        }
        //                        else
        //                        {
        //                            var aa = allergy.Where(a => a.IsNoKnownAllergy).FirstOrDefault();


        //                            if (aa == null)
        //                            {
        //                                model.IsAllergy = true;
        //                            }
        //                            else
        //                            {
        //                                model.IsAllergy = false;
        //                            }
        //                        }
        //                        var appointment = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
        //                        if (appointment.Count != 0)
        //                        {
        //                            var exist = appointment.Where(m => m.IsNoKnownAllergy == model.IsNoKnownAllergy).ToList();

        //                            if (exist.Count != 0)
        //                            {
        //                                if (model.IsNoKnownAllergy == true)
        //                                {
        //                                    var allergiclist = await _context.Allergics.Where(a => a.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToListAsync();
        //                                    _context.Allergics.RemoveRange(allergiclist);
        //                                    _context.SaveChanges();
        //                                }
        //                                else
        //                                {
        //                                    _context.SaveChanges();

        //                                }

        //                            }
        //                            var noexist = appointment.Where(m => m.IsNoKnownAllergy != model.IsNoKnownAllergy).ToList();

        //                            if (noexist.Count != 0)
        //                            {
        //                                var a = await _context.AllergicPatientReactions.ToListAsync();
        //                                _context.AllergicPatientReactions.RemoveRange(a);
        //                                await _context.SaveChangesAsync();
        //                                _context.Allergics.RemoveRange(appointment);
        //                                await _context.SaveChangesAsync();
        //                            }

        //                        }


        //                        allergic.AppointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId;
        //                        allergic.AllergicStatus = model.AllergicStatus;
        //                        allergic.AllergicTo = model.AllergicTo;
        //                        allergic.Remark = model.Remark;
        //                        allergic.ResolvedDate = model.ResolvedDate;
        //                        allergic.OnSetDate = model.OnSetDate;
        //                        allergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
        //                        allergic.AllergicTypeId = model.AllergicTypeId;
        //                        allergic.OnSetId = model.OnSetId;
        //                        allergic.OnSetTypeId = model.OnSetTypeId;
        //                        allergic.CreatedDate = CreatedDate;

        //                        await _context.Allergics.AddAsync(allergic);

        //                        await _context.SaveChangesAsync();
        //                        var v = _context.Allergics.ToList();
        //                        foreach (var ReactionId in lang.Where(x => x.IsChecked).Select(x => x.ReactionId))
        //                        {
        //                            var all = new AllergicPatientReaction
        //                            {
        //                                AllergicId = v.LastOrDefault().AllergicId,
        //                                ReactionId = ReactionId
        //                            };

        //                            await _context.AllergicPatientReactions.AddAsync(all);
        //                        }
        //                        await _context.SaveChangesAsync();


        //                        TempData["SuccessedAlertMessage"] = "Save Successful";

        //                    }
        //                    return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });
        //                }


        //                if (string.IsNullOrEmpty(model.AllergicTo) ||

        //                    model.AllergicTypeId == null ||
        //                    model.AllergicStatus == null)

        //                {
        //                    var appointment = _context.Allergics.Where(x => x.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToList();
        //                    if (appointment.Count != 0)
        //                    {
        //                        var exist = appointment.Where(m => m.IsNoKnownAllergy == model.IsNoKnownAllergy).ToList();

        //                        if (exist.Count != 0)
        //                        {
        //                            if (model.IsNoKnownAllergy == true)
        //                            {
        //                                var allergicList = _context.Allergics.Where(a => a.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToList();
        //                                _context.Allergics.RemoveRange(allergicList);
        //                                _context.SaveChanges();
        //                            }
        //                            else
        //                            {
        //                                _context.SaveChanges();

        //                            }

        //                        }
        //                        var noexist = appointment.Where(m => m.IsNoKnownAllergy != model.IsNoKnownAllergy).ToList();

        //                        if (noexist.Count != 0)
        //                        {
        //                            var a = _context.AllergicPatientReactions.ToList();
        //                            _context.AllergicPatientReactions.RemoveRange(a);
        //                            await _context.SaveChangesAsync();
        //                            _context.Allergics.RemoveRange(appointment);
        //                            _context.SaveChanges();
        //                        }

        //                    }

        //                    var allergy = _context.Allergics.Where(v => v.AppointmentId == model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId).ToList();
        //                    if (allergy == null || allergy.Count == 0)
        //                    {
        //                        model.IsAllergy = true;
        //                    }
        //                    else
        //                    {
        //                        var aa = allergy.Where(a => a.IsNoKnownAllergy).FirstOrDefault();


        //                        if (aa == null)
        //                        {
        //                            model.IsAllergy = true;
        //                        }
        //                        else
        //                        {
        //                            model.IsAllergy = false;
        //                        }
        //                    }
        //                    allergic.AppointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId;
        //                    allergic.AllergicStatus = model.AllergicStatus;
        //                    allergic.AllergicTo = model.AllergicTo;
        //                    allergic.Remark = model.Remark;
        //                    allergic.ResolvedDate = model.ResolvedDate;
        //                    allergic.OnSetDate = model.OnSetDate;
        //                    allergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
        //                    allergic.AllergicTypeId = model.AllergicTypeId;
        //                    allergic.OnSetId = model.OnSetId;
        //                    allergic.OnSetTypeId = model.OnSetTypeId;
        //                    allergic.CreatedDate = CreatedDate;

        //                    await _context.Allergics.AddAsync(allergic);

        //                    await _context.SaveChangesAsync();
        //                    var v = _context.Allergics.ToList();
        //                    foreach (var ReactionId in lang.Where(x => x.IsChecked).Select(x => x.ReactionId))
        //                    {
        //                        var all = new AllergicPatientReaction
        //                        {
        //                            AllergicId = v.LastOrDefault().AllergicId,
        //                            ReactionId = ReactionId
        //                        };

        //                        await _context.AllergicPatientReactions.AddAsync(all);
        //                    }
        //                    _context.SaveChanges();


        //                    TempData["SuccessedAlertMessage"] = "Save Successful";

        //                }
        //                return RedirectToAction(nameof(DrConsultationAllergic), new { appointmentId = model.DoctorConsultationViewModel.DoctorAppointmentData.AppointmentId });

        //            }

        //        }

        //        else
        //        {
        //            return View();
        //        }
        //    }
          
        //}
    }
}
