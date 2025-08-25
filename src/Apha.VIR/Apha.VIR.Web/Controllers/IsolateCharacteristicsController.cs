using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateCharacteristicsController : Controller
    {
        public IActionResult Edit(int isolateId)
        {
            var model = GetDummyCharacteristics(isolateId);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(int isolateId, List<IsolateCharacteristicsViewModel> characteristics)
        {
            if (!ModelState.IsValid)
            {
                return View(characteristics);
            }

            // TODO: Save characteristics to the database
            return RedirectToAction(nameof(Index), new { isolateId });
        }

        public IActionResult Cancel()
        {
            return RedirectToAction("Index", "Home");
        }

        private List<IsolateCharacteristicsViewModel> GetDummyCharacteristics(int isolateId)
        {
            return new List<IsolateCharacteristicsViewModel>
            {
                new IsolateCharacteristicsViewModel
                {
                    Id = 1, IsolateId = isolateId, CharacteristicId = 1,
                    CharacteristicName = "ICPI",
                    CharacteristicType = "Text",
                    Value = "Sample text"
                },
                new IsolateCharacteristicsViewModel
                {
                    Id = 2, IsolateId = isolateId, CharacteristicId = 2,
                    CharacteristicName = "ICPI",
                    CharacteristicType = "Numeric",
                    Value = "123"
                },
                new IsolateCharacteristicsViewModel
                {
                    Id = 3, IsolateId = isolateId, CharacteristicId = 3,
                    CharacteristicName = "PCPII",
                    CharacteristicType = "YesNo",
                    Value = "true"
                },
                new IsolateCharacteristicsViewModel
                {
                    Id = 4, IsolateId = isolateId, CharacteristicId = 4,
                    CharacteristicName = "Type",
                    CharacteristicType = "SingleList",
                    Options = new List<string> { "Option A", "Option B", "Option C" },
                    Value = "Option B"
                }
            };
        }
    }
}
