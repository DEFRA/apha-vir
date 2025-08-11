using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class IsolatesController : Controller
    {
        private readonly IIsolatesService _isolatesService;
        private readonly IMapper _mapper;

        public IsolatesController(IIsolatesService isolatesService, IMapper mapper)
        {
            _isolatesService = isolatesService;
            _mapper = mapper;
        }
        public async Task<IActionResult> ViewIsolateDetails(Guid IsolateId)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }
            var result = await _isolatesService.GetIsolateFullDetailsAsync(IsolateId);
            var isolateDetails = _mapper.Map<IsolateDetailsViewModel>(result);
            return View("IsolateDetails", isolateDetails);
        }
    }
}
