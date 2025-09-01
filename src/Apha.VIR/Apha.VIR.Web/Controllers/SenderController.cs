using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.Lookup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public async Task<IActionResult> Index(int pageNo = 1, int pageSize = 10)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid parameters.");
            }

            var pagedSenderDtos = await _senderService.GetAllSenderAsync(pageNo, pageSize);
            var senderList = _mapper.Map<IEnumerable<SenderMViewModel>>(pagedSenderDtos.data);

            var viewModel = new SenderListViewModel
            {
                Senders = senderList.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = pagedSenderDtos.TotalCount
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

            var pagedSenderDtos = await _senderService.GetAllSenderAsync(pageNo, pageSize);
            var senderList = _mapper.Map<IEnumerable<SenderMViewModel>>(pagedSenderDtos.data);

            var viewModel = new SenderListViewModel
            {
                Senders = senderList.ToList(),
                Pagination = new PaginationModel
                {
                    PageNumber = pageNo,
                    PageSize = pageSize,
                    TotalCount = pagedSenderDtos.TotalCount
                }
            };

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
            if (!ModelState.IsValid)
            {
                model.CountryList = await GetCountryDropdownList();
                return View("CreateSender", model);
            }

            var sender = _mapper.Map<SenderDTO>(model);
            await _senderService.AddSenderAsync(sender);


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid senderId, int currentPage = 1)
        {
            if (senderId == Guid.Empty || !ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var result = await _senderService.GetSenderAsync(senderId);
            if (result.SenderId == Guid.Empty)
            {
                return NotFound("Sender not found");
            }

            var senderviewModel = _mapper.Map<SenderMViewModel>(result);

            senderviewModel.CountryList = await GetCountryDropdownList();

            return View("EditSender", senderviewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SenderMViewModel model)
        {

            if (!ModelState.IsValid)
            {
                model.CountryList = await GetCountryDropdownList();
                return View("EditSender", model);
            }

            var sender = _mapper.Map<SenderDTO>(model);

            await _senderService.UpdateSenderAsync(sender);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(SenderMViewModel model, Guid senderId)
        {
            if (!ModelState.IsValid || senderId == Guid.Empty)
            {
                model.CountryList = await GetCountryDropdownList();
                return View("EditSender", model);
            }

            await _senderService.DeleteSenderAsync(senderId);

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SelectListItem>> GetCountryDropdownList()
        {
            var countries = await _lookupService.GetAllCountriesAsync();
            return countries.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }
    }
}
