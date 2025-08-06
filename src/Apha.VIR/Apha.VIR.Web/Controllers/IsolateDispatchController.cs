using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateDispatchController : Controller
    {
        private readonly IIsolateDispatchService _isolateDispatchService;
        private readonly IMapper _mapper;

        public IsolateDispatchController(IIsolateDispatchService isolateDispatchService, IMapper mapper)
        {
            _isolateDispatchService = isolateDispatchService;
            _mapper = mapper;
        }

        [HttpGet]
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
                    Nomenclature = dispatchHistoryRecords.First().Nomenclature,
                    DispatchHistoryRecords = dispatchHistoryRecords
                };
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid DispatchId, string LastModified, Guid IsolateId, string AVNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (DispatchId == Guid.Empty)
            {
                return BadRequest("Invalid Dispatch ID.");
            }
            if (string.IsNullOrWhiteSpace(LastModified))
            {
                return BadRequest("Last Modified cannot be empty.");
            }

            string UserName = "Test User";
            await _isolateDispatchService.DeleteDispatchAsync(
                DispatchId,
                Convert.FromBase64String(LastModified),
                UserName
            );

            return RedirectToAction("History", new { AVNumber = AVNumber, IsolateId = IsolateId });
        }

        [HttpGet]
        public IActionResult Confirmation(Guid Isolate)
        {
            if (Isolate == Guid.Empty || !ModelState.IsValid)
            {
                return BadRequest("Invalid Isolate ID.");
            }
          
            var result = _isolateDispatchService.GetDispatcheConfirmationAsync(Isolate).Result;
            
            var dislist = _mapper.Map<IEnumerable<IsolateDispatchHistory>>(result.IsolateDispatchDetails);

            var model = new IsolateDispatchConfirmatioViewModel
            {
                DispatchConfirmationMessage = "Isolate dispatch completed successfully.",
                RemainingAliquots = result.IsolateDetails?.NoOfAliquots ?? 0,
                DispatchHistorys = dislist
            };

            return View(model);
        }
    }
}
