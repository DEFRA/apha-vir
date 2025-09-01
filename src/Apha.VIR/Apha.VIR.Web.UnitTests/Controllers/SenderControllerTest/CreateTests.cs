using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Apha.VIR.Web.Models.Lookup;
using Apha.VIR.Application.DTOs;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    public class CreateTests
    {
        private readonly ISenderService _senderService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly SenderController _controller;

        public CreateTests()
        {
            _senderService = Substitute.For<ISenderService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SenderController(_senderService, _lookupService, _mapper);
        }


        [Fact]
        public async Task Create_Get_ReturnsViewWithModel()
        {
            // Arrange
            var countries = new List<SelectListItem> { new SelectListItem { Value = "1", Text = "Country" } };
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Country" } });
            _mapper.Map<IEnumerable<SelectListItem>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(countries);

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SenderMViewModel>(viewResult.Model);
            Assert.Equal("CreateSender", viewResult.ViewName);
            Assert.NotNull(model.CountryList);
            Assert.Single(model.CountryList);
        }


        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new SenderMViewModel { SenderName = "Test Sender",SenderAddress="test",SenderOrganisation="India" };
            _mapper.Map<SenderDTO>(model).Returns(new SenderDTO());

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(SenderController.Index), redirectResult.ActionName);
            await _senderService.Received(1).AddSenderAsync(Arg.Any<SenderDTO>());
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SenderMViewModel { SenderName = "", SenderAddress = "test", SenderOrganisation = "India" };
            _controller.ModelState.AddModelError("SenderName", "Required");
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("CreateSender", viewResult.ViewName);
            Assert.IsType<SenderMViewModel>(viewResult.Model);
        }
        [Fact]
        public async Task Create_Post_EmptyModel_ReturnsViewWithModel()
        {
            // Arrange
            SenderMViewModel model = null!;
            _lookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", viewResult.ActionName);
        }

    }
}
