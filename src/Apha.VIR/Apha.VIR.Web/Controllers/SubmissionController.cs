using System.Text;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Isolate = Apha.VIR.Web.Models.Isolate;
using Sample = Apha.VIR.Web.Models.Sample;
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

        public IActionResult SubmissionSamples(int id)
        {
            var model = GetDummySubmissionData(id);
            return View(model);
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

        public IActionResult SubmissionLetter()
        {
            // Dummy data for demonstration
            var sender = "John Doe";
            var senderOrganisation = "Org Example";
            var senderAddress = "123 Main St, City";
            var submittingCountryName = "United Kingdom";
            var avNumber = "AV123456";
            var sendersReferenceNumber = "SRN-001";
            var dateSubmissionReceived = DateTime.Today;
            var countryOfOriginName = "UK";
            var sampleId = "1";
            var isolateSampleId = "1";
            var samples = new List<Sample>
            {
                new Sample { Id = 1, SampleNumber = "S001", SMSReferenceNumber = "SMS001", SenderReferenceNumber = "SRN001", HostSpeciesName = "Species1", HostBreedName = "Breed1" }
            };
            var isolates = new List<Isolate>
            {
                new Isolate { Id = 1, SampleNumber = "S001", SMSReferenceNumber = "ISO001", SenderReferenceNumber = "SRN001", TypeName = "Avian adenovirus Group II splenomegaly Virus", YearOfIsolation = 2023, Characteristics = "Char1", Nomenclature = "Nom1", isUniqueNomenclature = true }
            };

            var viewModel = new SubmissionLetterViewModel
            {
                LetterContent = GenerateSubmissionLetter2(
                    sender,
                    senderOrganisation,
                    senderAddress,
                    submittingCountryName,
                    avNumber,
                    sendersReferenceNumber,
                    dateSubmissionReceived,
                    countryOfOriginName,
                    sampleId,
                    isolateSampleId,
                    samples,
                    isolates
                )
            };

            return View(viewModel);
        }

        private string GenerateDummyLetterContent()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dear [Recipient],");
            sb.AppendLine();
            sb.AppendLine("We are pleased to inform you that your submission to the Virus Isolates Repository has been received and is currently under review.");
            sb.AppendLine();
            sb.AppendLine("Submission Details:");
            sb.AppendLine("- Date Received: [Current Date]");
            sb.AppendLine("- Submission ID: [Random ID]");
            sb.AppendLine("- Virus Type: [Sample Virus Type]");
            sb.AppendLine();
            sb.AppendLine("Our team of experts will carefully examine your submission and get back to you with further information or any additional requirements within the next 5-7 business days.");
            sb.AppendLine();
            sb.AppendLine("If you have any questions or need to provide additional information, please don't hesitate to contact us at support@virusisolatesrepository.org.");
            sb.AppendLine();
            sb.AppendLine("Thank you for your contribution to our research efforts.");
            sb.AppendLine();
            sb.AppendLine("Best regards,");
            sb.AppendLine("The Virus Isolates Repository Team");

            return sb.ToString();
        }
        private string GenerateSubmissionLetter2(
            string sender,
            string senderOrganisation,
            string senderAddress,
            string submittingCountryName,
            string avNumber,
            string sendersReferenceNumber,
            DateTime dateSubmissionReceived,
            string countryOfOriginName,
            string SampleId,
            string IsolateSampleId,
            List<Sample> samples,
            List<Isolate> isolates
        )
        {
            // Helper for missing text
            string MissingText(object value) => value == null || string.IsNullOrWhiteSpace(value.ToString()) ? "[Missing]" : value.ToString();

            var NL = Environment.NewLine;
            var str = new StringBuilder();

            str.Append("Animal Health and Veterinary Laboratories Agency - Weybridge,").Append(NL);
            str.Append("Avian Virology, New Haw, Surrey KT15 3NB United Kingdom").Append(NL);
            str.Append("Facsimile +44 (0)1932 357856").Append(NL);
            str.Append(NL).Append(NL);
            str.Append("Date: ").Append(DateTime.Now.ToLongDateString()).Append(NL);
            str.Append(NL).Append(NL);
            str.Append(sender).Append(NL);
            str.Append(senderOrganisation).Append(NL);
            str.Append(senderAddress).Append(NL);
            str.Append(submittingCountryName).Append(NL);
            str.Append("Our Ref: ").Append(avNumber).Append(NL);
            str.Append("Your Submission Ref: ").Append(sendersReferenceNumber).Append(NL);
            str.Append("Date of Receipt: ").Append(dateSubmissionReceived.ToLongDateString()).Append(NL);
            str.Append(NL);
            str.Append("Dear ").Append(sender).Append(NL).Append(NL);
            str.Append("With regards to your recent submission of samples the following detail(s) were omitted. ").Append(NL).Append(NL);
            str.Append("Country of Virus Origin: ").Append(MissingText(countryOfOriginName)).Append(NL).Append(NL);

            if (samples == null || samples.Count == 0)
            {
                str.Append("Your Sample Ref: ").Append('\t').Append("[Missing]").Append(NL);
                str.Append("Species/Group: ").Append('\t').Append(MissingText(null)).Append(NL);
                str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL).Append(NL);
            }
            else
            {
                foreach (var samp in samples)
                {
                    str.Append("Your Sample Ref: ").Append('\t').Append(samp.SenderReferenceNumber ?? "[Missing]").Append(NL);
                    str.Append("Species/Group: ").Append('\t').Append(MissingText(samp.HostSpeciesName)).Append(NL);

                    bool found = false;
                    if (isolates == null || isolates.Count == 0)
                    {
                        // Assume SampleTypeName is not available, so just use "Virus Year of Isolation"
                        str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL);
                    }
                    else
                    {
                        foreach (var iso in isolates)
                        {
                            // SampleId/ID mapping: using SampleId from Sample and Isolate
                            if (!found && IsolateSampleId == SampleId)
                            {
                                // SampleTypeName is not available, so just use "Virus Year of Isolation"
                                str.Append("Virus Year of Isolation: ").Append(MissingText(iso.YearOfIsolation)).Append(NL);
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            str.Append("Virus Year of Isolation: ").Append(MissingText(null)).Append(NL);
                        }
                    }
                    str.Append(NL);
                }
            }

            str.Append(NL);
            str.Append("It is essential that we have these details and I would appreciate it if you could send the information to me as soon as possible. ");
            str.Append("Full and final analysis of your samples may be delayed until this information is received.").Append(NL).Append(NL).Append(NL);
            str.Append("Best Regards, ").Append(NL).Append(NL);
            str.Append("Pooja");

            return str.ToString();
        }



    }

}
