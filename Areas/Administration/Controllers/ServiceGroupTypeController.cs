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
    public class ServiceGroupTypeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ServiceGroupTypeViewModel serviceGroupTypeVM = new ServiceGroupTypeViewModel();
        public ServiceGroupTypeController(ApplicationDbContext context)
        {
            _context = context;
            serviceGroupTypeVM = new ServiceGroupTypeViewModel()
            {
                ServiceGroupType = new ServiceGroupType(),
                ServiceGroupTypes = new List<ServiceGroupType>(),
            };
        }

        [HttpGet]
        public async Task<IActionResult> ServiceGroupType(int ServiceGroupTypeId, string btnActionName)
        {
            var totalServiceGroupTypes = _context.ServiceGroupTypes.ToList();
            var currentServiceGroupType = totalServiceGroupTypes.Where(a => a.ServiceGroupTypeId == ServiceGroupTypeId).FirstOrDefault();
            if (ServiceGroupTypeId == 0)
            {
                ServiceGroupTypeViewModel serviceGroupTypeVM = new ServiceGroupTypeViewModel()
                {
                    ServiceGroupTypes = totalServiceGroupTypes,
                    ServiceGroupType = currentServiceGroupType,
                    BtnActionName = "Create",
                };

                return View(serviceGroupTypeVM);
            }
            else
            {
                List<ServiceGroupType> lstServiceGroupTypes = new List<ServiceGroupType>();
                lstServiceGroupTypes = await _context.ServiceGroupTypes.Where(a => a.ServiceGroupTypeId == ServiceGroupTypeId).ToListAsync();
                ServiceGroupTypeViewModel serviceGroupTypeVm = new ServiceGroupTypeViewModel()
                {
                    ServiceGroupTypes = totalServiceGroupTypes,
                    ServiceGroupType = lstServiceGroupTypes.FirstOrDefault(),
                    BtnActionName = btnActionName,
                };
                return View(serviceGroupTypeVm);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceGroupType(ServiceGroupTypeViewModel model, DateTime cDate, DateTime uDate)
        {
            if (ModelState.IsValid)
            {
                cDate = DateTime.UtcNow.AddMinutes(390);
                uDate = DateTime.UtcNow.AddMinutes(390);
                model.ServiceGroupType.CreatedDate = cDate;
                var totalServiceGroupTypes = await _context.ServiceGroupTypes.ToListAsync();
                var ServiceGroupTypeNameExist = await _context.ServiceGroupTypes.Where(x => x.ServiceGroupTypeName == model.ServiceGroupType.ServiceGroupTypeName).FirstOrDefaultAsync();
                if (ServiceGroupTypeNameExist == null && model.BtnActionName != "Delete")
                {
                    if (model.BtnActionName == "Edit")
                    {
                        try
                        {
                            var dati = _context.ServiceGroupTypes.Where(p => p.ServiceGroupTypeId == model.ServiceGroupType.ServiceGroupTypeId).Single();
                            dati.ServiceGroupTypeName = model.ServiceGroupType.ServiceGroupTypeName;
                            dati.UpdatedDate = uDate;
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(ServiceGroupType));
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ServiceGroupTypeExists(model.ServiceGroupType.ServiceGroupTypeId))
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
                        model.ServiceGroupType.CreatedDate = cDate;
                        model.ServiceGroupType.UpdatedDate = uDate;
                        await _context.ServiceGroupTypes.AddAsync(model.ServiceGroupType);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(ServiceGroupType));
                    }
                }
                else
                {
                    if (model.BtnActionName == "Delete")
                    {
                        ServiceGroupType servicegrouptype = await _context.ServiceGroupTypes
                            .FindAsync(model.ServiceGroupType.ServiceGroupTypeId);
                        _context.ServiceGroupTypes.Remove(servicegrouptype);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(ServiceGroupType));
                    }
                    else
                    {
                        try
                        {
                            ModelState.AddModelError("ServiceGroupType.ServiceGroupTypeName", "ServiceGroupType Name already exist");
                            ServiceGroupTypeViewModel serviceGroupTypeVm = new ServiceGroupTypeViewModel()
                            {
                                ServiceGroupTypes = totalServiceGroupTypes,
                            };
                            return View(serviceGroupTypeVm);
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!ServiceGroupTypeExists(model.ServiceGroupType.ServiceGroupTypeId))
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
                ModelState.AddModelError("ServiceGroupType.ServiceGroupTypeName", "ServiceGroupType Name can't create!");
                return View(model);
            }
        }
        [HttpPost]
        private bool SerivceGroupTypeNameExists(string ServiceGorupTypeName)
        {
            return _context.ServiceGroupTypes.Any(e => e.ServiceGroupTypeName == ServiceGorupTypeName);

        }
        private bool ServiceGroupTypeExists(int id)
        {
            return _context.ServiceGroupTypes.Any(e => e.ServiceGroupTypeId == id);
        }
    }
}
