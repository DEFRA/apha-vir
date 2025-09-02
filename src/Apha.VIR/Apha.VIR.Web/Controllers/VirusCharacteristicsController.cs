using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.VirusCharacteristic;
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
            return View("VirusCharacteristic");
        }
       
        [HttpGet]
        public async Task<IActionResult> ListEntries(Guid? characteristic, int pageNo = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (characteristic == null || characteristic == Guid.Empty)
                return BadRequest("Characteristic id is required");

            // reuse existing service that returns all and pick by id
            var all = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            var characteristicDto = all.FirstOrDefault(c => c.Id == characteristic.Value);

            var name = characteristicDto?.Name ?? string.Empty;
            var listDtos = await _listEntryService.GetVirusCharacteristicListEntries(characteristic.Value, pageNo, pageSize);
            var vcEntries = _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(listDtos.data);

            var vm = new VirusCharacteristicListEntriesViewModel
            {
                CharacteristicId = characteristic.Value,
                CharacteristicName = name,
                Entries = new VirusCharacteristicListEntryViewModel
                {
                    VirusCharacteristics = vcEntries.ToList(),
                    Pagination = new PaginationModel
                    {
                        PageNumber = pageNo,
                        PageSize = pageSize,
                        TotalCount = listDtos.TotalCount
                    }
                }
            };

            return View("VirusCharacteristicListEntries", vm);
        }

        [HttpGet]
        public async Task<IActionResult> BindCharacteristicEntriesGridOnPagination(Guid? characteristic, int pageNo, int pageSize)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (characteristic == null || characteristic == Guid.Empty)
                return BadRequest("Characteristic id is required");

            var listDtos = await _listEntryService.GetVirusCharacteristicListEntries(characteristic.Value, pageNo, pageSize);
            var vcEntries = _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(listDtos.data);


            var model = new VirusCharacteristicListEntryViewModel
            {
                VirusCharacteristics = vcEntries.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = listDtos.TotalCount
                }
            };

            return PartialView("_VirusCharacteristicListEntry", model);
        }

        [HttpGet]
        public IActionResult Back()
        {
            // redirect to virus characteristics list page
            return RedirectToAction("Index", "VirusCharacteristics");
        }
       

        [HttpGet]
        public IActionResult Create(Guid? characteristic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vm = new VirusCharacteristicListEntryModel
            {
                Id = Guid.Empty,
                VirusCharacteristicId = characteristic ?? Guid.Empty
            };
            return View("CreateVirusCharacteristicEntry", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VirusCharacteristicListEntryModel model)
        {
            if (!ModelState.IsValid)
                return View("CreateVirusCharacteristicEntry", model);

            var dto = _mapper.Map<VirusCharacteristicListEntryDTO>(model);
            await _listEntryService.AddEntryAsync(dto);

            return RedirectToAction("ListEntries", new { characteristic = model.VirusCharacteristicId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? characteristic, Guid? entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = (entry != null && entry != Guid.Empty)
                ? await _listEntryService.GetEntryByIdAsync(entry.Value)
                : null;

            if (dto == null) return NotFound();

            var vm = _mapper.Map<VirusCharacteristicListEntryModel>(dto);
            return View("EditVirusCharacteristicEntry", vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(VirusCharacteristicListEntryModel model)
        {
            if (!ModelState.IsValid)
                return View("EditVirusCharacteristicEntry", model);

            var dto = _mapper.Map<VirusCharacteristicListEntryDTO>(model);
            await _listEntryService.UpdateEntryAsync(dto);

            return RedirectToAction("ListEntries", new { characteristic = model.VirusCharacteristicId });
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
