using System.ComponentModel.DataAnnotations;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.AuditLog;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var viewModel = new AuditLogViewModel
            {
                ShowErrorSummary = false,
            };
            return View("Index", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SearchAudit(AuditLogSearchModel searchCriteria)
        {
            var showerrorSummary = false;

            if (ModelState.IsValid)
            {
                ValidateSearchModel(searchCriteria, ModelState);
                showerrorSummary = true;
            }

            if (!ModelState.IsValid)
            {
                var model = new AuditLogViewModel
                {
                    AVNumber = searchCriteria.AVNumber,
                    DateTimeFrom = searchCriteria.DateTimeFrom,
                    DateTimeTo = searchCriteria.DateTimeTo,
                    UserId = searchCriteria.UserId,
                    ShowErrorSummary = showerrorSummary
                    
                };
                return View("Index", model);
            }

            FormateSearchCriteria(searchCriteria);

            var result1 = await _auditLogService.GetCharacteristicsLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
                searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData1 = _mapper.Map<IEnumerable<AuditCharacteristicsLogModel>>(result1);

            var result = await _auditLogService.GetSubmissionLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
               searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData = _mapper.Map<IEnumerable<AuditSubmissionLogModel>>(result);

            var result2 = await _auditLogService.GetSamplLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
              searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData2 = _mapper.Map<IEnumerable<AuditSampleLogModel>>(result2);

            var result3 = await _auditLogService.GetDispatchLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
              searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData3 = _mapper.Map<IEnumerable<AuditDispatchLogModel>>(result3);

            var result4 = await _auditLogService.GetIsolateViabilityLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
             searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData4 = _mapper.Map<IEnumerable<AuditIsolateViabilityLogModel>>(result4);

            var result5 = await _auditLogService.GetIsolatLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
             searchCriteria.DateTimeTo, searchCriteria.UserId!);

            var reportData5 = _mapper.Map<IEnumerable<AuditIsolateLogModel>>(result5);


            var viewModel = new AuditLogViewModel
            {
                AVNumber = searchCriteria.AVNumber,
                DateTimeFrom = searchCriteria.DateTimeFrom,
                DateTimeTo = searchCriteria.DateTimeTo,
                UserId = searchCriteria.UserId,
                ShowErrorSummary = false,
                SampleLogs = reportData2.ToList(),
                SubmissionLogs = reportData.ToList(),
                CharacteristicsLogs = reportData1.ToList(),
                DispatchLogs = reportData3.ToList(),
                IsolateViabilityLogs = reportData4.ToList(),
                IsolateLogs = reportData5.ToList(),
            };

            return View("Index", viewModel);
        }

        private static void ValidateSearchModel(AuditLogSearchModel criteria, ModelStateDictionary modelState)
        {
            var context = new ValidationContext(criteria);
            var validationResult = criteria.Validate(context);
            foreach (var validation in validationResult)
            {
                foreach (var memberName in validation.MemberNames.Any() ? validation.MemberNames : new[] { "" })
                {
                    if (validation.ErrorMessage != null)
                        modelState.AddModelError(memberName, validation.ErrorMessage);
                }
            }
        }
        
        private static void FormateSearchCriteria(AuditLogSearchModel searchCriteria)
        {
           searchCriteria.AVNumber = Apha.VIR.Web.Models.Submission.AVNumberFormatted(searchCriteria.AVNumber!); ;

            if (searchCriteria.DateTimeFrom == null)
            {
                searchCriteria.DateTimeFrom = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Local);
            }

            if (searchCriteria.DateTimeTo == null)
            {
                searchCriteria.DateTimeTo = new DateTime(2015, 12, 31, 0, 0, 0, DateTimeKind.Local);
            }
            else
            {
                if (searchCriteria.DateTimeTo.Value.Hour == 0)
                {
                    searchCriteria.DateTimeTo = searchCriteria.DateTimeTo.Value.AddHours(24);
                }
            }

            if (string.IsNullOrEmpty(searchCriteria.UserId))
            {
                searchCriteria.UserId = "%";
            }
            else
            {
                searchCriteria.UserId = $"%{searchCriteria.UserId}%";
            }
        }
    }
}
