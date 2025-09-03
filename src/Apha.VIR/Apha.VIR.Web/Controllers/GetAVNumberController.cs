using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class GetAVNumberController : Controller
    {
        private readonly ISubmissionService _submissionService;        

        public GetAVNumberController(ISubmissionService submissionService)
        {
            _submissionService = submissionService;    
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var avNumbers = await _submissionService.GetLatestSubmissions();
            var avNumberModel = new GetAVNumberViewModel
            {
                LastAVNumbers = avNumbers.ToList()
            };

            return View(avNumberModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(string AVNumber)
        {
            if (!AVNumberUtil.AVNumberIsValidPotentially(AVNumber)) 
            {
                ModelState.AddModelError("AVNumber", "Please check the format of this number.");
            }

            if (!ModelState.IsValid)
            {
                var avNumbers = await _submissionService.GetLatestSubmissions();
                var avNumberModel = new GetAVNumberViewModel
                {
                    AVNumber = AVNumber,
                    LastAVNumbers = avNumbers.ToList()
                };
                return View(avNumberModel);
            }

            AVNumber = AVNumberUtil.AVNumberFormatted(AVNumber);
            var isExistInVIR = await _submissionService.AVNumberExistsInVirAsync(AVNumber);
            if (isExistInVIR) 
            {
                return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = AVNumber });
            }
            else
            {
                return RedirectToAction("Index", "SubmissionDetails", new { AVNumber = AVNumber });
            }              
        }
    }
}
