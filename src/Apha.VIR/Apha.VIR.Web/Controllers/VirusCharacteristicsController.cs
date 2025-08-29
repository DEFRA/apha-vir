using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicsController : Controller
    {
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;

        public VirusCharacteristicsController(
           IVirusCharacteristicService virusCharacteristicService,
           IVirusCharacteristicListEntryService listEntryService,
           IMapper mapper)
        {
            _virusCharacteristicService = virusCharacteristicService;
            _listEntryService = listEntryService;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View("VirusCharacteristicManagement");
        }

        // GET /VirusCharacteristics/ListEntries?characteristic={guid}
        [HttpGet]
        public async Task<IActionResult> ListEntries(Guid? characteristic)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            if (characteristic == null || characteristic == Guid.Empty)
                return BadRequest("Characteristic id is required");

            // reuse existing service that returns all and pick by id
            var all = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            var characteristicDto = all.FirstOrDefault(c => c.Id == characteristic.Value);

            var name = characteristicDto?.Name ?? string.Empty;
            var listDtos = await _listEntryService.GetEntriesByCharacteristicIdAsync(characteristic.Value);

            var vm = new VirusCharacteristicListEntriesViewModel
            {
                CharacteristicId = characteristic.Value,
                CharacteristicName = name,
                Entries = _mapper.Map<List<VirusCharacteristicListEntryViewModel>>(listDtos)
            };

            return View("VirusCharacteristicListEntries", vm);
        }
        [HttpGet]
        public IActionResult Back()
        {
            // redirect to virus characteristics list page
            return RedirectToAction("Index", "VirusCharacteristics");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? characteristic, Guid? entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (entry == null || entry == Guid.Empty)
            {
                // Add mode
                var vm = new VirusCharacteristicListEntryEditViewModel
                {
                    Id = Guid.Empty,
                    VirusCharacteristicId = characteristic ?? Guid.Empty
                };
                return View("VirusCharacteristicListEntryEdit", vm);
            }
            else
            {
                // Edit mode
                var dto = await _listEntryService.GetEntryByIdAsync(entry.Value);
                if (dto == null) return NotFound();
                var vm = _mapper.Map<VirusCharacteristicListEntryEditViewModel>(dto);
                return View("VirusCharacteristicListEntryEdit", vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VirusCharacteristicListEntryEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View("VirusCharacteristicListEntryEdit", model);

            if (model.Id == Guid.Empty)
            {
                // Add
                var dto = _mapper.Map<VirusCharacteristicListEntryDTO>(model);
                await _listEntryService.AddEntryAsync(dto);
            }
            else
            {
                // Edit
                var dto = _mapper.Map<VirusCharacteristicListEntryDTO>(model);
                await _listEntryService.UpdateEntryAsync(dto);
            }

            return RedirectToAction("ListEntries", "VirusCharacteristics", new { characteristic = model.VirusCharacteristicId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, Guid characteristic, string lastModified)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lastModifiedBytes = Convert.FromBase64String(lastModified);
            await _listEntryService.DeleteEntryAsync(id, lastModifiedBytes);
            return RedirectToAction("ListEntries", "VirusCharacteristics", new { characteristic });
        }

    }
}
