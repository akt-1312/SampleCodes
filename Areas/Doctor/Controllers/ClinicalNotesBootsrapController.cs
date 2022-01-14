using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using HMS.Models.ViewModels.Doctor;
using HMS.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace HMS.Areas.Doctor.Controllers
{
    [Area("Doctor")]

    public class ClinicalNotesBootsrapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment hostingEnvironment;

        [BindProperty]
        public ClinicalNotesBootstrapViewModel BSTextEditorVM { get; set; }
        public ClinicalNotesBootsrapController(ApplicationDbContext context,
                                            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            this.hostingEnvironment = hostingEnvironment;
        }
        [HttpGet]
        public ActionResult Index()
        {
            string uploadFolders = Path.Combine(hostingEnvironment.WebRootPath, SD.RemarkTextFileFolder);
            string uniqueFileName = ".txt";
            string filePath = Path.Combine(uploadFolders, uniqueFileName);
            string remarkText = System.IO.File.Exists(filePath) ? System.IO.File.ReadAllText(filePath) : "";

            ClinicalNotesBootstrapViewModel model = new ClinicalNotesBootstrapViewModel
            {
                Content = remarkText,
                //MRNO = mrno,
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(ClinicalNotesBootstrapViewModel model)
        {
            try
            {
                string uploadFolders = Path.Combine(hostingEnvironment.WebRootPath, SD.RemarkTextFileFolder);
                string uniqueFileName =".txt";
                string filePath = Path.Combine(uploadFolders, uniqueFileName);
                System.IO.File.WriteAllText(filePath, model.Content);
            }
            catch (Exception)
            {
                return View("Error");
            }
            return View(model);
        }



        private string UploadedFile(ClinicalNotesBootstrapViewModel BsteVm)
        {
            string uniqueFileName = null;
            if (BsteVm.TextFile != null)
            {
                uniqueFileName = Guid.NewGuid().ToString() + "_" + BsteVm.TextFile.FileName;
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\remarkFiles", uniqueFileName);
                using (var filestream = new FileStream(filePath, FileMode.Create))
                {
                    BsteVm.TextFile.CopyTo(filestream);
                }
                //patientRegistrationVm.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
            }
            return uniqueFileName;
        }
    }
}