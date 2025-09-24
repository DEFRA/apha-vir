using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicsListEntryController : Controller
    {
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public VirusCharacteristicsListEntryController(
           IVirusCharacteristicService virusCharacteristicService,
           IVirusCharacteristicListEntryService listEntryService,
           ICacheService cacheService,
           IMapper mapper)
        {
            _virusCharacteristicService = virusCharacteristicService;
            _listEntryService = listEntryService;
            _cacheService = cacheService;
            _mapper = mapper;
        }
   
        [HttpGet]
        public async Task<IActionResult> ListEntries(Guid? characteristicId, int pageNo = 1, int pageSize = 10)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (characteristicId == null || characteristicId == Guid.Empty)
                return BadRequest("Characteristic id is required");

            // reuse existing service that returns all and pick by id
            var all = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            var characteristicDto = all.FirstOrDefault(c => c.Id == characteristicId.Value);

            var name = characteristicDto?.Name ?? string.Empty;
            var listDtos = await _listEntryService.GetVirusCharacteristicListEntries(characteristicId.Value, pageNo, pageSize);
            var vcEntries = _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(listDtos.data);

            var vm = new VirusCharacteristicListEntriesViewModel
            {
                CharacteristicId = characteristicId.Value,
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
            _cacheService.AddOrUpdateBreadcrumb("/VirusCharacteristicsListEntry/ListEntries",
            new Dictionary<string, string> {
                { "characteristicId", characteristicId.ToString()??"" },
                { "pageNo", pageNo.ToString() },
                { "pageSize", pageSize.ToString() }
            });
            return View("VirusCharacteristicListEntries", vm);
        }

        [HttpGet]
        public async Task<IActionResult> BindCharacteristicEntriesGridOnPagination(Guid? characteristicId, int pageNo, int pageSize)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), "Account");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (characteristicId == null || characteristicId == Guid.Empty)
                return BadRequest("Characteristic id is required");

            var listDtos = await _listEntryService.GetVirusCharacteristicListEntries(characteristicId.Value, pageNo, pageSize);
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
            return RedirectToAction("List", "VirusCharacteristics");
        }


        [HttpGet]
        public IActionResult Create(Guid? characteristic)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), " Account");
            }

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
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to insert entry in VirusCharacteristicListEntry list.");
            }

            if (!ModelState.IsValid)
                return View("CreateVirusCharacteristicEntry", model);

            var dto = _mapper.Map<VirusCharacteristicListEntryDto>(model);
            await _listEntryService.AddEntryAsync(dto);

            return RedirectToAction("ListEntries", new { characteristicId = model.VirusCharacteristicId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid? characteristic, Guid? entry)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction(nameof(AccountController.AccessDenied), " Account");
            }

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
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to update entry in VirusCharacteristicListEntry list.");
            }

            if (!ModelState.IsValid)
                return View("EditVirusCharacteristicEntry", model);

            var dto = _mapper.Map<VirusCharacteristicListEntryDto>(model);
            await _listEntryService.UpdateEntryAsync(dto);

            return RedirectToAction("ListEntries", new { characteristicId = model.VirusCharacteristicId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, Guid characteristic, string lastModified)
        {
            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to delete entry in VirusCharacteristicListEntry list.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var lastModifiedBytes = Convert.FromBase64String(lastModified);
            await _listEntryService.DeleteEntryAsync(id, lastModifiedBytes);
            return RedirectToAction("ListEntries", "VirusCharacteristicsListEntry", new { characteristicId = characteristic });
        }
    }
}
