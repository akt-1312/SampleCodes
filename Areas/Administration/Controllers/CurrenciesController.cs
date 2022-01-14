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
using Microsoft.AspNetCore.Authorization;
using HMS.Extensions;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class CurrenciesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public CurrencyViewModel currencyVM { get; set; }
        public CurrenciesController(ApplicationDbContext context,
                                    IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            currencyVM = new CurrencyViewModel()
            {
                Currencies = new List<Currency>(),
            };
        }

        [HttpGet]
        [Authorize(Policy = "CurrencyView")]
        public async Task<IActionResult> Currency()
        {
            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            CurrencyViewModel model = new CurrencyViewModel();
            model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyName).ToListAsync();
            return View(model);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "CurrencyView")]
        public async Task<IActionResult> Currency(CurrencyViewModel model, string btnSubmit, int toDeleteCurrencyId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyName).ToListAsync();
            Currency currency = new Currency();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "CurrencyDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.Currencies.FindAsync(toDeleteCurrencyId);
                try
                {
                    if (delete != null)
                    {
                        _context.Currencies.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Currency));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Currency ({delete.CurrencyCode}) can't delete!.These table are use in another Table");
                    return View(model);
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //var NameExist = await _context.Townships.Where(x => x.Tsp_name == model.Township.Tsp_name).FirstOrDefaultAsync();
                    if (btnSubmit == "update")
                    {
                        if ((await authorizationService.AuthorizeAsync(User, "CurrencyUpdate")).Succeeded)
                        {
                            try
                            {
                                if (model.CurrencyId == null || model.CurrencyId.Value == 0)
                                {
                                    return View("Error");
                                }
                                else
                                {
                                    //if (await CurrencyExists(CurrencyCodeOrName: model.CurrencyCode, CurrencyId: model.CurrencyId) ||
                                    //    await CurrencyExists(CurrencyCodeOrName: model.CurrencyName, CurrencyId: model.CurrencyId))
                                    //{
                                    //    if (await CurrencyExists(CurrencyCodeOrName: model.CurrencyCode, CurrencyId: model.CurrencyId))
                                    //    {
                                    //        ModelState.AddModelError("CurrencyCode", $"Currency Code {model.CurrencyCode} already exist in current table.");
                                    //    }
                                    //    else
                                    //    {
                                    //        ModelState.AddModelError("CurrencyName", $"Currency Name {model.CurrencyName} already exist in current table.");
                                    //    }
                                    //    return View(model);

                                    //}
                                    //else
                                    //{
                                        var updated = await _context.Currencies.FindAsync(model.CurrencyId.Value);
                                        if (updated != null)
                                        {
                                            updated.CurrencyName = model.CurrencyName;
                                            updated.CurrencyCode = model.CurrencyCode;
                                            updated.UpdatedDate = UpdatedDate;
                                            _context.Currencies.Update(updated);
                                            await _context.SaveChangesAsync();
                                            TempData["SuccessedAlertMessage"] = "Update Successful";
                                        }
                                        else
                                        {
                                            return View("Error");
                                        }
                                    ////}
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
                        if ((await authorizationService.AuthorizeAsync(User, "CurrencyUpdate")).Succeeded)
                        {
                            //if (await CurrencyExists(CurrencyCodeOrName: model.CurrencyCode, CurrencyId: model.CurrencyId) ||
                            //            await CurrencyExists(CurrencyCodeOrName: model.CurrencyName, CurrencyId: model.CurrencyId))
                            //{
                            //    if (await CurrencyExists(CurrencyCodeOrName: model.CurrencyCode, CurrencyId: model.CurrencyId))
                            //    {
                            //        ModelState.AddModelError("CurrencyCode", $"Currency Code {model.CurrencyCode} already exist in current table.");
                            //    }
                            //    else
                            //    {
                            //        ModelState.AddModelError("CurrencyName", $"Currency Name {model.CurrencyName} already exist in current table.");
                            //    }
                            //    return View(model);
                            //}
                            //else
                            //{
                                currency.CurrencyName = model.CurrencyName;
                                currency.CurrencyCode = model.CurrencyCode;
                                currency.CreatedDate = CreatedDate;
                                await _context.Currencies.AddAsync(currency);
                                await _context.SaveChangesAsync();
                                TempData["SuccessedAlertMessage"] = "Save Successful";
                            //}
                        }
                        else
                        {
                            return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                        }
                    }
                    return RedirectToAction(nameof(Currency));
                }
                else
                {
                    ModelState.AddModelError("Currency.CurrencyName", "Currency Name can't create!");

                    return View(model);

                }
            }
        }
        //[HttpPost]
        //[Authorize(Policy = "CurrencyDelete")]
        //public async Task<IActionResult> DeleteCurrency(int? CurrencyId)
        //{
        //    ModelState.AddModelError("Currency.CurrencyName", "Currency Name already exist");
        //    CurrencyViewModel model = new CurrencyViewModel();
        //    model.Currencies = await _context.Currencies.OrderBy(x => x.CurrencyName).ToListAsync();
        //    var deletecurrency = await _context.Currencies.FindAsync(CurrencyId);
        //    try
        //    {

        //        if (deletecurrency != null)
        //        {
        //            _context.Currencies.Remove(deletecurrency);
        //            await _context.SaveChangesAsync();
        //            TempData["SuccessedAlertMessage"] = "Delete Successful";
        //        }
        //        else
        //        {
        //            //return View(model);
        //            return View("Error");
        //        }
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        ModelState.AddModelError("", $"Currency Name ({deletecurrency.CurrencyName}) can't delete!.These table are use in another Table");
        //        return View("Currency", model);
        //    }
        //    return RedirectToAction(nameof(Currency));

        //    //return View();
        //}

        //private async Task<bool> CurrencyExists(string CurrencyCodeOrName, int? CurrencyId)
        //{
        //    var currency = new Currency();
        //    if (CurrencyId != null)
        //    {
        //        currency = await _context.Currencies
        //            .Where(x => (x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim() == CurrencyCodeOrName.StringCompareFormat()
        //            || x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim() == CurrencyCodeOrName.StringCompareFormat()) && x.CurrencyId != CurrencyId).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        currency = await _context.Currencies
        //            .Where(x => x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim() == CurrencyCodeOrName.StringCompareFormat() ||
        //            x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim() == CurrencyCodeOrName.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    if (currency == null)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}

        //[AllowAnonymous]
        //[AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> CurrencyCodeIsInUse(string currencyCode, int? currencyId)
        //{
        //    Currency currency = new Currency();
        //    if (currencyId == null || currencyId.Value == 0)
        //    {
        //        currency = await _context.Currencies.Where(x => x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyCode.StringCompareFormat() || x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyCode.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        currency = await _context.Currencies.Where(x => x.CurrencyId != currencyId.Value && (x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyCode.StringCompareFormat() || x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyCode.StringCompareFormat())).FirstOrDefaultAsync();
        //    }

        //    if (currency == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Currency Code {currencyCode} is already exist in current table");
        //    }
        //}

        //[AllowAnonymous]
        //[AcceptVerbs("Get", "Post")]
        //public async Task<IActionResult> CurrencyNameIsInUse(string currencyName, int? CurrencyId)
        //{
        //    Currency currency = new Currency();
        //    if (CurrencyId == null || CurrencyId.Value == 0)
        //    {
        //        currency = await _context.Currencies.Where(x => x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyName.StringCompareFormat() || x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyName.StringCompareFormat()).FirstOrDefaultAsync();
        //    }
        //    else
        //    {
        //        currency = await _context.Currencies.Where(x => x.CurrencyId != CurrencyId.Value && (x.CurrencyCode.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyName.StringCompareFormat() || x.CurrencyName.Replace(" ", string.Empty).ToLower().Trim()
        //        == currencyName.StringCompareFormat())).FirstOrDefaultAsync();
        //    }
        //    if (currency == null)
        //    {
        //        return Json(true);
        //    }
        //    else
        //    {
        //        return Json($"Currency Name {currencyName} is already exist in currency table");
        //    }


        //}

        //[HttpGet]
        //public async Task<IActionResult> Currency(int CurrencyId, string btnActionName)
        //{
        //    ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
        //    var totalCurrencies = _context.Currencies.ToList();
        //    var currentCurrency = totalCurrencies.Where(a => a.CurrencyId == CurrencyId).FirstOrDefault();
        //    if (CurrencyId == 0)
        //    {
        //        CurrencyViewModel currencyVM = new CurrencyViewModel()
        //        {
        //            Currencies = totalCurrencies,
        //            Currency = currentCurrency,
        //            BtnActionName = "Create",
        //        };

        //        return View(currencyVM);
        //    }
        //    else
        //    {
        //        List<Currency> lstCurrencies = new List<Currency>();
        //        lstCurrencies = await _context.Currencies.Where(a => a.CurrencyId == CurrencyId).ToListAsync();
        //        CurrencyViewModel currencyVm = new CurrencyViewModel()
        //        {
        //            Currencies = totalCurrencies,
        //            Currency = lstCurrencies.FirstOrDefault(),
        //            BtnActionName = btnActionName,
        //        };
        //        return View(currencyVm);
        //    }

        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Currency(CurrencyViewModel model,DateTime cDate,DateTime uDate)
        //{
        //        var nameExist = await _context.Currencies.Where(a => a.CurrencyName == model.Currency.CurrencyName).FirstOrDefaultAsync();
        //        var codeExist = await _context.Currencies.Where(a => a.CurrencyCode == model.Currency.CurrencyCode).FirstOrDefaultAsync();
        //    if (ModelState.IsValid)
        //    {
        //        cDate = DateTime.UtcNow.AddMinutes(390);
        //        uDate = DateTime.UtcNow.AddMinutes(390);
        //        var totalCurrencies = await _context.Currencies.OrderBy(a => a.CurrencyName).ToListAsync();
        //        model.Currencies = totalCurrencies;
        //        var CurrencyNameExist = await _context.Currencies.Where(x => x.CurrencyName == model.Currency.CurrencyName || x.CurrencyCode == model.Currency.CurrencyCode).FirstOrDefaultAsync();
        //        if (CurrencyNameExist == null && model.BtnActionName != "Delete")
        //        {
        //            if (model.BtnActionName == "Edit")
        //            {
        //                if (string.IsNullOrEmpty(model.Currency.CurrencyName) ||
        //                          model.Currency.CurrencyCode == null)


        //                {
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyName))
        //                    {

        //                        ModelState.AddModelError("Currency.CurrencyName", "CurrencyName field is required");
        //                    }
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyCode))
        //                    {
        //                        ModelState.AddModelError("Currency.CurrencyCode", "CurrencyCode field is required");
        //                    }
        //                    return View(model);
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        var dati = _context.Currencies.Where(p => p.CurrencyId == model.Currency.CurrencyId).Single();
        //                        dati.CurrencyCode = model.Currency.CurrencyCode;
        //                        dati.CurrencyName = model.Currency.CurrencyName;
        //                        dati.UpdatedDate = uDate;
        //                        await _context.SaveChangesAsync();
        //                        TempData["SuccessedAlertMessage"] = "Update Successful";
        //                        return RedirectToAction(nameof(Currency));
        //                    }
        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        if (!CurrencyExists(model.Currency.CurrencyId))
        //                        {
        //                            return NotFound();
        //                        }
        //                        else
        //                        {
        //                            throw;
        //                        }
        //                    }
        //                }

        //            }
        //            else
        //            {


        //                if (string.IsNullOrEmpty(model.Currency.CurrencyName) ||
        //                           model.Currency.CurrencyCode == null)


        //                {
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyName))
        //                    {

        //                        ModelState.AddModelError("Currency.CurrencyName", "CurrencyName field is required");
        //                    }
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyCode))
        //                    {
        //                        ModelState.AddModelError("Currency.CurrencyCode", "CurrencyCode field is required");

        //                    }
        //                    return View(model);
        //                }
        //                else
        //                {
        //                    model.Currency.CreatedDate = cDate;
        //                    model.Currency.UpdatedDate = uDate;
        //                    await _context.Currencies.AddAsync(model.Currency);
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessedAlertMessage"] = "Save Successful";

        //                }


        //                return RedirectToAction(nameof(Currency));

        //            }
        //        }
        //        else
        //        {
        //            if (model.BtnActionName == "Delete")
        //            {
        //                try
        //                {
        //                    Currency currency = await _context.Currencies
        //                 .FindAsync(model.Currency.CurrencyId);
        //                    _context.Currencies.Remove(currency);
        //                    await _context.SaveChangesAsync();
        //                    TempData["SuccessedAlertMessage"] = "Delete Successful";
        //                }
        //                catch (DbUpdateException ex)
        //                {

        //                    ModelState.AddModelError("", "Currency Name can't delete!.These table are use in another Table");
        //                    return View(model);
        //                }

        //                return RedirectToAction(nameof(Currency));

        //            }


        //            else
        //            {
        //                if (string.IsNullOrEmpty(model.Currency.CurrencyName) ||
        //                          model.Currency.CurrencyCode == null)


        //                {
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyName))
        //                    {

        //                        ModelState.AddModelError("Currency.CurrencyName", "CurrencyName field is required");
        //                    }
        //                    if (string.IsNullOrEmpty(model.Currency.CurrencyCode))
        //                    {
        //                        ModelState.AddModelError("Currency.CurrencyCode", "Currency Code  field is required");

        //                    }

        //                    return View(model);
        //                }
        //                else
        //                {
        //                    try
        //                  {
        //                        if (nameExist != null)
        //                        {
        //                            ModelState.AddModelError("Currency.CurrencyName", "Currency Name already exist");

        //                        }
        //                        if(codeExist!=null)
        //                        {
        //                            ModelState.AddModelError("Currency.CurrencyCode", "Currency Code  already exist");

        //                        }


        //                        return View(model);
        //                    }
        //                    catch (DbUpdateConcurrencyException)
        //                    {
        //                        if (!CurrencyExists(model.Currency.CurrencyId))
        //                        {
        //                            return NotFound();
        //                        }
        //                        else
        //                        {
        //                            throw;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {

        //        //if (string.IsNullOrEmpty(model.Currency.CurrencyName) ||
        //        //                  model.Currency.CurrencyCode == null)


        //        //{
        //        //    if (string.IsNullOrEmpty(model.Currency.CurrencyName))
        //        //    {

        //        //        ModelState.AddModelError("Currency.CurrencyName", "CurrencyName field is required");
        //        //    }
        //        //    if (string.IsNullOrEmpty(model.Currency.CurrencyCode))
        //        //    {
        //        //        ModelState.AddModelError("Currency.CurrencyCode", "CurrencyCode field is required");

        //        //    }
        //        //    return View(model);
        //        //}
        //        //else
        //        //{
        //        //    //    var NameExist = await _context.Currencies.Where(x => x.CurrencyName == model.Currency.CurrencyName).FirstOrDefaultAsync();
        //        //    //    if (NameExist != null)
        //        //    //    {
        //        //    //        if (nameExist != null)
        //        //    //        {
        //        //    //            ModelState.AddModelError("Currency.CurrencyName", "Currency Name already exist");
        //        //    //        }
        //        //    //        if(codeExist!=null)
        //        //    //        {
        //        //    //        ModelState.AddModelError("Currency.CurrencyCode", "Currency Code  already exist");

        //        //    //        }
        //        //    //        return View(model);

        //        //    //    }

        //        //    //    else
        //        //    //    {
        //        //    //        await _context.Currencies.AddAsync(model.Currency);
        //        //    //        await _context.SaveChangesAsync();
        //        //    //        TempData["SuccessedAlertMessage"] = "Save Successful";

        //        //    //        ModelState.AddModelError("Gender.GenderName", "Gender Name can't create!");

        //        //    //    }
        //        //    //}

        //        //    //return RedirectToAction(nameof(Currency));
        //        //}
        //            return View("Error");
        //    }

        //}
        //[HttpPost]
        //private bool CurrencyNameExists(string CurrencyName,string CurrencyCode)
        //{
        //    return _context.Currencies.Any(e => e.CurrencyName == CurrencyName || e.CurrencyCode == CurrencyCode);

        //}
        //private bool CurrencyExists(int id)
        //{
        //    return _context.Currencies.Any(e => e.CurrencyId == id);
        //}
    }
}
