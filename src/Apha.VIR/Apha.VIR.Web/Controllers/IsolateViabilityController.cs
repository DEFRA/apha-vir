using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateViabilityController : Controller
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        public IsolateViabilityController(IIsolateViabilityService isolateViabilityService, IMapper mapper)
        {
            _isolateViabilityService = isolateViabilityService;
            _mapper = mapper;
        }

        public IActionResult History(string AVNumber, Guid Isolate)
        {
            var result = _isolateViabilityService.GetViabilityHistoryAsync(AVNumber, Isolate).Result;
       
            var viabilityHistories = _mapper.Map<IEnumerable<IsolateViabilityModel>>(result);

            var viewModel = new IsolateViabilityHistoryViewModel
            {
                Nomenclature = viabilityHistories.FirstOrDefault()?.Nomenclature!,
                ViabilityHistoryList = viabilityHistories
            };

            return View("ViabilityHistory", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid isolateViabilityId, string lastModified,string avNUmber, Guid isolateId)
        {
            string userid = "TestUser";

            byte[] lastModifiedbyte = Convert.FromBase64String(lastModified);

            await _isolateViabilityService.DeleteIsolateViabilityAsync(isolateViabilityId, lastModifiedbyte, userid);

            return RedirectToAction(nameof(History), new { AVNumber= avNUmber, Isolate = isolateId });
        }

    }
}
