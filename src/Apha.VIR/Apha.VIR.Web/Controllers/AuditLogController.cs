using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IMapper _mapper;

        public AuditLogController(IAuditLogService auditLogService, IMapper mapper)
        {
            _auditLogService = auditLogService;
            _mapper = mapper;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SearchAudit(AuditLogSearchModel model)
        {
            //ModelState.Remove(nameof(model.ReportData));

            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}

            var result = await _auditLogService.GetCharacteristicsLogsAsync(model.AVNumber, model.DateTimeFrom, model.DateTimeTo,model.UserId);

            //var reportData = _mapper.Map<IEnumerable<IsolateDispatchReportModel>>(result);

            var viewModel = new AuditLogViewModel
            {
                AVNumber = "AA"
            };

            return View("IsolateDispatchReport", viewModel);
        }
    }
}
