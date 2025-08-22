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

            return View("Lookup", lookups);
        }

        public async Task<IActionResult> LookupList(Guid lookupid, int pageNo = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }

            var lookupResult = await _lookupService.GetLookupsByIdAsync(lookupid);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupEntries = await _lookupService.GetAllLookupEntriesAsync(lookupid, pageNo, pageSize);
            var lookups = _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data);

            var viewModel = new LookupItemViewModel
            {
                LookupName = lookup.Name + " Look-up List",
                LookupId = lookupid,
                IsReadOnly = lookup.ReadOnly,
                LookupItemResult = new LookupItemListViewModel
                {
                    HasParent  = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                    HasAlternateName = lookup.AlternateName,
                    IsSMSRelated  =lookup.Smsrelated,
                    LookupItems = lookups.ToList(),
                    Pagination = new PaginationModel
                    {
                        PageNumber = pageNo,
                        PageSize = pageSize,
                        TotalCount = lookupEntries.TotalCount
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

            var lookupResult = await _lookupService.GetLookupsByIdAsync(lookupid);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupEntries = await _lookupService.GetAllLookupEntriesAsync(lookupid, pageNo, pageSize);
            var lookups = _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data);

            var LookupItemResult = new LookupItemListViewModel
            {
                HasParent = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                HasAlternateName = lookup.AlternateName,
                IsSMSRelated = lookup.Smsrelated,
                LookupItems = lookups.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = lookupEntries.TotalCount
                }
            };

            return PartialView("_LookupItemList", LookupItemResult);
        }
    }
}
