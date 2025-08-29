using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.Lookup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    public class DeleteTests
    {
        private readonly LookupController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;

        public DeleteTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new LookupController(_mockLookupService, _mockMapper);
        }


        [Fact]
        public async Task Delete_ValidModel_ReturnsRedirectToActionResult()
        {
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookupItem = new LookupItemModel
                {
                    Id = lookupItemId,
                    Name = "Test",

                }
            };

            var lookupListdto = new List<LookupItemDTO> { new LookupItemDTO { Id = lookupItemId } };
            var lookuitemList = new List<LookupItemModel> { new LookupItemModel { Id = lookupItemId } };

            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDTO>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDTO());

            _mockLookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);

            // Act
            var result = await _controller.Delete(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LookupList", redirectResult.ActionName);
        }

        [Fact]
        public async Task Delete_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel { LookupItem = new LookupItemModel() };
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Delete(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Delete_ItemInUse_ReturnsViewResultWithError()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Id = Guid.NewGuid() }
            };
            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            // Act
            var result = await _controller.Delete(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));
        }

        [Fact]
        public async Task Delete_ServiceThrowsException_ReturnsViewResultWithError()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookupItem = new LookupItemModel { Id = Guid.NewGuid() }
            };
            _mockLookupService.IsLookupItemInUseAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);
            _mockMapper.Map<LookupItemDTO>(Arg.Any<LookupItemModel>()).Returns(new LookupItemDTO());
            _mockLookupService.DeleteLookupItemAsync(Arg.Any<Guid>(), Arg.Any<LookupItemDTO>())
            .Returns(Task.FromException(new Exception("Test exception")));

            // Act
            var result = await _controller.Delete(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(viewResult.ViewData.ModelState.ContainsKey(""));
        }
    }
}
