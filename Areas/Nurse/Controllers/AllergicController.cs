using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Nurse;
using HMS.Models.ViewModels.Nurse;
using HMS.Models.Administration;
using HMS.Models.Enums;
using NPOI.SS.Formula.Functions;
using HMS.Models.Reception;
using Microsoft.AspNetCore.Authorization;
using HMS.Extensions;

namespace HMS.Areas.Nurse.Controllers
{
    [Area("Nurse")]
    public class AllergicController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public AllergicViewModel AllergicVM { get; set; }
        public AllergicController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            AllergicVM = new AllergicViewModel()
            {


                AllergicTypes = new List<AllergicType>(),
                OnSetTypes = new List<OnSetType>(),
                OnSets = new List<OnSet>(),
                Reactions = new List<Reaction>(),
                ReactionList = new List<Reaction>(),
                AllergicPatientReactionList = new List<AllergicPatientReaction>(),
                AllergicPatientReactions = new List<AllergicPatientReaction>(),
                Allergics = new List<AllergicReactionViewModel>(),
                DoctorAppointmentDataList = new List<DoctorAppointmentData>(),
            };
            _context = context;
        }
        [Authorize(Policy = "PatientAllergicView")]

        [HttpGet]
        public async Task<IActionResult> Allergic(int appointmentId)
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            AllergicViewModel model = new AllergicViewModel();
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

            var appointmentData = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == appointmentId)
                         .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                         .Include(a => a.Department).FirstOrDefaultAsync();

            model.AllergicTypes = await _context.AllergicTypes.OrderBy(v => v.AllergicTypeName).ToListAsync();
            model.OnSets = await _context.OnSets.OrderBy(v => v.OnSetName).ToListAsync();
            model.OnSetTypes = await _context.OnSetTypes.OrderBy(v => v.OnSetTypeName).ToListAsync();
            model.Reactions = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
            model.Allergics = allergic;

            model.DoctorAppointmentData = appointmentData;
            return View(model);

        }

        [Authorize(Policy = "PatientAllergicView")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Allergic(AllergicViewModel model, string btnSubmit, int? toDeleteAllergicId)
        {
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
                    return RedirectToAction(nameof(Allergic), new { appointmentId = toDeleteAllergic.AppointmentId });
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
                var noKnownAllergicList = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorAppointmentData.AppointmentId && x.IsNoKnownAllergy).ToListAsync();
                bool isNoKnownAllergyPatient = noKnownAllergicList.Count > 0;
                if (!isNoKnownAllergyPatient)
                {
                    var allReactionsOfAllergic = await _context.AllergicPatientReactions.Include(x => x.Reaction).OrderBy(x => x.Reaction.ReactionName).ToListAsync();
                    var allergicsOfAppointmentPatient = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorAppointmentData.AppointmentId).Include(x => x.DoctorAppointmentData)
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

                var appointmentData = await _context.DoctorAppointmentDatas.Where(p => p.AppointmentId == model.DoctorAppointmentData.AppointmentId)
                             .Include(m => m.PatientRegistration).Include(a => a.DoctorInfo).Include(a => a.Unit)
                             .Include(a => a.Department).FirstOrDefaultAsync();

                model.AllergicTypes = await _context.AllergicTypes.OrderBy(v => v.AllergicTypeName).ToListAsync();
                model.OnSets = await _context.OnSets.OrderBy(v => v.OnSetName).ToListAsync();
                model.OnSetTypes = await _context.OnSetTypes.OrderBy(v => v.OnSetTypeName).ToListAsync();
                model.Reactions = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
                model.Allergics = allergic;

                model.DoctorAppointmentData = appointmentData;
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "create")
                    {
                        try
                        {
                            var createAllergic = new Allergic();
                            if (model.IsNoKnownAllergy)
                            {
                                var alreadyExistPatientReactions = await _context.AllergicPatientReactions.Include(x => x.Allergic).Where(x => x.Allergic.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistPatientReactions != null && alreadyExistPatientReactions.Count > 0)
                                {
                                    _context.AllergicPatientReactions.RemoveRange(alreadyExistPatientReactions);
                                }

                                var alreadyExistAllergics = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistAllergics != null && alreadyExistAllergics.Count > 0)
                                {
                                    _context.Allergics.RemoveRange(alreadyExistAllergics);
                                }

                                createAllergic.IsNoKnownAllergy = model.IsNoKnownAllergy;
                                createAllergic.AppointmentId = model.DoctorAppointmentData.AppointmentId;
                                createAllergic.Remark = model.Remark;
                                createAllergic.CreatedDate = DateTime.UtcNow.AddMinutes(390);
                                createAllergic.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
                            }
                            else
                            {
                                var noKnownAllergy = await _context.Allergics.Where(x => x.IsNoKnownAllergy && x.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
                                _context.Allergics.RemoveRange(noKnownAllergy);

                                createAllergic.AppointmentId = model.DoctorAppointmentData.AppointmentId;
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
                            return RedirectToAction(nameof(Allergic), new { appointmentId = model.DoctorAppointmentData.AppointmentId });
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
                                var alreadyExistPatientReactions = await _context.AllergicPatientReactions.Include(x => x.Allergic).Where(x => x.Allergic.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
                                if (alreadyExistPatientReactions != null && alreadyExistPatientReactions.Count > 0)
                                {
                                    _context.AllergicPatientReactions.RemoveRange(alreadyExistPatientReactions);
                                }

                                var alreadyExistAllergics = await _context.Allergics.Where(x => x.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
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
                                var noKnownAllergy = await _context.Allergics.Where(x => x.IsNoKnownAllergy && x.AppointmentId == model.DoctorAppointmentData.AppointmentId).ToListAsync();
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
                            return RedirectToAction(nameof(Allergic), new { appointmentId = model.DoctorAppointmentData.AppointmentId });
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
    }
}
