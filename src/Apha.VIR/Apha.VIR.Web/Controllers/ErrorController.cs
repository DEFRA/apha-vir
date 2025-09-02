using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View("Error");
        }

        public IActionResult AccessDenied()
        {

            return View("Error");
        }
    }
}
