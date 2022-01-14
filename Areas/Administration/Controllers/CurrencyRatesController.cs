using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HMS.Data;
using HMS.Models.Administration;
using HMS.Models.ViewModels;
using HMS.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class CurrencyRatesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;

        public CurrencyRateViewModel CurrencyRateViewModel { get; set; }
        public CurrencyRatesController(ApplicationDbContext context,
                                       IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            CurrencyRateViewModel = new CurrencyRateViewModel()
            {
                Currencies = _context.Currencies.ToList(),
                CurrencyRates = new List<CurrencyRate>(),
            };

        }
        [HttpGet]
        [Authorize(Policy = "CurrencyRateView")]
        public async Task<IActionResult> CurrencyRates()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            CurrencyRateViewModel model = new CurrencyRateViewModel();
            var totalcr = _context.CurrencyRates.Include(x => x.Currency).OrderBy(a => a.Currency).ToList();
            model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyCode).ToListAsync();
            model.CurrencyRates = totalcr;
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CurrencyRateView")]
        public async Task<IActionResult> CurrencyRates(CurrencyRateViewModel model, string btnSubmit)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalcr = _context.CurrencyRates.Include(x => x.Currency).OrderBy(a => a.Currency).ToList();
            var total = await _context.CurrencyRates.OrderBy(a => a.Currency).ToListAsync();
            model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyCode).ToListAsync();
            model.CurrencyRates = totalcr;
            CurrencyRate currencyRate = new CurrencyRate();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (ModelState.IsValid)
            {
                //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
                if (btnSubmit == "update")
                {
                    if ((await authorizationService.AuthorizeAsync(User, "CurrencyRateUpdate")).Succeeded)
                    {
                        try
                        {
                            if (model.CurrencyRateId == null || model.CurrencyRateId.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await CRExists(CurrencyId: model.CurrencyId, FormulaRate: model.FormulaRate))
                                {
                                    ModelState.AddModelError("CurrencyRateId", "Currency Rate already exist");
                                    return View(model);

                                }
                                else
                                {
                                    var updated = await _context.CurrencyRates.FindAsync(model.CurrencyRateId.Value);
                                    if (updated != null)
                                    {
                                        updated.CurrencyId = model.CurrencyId;
                                        updated.FormulaSign = model.FormulaSign;
                                        updated.FormulaRate = model.FormulaRate;
                                        updated.UpdatedDate = UpdatedDate;
                                        _context.CurrencyRates.Update(updated);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
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
                    else
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                }
                else
                {
                    if ((await authorizationService.AuthorizeAsync(User, "CurrencyRateUpdate")).Succeeded)
                    {
                        if (await CRExists(CurrencyId: model.CurrencyId, FormulaRate:model.FormulaRate))
                        {
                            ModelState.AddModelError("CurrencyRateId", "Currency Rate already exist");
                            return View(model);
                        }
                        else
                        {
                            currencyRate.CurrencyId = model.CurrencyId;
                            currencyRate.FormulaSign = model.FormulaSign;
                            currencyRate.FormulaRate = model.FormulaRate;
                            currencyRate.CreatedDate = CreatedDate;
                            await _context.CurrencyRates.AddAsync(currencyRate);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";
                        }
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                    }
                }
                return RedirectToAction(nameof(CurrencyRates));
            }
            else
            {
                ModelState.AddModelError("CurrencyRate.Currency", "Currency Rate can't create!");

                return View(model);

            }
        }

        [HttpPost]
        [Authorize(Policy = "CurrencyRateDelete")]
        public async Task<IActionResult> DeleteCurrencyRate(int? currencyRateId)
        {
            ModelState.AddModelError("CurrencyRate.Currency", "Currency Rate already exist");
            CurrencyRateViewModel model = new CurrencyRateViewModel();
            var totalcr = _context.CurrencyRates.Include(x => x.Currency).OrderBy(a => a.Currency).ToList();
            model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyCode).ToListAsync();
            model.CurrencyRates = totalcr;
            var delete = await _context.CurrencyRates.FindAsync(currencyRateId);
            try
            {

                if (delete != null)
                {
                    _context.CurrencyRates.Remove(delete);
                    await _context.SaveChangesAsync();
                    TempData["SuccessedAlertMessage"] = "Delete Successful";
                }
                else
                {
                    //return View(model);
                    return View("Error");
                }
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", $"Unit Name ({delete.Currency}) can't delete!.These table are use in another Table");
                return View("CurrencyRates", model);
            }
            return RedirectToAction(nameof(CurrencyRates));

            //return View();
        }

        private async Task<bool> CRExists(int? CurrencyId,float? FormulaRate)
        {
            var cr = new CurrencyRate();
            if (CurrencyId != null || CurrencyId.Value!=0)
            {
                cr = await _context.CurrencyRates
                    .Where(x => x.CurrencyId == CurrencyId && x.FormulaRate==FormulaRate).FirstOrDefaultAsync();
            }
            if (cr == null)
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
