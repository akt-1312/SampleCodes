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
using HMS.Models.ViewModels.Administration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using NPOI.SS.UserModel;
using System.IO;
using System.Text;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using HMS.Utility;

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class CountryController : Controller
    {
        private readonly IAuthorizationService authorizationService;

        private readonly IWebHostEnvironment _hostingEnvironment;
        ISheet sheet;
        DataRow dataRow;
        private readonly ApplicationDbContext _context;
        public CountriesViewModel CountryViewModel { get; set; }

        public CountryController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService)
        {
            this.authorizationService = authorizationService;
            _context = context;
            _hostingEnvironment = webHostEnvironment;
            CountryViewModel = new CountriesViewModel()
            {
                Country = new Country(),
                CountryList = new List<Country>(),
            };

        }

       
       [Authorize(Policy = "CountryView")]

       [HttpGet]
        public IActionResult Country()
        {

           

            ViewBag.SuccessedAlertMessage = TempData["SuccessedAlertMessage"];
            CountriesViewModel model = new CountriesViewModel();
            var totalCountry = _context.Countries.OrderBy(a => a.Cty_name).ToList();
            model.CountryList = totalCountry;
            return View(model);


        }



        [Authorize(Policy = "CountryView")]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Country(CountriesViewModel model,string btnSubmit,int toDeleteId)
        {
            DateTime CreatedDate = DateTime.UtcNow.AddMinutes(390);
            DateTime UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            var totalCty = await _context.Countries.OrderBy(a => a.Cty_name).ToListAsync();
            model.CountryList = totalCty;
            Country country = new Country();
            btnSubmit = btnSubmit.ToLower().Trim();
            if (btnSubmit == "delete")
            {
                if (!((await authorizationService.AuthorizeAsync(User, "CountryDelete")).Succeeded))
                {
                    return RedirectToAction("AccessDenied", "UserAccount", new { area = "Administration" });
                }

                ModelState.Clear();
                var delete = await _context.Countries.FindAsync(toDeleteId);
                try
                {
                    if (delete != null)
                    {
                        _context.Countries.Remove(delete);
                        await _context.SaveChangesAsync();
                        TempData["SuccessedAlertMessage"] = "Delete Successful";
                        return RedirectToAction(nameof(Country));
                    }
                    else
                    {
                        return View("Error");
                    }
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Country Name ({delete.Cty_name}) can't delete!.These table are use in another Table");
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
                            if (model.Cty_id == null || model.Cty_id.Value == 0)
                            {
                                return View("Error");
                            }
                            else
                            {
                                if (await CountryExist(countryName: model.Cty_name, countryId: model.Cty_id))
                                {
                                    ModelState.AddModelError("Cty_name", "Country Name  already exist");
                                    return View(model);

                                }
                                else
                                {
                                    if (!(await authorizationService.AuthorizeAsync(User, "CountryUpdate")).Succeeded)
                                    {
                                        return RedirectToAction("AccessDenied", "UserAccount", "Administration");

                                    }

                                    var updated = await _context.Countries.FindAsync(model.Cty_id.Value);
                                    if (updated != null)
                                    {
                                        updated.Cty_name = model.Cty_name;
                                        updated.UpdatedDate = UpdatedDate;
                                        _context.Countries.Update(updated);
                                        await _context.SaveChangesAsync();
                                        TempData["SuccessedAlertMessage"] = "Update Successful";
                                        return RedirectToAction(nameof(Country));

                                    }
                                    else
                                    {
                                        return View("Error");
                                    }
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
                        if (await CountryExist(countryName: model.Cty_name, countryId: null))
                        {
                            ModelState.AddModelError("Cty_name", "Contry Name already exist");
                            return View(model);
                        }
                        else
                        {
                            if (!(await authorizationService.AuthorizeAsync(User, "CountryCreate")).Succeeded)
                            {
                                return RedirectToAction("AccessDenied", "UserAccount", "Administration");
                            }
                            country.Cty_name = model.Cty_name;
                            country.CreatedDate = CreatedDate;
                            await _context.Countries.AddAsync(country);
                            await _context.SaveChangesAsync();
                            TempData["SuccessedAlertMessage"] = "Save Successful";

                            return RedirectToAction(nameof(Country));
                        }

                    }
                }


                else
                {
                    ModelState.AddModelError("Cty_name", "CountryName can't create!");

                    return View(model);
                }
            }

          
        }




        private async Task<bool> CountryExist(string countryName, int? countryId)
        {
            var cty = new Country();
            if (countryId == null || countryId.Value == 0)
            {
                cty = await _context.Countries
                    .Where(x => x.Cty_name == countryName).FirstOrDefaultAsync();
            }
            else
            {
                cty = await _context.Countries
                    .Where(x => x.Cty_id != countryId && x.Cty_name == countryName).FirstOrDefaultAsync();
            }
            if (cty == null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }


        public JsonResult Add(string name)
        {
            Country countrys = new Country();
            countrys.Cty_name = name;
            countrys.CreatedDate = DateTime.UtcNow.AddMinutes(390);
            countrys.UpdatedDate = DateTime.UtcNow.AddMinutes(390);
            _context.Countries.Add(countrys);
            _context.SaveChanges();

            var CtyList = _context.Countries.ToList();
            return Json(CtyList.LastOrDefault());
        }

        //public ActionResult Country(string name)
        //{
        //    return View();
        //}

        //[HttpPost]
        //public ActionResult Country(IFormFile postedFile)
        //{
        //    if (postedFile != null)
        //    {
        //        try
        //        {
        //            string fileExtension = Path.GetExtension(postedFile.FileName);

        //            //Validate uploaded file and return error.
        //            if (fileExtension != ".xls" && fileExtension != ".xlsx")
        //            {
        //                ViewBag.Message = "Please select the excel file with .xls or .xlsx extension";
        //                return View();
        //            }

        //            //Start Excel file Save in root folder
        //            string webRootPath = _hostingEnvironment.WebRootPath;
        //            var folderPath = Path.Combine(webRootPath, SD.ExcelFolder);

        //            //Check Directory exists else create one
        //            if (!Directory.Exists(folderPath))
        //            {
        //                Directory.CreateDirectory(folderPath);
        //            }

        //            //Save file to folder
        //            var filePath = folderPath + Path.GetFileName(postedFile.FileName);

        //            DataTable dt = new DataTable();
        //            using (var filestream = new FileStream(filePath, FileMode.Create))
        //            {
        //                postedFile.CopyTo(filestream);
        //                filestream.Position = 0;
        //                switch (fileExtension)
        //                {
        //                    //If uploaded file is Excel 1997-2007.
        //                    case ".xls":
        //                        HSSFWorkbook hssfwb = new HSSFWorkbook(filestream);
        //                        sheet = hssfwb.GetSheetAt(0);
        //                        break;
        //                    //If uploaded file is Excel 2007 and above
        //                    case ".xlsx":
        //                        XSSFWorkbook xssfwb = new XSSFWorkbook(filestream);
        //                        sheet = xssfwb.GetSheetAt(0);
        //                        break;
        //                }


        //                IRow headerRow = sheet.GetRow(0); //Get Header Row
        //                int cellCount = headerRow.LastCellNum;

        //                for (int j = 0; j < cellCount; j++)
        //                {
        //                    NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);
        //                    if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
        //                    dt.Columns.Add(cell.ToString());
        //                }

        //                for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
        //                {
        //                    IRow row = sheet.GetRow(i);
        //                    IRow firstRow = sheet.GetRow(-i);
        //                    if (row == null) continue;
        //                    if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;

        //                    dataRow = dt.NewRow();
        //                    for (int j = row.FirstCellNum; j < cellCount; j++)
        //                    {
        //                        if (row.GetCell(j) != null)
        //                            dataRow[dt.Columns[j].ColumnName] = row.GetCell(j).ToString();

        //                    }
        //                    dt.Rows.Add(dataRow);
        //                }

        //            }
        //            //end Excel file Save in root folder

        //            foreach (DataRow row in dt.Rows)
        //            {
        //                _context.Countries.Add(GetEmployeeFromExcelRow(row));
        //            }
        //            _context.SaveChanges();
        //            ViewBag.Message = "Data Imported Successfully.";
        //        }
        //        catch (Exception ex)
        //        {
        //            ViewBag.Message = ex.Message;
        //        }
        //    }
        //    else
        //    {
        //        ViewBag.Message = "Please select the file first to upload.";
        //    }
        //    return View();
        //}

        //Convert each datarow into employee object
        //private Country GetEmployeeFromExcelRow(DataRow row)
        //{
        //    return new Country
        //    {
        //        Cty_name = row[0].ToString(),
        //        CreatedDate = DateTime.Parse(row[1].ToString()),
        //        UpdatedDate = DateTime.Parse(row[2].ToString()),

        //    };
        //}

    }
}
