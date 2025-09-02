using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> List()
        {
            VirusCharacteristicsViewModel model = new VirusCharacteristicsViewModel();
            model.list = new List<VirusCharacteristicDetails>();
            var result = await _virusCharacteristicService.GetAllVirusCharacteristicsAsync();
            var viewModel = _mapper.Map<List<VirusCharacteristicDetails>>(result);
            model.list = viewModel;
            return View(model);
        }

    }
}
