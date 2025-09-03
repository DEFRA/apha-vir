using System.Text.Json;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionControllerTest
{
    public class SubmissionControllerSenderTests
    {
        private readonly ILookupService _mockLookupService;
        private readonly ISenderService _mockSenderService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionController _controller;

        public SubmissionControllerSenderTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockSenderService = Substitute.For<ISenderService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionController(_mockLookupService, _mockSenderService, _mockSubmissionService, _mockMapper);
        }

        [Fact]
        public async Task Test_GetSenderDetails_ValidCountryId_ReturnsPartialView()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderBySenderAsync(countryId).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetSenderDetails(countryId);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_MainSenders", partialViewResult.ViewName);
            Assert.IsType<List<SubmissionSenderViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task Test_GetSenderDetails_NullCountryId_ReturnsPartialView()
        {
            // Arrange
            Guid? countryId = null;
            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderBySenderAsync(countryId).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetSenderDetails(countryId);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_MainSenders", partialViewResult.ViewName);
            Assert.IsType<List<SubmissionSenderViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task Test_GetSenderDetails_InvalidModelState_ReturnsPartialViewWithNullCountryId()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderBySenderAsync(null).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetSenderDetails(Guid.NewGuid());

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_MainSenders", partialViewResult.ViewName);
            Assert.IsType<List<SubmissionSenderViewModel>>(partialViewResult.Model);
            await _mockSenderService.Received().GetAllSenderOrderBySenderAsync(null);
        }

        [Fact]
        public async Task GetAddSender_ReturnsPartialViewResult()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var countryList = new List<SelectListItem>
            {
            new SelectListItem { Value = id1.ToString(), Text = "Country 1" },
            new SelectListItem { Value = id2.ToString(), Text = "Country 2" }
            };
            _mockLookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = id1, Name = "Country 1" },
            new LookupItemDTO { Id = id2, Name = "Country 2" }
            });

            // Act
            var result = await _controller.GetAddSender();

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task GetAddSender_ReturnsCorrectViewName()
        {
            // Arrange
            _mockLookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.GetAddSender() as PartialViewResult;

            // Assert
            Assert.Equal("_AddSender", result.ViewName);
        }

        [Fact]
        public async Task GetAddSender_ReturnsModelOfTypeSenderViewModel()
        {
            // Arrange
            _mockLookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.GetAddSender() as PartialViewResult;

            // Assert
            Assert.IsType<SubmissionSenderViewModel>(result.Model);
        }

        [Fact]
        public async Task GetAddSender_PopulatesCountryListInModel()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var countryList = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = id1, Name = "Country 1" },
            new LookupItemDTO { Id = id2, Name = "Country 2" }
            };
            _mockLookupService.GetAllCountriesAsync().Returns(countryList);

            // Act
            var result = await _controller.GetAddSender() as PartialViewResult;
            var model = result.Model as SubmissionSenderViewModel;

            // Assert
            Assert.NotNull(model?.CountryList);
            Assert.Equal(2, model.CountryList.Count);
            Assert.Contains(model.CountryList, item => item.Value == id1.ToString() && item.Text == "Country 1");
            Assert.Contains(model.CountryList, item => item.Value == id2.ToString() && item.Text == "Country 2");
        }

        [Fact]
        public async Task AddSender_ValidModel_ReturnsSuccessJsonResult()
        {
            // Arrange
            var senderModel = new SubmissionSenderViewModel { SenderName = "Test Sender", Country = Guid.NewGuid() };
            var senderDto = new SenderDTO();
            _mockMapper.Map<SenderDTO>(senderModel).Returns(senderDto);
            _mockSenderService.AddSenderAsync(senderDto).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.AddSender(senderModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.True(success);
            Assert.Equal("Sender add successfully!", message);
            await _mockSenderService.Received(1).AddSenderAsync(senderDto);
        }

        [Fact]
        public async Task AddSender_InvalidModel_ReturnsFailureJsonResult()
        {
            // Arrange
            var senderModel = new SubmissionSenderViewModel();
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = await _controller.AddSender(senderModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = JsonSerializer.Serialize(jsonResult.Value);
            JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement jsonElement = doc.RootElement;
            bool success = jsonElement.GetProperty("success").GetBoolean();
            string? message = jsonElement.GetProperty("message").GetString();
            Assert.False(success);
            Assert.Equal("Add sender failed!", message);
            await _mockSenderService.DidNotReceive().AddSenderAsync(Arg.Any<SenderDTO>());
        }

        [Fact]
        public async Task Test_GetOrganisationDetails_ValidCountryId()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderByOrganisationAsync(countryId).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetOrganisationDetails(countryId);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = result as PartialViewResult;
            Assert.Equal("_MainOrganisations", partialViewResult.ViewName);
            Assert.Equal(senderViewModels, partialViewResult.Model);
        }

        [Fact]
        public async Task Test_GetOrganisationDetails_NullCountryId()
        {
            // Arrange
            Guid? countryId = null;
            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderByOrganisationAsync(countryId).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetOrganisationDetails(countryId);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = result as PartialViewResult;
            Assert.Equal("_MainOrganisations", partialViewResult.ViewName);
            Assert.Equal(senderViewModels, partialViewResult.Model);
        }

        [Fact]
        public async Task Test_GetOrganisationDetails_InvalidModelState()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            _controller.ModelState.AddModelError("error", "some error");

            var senderDtos = new List<SenderDTO> { new SenderDTO() };
            var senderViewModels = new List<SubmissionSenderViewModel> { new SubmissionSenderViewModel() };

            _mockSenderService.GetAllSenderOrderByOrganisationAsync(null).Returns(senderDtos);
            _mockMapper.Map<List<SubmissionSenderViewModel>>(senderDtos).Returns(senderViewModels);

            // Act
            var result = await _controller.GetOrganisationDetails(countryId);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialViewResult = result as PartialViewResult;
            Assert.Equal("_MainOrganisations", partialViewResult.ViewName);
            Assert.Equal(senderViewModels, partialViewResult.Model);
            await _mockSenderService.Received(1).GetAllSenderOrderByOrganisationAsync(null);
        }
    }
}
