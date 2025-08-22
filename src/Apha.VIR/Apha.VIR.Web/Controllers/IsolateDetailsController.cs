using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    public class IsolateDetailsController : Controller
    {
        public ActionResult AddIsolate()
        {
            var virusFamilies = new List<VirusFamily>
            {
                new VirusFamily { Id = Guid.NewGuid(), Name = "Orthomyxoviridae" },
                new VirusFamily { Id = Guid.NewGuid(), Name = "Paramyxoviridae" },
                new VirusFamily { Id = Guid.NewGuid(), Name = "Coronaviridae" }
            };

            var personList = new List<Person>
            {
                new Person { Id = Guid.NewGuid(), Name = "person1" },
                new Person { Id = Guid.NewGuid(), Name = "person2" }
            };

            var viabilityList = new List<Viability>
            {
                new Viability { Id = Guid.NewGuid(), Name = "Non Viable" },
                new Viability { Id = Guid.NewGuid(), Name = "Viable" }
            };

            var isolationMethods = new List<IsolationMethod>
            {
                new IsolationMethod { Id = Guid.NewGuid(), Name = "Both" },
                new IsolationMethod { Id = Guid.NewGuid(), Name = "Mannual" },
                new IsolationMethod { Id = Guid.NewGuid(), Name = "Classical" }
            };

            var virusTypes = new List<VirusType>
            {
                new VirusType { Id = Guid.NewGuid(), Name = "Influenza A", FamilyId = virusFamilies[0].Id },
                new VirusType { Id = Guid.NewGuid(), Name = "Influenza B", FamilyId = virusFamilies[0].Id },
                new VirusType { Id = Guid.NewGuid(), Name = "Newcastle disease virus", FamilyId = virusFamilies[1].Id },
                new VirusType { Id = Guid.NewGuid(), Name = "SARS-CoV-2", FamilyId = virusFamilies[2].Id }
            };

            var freezerList = new List<Freezer>
            {
                new Freezer { Id = Guid.NewGuid(), Name = "Avian Viro 1" },
                new Freezer { Id = Guid.NewGuid(), Name = "AVIU Freezer Corridor Bld 70" },
                new Freezer { Id = Guid.NewGuid(), Name = "CRL NON Containment" }
            };

            var trayList = new List<Tray>
            {
                new Tray { Id = Guid.NewGuid(), Name = "Avian Viro 1" },
                new Tray { Id = Guid.NewGuid(), Name = "AVIU Freezer Corridor Bld 70" },
                new Tray { Id = Guid.NewGuid(), Name = "CRL NON Containment" }
            };

            var viewModel = new IsolateDetailsViewModel
            {
                VirusFamilies = virusFamilies,
                personList = personList,
                ViabilityList = viabilityList,
                IsolationMethods = isolationMethods,
                VirusTypes = virusTypes,
                FreezerList = freezerList,
                TrayList = trayList
            };

            return View("AddEditIsolate", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddIsolate(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


        public ActionResult EditIsolate(Guid Id)
        {
            var virusFamilies = new List<VirusFamily>
            {
                new VirusFamily { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Orthomyxoviridae" },
                new VirusFamily { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Paramyxoviridae" },
                new VirusFamily { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Coronaviridae" }
            };

            var personList = new List<Person>
            {
            new Person { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "person1" },
            new Person { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "person2" }
            };

            var viabilityList = new List<Viability>
            {
                new Viability { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Non Viable" },
                new Viability { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Viable" }
            };

            var isolationMethods = new List<IsolationMethod>
            {
                new IsolationMethod { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Both" },
                new IsolationMethod { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Mannual" },
                new IsolationMethod { Id = Guid.Parse("22222222-2222-2222-2222-333333333333"), Name = "Classical" }
            };

            var virusTypes = new List<VirusType>
            {
                new VirusType { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Influenza A", FamilyId = virusFamilies[0].Id },
                new VirusType { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Influenza B", FamilyId = virusFamilies[0].Id },
                new VirusType { Id = Guid.Parse("11111111-1111-1111-1111-222222222222"), Name = "Newcastle disease virus", FamilyId = virusFamilies[1].Id },
                new VirusType { Id = Guid.Parse("11111111-1111-1111-1111-333333333333"), Name = "SARS-CoV-2", FamilyId = virusFamilies[2].Id }
            };

            var freezerList = new List<Freezer>
            {
                new Freezer { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Avian Viro 1" },
                new Freezer { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "AVIU Freezer Corridor Bld 70" },
                new Freezer { Id = Guid.Parse("22222222-2222-2222-2222-333333333333"), Name = "CRL NON Containment" }
            };

            var trayList = new List<Tray>
            {
                new Tray { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Avian Viro 1" },
                new Tray { Id = Guid.NewGuid(), Name = "AVIU Freezer Corridor Bld 70" },
                new Tray { Id = Guid.NewGuid(), Name = "CRL NON Containment" }
            };

            var viewModel = new IsolateDetailsViewModel
            {
                IsolateId = Id,
                IsoSMSReferenceNumber = "REF1001",
                VirusFamilyId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                VirusTypeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                IsolationMethodId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                VirusFamilies = virusFamilies,
                personList = personList,
                ViabilityList = viabilityList,
                IsolationMethods = isolationMethods,
                VirusTypes = virusTypes,
                FreezerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FreezerList = freezerList,
                TrayId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                TrayList = trayList,
                PreviousCurrentViability = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PreviousCheckedBy = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PreviousDateChecked = DateTime.Now,
                AntiserumProduced = true,
                AntigenProduced = false,
                PhylogeneticAnalysis = "Phylogenetic analysis details",
                MaterialTransferAgreement = true,
                MTALocation = "India",
                Comment = "This is a comment for the isolate.",
                ValidToIssue = true,
                WhyNotValidToIssue = "This is why the isolate is not valid to issue.",
                Well = "A1",
                NoOfAliquots = 5,
                SampleAvailable = true,
                FirstViablePassageNumber = 1,
                DateChecked = DateTime.Now,
                AVNumber = "AV123456",
                Nomenclature = "Nomenclature Example"
               



            };

            return View("AddEditIsolate", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditIsolate(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }



    }
}
