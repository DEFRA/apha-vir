using System.Diagnostics;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Models;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    [Authorize()]
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISystemInfoService _sysInfoService;

        public HomeController(IConfiguration configuration, ISystemInfoService sysInfoService)
        {
            _configuration = configuration;
            _sysInfoService = sysInfoService ?? throw new ArgumentNullException(nameof(sysInfoService));
        }

        public IActionResult Index()
        {
            // Set URL for Error log button
            ViewBag.UserMgmtUrl = $"{UserMgmtUrl()}";

            if (HttpContext.Session.GetString("EnvironmentName") == null)
            {
                var envName = _sysInfoService.GetEnvironmentName().Result;
                HttpContext.Session.SetString("EnvironmentName", envName);
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private string UserMgmtUrl()
        {
            var url = _configuration["URL:UserMgmt"];
            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Azure Entra Group/Role Management URL configuration setting was not found");
            }
            return url;
        }
    }
}
