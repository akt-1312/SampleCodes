using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.GlobalDependencyData;
using HMS.Models.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.CommonAjaxCall.Controllers
{
    [Authorize]
    [Area("CommonAjaxCall")]
    public class DrDeptUnitPartialAjaxCallController : Controller
    {
        private readonly ApplicationDbContext db;

        public DrDeptUnitPartialAjaxCallController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public JsonResult GetUnits(int DepartmentId)
        {
            List<Unit> lstUnit = new List<Unit>();
            lstUnit = TheWholeGlobalSettingData.Units.Where(x => x.IsActiveUnit && x.DepartmentId == DepartmentId).OrderBy(x => x.UnitName).ToList();
            //await (from data in db.Units where data.DepartmentId == DepartmentId && data.IsActiveUnit orderby data.UnitName select data).ToListAsync();

            return Json(new SelectList(lstUnit, "UnitId", "UnitName"));
        }

        public async Task<JsonResult> GetDoctors(int DepartmentId, int UnitId)
        {
            List<DrInfo> lstDoctors = new List<DrInfo>();

            var drDeptUnit = await db.UnitsOfDoctors.Include(x => x.DoctorInfo).Include(dp => dp.Unit).ThenInclude(u => u.Department)
                .Where(a => a.UnitId == UnitId && a.Unit.DepartmentId == DepartmentId && a.DoctorInfo.IsActiveDoctor && a.Unit.IsActiveUnit && a.Unit.Department.IsActiveDepartment)
                .OrderBy(o => o.Unit.Department.DepartmentName).ToListAsync();


            foreach (var item in drDeptUnit)
            {
                lstDoctors.Add(new DrInfo
                {
                    DoctorId = item.DoctorId,
                    DoctorName = item.DoctorInfo.DoctorName,
                    DepartmentId = item.Unit.DepartmentId,
                    DepartmentName = item.Unit.Department.DepartmentName,
                    UnitId = item.UnitId,
                    UnitName = item.Unit.UnitName
                });
            }

            return Json(lstDoctors);
        }

        private class DrInfo
        {
            public string DoctorName { get; set; }
            public int DoctorId { get; set; }
            public string DepartmentName { get; set; }
            public int DepartmentId { get; set; }
            public string UnitName { get; set; }
            public int UnitId { get; set; }
        }

        //public JsonResult GetAllDoctors()
        //{
        //    List<DrInfo> lstDDU = new List<DrInfo>();
        //    List<DoctorInfoUnit> drDeptUnit = (from data in db.DoctorInfoUnits.Include(a => a.DoctorInfo).Include(d => d.Department)
        //                     .Include(u => u.Unit)
        //                                       orderby data.Department.Name
        //                                       select data).ToList();

        //    foreach (var item in drDeptUnit)
        //    {
        //        lstDDU.Add(new DrInfo
        //        {
        //            DoctorName = item.DoctorInfo.Name,
        //            DoctorId = item.DoctorId,
        //            DepartmentName = item.Department.Name,
        //            DepartmentId = item.DepartmentId,
        //            UnitName = item.Unit.Name,
        //            UnitId = item.UnitId
        //        });
        //    }

        //    //var json = JsonConvert.SerializeObject(drDeptUnit);

        //    return Json(lstDDU);
        //}

        public JsonResult GetAllDoctors(string GroupByMethod)
        {
            List<DrInfo> lstDDU = new List<DrInfo>();
            List<UnitsOfDoctor> drDeptUnit = new List<UnitsOfDoctor>();
            if (GroupByMethod == "GroupByName")
            {
                drDeptUnit = (from data in db.UnitsOfDoctors.Include(x => x.DoctorInfo)
                             .Include(u => u.Unit).ThenInclude(d => d.Department).Where(x => x.DoctorInfo.IsActiveDoctor && x.Unit.IsActiveUnit && x.Unit.Department.IsActiveDepartment)
                              orderby data.DoctorInfo.DoctorName, data.Unit.Department.DepartmentName, data.Unit.UnitName
                              select data).ToList();
            }
            else
            {
                drDeptUnit = (from data in db.UnitsOfDoctors.Include(a => a.DoctorInfo)
                             .Include(u => u.Unit).ThenInclude(d => d.Department).Where(x => x.DoctorInfo.IsActiveDoctor && x.Unit.IsActiveUnit && x.Unit.Department.IsActiveDepartment)
                              orderby data.Unit.Department.DepartmentName, data.DoctorInfo.DoctorName, data.Unit.UnitName
                              select data).ToList();
            }


            foreach (var item in drDeptUnit)
            {
                lstDDU.Add(new DrInfo
                {
                    DoctorName = item.DoctorInfo.DoctorName,
                    DoctorId = item.DoctorId,
                    DepartmentName = item.Unit.Department.DepartmentName,
                    DepartmentId = item.Unit.DepartmentId,
                    UnitName = item.Unit.UnitName,
                    UnitId = item.UnitId
                });
            }

            //var json = JsonConvert.SerializeObject(drDeptUnit);

            return Json(lstDDU);
        }



        public JsonResult GetTodayDoctors()
        {
            var todayDate = DateTime.UtcNow.AddMinutes(390).DayOfWeek;
            string today = todayDate.ToString();
            List<DrInfo> lstDDU = new List<DrInfo>();
            List<DoctorDuty> todayDrs = (from data in db.DoctorDuties.Where(dr => dr.DutyDay.Trim().ToLower() == today.ToString().Trim().ToLower() && dr.DoctorInfo.IsActiveDoctor && dr.Unit.IsActiveUnit && dr.Department.IsActiveDepartment)
                                         .Include(dr => dr.DoctorInfo).Include(dp => dp.Department).Include(ut => ut.Unit)
                                         orderby data.Department.DepartmentName
                                         select data).ToList();


            foreach (var item in todayDrs)
            {
                lstDDU.Add(new DrInfo
                {
                    DoctorName = item.DoctorInfo.DoctorName,
                    DoctorId = item.DoctorId,
                    DepartmentName = item.Department.DepartmentName,
                    DepartmentId = item.DepartmentId,
                    UnitName = item.Unit.UnitName,
                    UnitId = item.UnitId
                });
            }

            //var json = JsonConvert.SerializeObject(drDeptUnit);
            lstDDU = lstDDU.OrderBy(x => x.DoctorName).ThenBy(x => x.DepartmentName).ThenBy(x => x.UnitName).ToList();
            return Json(lstDDU);
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using HMS.Data;
//using HMS.GlobalDependencyData;
//using HMS.Models.Administration;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;

//namespace HMS.Areas.CommonAjaxCall.Controllers
//{
//    [Authorize]
//    [Area("CommonAjaxCall")]
//    public class DrDeptUnitPartialAjaxCallController : Controller
//    {
//        private readonly ApplicationDbContext db;

//        public DrDeptUnitPartialAjaxCallController(ApplicationDbContext db)
//        {
//            this.db = db;
//        }

//        public JsonResult GetUnits(int DepartmentId)
//        {
//            List<Unit> lstUnit = new List<Unit>();
//            lstUnit = TheWholeGlobalSettingData.Units.Where(x => x.IsActiveUnit && x.DepartmentId == DepartmentId).OrderBy(x => x.UnitName).ToList();
//                //await (from data in db.Units where data.DepartmentId == DepartmentId && data.IsActiveUnit orderby data.UnitName select data).ToListAsync();

//            return Json(new SelectList(lstUnit, "UnitId", "UnitName"));
//        }

//        public async Task<JsonResult> GetDoctors(int DepartmentId, int UnitId)
//        {
//            List<DrInfo> lstDoctors = new List<DrInfo>();

//            var drDeptUnit = await db.UnitsOfDoctors.Include(dp => dp.Unit).ThenInclude(u => u.Department)
//                .Where(a => a.UnitId == UnitId && a.Unit.DepartmentId == DepartmentId)
//                .Include(dr => dr.DoctorInfo).OrderBy(o => o.Unit.Department.DepartmentName).ToListAsync();


//            foreach (var item in drDeptUnit)
//            {
//                lstDoctors.Add(new DrInfo
//                {
//                    DoctorId = item.DoctorId,
//                    DoctorName = item.DoctorInfo.DoctorName,
//                    DepartmentId = item.Unit.DepartmentId,
//                    DepartmentName = item.Unit.Department.DepartmentName,
//                    UnitId = item.UnitId,
//                    UnitName = item.Unit.UnitName
//                });
//            }

//            return Json(lstDoctors);
//        }

//        private class DrInfo
//        {
//            public string DoctorName { get; set; }
//            public int DoctorId { get; set; }
//            public string DepartmentName { get; set; }
//            public int DepartmentId { get; set; }
//            public string UnitName { get; set; }
//            public int UnitId { get; set; }
//        }

//        //public JsonResult GetAllDoctors()
//        //{
//        //    List<DrInfo> lstDDU = new List<DrInfo>();
//        //    List<DoctorInfoUnit> drDeptUnit = (from data in db.DoctorInfoUnits.Include(a => a.DoctorInfo).Include(d => d.Department)
//        //                     .Include(u => u.Unit)
//        //                                       orderby data.Department.Name
//        //                                       select data).ToList();

//        //    foreach (var item in drDeptUnit)
//        //    {
//        //        lstDDU.Add(new DrInfo
//        //        {
//        //            DoctorName = item.DoctorInfo.Name,
//        //            DoctorId = item.DoctorId,
//        //            DepartmentName = item.Department.Name,
//        //            DepartmentId = item.DepartmentId,
//        //            UnitName = item.Unit.Name,
//        //            UnitId = item.UnitId
//        //        });
//        //    }

//        //    //var json = JsonConvert.SerializeObject(drDeptUnit);

//        //    return Json(lstDDU);
//        //}

//        public JsonResult GetAllDoctors(string GroupByMethod)
//        {
//            List<DrInfo> lstDDU = new List<DrInfo>();
//            List<UnitsOfDoctor> drDeptUnit = new List<UnitsOfDoctor>();
//            if (GroupByMethod == "GroupByName")
//            {
//                drDeptUnit = (from data in db.UnitsOfDoctors.Include(a => a.DoctorInfo)
//                             .Include(u => u.Unit).ThenInclude(d => d.Department)
//                              orderby data.DoctorInfo.DoctorName, data.Unit.Department.DepartmentName, data.Unit.UnitName
//                              select data).ToList();
//            }
//            else
//            {
//                drDeptUnit = (from data in db.UnitsOfDoctors.Include(a => a.DoctorInfo)
//                             .Include(u => u.Unit).ThenInclude(d => d.Department)
//                              orderby data.Unit.Department.DepartmentName, data.DoctorInfo.DoctorName, data.Unit.UnitName
//                              select data).ToList();
//            }


//            foreach (var item in drDeptUnit)
//            {
//                lstDDU.Add(new DrInfo
//                {
//                    DoctorName = item.DoctorInfo.DoctorName,
//                    DoctorId = item.DoctorId,
//                    DepartmentName = item.Unit.Department.DepartmentName,
//                    DepartmentId = item.Unit.DepartmentId,
//                    UnitName = item.Unit.UnitName,
//                    UnitId = item.UnitId
//                });
//            }

//            //var json = JsonConvert.SerializeObject(drDeptUnit);

//            return Json(lstDDU);
//        }



//        public JsonResult GetTodayDoctors()
//        {
//            var todayDate = DateTime.UtcNow.AddMinutes(390).DayOfWeek;
//            string today = todayDate.ToString();
//            List<DrInfo> lstDDU = new List<DrInfo>();
//            List<DoctorDuty> todayDrs = (from data in db.DoctorDuties.Where(dr => dr.DutyDay.Trim().ToLower() == today.ToString().Trim().ToLower())
//                                         .Include(dr => dr.DoctorInfo).Include(dp => dp.Department).Include(ut => ut.Unit)
//                                         orderby data.Department.DepartmentName
//                                         select data).ToList();


//            foreach (var item in todayDrs)
//            {
//                lstDDU.Add(new DrInfo
//                {
//                    DoctorName = item.DoctorInfo.DoctorName,
//                    DoctorId = item.DoctorId,
//                    DepartmentName = item.Department.DepartmentName,
//                    DepartmentId = item.DepartmentId,
//                    UnitName = item.Unit.UnitName,
//                    UnitId = item.UnitId
//                });
//            }

//            //var json = JsonConvert.SerializeObject(drDeptUnit);

//            return Json(lstDDU);
//        }
//    }
//}