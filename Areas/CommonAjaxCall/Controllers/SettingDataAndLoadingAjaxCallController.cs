using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.GlobalDependencyData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.CommonAjaxCall.Controllers
{
    [AllowAnonymous]
    [Area("CommonAjaxCall")]
    public class SettingDataAndLoadingAjaxCallController : Controller
    {
        //private readonly ApplicationDbContext db;
        //private readonly TheWholeGlobalSettingData globalSettingData;

        //public SettingDataAndLoadingAjaxCallController(ApplicationDbContext db,
        //                                                TheWholeGlobalSettingData globalSettingData)
        //{
        //    this.db = db;
        //    this.globalSettingData = globalSettingData;
        //}
        //public async Task<JsonResult> GetSettingData()
        //{
        //    try
        //    {
        //        #region OldAssignGlobalData
        //        //var settingData = await db.HomeLocalForeignerCurrencyTypes.Where(x => x.IsActive).Include(x => x.HomeCurrency).Include(x => x.LocalCurrency)
        //        //    .Include(x => x.ForeignerCurrency).Include(x => x.Nationality).FirstOrDefaultAsync();

        //        //TheWholeGlobalSettingData.HomeCurrencyId = settingData.HomeCurrencyTypeId;
        //        //TheWholeGlobalSettingData.HomeCurrencyCode = settingData.HomeCurrency.CurrencyCode;
        //        //TheWholeGlobalSettingData.LocalCurrencyId = settingData.LocalCurrencyTypeId;
        //        //TheWholeGlobalSettingData.LocalCurrencyCode = settingData.LocalCurrency.CurrencyCode;
        //        //TheWholeGlobalSettingData.ForeignerCurrencyId = settingData.ForeignerCurrencyTypeId;
        //        //TheWholeGlobalSettingData.ForeignerCurrencyCode = settingData.ForeignerCurrency.CurrencyCode;
        //        //TheWholeGlobalSettingData.LocalNationalityId = settingData.NationalityId;
        //        //TheWholeGlobalSettingData.LocalNationalityName = settingData.Nationality.NationalityName;
        //        //TheWholeGlobalSettingData.LocalMultiplyRate = settingData.MultiplyHomeToHome;
        //        //TheWholeGlobalSettingData.ForeignerMultiplyRate = settingData.MultiplyHomeToForeigner;

        //        //TheWholeGlobalSettingData.Prefixes = await db.Prefixes.ToListAsync();
        //        //TheWholeGlobalSettingData.Genders = await db.Genders.ToListAsync();
        //        //TheWholeGlobalSettingData.IdentityTypes = await db.IdentityTypes.ToListAsync();
        //        //TheWholeGlobalSettingData.Nationalities = await db.Nationalities.ToListAsync();
        //        //TheWholeGlobalSettingData.Occupations = await db.Occupations.ToListAsync();
        //        //TheWholeGlobalSettingData.Relationships = await db.Relationships.ToListAsync();
        //        //TheWholeGlobalSettingData.MaritalStatuses = await db.MaritalStatuses.ToListAsync();
        //        //TheWholeGlobalSettingData.ReferalHospitals = await db.ReferalHospitals.ToListAsync();
        //        //TheWholeGlobalSettingData.Countries = await db.Countries.ToListAsync();
        //        //TheWholeGlobalSettingData.States = await db.States.Include(x=> x.Country).ToListAsync();
        //        //TheWholeGlobalSettingData.Townships = await db.Townships.Include(x => x.State).ThenInclude(x => x.Country).ToListAsync();
        //        //TheWholeGlobalSettingData.Departments = await db.Departments.ToListAsync();
        //        //TheWholeGlobalSettingData.Units = await db.Units.Include(x => x.Department).ToListAsync();

        //        //TheWholeGlobalSettingData.AllergicTypes = await db.AllergicTypes.ToListAsync();
        //        //TheWholeGlobalSettingData.OnSets = await db.OnSets.ToListAsync();
        //        //TheWholeGlobalSettingData.OnSetTypes = await db.OnSetTypes.ToListAsync();
        //        //TheWholeGlobalSettingData.Reactions = await db.Reactions.ToListAsync();
        //        #endregion

        //        await globalSettingData.AssignGlobalSettingData();

        //        return Json(true);
        //    }
        //    catch (Exception)
        //    {
        //        return Json(false);
        //    }
        //}
    }
}