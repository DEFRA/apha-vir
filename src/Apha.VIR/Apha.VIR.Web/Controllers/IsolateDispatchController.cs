using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateDispatchController : Controller
    {
        private readonly IIsolateDispatchService _isolateDispatchService;
        private readonly IMapper _mapper;
        private readonly ILookupService _lookupService;

        public IsolateDispatchController(IIsolateDispatchService isolateDispatchService, IMapper mapper, ILookupService lookupService)
        {
            _isolateDispatchService = isolateDispatchService;
            _mapper = mapper;
            _lookupService = lookupService;
        }

        [HttpGet]
        public async Task<IActionResult> History(string AVNumber, Guid IsolateId)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrWhiteSpace(AVNumber) && IsolateId == Guid.Empty)
            {
                return View();
            }

            IEnumerable<IsolateDispatchInfoDTO> isolateDispatchInfoDTOs = await _isolateDispatchService.GetDispatchesHistoryAsync(
                AVNumber,
                IsolateId
            );

            var dispatchHistoryRecords = _mapper.Map<IEnumerable<IsolateDispatchHistory>>(isolateDispatchInfoDTOs);

            IsolateDispatchHistoryViewModel? viewModel = null;
            if (dispatchHistoryRecords != null && dispatchHistoryRecords.Any())
            {
                viewModel = new IsolateDispatchHistoryViewModel
                {
                    Nomenclature = dispatchHistoryRecords.First().Nomenclature,
                    DispatchHistoryRecords = dispatchHistoryRecords
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid DispatchId, string LastModified, Guid IsolateId, string AVNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (DispatchId == Guid.Empty)
            {
                return BadRequest("Invalid Dispatch ID.");
            }
            if (string.IsNullOrWhiteSpace(LastModified))
            {
                return BadRequest("Last Modified cannot be empty.");
            }

            string UserName = "Test User";
            await _isolateDispatchService.DeleteDispatchAsync(
                DispatchId,
                Convert.FromBase64String(LastModified),
                UserName
            );

            return RedirectToAction("History", new { AVNumber = AVNumber, IsolateId = IsolateId });
        }

        public async Task<IActionResult> Edit(string AVNumber, Guid DispatchId, Guid DispatchIsolateId)
        {

            if (DispatchId == Guid.Empty)
            {
                return BadRequest("Invalid Dispatch ID.");
            }

            if (DispatchIsolateId == Guid.Empty)
            {
                return BadRequest("Invalid Dispatch Isolate ID.");
            }
            if (string.IsNullOrWhiteSpace(AVNumber))
            {
                return BadRequest("AVNumber cannot be empty.");
            }

            string recepientLocation;

            if (!ModelState.IsValid)
            {
                return View();
            }

            IsolateDispatchInfoDTO isolateDispatchInfoDTO = 
                await _isolateDispatchService.GetDispatchForIsolateAsync(
                AVNumber,
                DispatchId,
                DispatchIsolateId
            );

            recepientLocation = isolateDispatchInfoDTO.RecipientId == null ? "External" : "Internal";

            IEnumerable<LookupItemDTO> viabilityLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllViabilityAsync());
            IEnumerable<LookupItemDTO> recepientLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllWorkGroupsAsync());
            IEnumerable<LookupItemDTO> dispatchedByLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllStaffAsync());

            var model = _mapper.Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO);

            model.ViabilityList = viabilityLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == isolateDispatchInfoDTO.ViabilityId }).ToList();
            model.RecipientList = recepientLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == isolateDispatchInfoDTO.RecipientId }).ToList();
            model.DispatchedByList = dispatchedByLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            model.DispatchedById = isolateDispatchInfoDTO.DispatchedById ?? Guid.Empty;
            model.LastModified = isolateDispatchInfoDTO.LastModified ?? Array.Empty<byte>();
            model.DispatchIsolateId = DispatchIsolateId;
            model.DispatchId = DispatchId;
            model.RecipientLocation = recepientLocation;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IsolateDispatchEditViewModel model)
        {

            ModelState.Remove(nameof(model.DispatchedByList));
            ModelState.Remove(nameof(model.ViabilityList));
            ModelState.Remove(nameof(model.RecipientList));
            ModelState.Remove(nameof(model.RecipientAddress));

            var viabilityLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllViabilityAsync());
            var recepientLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllWorkGroupsAsync());
            var dispatchedByLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllStaffAsync());

            model.ViabilityList = viabilityLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.ViabilityId }).ToList();
            model.RecipientList = recepientLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.RecipientId }).ToList();
            model.DispatchedByList = dispatchedByLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.DispatchedById }).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dispatchRecord = _mapper.Map<IsolateDispatchInfoDTO>(model);

            await _isolateDispatchService.UpdateDispatchAsync(
            dispatchRecord,
            "TestUser"
            );

            return RedirectToAction("History", new { AVNumber = model.Avnumber, IsolateId =model.DispatchIsolateId });
        }


    }
}
