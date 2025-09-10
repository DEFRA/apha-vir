using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
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
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;

        public VirusCharacteristicsController(IVirusCharacteristicService virusCharacteristicService, IMapper mapper, ILookupService lookupService)
        {
            _virusCharacteristicService = virusCharacteristicService;
            _mapper = mapper;
            _lookupService = lookupService;
        }
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                return View(new VirusCharacteristicsViewModel());
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> EditAsync()
        {
            try
            {
                VirusCharacteristicDetails model = new VirusCharacteristicDetails();
                var virusTypesDto = await _virusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync();
                model.CharacteristicTypeNameList = virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.DataType }).ToList();
                return View(model);

            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Edit(VirusCharacteristicDetails model)
        {
            if (!ModelState.IsValid)
                return View("CreateVirusCharacteristicEntry", model);

            var dto = _mapper.Map<VirusCharacteristicDTO>(model);
            await _virusCharacteristicService.AddEntryAsync(dto);

            return RedirectToAction("List");
        }
        [HttpGet]
        public async Task<IActionResult> List(int pageNo = 1, int pageSize = 10)
        {
            VirusCharacteristicsViewModel model = new VirusCharacteristicsViewModel();
            model.list = new List<VirusCharacteristicDetails>();
            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize);
            var viewModel = _mapper.Map<List<VirusCharacteristicDetails>>(result.data);
            model.list = viewModel;
            model.Pagination = new PaginationModel
            {
                PageNumber = pageNo,
                PageSize = pageSize,
                TotalCount = result.TotalCount
            };
            return View(model);
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
