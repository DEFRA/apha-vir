using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Apha.VIR.Application.Services;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> History(string AVNumber, Guid IsolateId)
        {
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
        public async Task<IActionResult> Delete(Guid DispatchId, string LastModified)
        {
            string UserName = "Test User";
            await _isolateDispatchService.DeleteDispatchAsync(
                DispatchId,
                Convert.FromBase64String(LastModified),
                UserName
            );
            
            return RedirectToAction("History");
        }




    }
}
