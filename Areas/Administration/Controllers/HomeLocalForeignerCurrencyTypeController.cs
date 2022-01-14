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

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class HomeLocalForeignerCurrencyTypeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthorizationService authorizationService;
        public HomeLocalForeignerCurrencyTypeViewModel HomeLocalForeignerCurrencyTypeVM { get; set; }

        public HomeLocalForeignerCurrencyTypeController(ApplicationDbContext context,
                                                        IAuthorizationService authorizationService)
        {
            _context = context;
            this.authorizationService = authorizationService;
            HomeLocalForeignerCurrencyTypeVM = new HomeLocalForeignerCurrencyTypeViewModel()
            {
                HomeLocalForeignerCurrencyType = new HomeLocalForeignerCurrencyType(),
                HomeLocalForeignerCurrencyTypes = new List<HomeLocalForeignerCurrencyType>(),
            };
        }
        [HttpGet]
        [Authorize(Policy = "ExchangeSetupView")]
        public async Task<IActionResult> HomeLocalForeignerCurrencyType(int? HomeLocalForeignerCurrencyTypeId, string btnActionName)
        {
            var totalHLFCurrencyTypes = _context.HomeLocalForeignerCurrencyTypes.ToList();
            HomeLocalForeignerCurrencyTypeId = totalHLFCurrencyTypes.Select(x => x.HomeLocalForeignerCurrencyTypeId).Last();
            HomeLocalForeignerCurrencyType hlfctype = new HomeLocalForeignerCurrencyType();
            var currentHLFCurrencyRate = totalHLFCurrencyTypes.Where(a => a.HomeLocalForeignerCurrencyTypeId == HomeLocalForeignerCurrencyTypeId).LastOrDefault();
            hlfctype = new HomeLocalForeignerCurrencyType()
            {
                HomeCurrencyTypeId = currentHLFCurrencyRate.HomeCurrencyTypeId,
                LocalCurrencyTypeId = currentHLFCurrencyRate.LocalCurrencyTypeId,
                ForeignerCurrencyTypeId = currentHLFCurrencyRate.ForeignerCurrencyTypeId,
                NationalityId = currentHLFCurrencyRate.NationalityId,
                MultiplyHomeToHome = currentHLFCurrencyRate.MultiplyHomeToHome,
                MultiplyHomeToForeigner = currentHLFCurrencyRate.MultiplyHomeToForeigner,
            };
            HomeLocalForeignerCurrencyTypeId = 0;
            ViewData["HomeCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["LocalCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["ForeignerCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Nation_Id", "NationalityName");
            if (HomeLocalForeignerCurrencyTypeId == 0)
            {
                HomeLocalForeignerCurrencyTypeViewModel HLFCurrencyTypeVM = new HomeLocalForeignerCurrencyTypeViewModel()
                {
                    HomeLocalForeignerCurrencyTypes = totalHLFCurrencyTypes,
                    HomeLocalForeignerCurrencyType = hlfctype,
                    BtnActionName = "Save",
                    
                };

                ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
                return View(HLFCurrencyTypeVM);
            }
            else
            {
                List<HomeLocalForeignerCurrencyType> lstHLFCurrencyTypes = new List<HomeLocalForeignerCurrencyType>();
                lstHLFCurrencyTypes = await _context.HomeLocalForeignerCurrencyTypes.Where(a => a.HomeLocalForeignerCurrencyTypeId == HomeLocalForeignerCurrencyTypeId).ToListAsync();
                HomeLocalForeignerCurrencyTypeViewModel HLFCurrencyTypeVm = new HomeLocalForeignerCurrencyTypeViewModel()
                {
                    HomeLocalForeignerCurrencyTypes = lstHLFCurrencyTypes,
                    HomeLocalForeignerCurrencyType = lstHLFCurrencyTypes.LastOrDefault(),
                    BtnActionName = btnActionName,
                };
                ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
                return View(HLFCurrencyTypeVm);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ExchangeSetupView")]
        public async Task<IActionResult> HomeLocalForeignerCurrencyType(HomeLocalForeignerCurrencyTypeViewModel model, int? HomeLocalForeignerCurrencyTypeId, DateTime cDate)
        {
            ViewData["HomeCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["LocalCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["ForeignerCurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            ViewData["NationalityId"] = new SelectList(_context.Nationalities, "Nation_Id", "NationalityName");
            model.HomeLocalForeignerCurrencyTypes = await _context.HomeLocalForeignerCurrencyTypes.ToListAsync();
            model.HomeLocalForeignerCurrencyType.CreatedDate = cDate;
            if (ModelState.IsValid)
            {
                if (((await authorizationService.AuthorizeAsync(User, "RegistrationServiceDelete")).Succeeded))
                {
                    model.HomeLocalForeignerCurrencyType.IsActive = true;
                    await _context.HomeLocalForeignerCurrencyTypes.AddAsync(model.HomeLocalForeignerCurrencyType);
                    await _context.SaveChangesAsync();
                }
                else
                { 
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }
                if (model.HomeLocalForeignerCurrencyType.HomeCurrencyTypeId == 0 ||
                    model.HomeLocalForeignerCurrencyType.ForeignerCurrencyTypeId == 0 ||
                    model.HomeLocalForeignerCurrencyType.LocalCurrencyTypeId == 0 ||
                    model.HomeLocalForeignerCurrencyType.NationalityId == 0 ||
                    model.HomeLocalForeignerCurrencyType.MultiplyHomeToHome == 0 ||
                    model.HomeLocalForeignerCurrencyType.MultiplyHomeToForeigner == 0)

                {
                    if (model.HomeLocalForeignerCurrencyType.MultiplyHomeToHome == 0){
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.MultiplyHomeToHome", "Please insert a value");

                    }
                    if(model.HomeLocalForeignerCurrencyType.ForeignerCurrencyTypeId == 0)
                    {
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.ForeignerCurrencyTypeId", "required");
                    }
                    if(model.HomeLocalForeignerCurrencyType.LocalCurrencyTypeId == 0)
                    {
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.LocalCurrencyTypeId","reqiierd");
                    }
                    if (model.HomeLocalForeignerCurrencyType.NationalityId==0)
                    {
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.NationalityId", "required");
                    }
                    if (model.HomeLocalForeignerCurrencyType.HomeCurrencyTypeId==0)
                    {
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.HomeCurrencyTypeId", "required");
                    }
                    if(model.HomeLocalForeignerCurrencyType.MultiplyHomeToForeigner == 0)
                    {
                        ModelState.AddModelError("HomeLocalForeignerCurrencyType.MultiplyHomeToForeigner", "required");
                    }
                    return View(model);
                }

                else
                {
                    List<HomeLocalForeignerCurrencyType> oldSettingData = await _context.HomeLocalForeignerCurrencyTypes
                    .Where(x => x.HomeLocalForeignerCurrencyTypeId
                    != model.HomeLocalForeignerCurrencyType.HomeLocalForeignerCurrencyTypeId).ToListAsync();

                    List<HomeLocalForeignerCurrencyType> oldSettingDataToUpdateFalse = new List<HomeLocalForeignerCurrencyType>();
                    foreach (var item in oldSettingData)
                    {
                        HomeLocalForeignerCurrencyType setting = new HomeLocalForeignerCurrencyType();
                        item.IsActive = false;
                        setting = item;
                        oldSettingDataToUpdateFalse.Add(setting);
                    }

                    _context.HomeLocalForeignerCurrencyTypes.UpdateRange(oldSettingDataToUpdateFalse);
                    await _context.SaveChangesAsync();

                    TempData["SuccessedAlertMessage"] = "Save Successful";

                    return RedirectToAction(nameof(HomeLocalForeignerCurrencyType));
                }
                
                
            }
            else
            {
                   return View("Error");
            }
        }
        [HttpPost]
       
        private bool HLFCurrencyTypeExists(int id)
        {
            return _context.HomeLocalForeignerCurrencyTypes.Any(e => e.HomeLocalForeignerCurrencyTypeId == id);
        }
    }
}
