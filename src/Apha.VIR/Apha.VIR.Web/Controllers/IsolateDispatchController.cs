using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateDispatchController : Controller
    {
        private readonly IIsolateDispatchService _isolateDispatchService;
        private readonly ILookupService _lookupService;
        private readonly IIsolatesService _isolatesService;
        private readonly ISubmissionService _submissionService;
        private readonly ISampleService _sampleService;
        private readonly IMapper _mapper;

        public IsolateDispatchController(IIsolateDispatchService isolateDispatchService,
            ILookupService lookupService,
            IIsolatesService isolatesService,
            ISubmissionService submissionService,
            ISampleService sampleService,
            IMapper mapper)
        {
            _isolateDispatchService = isolateDispatchService;
            _lookupService = lookupService;
            _isolatesService = isolatesService;
            _submissionService = submissionService;
            _sampleService = sampleService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.Administrator)]
        public async Task<IActionResult> History(string AVNumber, Guid IsolateId)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (string.IsNullOrWhiteSpace(AVNumber) && IsolateId == Guid.Empty)
            {
                return View();
            }

            IEnumerable<IsolateDispatchInfoDTO> isolateDispatchInfoDTOs = await _isolateDispatchService.GetDispatchesHistoryAsync(
                AVNumber,
                IsolateId
            );

            var dispatchHistoryRecords = _mapper.Map<IEnumerable<IsolateDispatchHistory>>(isolateDispatchInfoDTOs);

            IsolateDispatchHistoryViewModel? viewModel = null;
            if (dispatchHistoryRecords != null && dispatchHistoryRecords.Any())
            {
                viewModel = new IsolateDispatchHistoryViewModel
                {
                    IsolateId = IsolateId,
                    Nomenclature = dispatchHistoryRecords.First().Nomenclature,
                    DispatchHistoryRecords = dispatchHistoryRecords
                };
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.Administrator)]
        public async Task<ActionResult> Create(string AVNumber, Guid IsolateId, string Source)
        {
            bool isBtnDisabled = false;
            IsolateDispatchCreateViewModel dispatchCreateModel = new IsolateDispatchCreateViewModel { Avnumber = AVNumber, DispatchIsolateId = IsolateId, ValidToIssue = false };
            dispatchCreateModel.ViabilityList = await GetViabilityDropdownList();
            dispatchCreateModel.RecipientList = await GetWorkGroupsDropdownList();
            dispatchCreateModel.DispatchedByList = await GetStaffsDropdownList();

            var isolateInfo = await _isolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(AVNumber, IsolateId);
            if (isolateInfo != null)
            {
                dispatchCreateModel.Avnumber = AVNumber;
                dispatchCreateModel.NoOfAliquots = isolateInfo.NoOfAliquots ?? 0;
                dispatchCreateModel.Nomenclature = isolateInfo.Nomenclature;
                dispatchCreateModel.ValidToIssue = isolateInfo.ValidToIssue ?? false;
                dispatchCreateModel.MaterialTransferAgreement = isolateInfo.MaterialTransferAgreement;
                var lastViability = await _isolateDispatchService.GetLastViabilityByIsolateAsync(IsolateId);
                dispatchCreateModel.ViabilityId = lastViability?.Viable;
            }
            dispatchCreateModel.WarningMessages = ValidatDispatchForPageLoad(isolateInfo?.NoOfAliquots ?? 0, isolateInfo!.IsMixedIsolate, isolateInfo.ValidToIssue ?? false, ref isBtnDisabled);
            dispatchCreateModel.IsDispatchDisabled = isBtnDisabled;

            if (!await CheckFieldsVisibility(AVNumber, IsolateId))
            {
                dispatchCreateModel.IsFieldInVisible = true;
            }

            dispatchCreateModel.Source = Source;
            if (!ModelState.IsValid)
            {
                return View(dispatchCreateModel);
            }
            return View(dispatchCreateModel);
        }

        [HttpPost]
        public async Task<ActionResult> Create(IsolateDispatchCreateViewModel dispatchModel)
        {
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to create dispatch.");
            }
            ValidateAddIsolateDispatch(dispatchModel, ModelState);
            dispatchModel.ViabilityList = await GetViabilityDropdownList();
            dispatchModel.RecipientList = await GetWorkGroupsDropdownList();
            dispatchModel.DispatchedByList = await GetStaffsDropdownList();

            var isolateInfo = await _isolateDispatchService.GetIsolateInfoByAVNumberAndIsolateIdAsync(dispatchModel.Avnumber, dispatchModel.DispatchIsolateId);
            if (isolateInfo != null)
            {
                dispatchModel.NoOfAliquots = isolateInfo.NoOfAliquots ?? 0;
                dispatchModel.Nomenclature = isolateInfo.Nomenclature;
                dispatchModel.ValidToIssue = isolateInfo.ValidToIssue ?? false;
                dispatchModel.MaterialTransferAgreement = isolateInfo.MaterialTransferAgreement;
                var lastViability = await _isolateDispatchService.GetLastViabilityByIsolateAsync(dispatchModel.DispatchIsolateId);
                dispatchModel.ViabilityId = lastViability?.Viable;
            }

            if (!ModelState.IsValid)
            {
                return View(dispatchModel);
            }
            var dispatchRecord = _mapper.Map<IsolateDispatchInfoDTO>(dispatchModel);
            await _isolateDispatchService.AddDispatchAsync(dispatchRecord, "TestUser");
            string fromSource = dispatchModel!.Source!.ToLower();
            return fromSource switch
            {
                "search" => RedirectToAction("Confirmation", "IsolateDispatch", new { Isolate = dispatchRecord.DispatchIsolateId }),
                "summary" => RedirectToAction("Index", "SubmissionSamples", new { AVNumber = dispatchModel.Avnumber }),
                _ => RedirectToAction("Create", "IsolateDispatch")
            };
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.Administrator)]
        public async Task<IActionResult> Edit(string AVNumber, Guid DispatchId, Guid DispatchIsolateId)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            string recepientLocation;

            if (!ModelState.IsValid)
            {
                return View();
            }

            IsolateDispatchInfoDTO isolateDispatchInfoDTO =
                await _isolateDispatchService.GetDispatchForIsolateAsync(
                AVNumber,
                DispatchId,
                DispatchIsolateId
            );

            recepientLocation = isolateDispatchInfoDTO.RecipientId == null ? "External" : "Internal";

            IEnumerable<LookupItemDTO> viabilityLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllViabilityAsync());
            IEnumerable<LookupItemDTO> recepientLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllWorkGroupsAsync());
            IEnumerable<LookupItemDTO> dispatchedByLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllStaffAsync());

            var model = _mapper.Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO);

            model.ViabilityList = viabilityLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == isolateDispatchInfoDTO.ViabilityId }).ToList();
            model.RecipientList = recepientLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == isolateDispatchInfoDTO.RecipientId }).ToList();
            model.DispatchedByList = dispatchedByLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            model.DispatchedById = isolateDispatchInfoDTO.DispatchedById ?? Guid.Empty;
            model.LastModified = isolateDispatchInfoDTO.LastModified ?? Array.Empty<byte>();
            model.DispatchIsolateId = DispatchIsolateId;
            model.DispatchId = DispatchId;
            model.RecipientLocation = recepientLocation;

            if (!await CheckFieldsVisibility(AVNumber, DispatchIsolateId))
            {
                model.IsFieldInVisible = true;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IsolateDispatchEditViewModel model)
        {
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.Administrator))
            {
                throw new UnauthorizedAccessException("Not authorised to modify dispatch.");
            }
            ModelState.Remove(nameof(model.DispatchedByList));
            ModelState.Remove(nameof(model.ViabilityList));
            ModelState.Remove(nameof(model.RecipientList));
            ModelState.Remove(nameof(model.RecipientAddress));

            await ValidateEditIsolateDispatch(model, ModelState);


            if (!ModelState.IsValid)
            {
                var viabilityLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllViabilityAsync());
                var recepientLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllWorkGroupsAsync());
                var dispatchedByLookup = _mapper.Map<IEnumerable<LookupItemDTO>>(await _lookupService.GetAllStaffAsync());

                model.ViabilityList = viabilityLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.ViabilityId }).ToList();
                model.RecipientList = recepientLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.RecipientId }).ToList();
                model.DispatchedByList = dispatchedByLookup.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == model.DispatchedById }).ToList();

                return View(model);
            }

            var dispatchRecord = _mapper.Map<IsolateDispatchInfoDTO>(model);

            await _isolateDispatchService.UpdateDispatchAsync(
            dispatchRecord,
            "TestUser"
            );

            return RedirectToAction("History", new { AVNumber = model.Avnumber, IsolateId = model.DispatchIsolateId });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid DispatchId, string LastModified, Guid IsolateId, string AVNumber)
        {
            string UserName = "Test User";

            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.Administrator))
            {
                throw new UnauthorizedAccessException("Not authorised to delete dispatch.");
            }
            
            await ValidateDeleteIsolateDispatch(DispatchId, IsolateId, AVNumber, ModelState);

            if(!ModelState.IsValid)
            {
                IEnumerable<IsolateDispatchInfoDTO> isolateDispatchInfoDTOs = await _isolateDispatchService.GetDispatchesHistoryAsync(AVNumber, IsolateId);

                var dispatchHistoryRecords = _mapper.Map<IEnumerable<IsolateDispatchHistory>>(isolateDispatchInfoDTOs);

                IsolateDispatchHistoryViewModel? viewModel = null;
                if (dispatchHistoryRecords != null && dispatchHistoryRecords.Any())
                {
                    viewModel = new IsolateDispatchHistoryViewModel
                    {
                        IsolateId = IsolateId,
                        Nomenclature = dispatchHistoryRecords.First().Nomenclature,
                        DispatchHistoryRecords = dispatchHistoryRecords
                    };
                }

                return View("History",viewModel);
            }
            
            await _isolateDispatchService.DeleteDispatchAsync(
                DispatchId,
                Convert.FromBase64String(LastModified),
                UserName
            );

            return RedirectToAction("History", new { AVNumber = AVNumber, IsolateId = IsolateId });
        }

        [HttpGet]
        public async Task<IActionResult> Confirmation(Guid Isolate)
        {
            if (Isolate == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest("Invalid Isolate ID.");
            }

            var result = await _isolateDispatchService.GetDispatcheConfirmationAsync(Isolate);

            var dislist = _mapper.Map<IEnumerable<IsolateDispatchHistory>>(result.IsolateDispatchDetails);

            var model = new IsolateDispatchConfirmatioViewModel
            {
                DispatchConfirmationMessage = "Isolate dispatch completed successfully.",
                RemainingAliquots = result.IsolateDetails?.NoOfAliquots ?? 0,
                DispatchHistorys = dislist
            };

            return View(model);
        }

        public IActionResult CancelAction(string Source, string Avnumber)
        {
            string fromSource = Source == null ? "" : Source.ToLower();
            return fromSource switch
            {
                "search" => RedirectToAction("Search", "SearchRepository"),
                "summary" => RedirectToAction("Index", "SubmissionSamples", new { AVNumber = Avnumber }),
                _ => RedirectToAction("Create", "IsolateDispatch")
            };
        }

        private static List<string> ValidatDispatchForPageLoad(int NoOfAliquots, bool IsMixedIsolate, bool ValidToIssue, ref bool IsDispatchDisabled)
        {
            List<string> warningsMsgs = new List<string>();
            if (NoOfAliquots <= 1)
            {
                warningsMsgs.Add("Warning: This isolate cannot be dispatched as there are insufficient aliquots available.");
                IsDispatchDisabled = true;
            }

            if (IsMixedIsolate)
            {
                warningsMsgs.Add("Warning: This isolate is mixed, please double check for making a dispatch.");
            }

            if (!ValidToIssue)
            {
                warningsMsgs.Add("Warning: This isolate is marked as not valid for issue. Please keep this in mind before dispatching it.");
            }
            return warningsMsgs;
        }

        private static void ValidateAddIsolateDispatch(IsolateDispatchCreateViewModel dispatchModel, ModelStateDictionary modelState)
        {
            List<string> validationErrors = new List<string>();
            if (dispatchModel.NoOfAliquots.HasValue)
            {
                if (dispatchModel.NoOfAliquotsToBeDispatched >= dispatchModel.NoOfAliquots)
                {
                    validationErrors.Add("- Attempting to dispatch too many aliquots, must leave at least one aliquot in storage.");
                }
            }
            else
            {
                validationErrors.Add("- Isolate cannot be dispatched as there are no aliquots available.");
            }

            if (dispatchModel.DispatchedDate > DateTime.Now.Date)
            {
                validationErrors.Add("- Dispatched Date cannot be in the future.");
            }

            if (validationErrors.Count > 0)
            {
                modelState.AddModelError(string.Empty, "There are validation messages requiring your attention.");
                foreach (var error in validationErrors)
                {
                    modelState.AddModelError(string.Empty, error);
                }
            }
        }

        private async Task<List<SelectListItem>> GetViabilityDropdownList()
        {
            var viabilityLookup = await _lookupService.GetAllViabilityAsync();
            return viabilityLookup.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetWorkGroupsDropdownList()
        {
            var recepientLookup = await _lookupService.GetAllWorkGroupsAsync();
            return recepientLookup.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetStaffsDropdownList()
        {
            var dispatchedByLookup = await _lookupService.GetAllStaffAsync();
            return dispatchedByLookup.Select(f => new SelectListItem { Value = f.Id.ToString(), Text = f.Name }).ToList();
        }

        private async Task<bool> CheckFieldsVisibility(string AVNumber, Guid IsolateId)
        {
            bool isFieldVisible = true;
            var submission = await _submissionService.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            var isolate = await _isolatesService.GetIsolateByIsolateAndAVNumberAsync(AVNumber, IsolateId);
            if (submission != null && isolate != null)
            {
                var samplesDto = _sampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
                var sample = samplesDto.Result.FirstOrDefault(s => s.SampleId == isolate.IsolateSampleId);
                if (sample?.SampleTypeName == "FTA Cards" || sample?.SampleTypeName == "RNA")
                {
                    isFieldVisible = false;
                }
            }
            return isFieldVisible;
        }

        private async Task ValidateEditIsolateDispatch(IsolateDispatchEditViewModel dispatchModel, ModelStateDictionary modelState)
        {
            List<string> validationErrors = new List<string>();
            Guid isolateId = Guid.Empty;

            if (dispatchModel.NoOfAliquots>=0)
            {
                if (dispatchModel.NoOfAliquotsToBeDispatched >= dispatchModel.NoOfAliquots)
                {
                    validationErrors.Add("Attempting to dispatch too many aliquots, must leave at least one aliquot in storage.");
                }
            }
            else
            {
                validationErrors.Add("Isolate cannot be dispatched as there are no aliquots available.");
            }

            if (dispatchModel.DispatchedDate > DateTime.Now.Date)
            {
                validationErrors.Add("Dispatched Date cannot be in the future.");
            }

            if (dispatchModel.DispatchId == Guid.Empty)
            {
                validationErrors.Add("ID is required.");
            }
  
            if (dispatchModel.DispatchIsolateId.HasValue)
            {
                isolateId = (Guid)dispatchModel.DispatchIsolateId;
            }

            var dispatches = await _isolateDispatchService.GetDispatchesHistoryAsync(dispatchModel.Avnumber!, isolateId);

            int count = dispatches.Count(u => u.DispatchId == dispatchModel.DispatchId);

            if (count == 0)
            {
                ModelState.AddModelError(string.Empty, "The item does not exist");
            }

            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    modelState.AddModelError(string.Empty, error);
                }
            }
        }

        private async Task ValidateDeleteIsolateDispatch(Guid dispatchId, Guid IsolateId, string AVNumber, ModelStateDictionary modelState)
        {
            List<string> validationErrors = new List<string>();
             
            if (dispatchId == Guid.Empty)
            {
                validationErrors.Add("ID is required.");
            }

            var dispatches = await _isolateDispatchService.GetDispatchesHistoryAsync(AVNumber!, IsolateId);

            int count = dispatches.Count(u => u.DispatchId == dispatchId);

            if (count == 0)
            {
                ModelState.AddModelError(string.Empty, "The item does not exist");
            }

            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    modelState.AddModelError(string.Empty, error);
                }
            }
        }
    }
}
