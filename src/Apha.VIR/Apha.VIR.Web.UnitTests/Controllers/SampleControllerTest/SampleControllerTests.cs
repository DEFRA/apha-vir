using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SampleControllerTest
{
    public class SampleControllerTests
    {
        private readonly SampleController _controller;
        private readonly ISampleService _sampleService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;

        public SampleControllerTests()
        {
            _sampleService = Substitute.For<ISampleService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SampleController(_sampleService, _lookupService, _mapper);
        }

        [Fact]
        public async Task Index_WithValidSample_ReturnsViewWithViewModel()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var sampleDto = new SampleDTO { SampleId = sampleId };
            var viewModel = new SampleViewModel { SampleId = sampleId };

            _sampleService.GetSampleAsync(avNumber, sampleId).Returns(sampleDto);
            _mapper.Map<SampleViewModel>(sampleDto).Returns(viewModel);

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); // Ensure the model is not null
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure the model is not null before dereferencing                       
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithNullSample_ReturnsViewWithNewViewModel()
        {
            // Arrange
            var avNumber = "AV123";

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); // Ensure the model is not null before dereferencing
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure the model is not null before dereferencing            
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Sample error");

            // Act
            var result = await _controller.Create("AV123");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Index_CorrectMappingAndViewModelPopulation()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var sampleDto = new SampleDTO { SampleId = sampleId };
            var viewModel = new SampleViewModel
            {
                SampleId = sampleId,
                SampleTypeList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostSpeciesList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostBreedList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostPurposeList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>() // Initialize to avoid null
            };

            _sampleService.GetSampleAsync(avNumber, sampleId).Returns(sampleDto);
            _mapper.Map<SampleViewModel>(sampleDto).Returns(viewModel);

            _lookupService.GetAllSampleTypesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDTO>>(new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type1" }
            }));
            _lookupService.GetAllHostSpeciesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDTO>>(new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Species1" }
            }));
            _lookupService.GetAllHostPurposesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDTO>>(new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Purpose1" }
            }));
            _lookupService.GetAllHostBreedsAsync().Returns(Task.FromResult<IEnumerable<LookupItemDTO>>(new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed1" }
            }));

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure model is not null before dereferencing
            Assert.NotNull(model.SampleTypeList); // Ensure SampleTypeList is not null
            Assert.NotEmpty(model.SampleTypeList);
            Assert.NotNull(model.HostSpeciesList); // Ensure HostSpeciesList is not null
            Assert.NotEmpty(model.HostSpeciesList);
            Assert.NotNull(model.HostBreedList); // Ensure HostBreedList is not null
            Assert.NotEmpty(model.HostBreedList);
            Assert.NotNull(model.HostPurposeList); // Ensure HostPurposeList is not null
            Assert.NotEmpty(model.HostPurposeList);
        }

        [Fact]
        public async Task Insert_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "TEST123" };
            _mapper.Map<SampleDTO>(model).Returns(new SampleDTO());

            // Act
            var result = await _controller.Create(model);

            // Assert
            await _sampleService.Received(1).AddSample(Arg.Any<SampleDTO>(), "TEST123", "Test");
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;

            // Fix for CS8602: Ensure RouteValues is not null before accessing
            Assert.NotNull(redirectResult.RouteValues); // Ensure RouteValues is not null
            Assert.Equal("TEST123", redirectResult.RouteValues["AVNumber"]);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Insert_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SampleViewModel();
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting
            Assert.NotNull(viewResult.Model);
            var viewModel = (SampleViewModel)viewResult.Model;

            Assert.Equal(model, viewModel);
            await _lookupService.Received(1).GetAllSampleTypesAsync();
            await _lookupService.Received(1).GetAllHostSpeciesAsync();
            await _lookupService.Received(1).GetAllHostPurposesAsync();
            await _lookupService.Received(1).GetAllHostBreedsAsync();
        }

        [Fact]
        public async Task Update_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "TEST123" };
            var sampleDto = new SampleDTO();
            _mapper.Map<SampleDTO>(model).Returns(sampleDto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _sampleService.Received(1).UpdateSample(sampleDto, "Test");
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;

            // Fix for CS8602: Ensure RouteValues is not null before accessing
            Assert.NotNull(redirectResult.RouteValues); // Ensure RouteValues is not null
            Assert.Equal("TEST123", redirectResult.RouteValues["AVNumber"]);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Update_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var model = new SampleViewModel();
            _controller.ModelState.AddModelError("Error", "Model error");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting and dereferencing
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Update_SuccessfulUpdate_CallsUpdateSample()
        {
            // Arrange
            var model = new SampleViewModel();
            var sampleDto = new SampleDTO();
            _mapper.Map<SampleDTO>(model).Returns(sampleDto);

            // Act
            await _controller.Edit(model);

            // Assert
            await _sampleService.Received(1).UpdateSample(sampleDto, "Test");
        }

        [Fact]
        public async Task Update_ExceptionThrown_ReturnsViewResult()
        {
            // Arrange
            var model = new SampleViewModel();
            var sampleDto = new SampleDTO();
            _mapper.Map<SampleDTO>(model).Returns(sampleDto);
            _sampleService
                .UpdateSample(sampleDto, "Test")
                .Returns(Task.FromException(new Exception("Test exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(model));
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_ValidSpeciesId_ReturnsBreedList()
        {
            // Arrange
            var speciesId = Guid.NewGuid();
            var breeds = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed2" }
            };
            _lookupService.GetAllHostBreedsByParentAsync(speciesId).Returns(breeds);

            // Act
            var result = await _controller.GetBreedsBySpecies(speciesId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var breedList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, breedList.Count);
            Assert.Equal("Breed1", breedList[0].Text);
            Assert.Equal("Breed2", breedList[1].Text);
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_NullSpeciesId_ReturnsEmptyList()
        {
            // Arrange
            _lookupService.GetAllHostBreedsByParentAsync(null).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.GetBreedsBySpecies(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var breedList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Empty(breedList);
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Some error");

            // Act
            var result = await _controller.GetBreedsBySpecies(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsPartialView_WhenModelStateIsValid()
        {
            // Arrange
            var latinBreedDtos = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Breed", ParentName = "Test Species", AlternateName = "Test Alt", Active = true, Sms = true, Smscode = "123" }
            };
            _lookupService.GetAllHostBreedsAltNameAsync().Returns(latinBreedDtos);

            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_LatinBreed", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<List<LatinBreedModel>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsCorrectData()
        {
            // Arrange
            var latinBreedDtos = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Breed 1", ParentName = "Test Species 1", AlternateName = "Test Alt 1", Active = true, Sms = true, Smscode = "123" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Breed 2", ParentName = "Test Species 2", AlternateName = "Test Alt 2", Active = false, Sms = false, Smscode = "456" }
            };
            _lookupService.GetAllHostBreedsAltNameAsync().Returns(latinBreedDtos);

            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsAssignableFrom<List<LatinBreedModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Test Breed 1", model[0].Name);
            Assert.Equal("Test Breed 2", model[1].Name);
            Assert.True(model[0].Active);
            Assert.False(model[1].Active);
        }
    }
}

