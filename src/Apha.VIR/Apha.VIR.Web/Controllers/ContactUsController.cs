using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class ContactUsController : Controller
    {
        public IActionResult Index()
        {
            return View("ContactUs");
        }
    }
}
