using System.Reflection.PortableExecutable;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Apha.VIR.Web.Controllers
{
    [Route("Relocation")]
    public class IsolateAndTrayRelocationController : Controller
    {
        private readonly ILookupService _lookupService;
        private readonly IIsolateRelocateService _isolateRelocateService;
        private readonly CacheService _cacheService;
        private readonly IMapper _mapper;

        public IsolateAndTrayRelocationController(IIsolateRelocateService isolateRelocateService,
            ILookupService lookupService,
            CacheService cacheService,
            IMapper mapper)
        {
            _isolateRelocateService = isolateRelocateService;
            _lookupService = lookupService;
            _cacheService = cacheService;   
            _mapper = mapper;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("Isolate")]
        [Route("Tray")]
        public async Task<IActionResult> Relocation()
        {
            var model = new IsolateRelocationViewModel();
            await LoadIsolateAndTrayData(model);

            var jsonData = _cacheService.GetSessionValue("isolateRelocateSessionModel");
            if (!string.IsNullOrEmpty(jsonData))
            {
                var data = JsonConvert.DeserializeObject<IsolateRelocateViewModel>(jsonData);
                _cacheService.RemoveSessionValue("isolateRelocateSessionModel");
                if (data != null && data.Freezer != null)
                    model.SelectedFreezer = data.Freezer;
                if (data != null && data.Tray != null)
                    model.SelectedTray = data.Tray;                
                var searchModel = await _isolateRelocateService.GetIsolatesByCriteria(model.MinAVNumber!,
                    model.MaxAVNumber!, model.SelectedFreezer ?? Guid.Empty, model.SelectedTray ?? Guid.Empty);
                model.SearchResults = _mapper.Map<List<IsolateRelocateViewModel>>(searchModel);
            }
            else
            {
                model.TraysList = new List<SelectListItem>();
                model.SearchResults = [];
            }

            var path = HttpContext?.Request.Path.Value?.ToLower();

            if (path!.Contains("/isolate"))
            {
                return View("IsolateRelocation", model);
            }
            else if (path!.Contains("/tray"))
            {
                return View("TrayRelocation", model);
            }

            return NotFound();
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] IsolateRelocationViewModel model)
        {
            ValidateIsolatedFields(model!, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<IsolateRelocateViewModel>? results;
            var data = await _isolateRelocateService.GetIsolatesByCriteria(model.MinAVNumber!,
                model.MaxAVNumber!, model.SelectedFreezer ?? Guid.Empty, model.SelectedTray ?? Guid.Empty);
            results = _mapper.Map<List<IsolateRelocateViewModel>>(data);

            return PartialView("_SearchResults", results);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IActionResult> Save(IsolateRelocationViewModel model)
        {
            ValidateIsolatedSaveFields(model, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var isolate in model.SelectedNewIsolatedList!)
            {
                await _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(new IsolateRelocateDTO
                {
                    IsolateId = isolate.IsolatedId!.Value,
                    Freezer = model.SelectedNewFreezer!.Value,
                    Tray = model.SelectedNewTray!.Value,
                    Well = isolate.Well!,
                    UserID = "Test",
                    LastModified = isolate.LastModified,
                    UpdateType = RelocationType.Isolate.ToString()
                });
            }

            return Json(new { success = true });
        }

        [Route("Edit")]
        public async Task<IActionResult> Edit(IsolateRelocateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (_cacheService.GetSessionValue("isolateRelocateSessionModel") == null)
            {
                string jsonString = JsonConvert.SerializeObject(model);
                _cacheService.SetSessionValue("isolateRelocateSessionModel", jsonString);
            }
            var data = new IsolateRelocationViewModel();
            await LoadIsolateAndTrayData(data);
            ViewBag.FreezersList = data.FreezersList;
            ViewBag.TrayList = data.TraysList;
            return View(model);
        }

        [Route("Update")]
        public async Task<IActionResult> Update(IsolateRelocateViewModel model)
        {            
            if (ModelState.IsValid)
            {
                await _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(new IsolateRelocateDTO
                {
                    IsolateId = model.IsolateId,
                    Freezer = model.Freezer,
                    Tray = model.Tray,
                    Well = model.Well,
                    UserID = "Test",
                    LastModified = model.LastModified,
                    UpdateType = RelocationType.Isolate.ToString()
                });
                return RedirectToAction("Isolate", "Relocation");
            }

            var data = new IsolateRelocationViewModel();
            await LoadIsolateAndTrayData(data);
            ViewBag.FreezersList = data.FreezersList;
            ViewBag.TrayList = data.TraysList;
            return View("Edit", model);
        }

        [Route("GetTray")]
        public async Task<IActionResult> GetTraysByFreezerId(Guid? freezerId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trays = await _lookupService.GetAllTraysByParentAsync(freezerId);
            var trayList = trays.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            return Json(trayList);
        }

        [HttpPost]
        [Route("SearchIsolates")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchIsolates(IsolateRelocationViewModel model)
        {
            if (model.SelectedFreezer == null || model.SelectedTray == null)
            {
                ModelState.AddModelError(string.Empty, "You must select both a freezer and tray.");                
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<IsolateRelocateViewModel>? results;
            var data = await _isolateRelocateService.GetIsolatesByCriteria(model.MinAVNumber!,
                model.MaxAVNumber!, model.SelectedFreezer ?? Guid.Empty, model.SelectedTray ?? Guid.Empty);
            results = _mapper.Map<List<IsolateRelocateViewModel>>(data);

            return PartialView("_SearchIsolates", results);             
        }

        [HttpPost]
        [Route("RelocateTray")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelocateTray(IsolateRelocationViewModel model)
        {
            if (model.SelectedNewFreezer == null)
            {
                ModelState.AddModelError(string.Empty, "You must select a Freezer for the Tray to be relocated into.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(new IsolateRelocateDTO
            {
                Freezer = model.SelectedNewFreezer!.Value,
                Tray = model.SelectedTray!.Value,
                UpdateType = RelocationType.Tray.ToString()
            });

            var data = await _isolateRelocateService.GetIsolatesByCriteria(model.MinAVNumber!,
                model.MaxAVNumber!, model.SelectedFreezer ?? Guid.Empty, model.SelectedTray ?? Guid.Empty);

            foreach (var isolate in data!)
            {
                await _isolateRelocateService.UpdateIsolateFreezeAndTrayAsync(new IsolateRelocateDTO
                {
                    IsolateId = isolate.IsolateId,
                    Freezer = model.SelectedNewFreezer!.Value,
                    Tray = isolate.Tray,
                    Well = isolate.Well,
                    UserID = "Test",
                    LastModified = isolate.LastModified,
                    UpdateType = RelocationType.Isolate.ToString()
                });
            }

            return Json(new { success = true });
        }


        private async Task LoadIsolateAndTrayData(IsolateRelocationViewModel model)
        {
            var freezeDto = await _lookupService.GetAllFreezerAsync();
            var trayDto = await _lookupService.GetAllTraysAsync();

            model.FreezersList = [.. freezeDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })];
            model.TraysList = [.. trayDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })];
        }

        private static void ValidateIsolatedFields(IsolateRelocationViewModel model, ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(model.MinAVNumber) && model.SelectedFreezer == null)
            {
                modelState.AddModelError(string.Empty, "You must select at least one criteria.");
            }
            else
            {

                if (!(string.IsNullOrEmpty(model.MinAVNumber) || AVNumberUtil.AVNumberIsValidPotentially(model.MinAVNumber)))
                {
                    modelState.AddModelError(string.Empty, "Minimum AV Number must be in a valid format.");
                }

                if (!(string.IsNullOrEmpty(model.MaxAVNumber) || AVNumberUtil.AVNumberIsValidPotentially(model.MaxAVNumber)))
                {
                    modelState.AddModelError(string.Empty, "Maximum AV Number must be in a valid format.");
                }
            }

            // If validationMessage is empty, no validation errors
            if (modelState.IsValid)
            {

                // If txtMaximum.Text is empty, set both min and max to the formatted minimum value
                if (string.IsNullOrEmpty(model.MaxAVNumber))
                {
                    model.MinAVNumber = AVNumberUtil.AVNumberFormatted(model.MinAVNumber!);
                    model.MaxAVNumber = AVNumberUtil.AVNumberFormatted(model.MinAVNumber!);
                }
                else
                {
                    model.MinAVNumber = AVNumberUtil.AVNumberFormatted(model.MinAVNumber!);
                    model.MaxAVNumber = AVNumberUtil.AVNumberFormatted(model.MaxAVNumber!);
                }
            }
        }

        private static void ValidateIsolatedSaveFields(IsolateRelocationViewModel model, ModelStateDictionary modelState)
        {
            if (model.SelectedNewFreezer == null || model.SelectedNewTray == null)
            {
                modelState.AddModelError(string.Empty, "You must select a freezer and tray for the isolates to be relocated into.");
            }
            else if (model.SelectedNewIsolatedList == null || model.SelectedNewIsolatedList.Count == 0)
            {
                modelState.AddModelError(string.Empty, "You must select at least one isolate.");
            }
        }

    }
}
