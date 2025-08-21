using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Services;
using Newtonsoft.Json;

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

            return View("Lookup", lookups);
        }

        public async Task<IActionResult> LookupList(Guid lookupid, int pageNo = 1, int pageSize = 10)
        {
            var result = await _lookupService.GetAllLookupEntriesAsync(lookupid, pageNo, pageSize);

            var lookups = _mapper.Map<IEnumerable<LookupItemModel>>(result.data);

            var viewModel = new LookupItemViewModel
            {
                LookupName = "ANIm",
                LookupId = lookupid,
                LookupItemResult = new LookupItemListViewModel
                {
                    LookupItems = lookups.ToList(),
                    Pagination = new PaginationModel
                    {
                        PageNumber = pageNo,
                        PageSize = pageSize,
                        TotalCount = result.TotalCount
                    }
                }
            };

            return View("LookupItem", viewModel);
        }

        public async Task<IActionResult> BindLookupItemGridOnPagination( Guid lookupid, int pageNo,int pageSize)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }


            var result = await _lookupService.GetAllLookupEntriesAsync(lookupid, pageNo, pageSize);

            var lookups = _mapper.Map<IEnumerable<LookupItemModel>>(result.data);

            var LookupItemResult = new LookupItemListViewModel
            {
                LookupItems = lookups.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    //PageSize = pageSize,
                    TotalCount = result.TotalCount
                }
            };


            return PartialView("_IsolateSearchResults", LookupItemResult);
        }
    }
}
