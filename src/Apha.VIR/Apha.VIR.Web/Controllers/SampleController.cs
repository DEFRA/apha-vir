using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Mappings;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.VIR.Web.Controllers
{
    public class SampleController : Controller
    {
        private readonly ISampleService _sampleService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private const string sampleIndex = "Index";

        public SampleController(ISampleService sampleService, ILookupService lookupService, IMapper mapper)
        {
            _lookupService = lookupService;
            _sampleService = sampleService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Create(string AVNumber)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var viewModel = new SampleViewModel();            
            viewModel.AVNumber = AVNumber;
            await LoadSampleDetailsData(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SampleViewModel model)
        {
            if (!ModelState.IsValid)
            {  
                await LoadSampleDetailsData(model);
                return View(model);
            }

            var sample = _mapper.Map<SampleDTO>(model);
            await _sampleService.AddSample(sample, model.AVNumber!, "Test");
            return RedirectToAction("Index", "SubmissionSamples", new { AVNumber = model.AVNumber});
        }


        [HttpGet]
        public async Task<IActionResult> Edit(string AVNumber, Guid Sample)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await this._sampleService.GetSampleAsync(AVNumber, Sample);
            var viewModel = _mapper.Map<SampleViewModel>(result);            

            viewModel.AVNumber = AVNumber;
            await LoadSampleDetailsData(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SampleViewModel model)
        {
            if (!ModelState.IsValid)
            {  
                await LoadSampleDetailsData(model);
                return View(model);
            }

            var sample = _mapper.Map<SampleDTO>(model);
            await _sampleService.UpdateSample(sample, "Test");
            return RedirectToAction(sampleIndex, "SubmissionSamples", new { AVNumber = model.AVNumber });
        }

        [HttpGet]
        public async Task<IActionResult> GetBreedsBySpecies(Guid? speciesId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var breeds = await _lookupService.GetAllHostBreedsByParentAsync(speciesId);
            var breedList = breeds.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = b.Name
            }).ToList();

            return Json(breedList);
        }

        [HttpGet]
        public async Task<IActionResult> GetLatinBreadList()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var latinBreedDto = await _lookupService.GetAllHostBreedsAsync();
            List<LatinBreed> latinBreedList = latinBreedDto.Select(p => new LatinBreed
            {
                Id = p.Id,
                Name = p.Name,
                ParentName = p.ParentName,
                AlternateName = p.AlternateName,
                Active = p.Active,
                Sms = p.Sms,
                Smscode = p.Smscode
            }).ToList();
            return PartialView("_LatinBreed", latinBreedList);
        }

        private async Task LoadSampleDetailsData(SampleViewModel model)
        {
            var sampleTypeDto = await _lookupService.GetAllSampleTypesAsync();
            var hostSpecyDto = await _lookupService.GetAllHostSpeciesAsync();
            var hostPurposeDto = await _lookupService.GetAllHostPurposesAsync();
            var hostBreedDto = await _lookupService.GetAllHostBreedsAsync();

            model.SampleTypeList = sampleTypeDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.HostSpeciesList = hostSpecyDto.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList();
            model.HostBreedList = hostBreedDto.Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Name }).ToList();
            model.HostPurposeList = hostPurposeDto.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }
    }
}
