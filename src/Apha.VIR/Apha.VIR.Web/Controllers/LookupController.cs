using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class LookupController : Controller
    {
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        public LookupController(ILookupService lookupService, IMapper mapper)
        {
            _lookupService = lookupService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            var result = await _lookupService.GetAllLookupsAsync();

            var lookups = _mapper.Map<IEnumerable<LookupViewModel>>(result);

            return View("Lookup",lookups);
        }

        public async Task<IActionResult> LookupList(Guid lookupid)
        {
            var result = await _lookupService.GetAllLookupEntriesAsync(lookupid);

            var lookups = _mapper.Map<IEnumerable<LookupItemViewModel>>(result);

            return View("LookupItemList", lookups);
        }
    }
}
