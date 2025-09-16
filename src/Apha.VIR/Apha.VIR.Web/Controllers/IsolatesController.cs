using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class IsolatesController : Controller
    {
        private readonly IIsolatesService _isolatesService;
        private readonly ILookupService _lookupService;
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly ISubmissionService _submissionService;
        private readonly ISampleService _sampleService;
        private readonly IMapper _mapper;
        private const string IndexActionName = "Index";

        public IsolatesController(IIsolatesService isolatesService,
             ILookupService lookupService,
             IIsolateViabilityService isolateViabilityService,
             ISubmissionService submissionService,
             ISampleService sampleService,
        IMapper mapper)
        {
            _isolatesService = isolatesService;
            _lookupService = lookupService;
            _isolateViabilityService = isolateViabilityService;
            _submissionService = submissionService;
            _sampleService = sampleService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager + "," + AppRoleConstant.IsolateViewer)]
        public async Task<IActionResult> ViewIsolateDetails(Guid IsolateId)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }
            var result = await _isolatesService.GetIsolateFullDetailsAsync(IsolateId);
            var isolateDetails = _mapper.Map<IsolateDetailsViewModel>(result);
            if (isolateDetails != null && isolateDetails.IsolateDetails != null)
            {
                if (AuthorisationUtil.CanGetItem(AppRoleConstant.Administrator))
                {
                    isolateDetails.IsolateDetails.IsEditHistory = true;
                }
                if (AuthorisationUtil.CanGetItem(AppRoleConstant.IsolateManager))
                {
                    isolateDetails.IsolateDetails.IsFullViewIsolateDetails = true;
                }
            }
            return View("IsolateDetails", isolateDetails);
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Create(string AVNumber, Guid SampleId)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var isolateCreateModel = new IsolateAddEditViewModel
            {
                AVNumber = AVNumber,
                IsolateSampleId = SampleId,
                VirusFamilyList = await GetVirusFamiliesDropdownList(),
                VirusTypeList = await GetVirusTypesDropdownList(null),
                IsolationMethodList = await GetIsolationMethodsDropdownList(),
                FreezerList = await GetFreezerDropdownList(),
                TrayList = await GetTraysDropdownList(null),
                ViabilityList = await GetViabilityDropdownList(),
                StaffList = await GetCheckedByDropdownList(),
                NoOfAliquots = 4
            };

            var submission = await _submissionService.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            if (submission != null)
            {
                isolateCreateModel.YearOfIsolation = submission.DateSubmissionReceived!.Value.Year;

                var samplesDto = _sampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
                var sample = samplesDto.Result.FirstOrDefault(s => s.SampleId == SampleId);
                if (sample?.SampleTypeName == "FTA Cards" || sample?.SampleTypeName == "RNA")
                {
                    isolateCreateModel.IsChkDetection = true;
                }
                isolateCreateModel.Nomenclature = $"[Virus Type]/" +
                    $"{(string.IsNullOrEmpty(sample?.HostBreedName) ? sample?.HostSpeciesName : sample.HostBreedName)}/" +
                    $"{submission.CountryOfOriginName}/{sample?.SenderReferenceNumber}/[Year of Isolation]";
            }            

            return View(isolateCreateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(IsolateAddEditViewModel isolateModel)
        {
            if (!AuthorisationUtil.CanAddItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to create isolate.");
            }

            await ValidateIsolateDetails(isolateModel, ModelState);

            if (!ModelState.IsValid)
            {
                isolateModel.VirusFamilyList = await GetVirusFamiliesDropdownList();
                isolateModel.VirusTypeList = await GetVirusTypesDropdownList(isolateModel.Family);
                isolateModel.IsolationMethodList = await GetIsolationMethodsDropdownList();
                isolateModel.FreezerList = await GetFreezerDropdownList();
                isolateModel.TrayList = await GetTraysDropdownList(isolateModel.Freezer);
                isolateModel.ViabilityList = await GetViabilityDropdownList();
                isolateModel.StaffList = await GetCheckedByDropdownList();
                return View(isolateModel);
            }

            isolateModel.CreatedBy = "testuser";

            var isolateDto = _mapper.Map<IsolateDTO>(isolateModel);
            isolateModel.IsolateId = await _isolatesService.AddIsolateDetailsAsync(isolateDto);

            if (isolateModel.IsViabilityInsert)
            {
                var isolateViability = _mapper.Map<IsolateViabilityInfoDTO>(isolateModel);
                await _isolateViabilityService.AddIsolateViabilityAsync(isolateViability, isolateModel.CreatedBy);
            }

            if (isolateModel.ActionType == "SaveAndContinue")
            {
                return RedirectToAction("Edit", "IsolateCharacteristics", new { AVNumber = isolateModel.AVNumber, IsolateId = isolateModel.IsolateId });
            }
            else
            {
                return RedirectToAction(IndexActionName, "SubmissionSamples", new { AVNumber = isolateModel.AVNumber });
            }
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Edit(string AVNumber, Guid SampleId, Guid IsolateId)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }

            var isolate = await _isolatesService.GetIsolateByIsolateAndAVNumberAsync(AVNumber, IsolateId);
            var isolateModel = _mapper.Map<IsolateAddEditViewModel>(isolate);
            isolateModel.AVNumber = AVNumber;
            isolateModel.VirusFamilyList = await GetVirusFamiliesDropdownList();
            isolateModel.VirusTypeList = await GetVirusTypesDropdownList(isolate.Family);
            isolateModel.IsolationMethodList = await GetIsolationMethodsDropdownList();
            isolateModel.FreezerList = await GetFreezerDropdownList();
            isolateModel.TrayList = await GetTraysDropdownList(isolate.Freezer);
            isolateModel.ViabilityList = await GetViabilityDropdownList();
            isolateModel.StaffList = await GetCheckedByDropdownList();
            var viability = await _isolateViabilityService.GetLastViabilityByIsolateAsync(IsolateId);
            if (viability != null)
            {
                isolateModel.PreviousViable = viability.Viable;
                isolateModel.PreviousDateChecked = viability.DateChecked;
                isolateModel.PreviousCheckedBy = viability.CheckedById;
            }

            var submission = await _submissionService.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            if (submission != null)
            {
                var samplesDto = _sampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
                var sample = samplesDto.Result.FirstOrDefault(s => s.SampleId == SampleId);
                if (sample?.SampleTypeName == "FTA Cards" || sample?.SampleTypeName == "RNA")
                {
                    isolateModel.IsChkDetection = true;
                }
            }

            return View(isolateModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IsolateAddEditViewModel isolateModel)
        {
            if (!AuthorisationUtil.CanEditItem(AppRoleConstant.IsolateManager))
            {
                throw new UnauthorizedAccessException("Not authorised to modify isolate.");
            }

            await ValidateIsolateDetails(isolateModel, ModelState);

            if (!ModelState.IsValid)
            {
                isolateModel.VirusFamilyList = await GetVirusFamiliesDropdownList();
                isolateModel.VirusTypeList = await GetVirusTypesDropdownList(isolateModel.Family);
                isolateModel.IsolationMethodList = await GetIsolationMethodsDropdownList();
                isolateModel.FreezerList = await GetFreezerDropdownList();
                isolateModel.TrayList = await GetTraysDropdownList(isolateModel.Freezer);
                isolateModel.ViabilityList = await GetViabilityDropdownList();
                isolateModel.StaffList = await GetCheckedByDropdownList();
                return View(isolateModel);
            }

            isolateModel.CreatedBy = "testuser";
            var isolateDto = _mapper.Map<IsolateDTO>(isolateModel);
            await _isolatesService.UpdateIsolateDetailsAsync(isolateDto);

            if (isolateModel.IsViabilityInsert)
            {
                var isolateViability = _mapper.Map<IsolateViabilityInfoDTO>(isolateModel);
                await _isolateViabilityService.AddIsolateViabilityAsync(isolateViability, isolateModel.CreatedBy);
            }

            if (isolateModel.ActionType == "SaveAndContinue")
            {
                return RedirectToAction(IndexActionName, "IsolateCharacteristics", new { AVNumber = isolateModel.AVNumber, IsolateId = isolateModel.IsolateId });
            }
            else
            {
                return RedirectToAction(IndexActionName, "SubmissionSamples", new { AVNumber = isolateModel.AVNumber });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetVirusTypesByVirusFamily(Guid? virusFamilyId)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                throw new UnauthorizedAccessException("User not authorised to retrieve this list");
            }
            if (!ModelState.IsValid)
                return Json(new List<CustomSelectListItem>());

            return Json(await GetVirusTypesDropdownList(virusFamilyId));
        }

        [HttpGet]
        public async Task<JsonResult> GetTraysByFeezer(Guid? freezer)
        {
            if (!AuthorisationUtil.IsUserInAnyRole())
            {
                throw new UnauthorizedAccessException("User not authorised to retrieve this list");
            }
            if (!ModelState.IsValid)
                return Json(new List<SelectListItem>());

            return Json(await GetTraysDropdownList(freezer));
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<string> GenerateNomenclature(string avNumber, Guid sampleId, string virusType, string yearOfIsolation)
        {
            if (!ModelState.IsValid)
            {
                ArgumentNullException.ThrowIfNull(avNumber);
                ArgumentNullException.ThrowIfNull(sampleId);
            }
            string nomenclature = await _isolatesService.GenerateNomenclature(avNumber, sampleId, virusType, yearOfIsolation);
            return nomenclature;
        }

        private async Task<List<SelectListItem>> GetVirusFamiliesDropdownList()
        {
            var virusFamiliesDto = await _lookupService.GetAllVirusFamiliesAsync();
            return virusFamiliesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }

        private async Task<List<CustomSelectListItem>> GetVirusTypesDropdownList(Guid? virusFamilyId)
        {
            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(virusFamilyId))
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesAsync();
                return virusTypesDto.Select(t => new CustomSelectListItem { Value = t.Id.ToString(), Text = t.Name, DataType = t.AlternateName ?? "" }).ToList();
            }
            else
            {
                var virusTypesDto = await _lookupService.GetAllVirusTypesByParentAsync(virusFamilyId);
                return virusTypesDto.Select(t => new CustomSelectListItem { Value = t.Id.ToString(), Text = t.Name, DataType = t.AlternateName ?? "" }).ToList();
            }
        }

        private async Task<List<SelectListItem>> GetIsolationMethodsDropdownList()
        {
            var virusFamiliesDto = await _lookupService.GetAllIsolationMethodsAsync();
            return virusFamiliesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetFreezerDropdownList()
        {
            var freezersDto = await _lookupService.GetAllFreezerAsync();
            return freezersDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetTraysDropdownList(Guid? freezer)
        {
            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(freezer))
            {
                var virusTypesDto = await _lookupService.GetAllTraysAsync();
                return virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
            else
            {
                var virusTypesDto = await _lookupService.GetAllTraysByParentAsync(freezer);
                return virusTypesDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
            }
        }

        private async Task<List<SelectListItem>> GetViabilityDropdownList()
        {
            var viabilityDto = await _lookupService.GetAllViabilityAsync();
            return viabilityDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }

        private async Task<List<SelectListItem>> GetCheckedByDropdownList()
        {
            var viabilityDto = await _lookupService.GetAllStaffAsync();
            return viabilityDto.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList();
        }

        private async Task ValidateIsolateDetails(IsolateAddEditViewModel isolateModel, ModelStateDictionary modelState)
        {
            List<string> validationErrors = new List<string>();
            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.Family))
            {
                validationErrors.Add("- Isolate must have a Virus Family selected.");
            }

            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.Type))
            {
                validationErrors.Add("- Isolate must have a Virus Type selected.");
            }

            if (isolateModel.YearOfIsolation.HasValue && isolateModel.YearOfIsolation > DateTime.Now.Year)
            {
                validationErrors.Add("- Year of Isolation cannot be in the future.");
            }

            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.Freezer))
            {
                validationErrors.Add("- Isolate must have a Freezer selected.");
            }

            if (Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.Tray))
            {
                validationErrors.Add("- Isolate must have a Freezer Tray selected.");
            }

            if (!Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.Viable) || isolateModel.DateChecked.HasValue || !Apha.VIR.Web.Models.SearchCriteria.IsNullOrEmptyGuid(isolateModel.CheckedBy))
            {
                bool IsViabilityInsert = true;
                IsViabilityInsert = await ValidateViabilityDetails(isolateModel.IsolateId, isolateModel.Viable, isolateModel.DateChecked, isolateModel.CheckedBy, validationErrors, IsViabilityInsert);
                isolateModel.IsViabilityInsert = IsViabilityInsert;
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

        private async Task<bool> ValidateViabilityDetails(Guid? IsolateId, Guid? Viability, DateTime? DateChecked, Guid? CheckedBy, List<string> ValidationErrors, bool IsViabilityInsert)
        {
            if (SearchCriteria.IsNullOrEmptyGuid(Viability))
            {
                ValidationErrors.Add("- Viability Status of the isolate must be recorded.");
                IsViabilityInsert = false;
            }

            if (DateChecked.HasValue)
            {
                if (DateChecked.Value > DateTime.Now)
                {
                    ValidationErrors.Add("- Date viability checked cannot be in the future.");
                    IsViabilityInsert = false;
                }
                else
                {
                    var isolateViabilities = await _isolateViabilityService.GetViabilityByIsolateIdAsync(IsolateId ?? Guid.Empty);
                    if (isolateViabilities != null && isolateViabilities.Any(v => v.DateChecked == DateChecked))
                    {
                        ValidationErrors.Add("- There is already a viability for this isolate on this date.  (This can be edited via the search screen).");
                        IsViabilityInsert = false;
                    }
                }
            }
            else
            {
                ValidationErrors.Add("- Date viability checked must be entered.");
                IsViabilityInsert = false;
            }

            if (SearchCriteria.IsNullOrEmptyGuid(CheckedBy))
            {
                ValidationErrors.Add("- Viability Checked By must be recorded.");
                IsViabilityInsert = false;
            }

            return IsViabilityInsert;
        }              
    }
}
