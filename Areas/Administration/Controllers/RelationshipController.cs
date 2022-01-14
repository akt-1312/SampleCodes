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
    public class RelationshipController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly ApplicationDbContext _context;
        public RelationshipViewModel RelationshipViewModel { get; set; }
        public RelationshipController(ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            RelationshipViewModel = new RelationshipViewModel()
            {
                Relationship = new Relationship(),
                RelationshipList = new List<Relationship>(),
            };
        }

        // GET: Administration/Relationship
        [Authorize(Policy = "RelationshipView")]

        [HttpGet]
        public IActionResult Relationship()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            RelationshipViewModel model = new RelationshipViewModel();
            var total = _context.Relationships.OrderBy(a => a.RelationshipName).ToList();
            model.RelationshipList = total;
            return View(model);
          
        }
        [Authorize(Policy = "RelationshipView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Relationship(RelationshipViewModel model,string btnSubmit,int toDeleteId)
        {

            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            btnSubmit = btnSubmit.ToLower().Trim();
            var total = await _context.Relationships.OrderBy(a => a.RelationshipName).ToListAsync();
            model.RelationshipList = total;
            Relationship relationships = new Relationship();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "RelationshipDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var relationship = await _context.Relationships.FindAsync(toDeleteId);
                try
                {
                    if (relationship != null)
                    {
                        _context.Relationships.Remove(relationship);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Relationship));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Relationship Name ({relationship.RelationshipName}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            if (ModelState.IsValid)
            {



                if (btnSubmit == "update")
                {
                    try
                    {

                        if (model.RelationshipId == null || model.RelationshipId.Value == 0)
                        {
                            return View("Error");
                        }
                        else
                        {
                            if (await RelationshipExist(relationshipName: model.RelationshipName, relationshipId: model.RelationshipId))
                            {
                                ModelState.AddModelError("RelationshipName", "RelationshipName   already exist");
                                return View(model);

                            }
                            else
                            {
                                if (!(await authorizationService.AuthorizeAsync(User, "RelationshipUpdate")).Succeeded)
                                {
                                    return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                }
                                var updated = await _context.Relationships.FindAsync(model.RelationshipId.Value);
                                updated.RelationshipName = model.RelationshipName;
                                updated.UpdatedDate = UpdatedDate;

                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Update Successful";

                                return RedirectToAction(nameof(Relationship));
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
                    if (await RelationshipExist(relationshipName: model.RelationshipName, relationshipId: null))
                    {
                        ModelState.AddModelError("RelationshipName", "RelationshipName already exist");
                        return View(model);
                    }
                    else
                    {
                        if (!(await authorizationService.AuthorizeAsync(User, "RelationshipCreate")).Succeeded)
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                        }
                        relationships.RelationshipName = model.RelationshipName;
                        relationships.CreatedDate = CreatedDate;
                        await _context.Relationships.AddAsync(relationships);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Save Successful";

                        return RedirectToAction(nameof(Relationship));
                    }

                } 

         
            }
            else
            {

                ModelState.AddModelError("RelationshipName", "RelationshipName can't create!");

                return View(model);
            }
        }
        private async Task<bool> RelationshipExist(string relationshipName, int? relationshipId)
        {
            var relationship = new Relationship();
            if (relationshipId == null || relationshipId.Value == 0)
            {
                relationship = await _context.Relationships
                    .Where(x => x.RelationshipName == relationshipName).FirstOrDefaultAsync();
            }
            else
            {
                relationship = await _context.Relationships
                    .Where(x => x.RelationshipId != relationshipId && x.RelationshipName == relationshipName).FirstOrDefaultAsync();
            }
            if (relationship == null)
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
