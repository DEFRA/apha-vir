using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicFamilyAndAssociateController : Controller
    {
        private readonly ILookupService _lookupService;
        private readonly IVirusCharacteristicService _characteristicService;
        private readonly IVirusTypeCharacteristicService _typeCharacteristicService;

        public VirusCharacteristicFamilyAndAssociateController(
             ILookupService lookupService,
             IVirusCharacteristicService characteristicService,
             IVirusTypeCharacteristicService typeCharacteristicService)
        {
            _lookupService = lookupService;
            _characteristicService = characteristicService;
            _typeCharacteristicService = typeCharacteristicService;
        }

        public async Task<IActionResult> Index(Guid? familyId, Guid? typeId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var families = await _lookupService.GetAllVirusFamiliesAsync();
            var selectedFamilyId = familyId ?? families.FirstOrDefault()?.Id;

            var types = selectedFamilyId.HasValue
                ? await _lookupService.GetAllVirusTypesByParentAsync(selectedFamilyId)
                : Enumerable.Empty<LookupItemDTO>();

            var selectedTypeId = typeId
                ?? types.FirstOrDefault()?.Id;

            var present = selectedTypeId.HasValue
                ? await _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(selectedTypeId, false)
                : Enumerable.Empty<VirusCharacteristicDTO>();
            var absent = selectedTypeId.HasValue
                ? await _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(selectedTypeId, true)
                : Enumerable.Empty<VirusCharacteristicDTO>();

            var vm = new VirusFamilyAndTypeViewModel
            {
                SelectedFamilyId = selectedFamilyId,
                SelectedTypeId = selectedTypeId,
                VirusFamilies = families,
                VirusTypes = types,
                CharacteristicsPresent = present,
                CharacteristicsAbsent = absent
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCharacteristic(Guid typeId, Guid characteristicId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _typeCharacteristicService.AssignCharacteristicToTypeAsync(typeId, characteristicId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCharacteristic(Guid typeId, Guid characteristicId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _typeCharacteristicService.RemoveCharacteristicFromTypeAsync(typeId, characteristicId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetVirusTypes(Guid familyId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var types = await _lookupService.GetAllVirusTypesByParentAsync(familyId);
            return Json(types);
        }
    }
}
