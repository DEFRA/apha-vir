using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    public class EditTests
    {
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;

        public EditTests()
        {
            _senderService = Substitute.For<ISenderService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SenderController(_senderService, _lookupService, _mapper);
        }

        [Fact]
        public async Task Edit_Get_ValidSenderId_ReturnsViewWithModel()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var senderDto = new SenderDTO { SenderId = senderId, SenderName = "Test Sender" };
            var senderViewModel = new SenderViewModel
            {
                SenderId = senderId,
                SenderName = "Test Sender",
                SenderAddress = "india",
                SenderOrganisation = "India"
            };
            var countryList = new List<SelectListItem> { new SelectListItem("Country", "1") };

            _senderService.GetSenderAsync(senderId).Returns(senderDto);
            _mapper.Map<SenderViewModel>(senderDto).Returns(senderViewModel);

            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>
            { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Country" } });

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SenderViewModel>(viewResult.Model);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(senderId, model.SenderId);
            Assert.Equal("Test Sender", model.SenderName);
            Assert.NotEmpty(model.CountryList!);
            await _senderService.Received(1).GetSenderAsync(senderId);
            await _lookupService.Received(1).GetAllCountriesAsync();
        }

        [Fact]
        public async Task Edit_Get_ReturnsBadRequest_WhenInvalidSenderId()
        {
            // Arrange
            var invalidSenderId = Guid.Empty;

            // Act
            var result = await _controller.Edit(invalidSenderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_ReturnsBadRequest_WhenInvalidModelStat()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenSenderNotFound()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            _senderService.GetSenderAsync(senderId).Returns(new SenderDTO { SenderId = Guid.Empty });

            // Act
            var result = await _controller.Edit(senderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Sender not found", notFoundResult.Value);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SenderViewModel
            {
                SenderId = Guid.NewGuid(),
                SenderName = "Test Sender",
                SenderAddress = "India",
                SenderOrganisation = "India"
            };
            var senderDto = new SenderDTO();
            _mapper.Map<SenderDTO>(model).Returns(senderDto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _senderService.Received(1).UpdateSenderAsync(Arg.Is<SenderDTO>(dto => dto == senderDto));
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SenderViewModel
            {
                SenderId = Guid.NewGuid(),
                SenderName = "",
                SenderAddress = "India",
                SenderOrganisation = "India"
            };

            _controller.ModelState.AddModelError("SenderName", "Required");
            var countryList = new List<SelectListItem>();
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());
            _mapper.Map<List<SelectListItem>>(Arg.Any<List<LookupItemDTO>>()).Returns(countryList);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            var returnedModel = Assert.IsType<SenderViewModel>(viewResult.Model);
            Assert.Equal(model, returnedModel);
            Assert.Equal(countryList, returnedModel.CountryList);
        }
    }
}
