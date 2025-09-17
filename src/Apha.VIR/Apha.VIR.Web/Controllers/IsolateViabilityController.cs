using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateViabilityController : Controller
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly ILookupService _lookupService;

        public IsolateViabilityController(IIsolateViabilityService isolateViabilityService,
            ILookupService lookupService,
            IMapper mapper)
        {
            _isolateViabilityService = isolateViabilityService;
            _lookupService = lookupService;
            _mapper = mapper;
        }

        [Authorize(Roles = AppRoleConstant.Administrator)]
        public IActionResult History(string AVNumber, Guid Isolate)
        {
            if (string.IsNullOrWhiteSpace(AVNumber) || Isolate == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var result = _isolateViabilityService.GetViabilityHistoryAsync(AVNumber, Isolate).Result;

            var viabilityHistories = _mapper.Map<IEnumerable<IsolateViabilityModel>>(result);

            var viewModel = new IsolateViabilityHistoryViewModel
            {
                IsolateId = Isolate,
                Nomenclature = viabilityHistories.FirstOrDefault()?.Nomenclature!,
                ViabilityHistoryList = viabilityHistories
            };

            return View("ViabilityHistory", viewModel);
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.Administrator)]
        public async Task<IActionResult> Edit(string AVNumber, Guid Isolate, Guid IsolateViabilityId)
        {
            if (string.IsNullOrWhiteSpace(AVNumber) || Isolate == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var result = await _isolateViabilityService.
                GetViabilityHistoryAsync(AVNumber, Isolate);

            var viability = _mapper.Map<IEnumerable<IsolateViabilityModel>>
                (result.Where(x => x.IsolateViabilityId == IsolateViabilityId));

            var vaibilities = await _lookupService.GetAllViabilityAsync();
            var staffs = await _lookupService.GetAllStaffAsync();

            var viewModel = new IsolateViabilityViewModel
            {
                IsolateViability = _mapper.Map<IsolateViabilityModel>(viability.FirstOrDefault()),
                ViabilityList = vaibilities.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
                CheckedByList = staffs.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList(),
            };

            return View("Edit", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IsolateViabilityViewModel model)
        {
            string userid = "TestUser";

            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.Administrator))
            {
                throw new UnauthorizedAccessException("Not authorised to modify viability.");
            }
           
            await ValidateModel(model.IsolateViability.IsolateViabilityId,
                Convert.ToBase64String(model.IsolateViability.LastModified), 
                model.IsolateViability.IsolateViabilityIsolateId);

            if (!ModelState.IsValid)
            {
                var vaibilities = await _lookupService.GetAllViabilityAsync();
                var staffs = await _lookupService.GetAllStaffAsync();
                model.ViabilityList = vaibilities.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
                model.CheckedByList = staffs.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();

                return View("Edit", model);
            }

            var dto = _mapper.Map<IsolateViabilityInfoDto>(model.IsolateViability);

            await _isolateViabilityService.UpdateIsolateViabilityAsync(dto, userid);

            return RedirectToAction(nameof(History), new { AVNumber = model.IsolateViability.AVNumber, Isolate = model.IsolateViability.IsolateViabilityIsolateId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid isolateViabilityId, string lastModified, string avNUmber, Guid isolateId)
        {
            string userid = "TestUser";

            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.Administrator))
            {
                throw new UnauthorizedAccessException("Not authorised to delete viability.");
            }
           
            await ValidateModel(isolateViabilityId, lastModified, isolateId);

            if (!ModelState.IsValid)
            {
                var result = _isolateViabilityService.GetViabilityHistoryAsync(avNUmber, isolateId).Result;

                var viabilityHistories = _mapper.Map<IEnumerable<IsolateViabilityModel>>(result);

                var viewModel = new IsolateViabilityHistoryViewModel
                {
                    IsolateId = isolateId,
                    Nomenclature = viabilityHistories.FirstOrDefault()?.Nomenclature!,
                    ViabilityHistoryList = viabilityHistories
                };

                return View("ViabilityHistory", viewModel);
            }
            
            byte[] lastModifiedbyte = Convert.FromBase64String(lastModified);

            await _isolateViabilityService.DeleteIsolateViabilityAsync(isolateViabilityId, lastModifiedbyte, userid);

            return RedirectToAction(nameof(History), new { AVNumber = avNUmber, Isolate = isolateId });
        }

        private async Task ValidateModel(Guid isolateViabilityId, string lastModified, Guid isolateId)
        {
            if (isolateViabilityId == Guid.Empty)
            {
                ModelState.AddModelError(string.Empty, "ID is required.");
            }
            if (string.IsNullOrWhiteSpace(lastModified))
            {
                ModelState.AddModelError(string.Empty, "Last Modified cannot be empty.");
            }

            var isolateViabilities = await _isolateViabilityService.GetViabilityByIsolateIdAsync(isolateId);
            int count = isolateViabilities.Count(u => u.IsolateViabilityId == isolateViabilityId);

            if (count == 0)
            {
                ModelState.AddModelError(string.Empty, "The item does not exist");
            }
        }
    }
}
