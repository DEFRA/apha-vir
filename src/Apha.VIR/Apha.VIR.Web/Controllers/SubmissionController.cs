using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                SamplingLocationPremises = "Farm 1"
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(EditSubmissionDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate lists for redisplay
                model.SubmittingLabList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lab A" },
                new SelectListItem { Value = "2", Text = "Lab B" }
            };
                model.CountryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "UK", Text = "United Kingdom" },
                new SelectListItem { Value = "US", Text = "United States" },
                new SelectListItem { Value = "FR", Text = "France" }
            };
                model.ReasonList = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Research" },
                new SelectListItem { Value = "2", Text = "Diagnostic" }
            };
                return View(model);
            }
            // Save logic here
            return RedirectToAction("Index");
        }

        public IActionResult SubmissionSamples(int id)
        {
            var model = GetDummySubmissionData(id);
            return View(model);
        }

        public IActionResult EditSubmission(int id)
        {
            // Logic for editing submission
            return RedirectToAction("Index", new { id });
        }

        public IActionResult AddSample(int submissionId)
        {
            // Logic for adding a new sample
            return RedirectToAction("Index", new { id = submissionId });
        }

        public IActionResult EditSample(int id)
        {
            // Logic for editing a sample
            return RedirectToAction("Index", new { id = GetSubmissionIdForSample(id) });
        }

        public IActionResult DeleteSample(int id)
        {
            // Logic for deleting a sample
            return RedirectToAction("Index", new { id = GetSubmissionIdForSample(id) });
        }

        public IActionResult AddIsolate(int sampleId)
        {
            // Logic for adding a new isolate
            return RedirectToAction("Index", new { id = GetSubmissionIdForSample(sampleId) });
        }

        public IActionResult EditIsolate(int id)
        {
            // Logic for editing an isolate
            return RedirectToAction("Index", new { id = GetSubmissionIdForIsolate(id) });
        }

        public IActionResult DeleteIsolate(int id)
        {
            // Logic for deleting an isolate
            return RedirectToAction("Index", new { id = GetSubmissionIdForIsolate(id) });
        }

        public IActionResult DispatchIsolate(int id)
        {
            // Logic for dispatching an isolate
            return RedirectToAction("Index", new { id = GetSubmissionIdForIsolate(id) });
        }

        public IActionResult GenerateMissingDetailsLetter(int submissionId)
        {
            // Logic for generating missing details letter
            return RedirectToAction("Index", new { id = submissionId });
        }

        public IActionResult DeleteSubmission(int id)
        {
            // Logic for deleting a submission
            return RedirectToAction("Index", "Home");
        }

        public IActionResult FinishEditing(int id)
        {
            // Logic for finishing editing a submission
            return RedirectToAction("Index", "Home");
        }

        private SubmissionViewModel GetDummySubmissionData(int id)
        {
            return new SubmissionViewModel
            {
                Id = id,
                AVNumber = $"AV{id:D6}",
                SendersReferenceNumber = $"REF-{id:D4}",
                SenderOrganisation = "Sample Organization",
                CountryOfOriginName = "Sample Country",
                Samples = new List<Sample>
                {
                    new Sample { Id = 1, SampleNumber = "S001", SMSReferenceNumber = "SMS001", SenderReferenceNumber = "SRN001", HostSpeciesName = "Species1", HostBreedName = "Breed1" },
                    new Sample { Id = 2, SampleNumber = "S002", SMSReferenceNumber = "SMS002", SenderReferenceNumber = "SRN002", HostSpeciesName = "Species2", HostBreedName = "Breed2" }
                },
                Isolates = new List<Isolate>
                {
                    new Isolate { Id = 1, SampleNumber = "S001", SMSReferenceNumber = "ISO001", SenderReferenceNumber = "SRN001", TypeName = "Avian adenovirus Group II splenomegaly Virus", YearOfIsolation = 2023, Characteristics = "Char1", Nomenclature = "Nom1", isUniqueNomenclature = true },
                    new Isolate { Id = 1, SampleNumber = "S002", SMSReferenceNumber = "ISO002", SenderReferenceNumber = "SRN002", TypeName = "Avian adenovirus Group I splenomegaly Virus", YearOfIsolation = 2023, Characteristics = "Char2", Nomenclature = "Nom1", isUniqueNomenclature = false}
                },
                ShowLetterLink = true,
                IsLetterRequired = true,
                ShowDeleteLink = true,
                IsolatesHeader = "Isolates Header"
            };
        }

        private int GetSubmissionIdForSample(int sampleId)
        {
            // Dummy method to get submission ID for a sample
            return 1;
        }

        private int GetSubmissionIdForIsolate(int isolateId)
        {
            // Dummy method to get submission ID for an isolate
            return 1;
        }
    }
}
