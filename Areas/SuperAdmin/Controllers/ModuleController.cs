using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMS.Data;
using Microsoft.AspNetCore.Mvc;
using HMS.Models.ViewModels.SuperAdmin;
using HMS.Models.SuperAdmin;
using Microsoft.EntityFrameworkCore;

namespace HMS.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    public class ModuleController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ModuleViewModel moduleVM { get; set; }
        public ModuleController(ApplicationDbContext context)
        {
            _context = context;
            moduleVM = new ModuleViewModel()
            {
                Module = new Module(),
                Modules = new List<Module>(),
            };
        }
        [HttpGet]
        public async Task<IActionResult> Module(int ModuleId, string btnActionName)
        {
            var totalModules =  _context.Modules.OrderBy(x=>x.ModuleName).ToList();
            var currentModule = totalModules.Where(a => a.ModuleId == ModuleId).FirstOrDefault();
            if (ModuleId == 0)
            {
                ModuleViewModel moduleVM = new ModuleViewModel()
                {
                    Modules = totalModules,
                    Module = currentModule,
                    BtnActionName = "Create",
                };

                return View(moduleVM);
            }
            else
            {
                List<Module> lstModules = new List<Module>();
                lstModules = await _context.Modules.Where(a => a.ModuleId == ModuleId).ToListAsync();
                ModuleViewModel moduleVm = new ModuleViewModel()
                {
                    Modules = lstModules,
                    Module = lstModules.FirstOrDefault(),
                    BtnActionName = btnActionName,
                };
                return View(moduleVm);
            }
            
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Module(ModuleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var totalModules = await _context.Modules.ToListAsync();
                var ModuleNameExist = await _context.Modules.Where(x => x.ModuleName == model.Module.ModuleName).FirstOrDefaultAsync();
                if (ModuleNameExist == null && model.BtnActionName != "Delete")
                {
                    if (model.BtnActionName == "Edit")
                    {
                        try
                        {
                            var dati = _context.Modules.Where(p => p.ModuleId == model.Module.ModuleId).Single();
                            dati.ModuleName = model.Module.ModuleName;
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Module));
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ModuleExists(model.Module.ModuleId))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        await _context.Modules.AddAsync(model.Module);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Module));
                    }
                }
                else
                {
                    if (model.BtnActionName == "Delete")
                    {
                        Module module = await _context.Modules
                            .FindAsync(model.Module.ModuleId);
                        _context.Modules.Remove(module);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(module));
                    }
                    else
                    {
                        try
                        {
                            ModelState.AddModelError("Module.ModuleName", "Module Name already exist");
                            return View(model);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ModuleExists(model.Module.ModuleId))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            else
            {
                await _context.Modules.AddAsync(model.Module);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Module));
            }
        }
        [HttpPost]
        private bool ModuleNameExists(string ModuleName)
        {
            return _context.Modules.Any(e => e.ModuleName == ModuleName );

        }
        private bool ModuleExists(int id)
        {
            return _context.Modules.Any(e => e.ModuleId == id);
        }
    }
}
