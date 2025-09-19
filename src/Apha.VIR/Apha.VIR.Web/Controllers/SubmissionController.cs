using System.Security;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Create(string AVNumber)
        {
            if(string.IsNullOrEmpty(AVNumber))
            {
                return RedirectToAction("Index", "Home");
            }

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
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to create submission.");
            }

            if (!ModelState.IsValid)
            {
                submission.CountryList = await GetCountryDropdownList();
                submission.SubmittingLabList = await GetSubmittingLabDropdownList();
                submission.SubmissionReasonList = await GetSubmissionReasonDropdownList();
                submission.Senders = new List<SubmissionSenderViewModel>();
                submission.Organisations = new List<SubmissionSenderViewModel>();
                return View(submission);
            }
            var submissionDto = _mapper.Map<SubmissionDto>(submission);
            await _submissionService.AddSubmissionAsync(submissionDto, AuthorisationUtil.GetUserId());

            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = submission.AVNumber });
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Edit(string AVNumber)
        {
            if (string.IsNullOrEmpty(AVNumber))
            {
                return RedirectToAction("Index", "Home");
            }

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
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to modify submission.");
            }

            if (!ModelState.IsValid)
            {
                submission.CountryList = await GetCountryDropdownList();
                submission.SubmittingLabList = await GetSubmittingLabDropdownList();
                submission.SubmissionReasonList = await GetSubmissionReasonDropdownList();
                submission.Senders = new List<SubmissionSenderViewModel>();
                submission.Organisations = new List<SubmissionSenderViewModel>();
                return View(submission);
            }
            var submissionDto = _mapper.Map<SubmissionDto>(submission);
            await _submissionService.UpdateSubmissionAsync(submissionDto,AuthorisationUtil.GetUserId());

            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = submission.AVNumber });
        }

        [HttpGet]
        public async Task<PartialViewResult> GetSenderDetails(Guid? countryId)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                throw new SecurityException("User not authorised to retrieve this list.");
            }

            if (!ModelState.IsValid)
                countryId = null;

            var senderDto = await _senderService.GetAllSenderOrderBySenderAsync(countryId);
            var senderModel = _mapper.Map<List<SubmissionSenderViewModel>>(senderDto);
            return PartialView("_MainSenders", senderModel);
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
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
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to create sender.");
            }

            if (ModelState.IsValid)
            {
                var sender = _mapper.Map<SenderDto>(senderModel);
                await _senderService.AddSenderAsync(sender);
                return Json(new { success = true, message = "Sender add successfully!" });
            }
            return Json(new { success = false, message = "Add sender failed!" });
        }

        [HttpGet]
        public async Task<PartialViewResult> GetOrganisationDetails(Guid? countryId)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                throw new SecurityException("User not authorised to retrieve this list.");
            }

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

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> SubmissionLetter(string AVNumber)
        {
            var isExistinVir = await _submissionService.AVNumberExistsInVirAsync(AVNumber);
            if (!isExistinVir)
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new SubmissionLetterViewModel
            {
                LetterContent = await _submissionService.SubmissionLetter(AVNumber, AuthorisationUtil.GetUserId()),
                AVNumber = AVNumber
            };
            return View(viewModel);
        }

    }
}
