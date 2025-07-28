using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class LookupController : Controller
    {
        private readonly ILookupService _lookupService;
        readonly ILogger<LookupController> _logger;
        private readonly IMapper _mapper;
        public LookupController(ILookupService lookupService, IMapper mapper, ILogger<LookupController> logger)
        {
            _lookupService = lookupService;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<IActionResult> GetLookup()
        {
            var result = await _lookupService.GetAllLookupsAsync();

            var lookups = _mapper.Map<IEnumerable<LookupViewModel>>(result);

            return View(lookups);
        }
    }
}
