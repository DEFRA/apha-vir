using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    [Route("Relocation")]
    public class IsolateAndTrayRelocationController : Controller
    {        
        public IActionResult Index()
        {
            return View();
        }
    }
}
