using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class AVController : Controller
    {
        public IActionResult GetAVNumber()
        {
            var model = new GetAVNumberViewModel
            {
                RecentAVNumbers = GetRecentAVNumbers()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetAVNumber(GetAVNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Process the valid AV Number
                // For example, redirect to SubmissionSamples page
                return RedirectToAction("SubmissionSamples", new { avNumber = model.AVNumber });
            }

            // If we got this far, something failed; redisplay form
            model.RecentAVNumbers = GetRecentAVNumbers();
            return View(model);
        }

        private List<string> GetRecentAVNumbers()
        {
            // This is a dummy implementation. In a real application, you would fetch this from a database or service.
            return new List<string>
            {
                "AV000001-01",
                "PD0001-01",
                "SI000001-01",
                "BN000001-01"
            };
        }
    }
}
