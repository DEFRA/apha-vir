using System.Diagnostics;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Set URL for Error log button
            ViewBag.UserMgmtUrl = $"{UserMgmtUrl()}";

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
