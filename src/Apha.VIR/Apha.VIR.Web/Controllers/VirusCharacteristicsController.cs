using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace Apha.VIR.Web.Controllers
{
    public class VirusCharacteristicsController : Controller
    {
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IMapper _mapper;

        public VirusCharacteristicsController(IVirusCharacteristicService virusCharacteristicService, IMapper mapper)
        {
            _virusCharacteristicService = virusCharacteristicService;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new VirusCharacteristicsViewModel());
        }
        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {

            VirusCharacteristicDetails model = new VirusCharacteristicDetails();
            var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
            model.CharacteristicTypeNameList = virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType }).ToList();
            return View(model);

        }
        [HttpGet]
        public async Task<IActionResult> EditAsync(Guid? id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _virusCharacteristicService.GetVirusCharacteristicsByIdAsync(id);
            var viewModel = _mapper.Map<VirusCharacteristicDetails>(result);

            //VirusCharacteristicDetails model = new VirusCharacteristicDetails();
            var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
            viewModel.CharacteristicTypeNameList = virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType }).ToList();
            return View(viewModel);

        }
        [HttpPost]
        public async Task<IActionResult> Edit(VirusCharacteristicDetails model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate the dropdown list
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
                model.CharacteristicTypeNameList = virusTypesDto
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType })
                    .ToList();
                return View("Edit", model);
            }
            var dto = _mapper.Map<VirusCharacteristicDTO>(model);
            await _virusCharacteristicService.UpdateEntryAsync(dto);

            return RedirectToAction("List");
        }
        [HttpPost]
        public async Task<IActionResult> Create(VirusCharacteristicDetails model)
        {
            var validationErrors = await ValidateVirusCharacteristicAdd(model);

            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            if (!ModelState.IsValid)
            {
                // Repopulate the dropdown list
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
                model.CharacteristicTypeNameList = virusTypesDto
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType })
                    .ToList();
                return View("Create", model);
            }
            var dto = _mapper.Map<VirusCharacteristicDTO>(model);
            await _virusCharacteristicService.AddEntryAsync(dto);

            return RedirectToAction("List");
        }
        [HttpGet]
        public async Task<IActionResult> List(int pageNo = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize);
            var viewModel = _mapper.Map<List<VirusCharacteristicDetails>>(result.data);

            VirusCharacteristicsViewModel model = new VirusCharacteristicsViewModel
            {
                list = viewModel,
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = result.TotalCount
                }
            };

            return View(model);
        }
        private async Task<List<string>> ValidateVirusCharacteristicAdd(VirusCharacteristicDetails model)
        {
            var errors = new List<string>();

            // Ensure the item does not already exist
            var allCharacteristics = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            if (allCharacteristics.Any(vc => vc.Id == model.Id))
            {
                errors.Add("- Item already exists.");
            }          

            // Ensure that the length is not greater than 100 characters
            if (model.Length.HasValue && model.Length.Value > 100)
            {
                errors.Add("- Maximum length must be no more than 100 characters.<br />");
            }

            return errors;
        }

        public async Task<IActionResult> Delete(VirusCharacteristicDetails model, Guid id)
        {
            if (!ModelState.IsValid || id == Guid.Empty)
            {
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
                model.CharacteristicTypeNameList = virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType }).ToList();
                return View("Edit", model);
            }
            else if (CheckEntries(id).Result)
            {
                ModelState.AddModelError("", "Virus Characteristic cannot be deleted as it is already assigned to one or more Virus Isolates.");
                return View("Edit", model);
            }
            else
            {
                await _virusCharacteristicService.DeleteVirusCharactersticsAsync(id, model.LastModified);
            }

            return RedirectToAction(nameof(List));
        }

        private async Task<bool> CheckEntries(Guid id)
        {
            return await _virusCharacteristicService.CheckVirusCharactersticsUsageByIdAsync(id);
        }

        public async Task<IActionResult> BindCharacteristicEntriesGridOnPagination(int pageNo, int pageSize)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            VirusCharacteristicsViewModel model = new VirusCharacteristicsViewModel();

            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize);
            var viewModel = _mapper.Map<List<VirusCharacteristicDetails>>(result.data);
            model.list = viewModel;
            model.Pagination = new PaginationModel
            {
                PageNumber = pageNo,
                PageSize = pageSize,
                TotalCount = result.TotalCount
            };

            return PartialView("_VirusCharatersticsList", model);
        }

    }
}
