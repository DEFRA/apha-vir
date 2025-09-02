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

            var validationErrors = new List<string>();
            if (ModelState.IsValid)
            {
                var existingVirusCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
                foreach (var characteristic in characteristics)
                {
                    var virusCharacteristics = existingVirusCharacteristics.FirstOrDefault(e => e.Id == characteristic.VirusCharacteristicId);
                    if (virusCharacteristics != null)
                    {
                        string errorMessage = ValidateCharacteristic(characteristic, virusCharacteristics);
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            validationErrors.Add(errorMessage);
                        }
                        else
                        {
                            var data = _mapper.Map<IsolateCharacteristicInfoDTO>(characteristic);
                            await _isolatesService.UpdateIsolateCharacteristicsAsync(data, "Test");
                        }
                    }
                }
            }           

            if (validationErrors.Count > 0 || !ModelState.IsValid)
            {
                foreach (var item in characteristics)
                {
                    if (item.CharacteristicType == "SingleList" && item.VirusCharacteristicId.HasValue && item.CharacteristicValue != null)
                    {
                        item.CharacteristicValueDropDownList = await GetDropDownList(item.VirusCharacteristicId.Value, item.CharacteristicValue);
                    }
                }

                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(characteristics);
            }
            
            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = characteristics.Select(e=> e.AVNumber).Distinct() });
        }
        
        public async Task<List<SelectListItem>> GetDropDownList(Guid virusCharacteristicId, string characteristicValue)
        {
            if (!ModelState.IsValid)
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

        static string ValidateCharacteristic(IsolateCharacteristicInfoModel characteristicViewModel, VirusCharacteristicDTO virusCharacteristicDTO)
        {
            if (characteristicViewModel.VirusCharacteristicId == Guid.Empty)
            {
                return "- Id not specified for this item.";
            }

            if (virusCharacteristicDTO == null)
            {
                return "- Item does not exist.";
            }

            switch (characteristicViewModel.CharacteristicType)
            {
                case "Text":
                    if (!string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue) &&
                        characteristicViewModel.CharacteristicValue.Length > virusCharacteristicDTO.Length)
                    {
                        return "- Value entered for " + characteristicViewModel.CharacteristicName + " exceeds maximum length requirement (Maximum Length: " + virusCharacteristicDTO.Length + "";
                    }
                    break;
                case "Numeric":
                    if (!string.IsNullOrEmpty(characteristicViewModel.CharacteristicValue))
                    {
                        if (double.TryParse(characteristicViewModel.CharacteristicValue, out double itemValue))
                        {
                            if (virusCharacteristicDTO.MinValue.HasValue && itemValue < virusCharacteristicDTO.MinValue)
                            {
                                return $"- Value entered for {characteristicViewModel.CharacteristicName} is below the minimum value requirement (Range: {virusCharacteristicDTO.MinValue} to {virusCharacteristicDTO.MaxValue}).";
                            }

                            if (virusCharacteristicDTO.MaxValue.HasValue && itemValue > virusCharacteristicDTO.MaxValue)
                            {
                                return $"- Value entered for {characteristicViewModel.CharacteristicName} exceeds the maximum value requirement (Range: {virusCharacteristicDTO.MinValue} to {virusCharacteristicDTO.MaxValue}).";
                            }

                            if (virusCharacteristicDTO.DecimalPlaces.HasValue && virusCharacteristicDTO.DecimalPlaces != 0)
                            {
                                var parts = characteristicViewModel.CharacteristicValue.Split('.');
                                if (parts.Length == 1 || parts[1].Length < virusCharacteristicDTO.DecimalPlaces.Value)
                                {
                                    return $"- Value entered for {characteristicViewModel.CharacteristicName} does not include the required number of decimal places (Decimal Places: {virusCharacteristicDTO.DecimalPlaces}).";
                                }
                            }
                        }
                        else
                        {
                            return $"- Value entered for {characteristicViewModel.CharacteristicName} is not a valid number.";
                        }
                    }
                    break;
                case "Yes/No":
                    // Implement validation for Yes/No if needed
                    break;
                case "SingleList":
                    // Implement validation for SingleList if needed
                    break;
                default:
                    return "";
            }
            return "";
        }
    }
}
