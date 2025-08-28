using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;


namespace Apha.VIR.Web.Controllers
{
    public class IsolateRelocateController : Controller 
    {
        public IActionResult Index()
        {
            var model = new IsolateRelocateViewModel
            {
                Freezers = GetDummyFreezerList(),
                Trays = GetDummyTrayList(),
                SearchResults = new List<IsolateRelocation>()
            };
            return View(model);
        }

        [HttpPost]
       public IActionResult Search([FromBody] IsolateRelocateViewModel model)
        {
            var results = GetDummySearchResults();

            return PartialView("_SearchResults", results);
        }



        [HttpPost]
        public IActionResult Save(IsolateRelocateViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Update successful.";
                return RedirectToAction("Index");
            }
            model.Freezers = GetDummyFreezerList();
            model.Trays = GetDummyTrayList();
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult SelectAll()
        {
            var model = new IsolateRelocateViewModel
            {
                SearchResults = GetDummySearchResults(),
                Freezers = GetDummyFreezerList(),
                Trays = GetDummyTrayList()
            };
            foreach (var item in model.SearchResults)
            {
                item.IsSelected = true;
            }
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult UnselectAll()
        {
            var model = new IsolateRelocateViewModel
            {
                SearchResults = GetDummySearchResults(),
                Freezers = GetDummyFreezerList(),
                Trays = GetDummyTrayList()
            };
            foreach (var item in model.SearchResults)
            {
                item.IsSelected = false;
            }
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult UpdateIsolate(Guid id, string freezer, string tray)
        {
            TempData["SuccessMessage"] = $"Isolate {id} updated successfully.";
            return RedirectToAction("Index");
        }

        private List<IsolateRelocateSelectListItem> GetDummyFreezerList()
        {
            return new List<IsolateRelocateSelectListItem>
            {
                new IsolateRelocateSelectListItem { Value = "", Text = "Select Freezer" },
                new IsolateRelocateSelectListItem { Value = "1", Text = "Freezer 1" },
                new IsolateRelocateSelectListItem { Value = "2", Text = "Freezer 2" },
                new IsolateRelocateSelectListItem { Value = "3", Text = "Freezer 3" }
            };
        }

        private List<IsolateRelocateSelectListItem> GetDummyTrayList()
        {
            return new List<IsolateRelocateSelectListItem>
            {
                new IsolateRelocateSelectListItem { Value = "", Text = "Select Tray" },
                new IsolateRelocateSelectListItem { Value = "1", Text = "Tray 1" },
                new IsolateRelocateSelectListItem { Value = "2", Text = "Tray 2" },
                new IsolateRelocateSelectListItem { Value = "3", Text = "Tray 3" }
            };
        }

        private List<IsolateRelocation>? GetDummySearchResults()
        {
            return new List<IsolateRelocation>
            {
                new IsolateRelocation { ID = Guid.NewGuid(), AVNumber = "AV001", Nomenclature = "Isolate 1", FreezerName = "Freezer 1", TrayName = "Tray 1", Well = "A1" },
                new IsolateRelocation { ID = Guid.NewGuid(), AVNumber = "AV002", Nomenclature = "Isolate 2", FreezerName = "Freezer 1", TrayName = "Tray 2", Well = "B2" },
                new IsolateRelocation { ID = Guid.NewGuid(), AVNumber = "AV003", Nomenclature = "Isolate 3", FreezerName = "Freezer 2", TrayName = "Tray 1", Well = "C3" }
            };
        }
    }


}
