using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class SubmissionController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ILookupService _lookupService;
        private readonly ISenderService _senderService;
        private readonly ISubmissionService _submissionService;

        public SubmissionController(ILookupService lookupService,
            ISenderService senderService,
            ISubmissionService submissionService,
            IMapper mapper)
        {
            _lookupService = lookupService;
            _senderService = senderService;
            _submissionService = submissionService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string AVNumber)
        {
            var submissionModel = new SubmissionCreateViewModel
            {
                AVNumber = AVNumber,
                CountryList = await GetCountryDropdownList(),
                SubmittingLabList = await GetSubmittingLabDropdownList(),
                SubmissionReasonList = await GetSubmissionReasonDropdownList(),
                Senders = new List<SubmissionSenderViewModel>(),
                Organisations = new List<SubmissionSenderViewModel>()
            };

            return View(submissionModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubmissionCreateViewModel submission)
        {
            if (!ModelState.IsValid)
            {
                submission.CountryList = await GetCountryDropdownList();
                submission.SubmittingLabList = await GetSubmittingLabDropdownList();
                submission.SubmissionReasonList = await GetSubmissionReasonDropdownList();
                submission.Senders = new List<SubmissionSenderViewModel>();
                submission.Organisations = new List<SubmissionSenderViewModel>();
                return View(submission);
            }
            var submissionDto = _mapper.Map<SubmissionDTO>(submission);
            await _submissionService.AddSubmissionAsync(submissionDto, "TestUser");

            return RedirectToAction("Index", "SubmissionSamples");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string AVNumber)
        {
            var isAvNumberPresent = await _submissionService.AVNumberExistsInVirAsync(AVNumber);
            if (!isAvNumberPresent)
            {
                return RedirectToAction("Create", new { AVNumber = AVNumber });
            }

            var submissionDto = await _submissionService.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            SubmissionEditViewModel submissionModel = _mapper.Map<SubmissionEditViewModel>(submissionDto);
            submissionModel.CountryList = await GetCountryDropdownList();
            submissionModel.SubmittingLabList = await GetSubmittingLabDropdownList();
            submissionModel.SubmissionReasonList = await GetSubmissionReasonDropdownList();
            submissionModel.Senders = new List<SubmissionSenderViewModel>();
            submissionModel.Organisations = new List<SubmissionSenderViewModel>();

            return View(submissionModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SubmissionEditViewModel submission)
        {
            if (!ModelState.IsValid)
            {
                submission.CountryList = await GetCountryDropdownList();
                submission.SubmittingLabList = await GetSubmittingLabDropdownList();
                submission.SubmissionReasonList = await GetSubmissionReasonDropdownList();
                submission.Senders = new List<SubmissionSenderViewModel>();
                submission.Organisations = new List<SubmissionSenderViewModel>();
                return View(submission);
            }
            var submissionDto = _mapper.Map<SubmissionDTO>(submission);
            await _submissionService.UpdateSubmissionAsync(submissionDto, "TestUser");

            return RedirectToAction("Index", "SubmissionSamples");
        }

        [HttpGet]
        public async Task<PartialViewResult> GetSenderDetails(Guid? countryId)
        {
            if (!ModelState.IsValid)
                countryId = null;

            var senderDto = await _senderService.GetAllSenderOrderBySenderAsync(countryId);
            var senderModel = _mapper.Map<List<SubmissionSenderViewModel>>(senderDto);
            return PartialView("_MainSenders", senderModel);
        }

        [HttpGet]
        public async Task<PartialViewResult> GetAddSender()
        {
            var senderModel = new SubmissionSenderViewModel
            {
                CountryList = await GetCountryDropdownList()
            };
            return PartialView("_AddSender", senderModel);
        }

        [HttpPost]
        public async Task<JsonResult> AddSender(SubmissionSenderViewModel senderModel)
        {
            if (ModelState.IsValid)
            {
                var sender = _mapper.Map<SenderDTO>(senderModel);
                await _senderService.AddSenderAsync(sender);
                return Json(new { success = true, message = "Sender add successfully!" });
            }
            return Json(new { success = false, message = "Add sender failed!" });
        }

        [HttpGet]
        public async Task<PartialViewResult> GetOrganisationDetails(Guid? countryId)
        {
            if (!ModelState.IsValid)
                countryId = null;

            var senderDto = await _senderService.GetAllSenderOrderByOrganisationAsync(countryId);
            var senderModel = _mapper.Map<List<SubmissionSenderViewModel>>(senderDto);
            return PartialView("_MainOrganisations", senderModel);
        }

        private async Task<List<SelectListItem>> GetCountryDropdownList()
        {
            var countries = await _lookupService.GetAllCountriesAsync();
            return countries.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetSubmittingLabDropdownList()
        {
            var submittingLabs = await _lookupService.GetAllSubmittingLabAsync();
            return submittingLabs.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetSubmissionReasonDropdownList()
        {
            var submissionReasons = await _lookupService.GetAllSubmissionReasonAsync();
            return submissionReasons.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }
    }
}
