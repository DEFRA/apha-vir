using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Validation;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.DataAccess.Repositories;
using Apha.VIR.Web.Models;
using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
        public async Task<IActionResult> Edit(string AVNumber, Guid Isolate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var isolateCharacteristicInfoList = await _isolatesService.GetIsolateCharacteristicInfoAsync(Isolate);
            var model = _mapper.Map<List<IsolateCharacteristicInfoModel>>(isolateCharacteristicInfoList);
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
        public async Task<IActionResult> Edit(List<IsolateCharacteristicInfoModel> characteristics)
        {
            if (!ModelState.IsValid)
            {
                await PrepareDropDownLists(characteristics);
                return View(characteristics);
            }

            var validationErrors = await ProcessCharacteristics(characteristics);

            if (validationErrors.Any())
            {
                await PrepareDropDownLists(characteristics);
                AddModelErrors(validationErrors);
                return View(characteristics);
            }

            var avNumbers = characteristics.Select(c => c.AVNumber).Distinct();
            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = avNumbers });
        }

        private async Task<List<string>> ProcessCharacteristics(List<IsolateCharacteristicInfoModel> characteristics)
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
            if (!errors.Any())
            {
                foreach (var characteristic in characteristics)
                {
                    var dto = _mapper.Map<IsolateCharacteristicInfoDTO>(characteristic);
                    await _isolatesService.UpdateIsolateCharacteristicsAsync(dto, "Test");
                }
            }
            return errors;
        }

        private async Task PrepareDropDownLists(List<IsolateCharacteristicInfoModel> characteristics)
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

        public static string ValidateCharacteristic(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO)
        {
            if (characteristicViewModel.VirusCharacteristicId == Guid.Empty)
                return "- Id not specified for this item.";

            if (virusCharacteristicDTO == null)
                return "- Item does not exist.";

            switch (characteristicViewModel.CharacteristicType)
            {
                case "Text":
                    return ValidateText(characteristicViewModel, virusCharacteristicDTO);
                case "Numeric":
                    return ValidateNumeric(characteristicViewModel, virusCharacteristicDTO);
                case "Yes/No":
                case "SingleList":
                    return ""; // Implement validation if needed
                default:
                    return ""; // Handle other types or return empty if none matched
            }
        }

        private static string ValidateText(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO)
        {
            if (string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue)) return "";

            if (characteristicViewModel.CharacteristicValue.Length > virusCharacteristicDTO.Length)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} exceeds maximum length requirement (Maximum Length: {virusCharacteristicDTO.Length})";
            }

            return "";
        }

        private static string ValidateNumeric(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO)
        {
            if (string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue)) return "";

            if (!double.TryParse(characteristicViewModel.CharacteristicValue, out double itemValue))
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} is not a valid number.";
            }

            var rangeReturn = ValidateNumericRange(characteristicViewModel, virusCharacteristicDTO, itemValue);

            return string.IsNullOrEmpty(rangeReturn)
                ? ValidateNumericDecimalPlaces(characteristicViewModel, virusCharacteristicDTO, itemValue): rangeReturn;
        }

        private static string ValidateNumericRange(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO, double itemValue)
        {
            if (virusCharacteristicDTO.MinValue.HasValue && itemValue < virusCharacteristicDTO.MinValue)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} is below the minimum value requirement (Range: {virusCharacteristicDTO.MinValue} to {virusCharacteristicDTO.MaxValue}).";
            }

            if (virusCharacteristicDTO.MaxValue.HasValue && itemValue > virusCharacteristicDTO.MaxValue)
            {
                return $"- Value entered for {characteristicViewModel.CharacteristicName} exceeds the maximum value requirement (Range: {virusCharacteristicDTO.MinValue} to {virusCharacteristicDTO.MaxValue}).";
            }

            return "";
        }

        private static string ValidateNumericDecimalPlaces(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO, double itemValue)
        {
            if (!virusCharacteristicDTO.DecimalPlaces.HasValue || virusCharacteristicDTO.DecimalPlaces == 0) return "";
            if (!string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue))
            {
                var parts = characteristicViewModel.CharacteristicValue.Split('.');
                if (parts.Length > 1 && parts[1].Length >= virusCharacteristicDTO.DecimalPlaces.Value)
                    return "";
            }
            return $"- Value entered for {characteristicViewModel.CharacteristicName} does not include the required number of decimal places (Decimal Places: {virusCharacteristicDTO.DecimalPlaces}).";
        }       
    }
}
