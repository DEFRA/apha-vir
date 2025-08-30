using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Web.Models;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Web.Controllers
{
    public class SenderController : Controller
    {
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;

        public SenderController(ISenderService senderService, ILookupService lookupService, IMapper mapper)
        {
            _senderService = senderService;
            _lookupService = lookupService;
            _mapper = mapper;
        }
        public async Task<IActionResult> Index()
        {
            int pageNo = 1;
            int pageSize = 10;

            var senders = await _senderService.GetAllSenderAsync(pageNo, pageSize);
            var senderList = _mapper.Map<IEnumerable<SenderMViewModel>>(senders.data);

            var viewModel = new SenderListViewModel
            {
                Senders = senderList.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = senders.TotalCount
                }
            };

            return View("Sender", viewModel);
        }

        public async Task<IActionResult> BindSenderGridOnPagination(int pageNo, int pageSize)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }

            var senders = await _senderService.GetAllSenderAsync(pageNo, pageSize);
            var senderList = _mapper.Map<IEnumerable<SenderMViewModel>>(senders.data);

            var viewModel = new SenderListViewModel
            {
                Senders = senderList.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = senders.TotalCount
                }
            }; ;

            return PartialView("_SenderList", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var viewModel = new SenderMViewModel
            {
                SenderName = string.Empty,
                SenderOrganisation = string.Empty,
                SenderAddress = string.Empty,
                CountryList = await GetCountryDropdownList(),
            };

            return View("CreateSender", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SenderMViewModel model)
        {
            var showerrorSummary = false;

            if (ModelState.IsValid)
            {
                // await ValidateModel(model, ModelState, "create");
                showerrorSummary = true;
            }

            if (!ModelState.IsValid)
            {
                // model.ShowErrorSummary = showerrorSummary;
                return View("CreateSender", model);
            }

            //var dto = _mapper.Map<LookupItemDTO>(model.LookupItem);

            // await _lookupService.InsertLookupItemAsync(model.LookupId, dto);

            return RedirectToAction(nameof(Index));
        }
        private async Task<List<SelectListItem>> GetCountryDropdownList()
        {
            var countries = await _lookupService.GetAllCountriesAsync();
            return countries.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }
    }
}
