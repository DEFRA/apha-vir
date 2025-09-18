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
    public class IsolateCharacteristicsController : Controller
    {
        private readonly IIsolatesService _isolatesService;
        private readonly IVirusCharacteristicListEntryService _virusCharacteristicListEntryService;
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IMapper _mapper;

        public IsolateCharacteristicsController(IIsolatesService isolatesService,
            IVirusCharacteristicListEntryService virusCharacteristicListEntryService,
            IVirusCharacteristicService virusCharacteristicService,
            IMapper mapper)
        {
            _virusCharacteristicListEntryService = virusCharacteristicListEntryService;
            _virusCharacteristicService = virusCharacteristicService;
            _isolatesService = isolatesService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Edit(string AVNumber, Guid Isolate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isolateCharacteristicInfoList = await _isolatesService.GetIsolateCharacteristicInfoAsync(Isolate);
            var model = _mapper.Map<List<IsolateCharacteristicViewModel>>(isolateCharacteristicInfoList);
            foreach (var item in model)
            {
                if (item.CharacteristicType == "SingleList" && item.VirusCharacteristicId.HasValue && item.CharacteristicValue != null)
                {
                    item.CharacteristicValueDropDownList = await GetDropDownList(item.VirusCharacteristicId.Value, item.CharacteristicValue);
                }
                item.AVNumber = AVNumber;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(List<IsolateCharacteristicViewModel> characteristics)
        {
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("User not authorised to update isolate characteristics.");
            }
            if (characteristics == null || characteristics.Count==0)
            {  
                ModelState.AddModelError("", "No characteristics data was provided.");
                return View(characteristics); // return back with the error
            }
            const int maxAllowedItems = 5;
            if (characteristics.Count > maxAllowedItems)
            {
                ModelState.AddModelError("", $"You can only submit up to {maxAllowedItems} characteristics at a time.");
                return View(characteristics); // return back with the error
            }

            if (!ModelState.IsValid)
            {
                await PrepareDropDownLists(characteristics);
                return View(characteristics);
            }

            var validationErrors = await ProcessCharacteristics(characteristics);

            if (validationErrors.Count > 0)
            {
                await PrepareDropDownLists(characteristics);
                AddModelErrors(validationErrors);
                return View(characteristics);
            }

            var avNumbers = characteristics.Select(c => c.AVNumber).Distinct();
            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = avNumbers });
        }

        private async Task<List<string>> ProcessCharacteristics(List<IsolateCharacteristicViewModel> characteristics)
        {
            var errors = new List<string>();
            var existingVirusCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();

            foreach (var characteristic in characteristics)
            {
                var virusCharacteristic = existingVirusCharacteristics
                    .FirstOrDefault(vc => vc.Id == characteristic.VirusCharacteristicId);

                if (virusCharacteristic == null)
                    continue;

                var error = ValidateCharacteristic(characteristic, virusCharacteristic);

                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(error);
                }
            }
            if (errors.Count == 0)
            {
                foreach (var characteristic in characteristics)
                {
                    var dto = _mapper.Map<IsolateCharacteristicDto>(characteristic);
                    await _isolatesService.UpdateIsolateCharacteristicsAsync(dto, AuthorisationUtil.GetUserId());
                }
            }
            return errors;
        }

        private async Task PrepareDropDownLists(List<IsolateCharacteristicViewModel> characteristics)
        {
            foreach (var item in characteristics)
            {
                if (item.CharacteristicType == "SingleList" &&
                    item.VirusCharacteristicId.HasValue &&
                    item.CharacteristicValue != null)
                {
                    item.CharacteristicValueDropDownList = await GetDropDownList(
                        item.VirusCharacteristicId.Value, item.CharacteristicValue);
                }
            }
        }

        private void AddModelErrors(IEnumerable<string> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        public async Task<List<SelectListItem>> GetDropDownList(Guid virusCharacteristicId, string characteristicValue)
        {
            if (!ModelState.IsValid || virusCharacteristicId == Guid.Empty)
            {
                return new List<SelectListItem>();
            }

            var options = await _virusCharacteristicListEntryService.GetEntriesByCharacteristicIdAsync(virusCharacteristicId);
            var characteristicValueDropDownList = options.Select(o => new SelectListItem
            {
                Value = o.Name,
                Text = o.Name,
                Selected = (o.Name == characteristicValue)
            }).ToList();
            return characteristicValueDropDownList;
        }

        public static string ValidateCharacteristic(IsolateCharacteristicViewModel characteristicViewModel, VirusCharacteristicDto virusCharacteristicDto)
        {
            if (characteristicViewModel.VirusCharacteristicId == Guid.Empty)
                return "- Id not specified for this item.";

            if (virusCharacteristicDto == null)
                return "- Item does not exist.";

            switch (characteristicViewModel.CharacteristicType)
            {
                case "Text":
                    return ValidateText(characteristicViewModel, virusCharacteristicDto);
                case "Numeric":
                    return ValidateNumeric(characteristicViewModel, virusCharacteristicDto);
                case "Yes/No":
                case "SingleList":
                    return ""; // Implement validation if needed
                default:
                    return ""; // Handle other types or return empty if none matched
            }
        }

        private static string ValidateText(IsolateCharacteristicViewModel characteristicViewModel, VirusCharacteristicDto virusCharacteristicDto)
        {
            if (string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue)) return "";

            if (characteristicViewModel.CharacteristicValue.Length > virusCharacteristicDto.Length)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} exceeds maximum length requirement (Maximum Length: {virusCharacteristicDto.Length})";
            }

            return "";
        }

        private static string ValidateNumeric(IsolateCharacteristicViewModel characteristicViewModel, VirusCharacteristicDto virusCharacteristicDto)
        {
            if (string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue)) return "";

            if (!double.TryParse(characteristicViewModel.CharacteristicValue, out double itemValue))
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} is not a valid number.";
            }

            var rangeReturn = ValidateNumericRange(characteristicViewModel, virusCharacteristicDto, itemValue);

            return string.IsNullOrEmpty(rangeReturn)
                ? ValidateNumericDecimalPlaces(characteristicViewModel, virusCharacteristicDto): rangeReturn;
        }

        private static string ValidateNumericRange(IsolateCharacteristicViewModel characteristicViewModel, VirusCharacteristicDto virusCharacteristicDto, double itemValue)
        {
            if (virusCharacteristicDto.MinValue.HasValue && itemValue < virusCharacteristicDto.MinValue)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} is below the minimum value requirement (Range: {virusCharacteristicDto.MinValue} to {virusCharacteristicDto.MaxValue}).";
            }

            if (virusCharacteristicDto.MaxValue.HasValue && itemValue > virusCharacteristicDto.MaxValue)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} exceeds the maximum value requirement (Range: {virusCharacteristicDto.MinValue} to {virusCharacteristicDto.MaxValue}).";
            }

            return "";
        }

        private static string ValidateNumericDecimalPlaces(IsolateCharacteristicViewModel characteristicViewModel, VirusCharacteristicDto virusCharacteristicDto)
        {
            if (!virusCharacteristicDto.DecimalPlaces.HasValue || virusCharacteristicDto.DecimalPlaces == 0) return "";
            if (!string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue))
            {
                var parts = characteristicViewModel.CharacteristicValue.Split('.');
                if (parts.Length > 1 && parts[1].Length >= virusCharacteristicDto.DecimalPlaces.Value)
                    return "";
            }
            return $"- Value entered for {characteristicViewModel.CharacteristicName} does not include the required number of decimal places (Decimal Places: {virusCharacteristicDto.DecimalPlaces}).";
        }       
    }
}
