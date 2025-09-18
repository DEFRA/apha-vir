using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class SubmissionSamplesController : Controller
    {
        private readonly ISubmissionService _submissionService;
        private readonly ISampleService _sampleService;
        private readonly IIsolatesService _isolatesService;      
        private readonly IIsolateDispatchService _isolatesDispatchService;
        private readonly IMapper _mapper;

        public SubmissionSamplesController(ISubmissionService submissionService, 
            ISampleService sampleService, 
            IIsolatesService isolatesService,
            IIsolateDispatchService isolatesDispatchService,
            IMapper mapper)
        {
            _submissionService = submissionService;
            _sampleService = sampleService;
            _isolatesService = isolatesService;
            _isolatesDispatchService = isolatesDispatchService;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = AppRoleConstant.IsolateManager)]
        public async Task<IActionResult> Index(string AVNumber)
        {
            var isExistInVir = await _submissionService.AVNumberExistsInVirAsync(AVNumber);
            if (!isExistInVir)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            bool hasIsolates = false;
            bool hasDetections = false;
            string isolateGridHeader = string.Empty;

            var submission = await _submissionService.GetSubmissionDetailsByAVNumberAsync(AVNumber);
            var samples = await _sampleService.GetSamplesBySubmissionIdAsync(submission.SubmissionId);
            List<SubmissionSamplesModel> sampleList = _mapper.Map<List<SubmissionSamplesModel>>(samples);
            var isolates = await _isolatesService.GetIsolateInfoByAVNumberAsync(AVNumber);
            List<SubmissionIsolatesModel> submissionIsolates = _mapper.Map<List<SubmissionIsolatesModel>>(isolates);

            foreach (var subIsolate in submissionIsolates) 
            {
                subIsolate.IsUniqueNomenclature = await _isolatesService.UniqueNomenclatureAsync(subIsolate.IsolateId, subIsolate.FamilyName, subIsolate.Type);
            }

            CheckDetectionForSampleGrid(sampleList, hasIsolates, hasDetections, ref isolateGridHeader);

            if (AuthorisationUtil.CanDeleteItem(AppRoleConstant.IsolateDeleter))
            {
                sampleList.ForEach(s => s.IsDeleteEnabled = true);
                submissionIsolates.ForEach(i => i.IsDeleteEnabled = true);
            }

            if (AuthorisationUtil.CanAddItem(AppRoleConstant.IsolateManager))
            {
                submissionIsolates.ForEach(i => i.IsDispatchEnabled = true);
            }

            var viewModel = new SubmissionSamplesViewModel
            {
                SubmissionId = submission.SubmissionId,
                AVNumber = AVNumber,
                SendersReferenceNumber = submission.SendersReferenceNumber,
                SenderOrganisation = submission.SenderOrganisation,
                CountryOfOriginName = submission.CountryOfOriginName,                
                IsolatesGridHeader = isolateGridHeader,
                LastModified = submission.LastModified,
                IsLetterRequired = !isolates.Any() || !samples.Any(),
                ShowDeleteLink = AuthorisationUtil.CanDeleteItem(AppRoleConstant.IsolateDeleter),
                Samples = sampleList,
                Isolates = submissionIsolates,
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SubmissionDelete(string AVNumber, Guid SubmissionId, byte[] LastModified)
        {
            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.IsolateDeleter))
            {
                throw new UnauthorizedAccessException("Not authorised to delete submission.");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }
            string userId = "testUser";
            var samples = await _sampleService.GetSamplesBySubmissionIdAsync(SubmissionId);            
            var isolates = await _isolatesService.GetIsolateInfoByAVNumberAsync(AVNumber);
            if (samples.Any() || isolates.Any())
            {
                return Json(new { success = false, message = "Unable to delete this Submission, it still has Isolates or Samples." });
            }

            await _submissionService.DeleteSubmissionAsync(SubmissionId, userId, LastModified);
            return Json(new { success = true, message = "Submission deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> SampleDelete(string AVNumber, Guid SampleId, byte[] LastModified)
        {
            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.IsolateDeleter))
            {
                throw new UnauthorizedAccessException("Not authorised to delete sample.");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }
            string userId = "testUser";
            var isolates = await _isolatesService.GetIsolateInfoByAVNumberAsync(AVNumber);
            var isolateSample = isolates.FirstOrDefault(i => i.IsolateSampleId == SampleId);
            if (isolateSample != null)
            {
                return Json(new { success = false, message = "You cannot delete this Sample as it still has Isolates recorded against it." });
            }

            await _sampleService.DeleteSampleAsync(SampleId, userId, LastModified);
            return Json(new { success = true, message = "Sample deleted successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> IsolateDelete(string AVNumber, Guid IsolateId, byte[] LastModified)
        {
            if (!AuthorisationUtil.CanDeleteItem(AppRoleConstant.IsolateDeleter))
            {
                throw new UnauthorizedAccessException("Not authorised to delete isolate.");
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid parameters.");
                return BadRequest(ModelState);
            }
            string userId = "testUser";
            var isolates = await _isolatesService.GetIsolateInfoByAVNumberAsync(AVNumber);
            var isolate = isolates.FirstOrDefault(i => i.IsolateId == IsolateId);
            if (isolate != null)
            {
                var isolateDispatchCount = await _isolatesDispatchService.GetIsolateDispatchRecordCountAsync(isolate.IsolateId);
                if (isolateDispatchCount > 0) 
                {
                    return Json(new { success = false, message = "Isolate cannot be deleted as it has one or more dispatches recorded against it." });
                }                
            }

            await _isolatesService.DeleteIsolateAsync(IsolateId, userId, LastModified);
            return Json(new { success = true, message = "Isolate deleted successfully." });
        }

        private static void CheckDetectionForSampleGrid(List<SubmissionSamplesModel> sampleList, bool hasIsolates, bool hasDetections, ref string isolateGridHeader)
        {
            foreach(var sample in sampleList)
            {
                if(sample.SampleTypeName == "FTA Cards" || sample.SampleTypeName == "RNA")
                {
                    sample.IsDetection = true;
                    hasDetections = true;
                }
                else
                {
                    hasIsolates = true;
                }                
            }

            if (hasIsolates && hasDetections)
            {
                isolateGridHeader = "Isolates / Detections for this submission";
            }
            else if (hasDetections)
            {
                isolateGridHeader = "Detections for this submission";
            }
            else if (hasIsolates)
            {
                isolateGridHeader = "Isolates for this submission";
            }
            else
            {
                isolateGridHeader = "Isolates / Detections for this submission";
            }
        }
    }
}
