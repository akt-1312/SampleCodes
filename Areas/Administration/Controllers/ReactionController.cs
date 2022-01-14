using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ReactionController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        [BindProperty]
        public ReactionViewModel ReactionViewModel { get; set; }
        public ReactionController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            ReactionViewModel = new ReactionViewModel()
            {
                Reaction = new Reaction(),
                ReactionList = new List<Reaction>(),
            };

        }

        // GET: Administration/Gender

        [Authorize(Policy = "AllergicReactionView")]

        [HttpGet]
        public IActionResult Reaction()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            ReactionViewModel model = new ReactionViewModel();
            var total = _context.Reactions.OrderBy(a => a.ReactionName).ToList();
            model.ReactionList = total;
            return View(model);

        }
        [Authorize(Policy = "AllergicReactionView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reaction(ReactionViewModel model, string btnSubmit, int toDeleteId)

        {
            btnSubmit = btnSubmit.ToLower().Trim();

            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var total = await _context.Reactions.OrderBy(a => a.ReactionName).ToListAsync();
            model.ReactionList = total;
            Reaction reaction = new Reaction();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "AllergicReactionDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.Reactions.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.Reactions.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Reaction));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Reaction Name ({delete.ReactionName}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    if (btnSubmit == "update")
                    {
                        try
                        {
                            if (model.ReactionId == null || model.ReactionId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await ReactionExist(reactionName: model.ReactionName, reactionId: model.ReactionId))
                                {
                                    ModelState.AddModelError("ReactionName", "Reaction Name  already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "AllergicReactionUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }
                                    var updated = await _context.Reactions.FindAsync(model.ReactionId.Value);
                                    updated.ReactionName = model.ReactionName;
                                    updated.UpdatedDate = UpdatedDate;
                                    await _context.SaveChangesAsync();
                                    TempData["SuccessedAlertMessage"] = "Update Successful";

                                    return RedirectToAction(nameof(Reaction));
                                }
                            }

                        }
                        catch
                        {
                            throw;
                        }
                    }



                    else
                    {
                        if (await ReactionExist(reactionName: model.ReactionName, reactionId: null))
                        {
                            ModelState.AddModelError("ReactionName", "Reaction Name  already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "AllergicReactionCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                            }
                            reaction.ReactionName = model.ReactionName;
                            reaction.CreatedDate = CreatedDate;
                            await _context.Reactions.AddAsync(reaction);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(Reaction));
                        }
                        
                    }
                }


                else
                {
                    ModelState.AddModelError("ReactionName", "ReactionName  can't create!");

                    return View(model);
                }



            }

        }
        private async Task<bool> ReactionExist(string reactionName, int? reactionId)
        {
            var reaction = new Reaction();
            if (reactionId == null || reactionId.Value == 0)
            {
                reaction = await _context.Reactions
                    .Where(x => x.ReactionName == reactionName).FirstOrDefaultAsync();
            }
            else
            {
                reaction = await _context.Reactions
                    .Where(x => x.ReactionId != reactionId && x.ReactionName == reactionName).FirstOrDefaultAsync();
            }
            if (reaction == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

    }
}
