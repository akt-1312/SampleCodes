using HMS.Data;
using HMS.Models.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HMS.GlobalDependencyData
{
    public class TheWholeGlobalSettingData
    {
        private readonly ApplicationDbContext db;

        public TheWholeGlobalSettingData(ApplicationDbContext db)
        {
            this.db = db;
        }
        #region OldProperties
        //public static int HomeCurrencyId { get; set; }
        //public static string HomeCurrencyCode { get; set; }
        //public static int LocalCurrencyId { get; set; }
        //public static string LocalCurrencyCode { get; set; }
        //public static int ForeignerCurrencyId { get; set; }
        //public static string ForeignerCurrencyCode { get; set; }
        //public static int LocalNationalityId { get; set; }
        //public static string LocalNationalityName { get; set; }
        //public static float LocalMultiplyRate { get; set; }
        //public static float ForeignerMultiplyRate { get; set; }

        //public static List<Prefix> Prefixes { get; set; }
        //public static List<Gender> Genders { get; set; }
        //public static List<IdentityType> IdentityTypes { get; set; }
        //public static List<Nationality> Nationalities { get; set; }
        //public static List<Occupation> Occupations { get; set; }
        //public static List<Relationship> Relationships { get; set; }
        //public static List<MaritalStatus> MaritalStatuses { get; set; }
        //public static List<ReferalHospital> ReferalHospitals { get; set; }
        //public static List<Country> Countries { get; set; }
        //public static List<State> States { get; set; }
        //public static List<Township> Townships { get; set; }
        //public static List<Department> Departments { get; set; }
        //public static List<Unit> Units { get; set; }

        //public static List<AllergicType> AllergicTypes { get; set; }
        //public static List<OnSet> OnSets { get; set; }
        //public static List<OnSetType> OnSetTypes { get; set; }
        //public static List<Reaction> Reactions { get; set; }
        #endregion

        public static int HomeCurrencyId { get; private set; }
        public static string HomeCurrencyCode { get; private set; }
        public static int LocalCurrencyId { get; private set; }
        public static string LocalCurrencyCode { get; private set; }
        public static int ForeignerCurrencyId { get; private set; }
        public static string ForeignerCurrencyCode { get; private set; }
        public static int LocalNationalityId { get; private set; }
        public static string LocalNationalityName { get; private set; }
        public static float LocalMultiplyRate { get; private set; }
        public static float ForeignerMultiplyRate { get; private set; }

        public static List<Prefix> Prefixes { get; private set; }
        public static List<Gender> Genders { get; private set; }
        public static List<IdentityType> IdentityTypes { get; private set; }
        public static List<Nationality> Nationalities { get; private set; }
        public static List<Occupation> Occupations { get; private set; }
        public static List<Relationship> Relationships { get; private set; }
        public static List<MaritalStatus> MaritalStatuses { get; private set; }
        public static List<ReferalHospital> ReferalHospitals { get; private set; }
        public static List<Country> Countries { get; private set; }
        public static List<State> States { get; private set; }
        public static List<Township> Townships { get; private set; }
        public static List<Department> Departments { get; private set; }
        public static List<Unit> Units { get; private set; }

        public static List<AllergicType> AllergicTypes { get; private set; }
        public static List<OnSet> OnSets { get; private set; }
        public static List<OnSetType> OnSetTypes { get; private set; }
        public static List<Reaction> Reactions { get; private set; }

        public async Task AssignGlobalSettingData()
        {
            var settingData = await db.HomeLocalForeignerCurrencyTypes.Where(x => x.IsActive).Include(x => x.HomeCurrency).Include(x => x.LocalCurrency)
                .Include(x => x.ForeignerCurrency).Include(x => x.Nationality).FirstOrDefaultAsync();

            if (settingData != null)
            {
                HomeCurrencyId = settingData.HomeCurrencyTypeId;
                HomeCurrencyCode = settingData.HomeCurrency.CurrencyCode;
                LocalCurrencyId = settingData.LocalCurrencyTypeId;
                LocalCurrencyCode = settingData.LocalCurrency.CurrencyCode;
                ForeignerCurrencyId = settingData.ForeignerCurrencyTypeId;
                ForeignerCurrencyCode = settingData.ForeignerCurrency.CurrencyCode;
                LocalNationalityId = settingData.NationalityId;
                LocalNationalityName = settingData.Nationality.NationalityName;
                LocalMultiplyRate = settingData.MultiplyHomeToHome;
                ForeignerMultiplyRate = settingData.MultiplyHomeToForeigner;
            }


            Prefixes = await db.Prefixes.OrderBy(x => x.PrefixName).ToListAsync();
            Genders = await db.Genders.OrderBy(x => x.GenderName).ToListAsync();
            IdentityTypes = await db.IdentityTypes.OrderBy(x => x.IdentityTypeName).ToListAsync();
            Nationalities = await db.Nationalities.OrderBy(x => x.NationalityName).ToListAsync();
            Occupations = await db.Occupations.OrderBy(x => x.OccupationName).ToListAsync();
            Relationships = await db.Relationships.OrderBy(x => x.RelationshipName).ToListAsync();
            MaritalStatuses = await db.MaritalStatuses.OrderBy(x => x.Marital_Status).ToListAsync();
            ReferalHospitals = await db.ReferalHospitals.OrderBy(x => x.HospitalName).ToListAsync();
            Countries = await db.Countries.OrderBy(x => x.Cty_name).ToListAsync();
            States = await db.States.Include(x => x.Country).OrderBy(x => x.State_name).ThenBy(x => x.Country.Cty_name).ToListAsync();
            Townships = await db.Townships.Include(x => x.State).ThenInclude(x => x.Country).OrderBy(x => x.Tsp_name).ThenBy(x => x.State.State_name).ThenBy(x => x.State.Country.Cty_name).ToListAsync();
            Departments = await db.Departments.OrderBy(x => x.DepartmentName).ToListAsync();
            Units = await db.Units.Include(x => x.Department).OrderBy(x => x.UnitName).ThenBy(x => x.Department.DepartmentName).ToListAsync();

            AllergicTypes = await db.AllergicTypes.OrderBy(x => x.AllergicTypeName).ToListAsync();
            OnSets = await db.OnSets.OrderBy(x => x.OnSetName).ToListAsync();
            OnSetTypes = await db.OnSetTypes.OrderBy(x => x.OnSetTypeName).ToListAsync();
            Reactions = await db.Reactions.OrderBy(x => x.ReactionName).ToListAsync();

        }
    }
}
