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

namespace HMS.Areas.Administration.Controllers
{
    [Area("Administration")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ServiceViewModel serviceVM { get; set; }

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
            serviceVM = new ServiceViewModel()
            {
                Service = new Service(),
                Services = new List<Service>(),
            };
        }

        [HttpGet]
        public async Task<IActionResult> Service(int ServiceId, string btnActionName)
        {
            var totalServices = _context.Services.ToList();
            var currentService = totalServices.Where(a => a.ServiceId == ServiceId).FirstOrDefault();
            ViewData["ServiceGroupTypeId"] = new SelectList(_context.ServiceGroupTypes, "ServiceGroupTypeId", "ServiceGroupTypeName");
            ViewData["CurrencyTypeId"] = new SelectList(_context.Currencies, "CurrencyId", "CurrencyCode");
            if (ServiceId == 0)
            {
                ServiceViewModel serviceVM = new ServiceViewModel()
                {
                    Services = totalServices,
                    Service = currentService,
                    BtnActionName = "Create",
                };

                return View(serviceVM);
            }
            else
            {
                List<Service> lstServices = new List<Service>();
                lstServices = await _context.Services.Where(a => a.ServiceId == ServiceId).ToListAsync();
                ServiceViewModel serviceVm = new ServiceViewModel()
                {
                    Services = totalServices,
                    Service = totalServices.FirstOrDefault(),
                    BtnActionName = btnActionName,
                };
                return View(serviceVm);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Service(ServiceViewModel model, string ServiceName,DateTime uDate, DateTime cDate)
        {
            if (ModelState.IsValid)
            {
                cDate = DateTime.UtcNow.AddMinutes(390);
                uDate = DateTime.UtcNow.AddMinutes(390);
                var totalServices = await _context.Services.ToListAsync();
                var ServiceNameExist = await _context.Services.Where(x => x.ServiceName == model.Service.ServiceName).FirstOrDefaultAsync();
                if (ServiceNameExist == null && model.BtnActionName != "Delete")
                {
                    if (model.BtnActionName == "Edit")
                    {
                        try
                        {
                            var dati = _context.Services.Where(p => p.ServiceId == model.Service.ServiceId).Single();
                            dati.ServiceId = model.Service.ServiceId;
                            dati.ServiceName = model.Service.ServiceName;
                            dati.UpdatedDate = uDate;
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Service));
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ServiceExists(model.Service.ServiceId))
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
                        model.Service.CreatedDate = cDate;
                        model.Service.UpdatedDate = uDate;
                        await _context.Services.AddAsync(model.Service);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Service));
                    }
                }
                else
                {
                    if (model.BtnActionName == "Delete")
                    {
                        Service service = await _context.Services
                            .FindAsync(model.Service.ServiceId);
                        _context.Services.Remove(service);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Service));
                    }
                    else
                    {
                        try
                        {
                            if (ServiceNameExists(ServiceName))
                            {
                                ModelState.AddModelError("Service.ServiceName", "Service Name already exist");
                                ServiceViewModel serviceVm = new ServiceViewModel()
                                {
                                    Services = totalServices,
                                };
                                return View(serviceVm);
                            }
                            return View(model);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ServiceExists(model.Service.ServiceId))
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
                ModelState.AddModelError("Service.ServiceName", "Service Name can't create!");
                return View(model);
            }
        }
        [HttpPost]
        private bool ServiceNameExists(string ServiceName)
        {
            return _context.Services.Any(e => e.ServiceName ==ServiceName);

        }
        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
