using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                LookupId = lookup.Id,
                IsReadOnly = lookup.ReadOnly,
                LookupItemResult = new LookupItemListViewModel
                {
                    LookupId=lookup.Id,
                    ShowParent  = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                    ShowAlternateName = lookup.AlternateName,
                    ShowSMSRelated = lookup.Smsrelated,
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
                LookupId = lookup.Id,
                ShowParent = lookup.Parent == Guid.Empty || lookup.Parent == null ? false : true,
                ShowAlternateName = lookup.AlternateName,
                ShowSMSRelated = lookup.Smsrelated,
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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid lookupId, Guid lookupItemId,int currentPage=1)
        {
            if (lookupId == Guid.Empty || lookupItemId == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var lookupResult = await _lookupService.GetLookupsByIdAsync(lookupId);
            var lookup = _mapper.Map<LookupViewModel>(lookupResult);

            var lookupItemResult = await _lookupService.GetLookupItemAsync(lookupId,lookupItemId);
            var lookupItem = _mapper.Map<LookupItemModel>(lookupItemResult);

            var viewModel = new LookupItemtViewModel
            {
                //LookupId = lookup.Id,
                ShowParent = lookup.Parent != Guid.Empty && lookup.Parent.HasValue ? true : false,
                ShowAlternateName = lookup.AlternateName,
                ShowSMSRelated = lookup.Smsrelated,
                IsReadOnly = lookup.ReadOnly,
                LookupParentList = lookup.Parent != Guid.Empty && lookup.Parent.HasValue 
                ? GetLookupItemPresents(lookup).Result.Select(f => new SelectListItem 
                        { Value = f.Id.ToString(), 
                         Text = f.Name }).ToList()
                : null,
                LookkupItem = lookupItem
            };
            return View("EditLookupItem", viewModel);
        }

        private async Task<IEnumerable<LookupItemModel>> GetLookupItemPresents(LookupViewModel lookup)
        {
            if (lookup.Parent.HasValue && lookup.Parent != Guid.Empty)
            {
                Guid parentId = lookup.Parent.Value;
                var lookupParentResult = await _lookupService.GetLookupItemParentListAsync(parentId);
                var lookupParentList = _mapper.Map<IEnumerable<LookupItemModel>>(lookupParentResult);
                return lookupParentList;
            }
            return new List<LookupItemModel>();
        }
    }
}
