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
            var viewModel = new SampleViewModel { SampleId = sampleId, IsEditMode = true };

            _sampleService.GetSampleAsync(avNumber, sampleId).Returns(sampleDto);
            _mapper.Map<SampleViewModel>(sampleDto).Returns(viewModel);

            // Act
            var result = await _controller.Index(avNumber, sampleId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); // Ensure the model is not null
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure the model is not null before dereferencing
            Assert.Equal(sampleId, model.SampleId);
            Assert.True(model.IsEditMode);
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithNullSample_ReturnsViewWithNewViewModel()
        {
            // Arrange
            var avNumber = "AV123";

            // Act
            var result = await _controller.Index(avNumber, null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.False(model.IsEditMode);
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Sample error");

            // Act
            var result = await _controller.Index("AV123", null);

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
                IsEditMode = true,
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
            var result = await _controller.Index(avNumber, sampleId) as ViewResult;

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
            var result = await _controller.Insert(model);

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
            var result = await _controller.Insert(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.Equal("Index", viewResult.ViewName);

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting
            Assert.NotNull(viewResult.Model);
            var viewModel = (SampleViewModel)viewResult.Model;

            Assert.Equal(model, viewModel);
            Assert.False(viewModel.IsEditMode);
            await _lookupService.Received(1).GetAllSampleTypesAsync();
            await _lookupService.Received(1).GetAllHostSpeciesAsync();
            await _lookupService.Received(1).GetAllHostPurposesAsync();
            await _lookupService.Received(1).GetAllHostBreedsAsync();
        }

        [Fact]
        public async Task Insert_NullModel_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _controller.Insert(null!));
        }

        [Fact]
        public async Task Update_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "TEST123" };
            var sampleDto = new SampleDTO();
            _mapper.Map<SampleDTO>(model).Returns(sampleDto);

            // Act
            var result = await _controller.Update(model);

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
            var result = await _controller.Update(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting and dereferencing
            Assert.NotNull(viewResult.Model);
            var viewModel = (SampleViewModel)viewResult.Model;

            Assert.Equal("Index", viewResult.ViewName);
            Assert.True(viewModel.IsEditMode);
        }

        [Fact]
        public async Task Update_SuccessfulUpdate_CallsUpdateSample()
        {
            // Arrange
            var model = new SampleViewModel();
            var sampleDto = new SampleDTO();
            _mapper.Map<SampleDTO>(model).Returns(sampleDto);

            // Act
            await _controller.Update(model);

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
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(model));
        }
    }
}

