using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    public class DeleteTests
    {
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;

        public DeleteTests()
        {
            _senderService = Substitute.For<ISenderService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SenderController(_senderService, _lookupService, _mapper);
        }


        [Fact]
        public async Task Delete_DeleteSenderAndRedirectToIndex_WhenValidModel()
        {
            // Arrange
            var model = new SenderMViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.NewGuid();

            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _senderService.Received(1).DeleteSenderAsync(senderId);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(SenderController.Index), redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_ReturnsViewWithError_WhenInvalidModelState()
        {
            // Arrange
            var model = new SenderMViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.NewGuid();
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _lookupService.Received(1).GetAllCountriesAsync();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task Delete_ReturnsViewWithModel_WhenEmptySenderId()
        {
            // Arrange
            var model = new SenderMViewModel { SenderName = "Test Sender", SenderAddress = "test", SenderOrganisation = "India" };
            var senderId = Guid.Empty;

            // Act
            var result = await _controller.Delete(model, senderId);

            // Assert
            await _lookupService.Received(1).GetAllCountriesAsync();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditSender", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }
    }
}
