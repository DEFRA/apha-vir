using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
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
        public async Task<IActionResult> SystemInfo()
        {
            var sysInfoDTO = await _sysInfoService.GetLatestSysInfo();

            var sysInfo = _mapper.Map<SystemInformationViewModel>(sysInfoDTO);

            // Populate additional information
            sysInfo.FrameworkVersion = Environment.Version.ToString();
            sysInfo.UserName = User.Identity?.Name;
            sysInfo.HostAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            sysInfo.AuthenticationType = User.Identity?.AuthenticationType;
            sysInfo.IsAuthenticated = (User.Identity?.IsAuthenticated ?? false) ? "Yes" : "No";

            // Set URL for Error log button
            ViewBag.ErrorLogUrl = $"{ErrorReportingURL()}";

            return View(sysInfo);
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
    }
}