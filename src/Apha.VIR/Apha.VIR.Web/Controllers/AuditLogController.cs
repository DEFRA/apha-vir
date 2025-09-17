using System.ComponentModel.DataAnnotations;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models.AuditLog;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace Apha.VIR.Web.Controllers
{
    [Authorize(Roles = AppRoleConstant.Administrator)]
    [Route("AuditLog")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public AuditLogController(IAuditLogService auditLogService, ICacheService cacheService, IMapper mapper)
        {
            _auditLogService = auditLogService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var viewModel = new AuditLogViewModel
            {
                ShowErrorSummary = false,
                ShowDefaultView = true
            };
            return View("AuditLog", viewModel);
        }

        [HttpPost("SearchAudit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchAudit(AuditLogSearchModel searchCriteria, bool IsNewSearch = false)
        {
            var showerrorSummary = false;
            if (IsNewSearch)
            {
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
                    return View("AuditLog", model);
                }

                FormateSearchCriteria(searchCriteria);

                await _cacheService.RemoveCacheValueAsync("SearchCriteria");
                await _cacheService.SetCacheValueAsync("SearchCriteria", JsonConvert.SerializeObject(searchCriteria));

                var result = await _auditLogService.GetSubmissionLogsAsync(searchCriteria.AVNumber, searchCriteria.DateTimeFrom,
                   searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditSubmissionLogModel>>(result);

                var viewModel = new AuditLogViewModel
                {
                    AVNumber = searchCriteria.AVNumber,
                    DateTimeFrom = searchCriteria.DateTimeFrom,
                    DateTimeTo = searchCriteria.DateTimeTo,
                    UserId = searchCriteria.UserId,
                    ShowErrorSummary = false,
                    IsNewSearch = IsNewSearch,
                    SubmissionLogs = reportData.ToList(),
                };

                return View("AuditLog", viewModel);
            }
            else
            {
                var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");
                ModelState.Remove("AVNumber");
                if (!String.IsNullOrEmpty(criteriaString))
                {
                    var IsolatesearchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                    var result = await _auditLogService.GetIsolatLogsAsync(IsolatesearchCriteria.AVNumber,
                        IsolatesearchCriteria.DateTimeFrom, IsolatesearchCriteria.DateTimeTo, IsolatesearchCriteria.UserId!);

                    var reportData = _mapper.Map<IEnumerable<AuditIsolateLogModel>>(result);

                    var viewModel = new AuditLogViewModel
                    {
                        AVNumber = IsolatesearchCriteria.AVNumber,
                        DateTimeFrom = IsolatesearchCriteria.DateTimeFrom,
                        DateTimeTo = IsolatesearchCriteria.DateTimeTo,
                        UserId = IsolatesearchCriteria.UserId == "%" ? string.Empty : IsolatesearchCriteria.UserId,
                        ShowErrorSummary = false,
                        IsNewSearch = IsNewSearch,
                        IsolateLogs = reportData.ToList(),
                    };

                    return View("AuditLog", viewModel);
                }
                else
                {
                    var viewModel = new AuditLogViewModel
                    {
                        ShowErrorSummary = false,
                        ShowDefaultView = true
                    };
                    return View("AuditLog", viewModel);
                }
            }
        }

        [HttpGet("IsolateLog")]
        public async Task<IActionResult> GetIsolateLogDetail(Guid logid, string AvNumber)
        {
            if (!ModelState.IsValid)
            {
                var model = new AuditIsolateLogDetailsViewModel
                {
                    AVNumber = AvNumber,
                };
                return View("IsolateAuditLogDetail", model);
            }

            var result = await _auditLogService.GetIsolatLogDetailAsync(logid);

            var viewModel = _mapper.Map<AuditIsolateLogDetailsViewModel>(result);

            viewModel.AVNumber = AvNumber;

            return View("IsolateAuditLogDetail", viewModel);
        }

        [HttpPost("GetAuditLogs")]
        public async Task<IActionResult> GetAuditLogs(string requesttype)
        {
            switch (requesttype.ToLower())
            {
                case "submission":
                    return await GetSubmissionAuditLogs();
                case "sample":
                    return await GetSampleAuditLogs();
                case "isolate":
                    return await GetIsolateAuditLogs();
                case "characteristics":
                    return await GetCharacteristicsAuditLogs();
                case "dispatch":
                    return await GetDispatchAuditLogs();
                case "viability":
                    return await GetViabilityAuditLogs();
                default:
                    return await GetSubmissionAuditLogs(); ;
            }
        }

        private async Task<IActionResult> GetSubmissionAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetSubmissionLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditSubmissionLogModel>>(result);
                return PartialView("_SubmissionAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_SubmissionAuditLogResults", new AuditSubmissionLogModel());
            }
        }

        private async Task<IActionResult> GetSampleAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetSamplLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditSampleLogModel>>(result);

                return PartialView("_SampleAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_SampleAuditLogResults", new AuditSampleLogModel());
            }
        }

        private async Task<IActionResult> GetIsolateAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetIsolatLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditIsolateLogModel>>(result);

                return PartialView("_IsolateAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_IsolateAuditLogResults", new AuditIsolateLogModel());
            }
        }

        private async Task<IActionResult> GetDispatchAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetDispatchLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditDispatchLogModel>>(result);

                return PartialView("_DispatchAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_DispatchAuditLogResults", new AuditDispatchLogModel());
            }
        }

        private async Task<IActionResult> GetViabilityAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetIsolateViabilityLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditIsolateViabilityLogModel>>(result);

                return PartialView("_IsolateViabilityAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_IsolateViabilityAuditLogResults", new AuditIsolateViabilityLogModel());
            }
        }

        private async Task<IActionResult> GetCharacteristicsAuditLogs()
        {
            var criteriaString = await _cacheService.GetCacheValueAsync<string>("SearchCriteria");

            if (!String.IsNullOrEmpty(criteriaString))
            {
                var searchCriteria = JsonConvert.DeserializeObject<AuditLogSearchModel>(criteriaString) ?? new AuditLogSearchModel();

                var result = await _auditLogService.GetCharacteristicsLogsAsync(searchCriteria.AVNumber,
                    searchCriteria.DateTimeFrom, searchCriteria.DateTimeTo, searchCriteria.UserId!);

                var reportData = _mapper.Map<IEnumerable<AuditCharacteristicsLogModel>>(result);

                return PartialView("_CharacteristicsAuditLogResults", reportData);
            }
            else
            {
                return PartialView("_CharacteristicsAuditLogResults", new AuditCharacteristicsLogModel());
            }
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
            searchCriteria.AVNumber = AVNumberUtil.AVNumberFormatted(searchCriteria.AVNumber!); ;

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
