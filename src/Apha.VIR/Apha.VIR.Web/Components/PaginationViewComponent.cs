using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Components
{
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationModel pagination)
        {
            return View(pagination);
        }
    }
}
