using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class SubmissionController : Controller
    {
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
