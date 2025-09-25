using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    [Authorize(Roles = AppRoleConstant.Administrator)]
    public class SystemInformationController : Controller
    {
        private readonly ISystemInfoService _sysInfoService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public SystemInformationController(ISystemInfoService sysInfoService, IMapper mapper, IConfiguration configuration)
        {
            _sysInfoService = sysInfoService ?? throw new ArgumentNullException(nameof(sysInfoService));
            _mapper = mapper;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sysInfoDTO = await _sysInfoService.GetLatestSysInfo();

            var sysInfo = _mapper.Map<SystemInformationViewModel>(sysInfoDTO);

            // Populate additional information
            sysInfo.FrameworkVersion = Environment.Version.ToString();
            sysInfo.UserName = User.Identity?.Name;
            sysInfo.HostAddress = GetClientIP(); 
            sysInfo.AuthenticationType = User.Identity?.AuthenticationType;
            sysInfo.IsAuthenticated = (User.Identity?.IsAuthenticated ?? false) ? "True" : "False";

            // Set URL for Error log button
            ViewBag.ErrorLogUrl = $"{ErrorReportingURL()}";

            return View("SystemInfo", sysInfo);
        }

        private string ErrorReportingURL()
        {
            var url = _configuration["URL:LogMonitor"];
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("AWS Log Monitoring URL configuration setting was not found");
            }
            return url;
        }

        private string GetClientIP()
        {
              string forwardedFor = HttpContext.Request.Headers
                .FirstOrDefault(h => h.Key.Equals("X-Forwarded-For", StringComparison.OrdinalIgnoreCase))
                .Value
                .ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(forwardedFor))
            {
                // Fallback to the direct IP address (in case no proxy adds the header)
               forwardedFor = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
            }
            return forwardedFor;
        }
    }
}