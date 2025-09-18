using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicAssociationController : Controller
    {
        private readonly ILookupService _lookupService;
        private readonly IVirusCharacteristicService _characteristicService;
        private readonly IVirusCharacteristicAssociationService _typeCharacteristicService;

        public VirusCharacteristicAssociationController(
             ILookupService lookupService,
             IVirusCharacteristicService characteristicService,
             IVirusCharacteristicAssociationService typeCharacteristicService)
        {
            _lookupService = lookupService;
            _characteristicService = characteristicService;
            _typeCharacteristicService = typeCharacteristicService;
        }

        public async Task<IActionResult> Index(Guid? familyId, Guid? typeId)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var families = await _lookupService.GetAllVirusFamiliesAsync();
            var selectedFamilyId = familyId ?? families.FirstOrDefault()?.Id;

            var virusTypes = selectedFamilyId.HasValue
                ? await _lookupService.GetAllVirusTypesByParentAsync(selectedFamilyId)
                : Enumerable.Empty<LookupItemDto>();

            var selectedVirusTypeId = typeId
                ?? virusTypes.FirstOrDefault()?.Id;

            var presentVirusCharacteristics = selectedVirusTypeId.HasValue
                ? await _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(selectedVirusTypeId, false)
                : Enumerable.Empty<VirusCharacteristicDto>();
            var absentVirusCharacteristics = selectedVirusTypeId.HasValue
                ? await _characteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(selectedVirusTypeId, true)
                : Enumerable.Empty<VirusCharacteristicDto>();

            var vm = new VirusCharacteristicAssociationViewModel
            {
                SelectedFamilyId = selectedFamilyId,
                SelectedVirusTypeId = selectedVirusTypeId,
                VirusFamilies = families,
                VirusTypes = virusTypes,
                CharacteristicsPresent = presentVirusCharacteristics,
                CharacteristicsAbsent = absentVirusCharacteristics
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignCharacteristic(Guid typeId, Guid characteristicId)
        {
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to assign a VirusCharacteristic to a VirusType.");
            }
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
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.LookupDataManager))
            {
                throw new UnauthorizedAccessException("Not authorised to remove VirusCharacteristic from VirusType.");
            }
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
