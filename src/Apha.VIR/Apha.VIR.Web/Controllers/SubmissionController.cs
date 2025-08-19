using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sender = Apha.VIR.Web.Models.Sender;

namespace Apha.VIR.Web.Controllers
{
    public class SubmissionController : Controller
    {
        public IActionResult Edit()
        {
            var model = new EditSubmissionDetailsViewModel
            {
                AVNumber = "AV123456",
                RLReferenceNumber = "RL987654",
                SubmittingLab = "1",
                SubmittingLabList = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Lab A" },
                        new SelectListItem { Value = "2", Text = "Lab B" }
                    },
                SendersReferenceNumber = "SRN-001",
                SubmittingCountry = "UK",
                CountryList = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "UK", Text = "United Kingdom" },
                        new SelectListItem { Value = "US", Text = "United States" },
                        new SelectListItem { Value = "FR", Text = "France" }
                    },
                Sender = "John Doe",
                SenderOrganisation = "Org Example",
                SenderAddress = "123 Main St, City",
                CountryOfOrigin = "UK",
                ReasonForSubmission = "1",
                ReasonList = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Research" },
                        new SelectListItem { Value = "2", Text = "Diagnostic" }
                    },
                DateSubmissionReceived = DateTime.Today,
                NumberOfSamples = 5,
                CPHNumber = "12/345/6789",
                Owner = "Jane Smith",
                SamplingLocationPremises = "Farm 1",
            };



            return View(model);
        }

        public IActionResult PopupSender()
        {
            List<Sender> Senders = new List<Sender>
                {
                    new Sender
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "John Doe",
                        Organisation = "Org Example",
                        Address = "123 Main St, City",
                        CountryOfOrigin = "United Kingdom"
                    },
                    new Sender
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "Sachin R",
                        Organisation = "Org Example2",
                        Address = "Mumbai",
                        CountryOfOrigin = "India"
                    }
                };
            return PartialView("_MainSenders", Senders);
        }

        public IActionResult AddSender()
        {
            var model = new Sender
            {
                CountryList = new List<string> { "India", "USA", "UK", "Germany", "France" }
            };
            return PartialView("_AddSender", model);
        }

            [HttpPost]
    public IActionResult SaveSender(Sender model)
    {
        if (ModelState.IsValid)
        {
            // Save to DB here
            model.Id = Guid.NewGuid();

            return Json(new { success = true, message = "Sender saved successfully!" });
        }
        return Json(new { success = false, message = "Validation failed!" });
    }

        public IActionResult PopupOrganisation()
        {
            List<Organisation> Organisations = new List<Organisation>
                {
                    new Organisation
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Name = "Org Example",
                        Sender = "John Doe",
                        Address = "123 Main St, City",
                        CountryOfOrigin = "United Kingdom"
                    },
                    new Organisation
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Name = "Org Example2",
                        Sender = "Sachin R",
                        Address = "Mumbai",
                        CountryOfOrigin = "India"
                    }
                };
            return PartialView("_MainOrganisations", Organisations);
        }
    }
}
