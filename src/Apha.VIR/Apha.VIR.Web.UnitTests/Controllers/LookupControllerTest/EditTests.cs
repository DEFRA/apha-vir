using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.Lookup;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    public class EditTests
    {
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly LookupController _controller;

        public EditTests()
        {
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new LookupController(_lookupService, _mapper);
        }

        [Fact]
        public async Task Edit_ValidInputs_ReturnsViewWithViewModel()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookup = new LookupViewModel { Id = lookupId, Parent = Guid.NewGuid() };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetLookupsByIdAsync(lookupId).Returns(new LookupDTO());
            _lookupService.GetLookupItemAsync(lookupId, lookupItemId).Returns(new LookupItemDTO());
            _mapper.Map<LookupViewModel>(Arg.Any<LookupDTO>()).Returns(lookup);
            _mapper.Map<LookupItemModel>(Arg.Any<LookupItemDTO>()).Returns(lookupItem);

            // Act
            var result = await _controller.Edit(lookupId, lookupItemId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EditLookupItem", result.ViewName);
            Assert.IsType<LookupItemViewModel>(result.Model);
        }


        [Fact]
        public async Task Edit_InvalidLookupId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(Guid.Empty, Guid.NewGuid()) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey(""));
            Assert.Equal("Invalid parameters.", ((string[])modelState[""])[0]);
        }


        [Fact]
        public async Task Edit_InvalidLookupItemId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(Guid.NewGuid(), Guid.Empty) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey(""));
            Assert.Equal("Invalid parameters.", ((string[])modelState[""])[0]);
        }

        [Fact]
        public async Task Edit_GetLookupsByIdAsyncThrowsException_ThrowsException()
        {
            // Arrange
            _lookupService.GetLookupsByIdAsync(Arg.Any<Guid>()).Returns(Task.FromException<LookupDTO>(new Exception("Service exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task Edit_GetLookupItemAsyncThrowsException_ThrowsException()
        {
            // Arrange
            _lookupService.GetLookupsByIdAsync(Arg.Any<Guid>()).Returns(new LookupDTO());
            _lookupService.GetLookupItemAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromException<LookupItemDTO>(new Exception("Service exception")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task Edit_DifferentCurrentPageValues_ReturnsViewWithCorrectViewModel(int currentPage)
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var lookup = new LookupViewModel { Id = lookupId };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetLookupsByIdAsync(lookupId).Returns(new LookupDTO());
            _lookupService.GetLookupItemAsync(lookupId, lookupItemId).Returns(new LookupItemDTO());
            _mapper.Map<LookupViewModel>(Arg.Any<LookupDTO>()).Returns(lookup);
            _mapper.Map<LookupItemModel>(Arg.Any<LookupItemDTO>()).Returns(lookupItem);

            // Act
            var result = await _controller.Edit(lookupId, lookupItemId, currentPage) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("EditLookupItem", result.ViewName);
            var viewModel = Assert.IsType<LookupItemViewModel>(result.Model);
            Assert.Equal(lookupId, viewModel.LookupId);
            Assert.Equal(lookupItem, viewModel.LookkupItem);
        }


        [Fact]
        public async Task Edit_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange

            var lookupId = Guid.NewGuid();
            var lookupItemId = Guid.NewGuid();
            var model = new LookupItemViewModel
            {
                LookupId = lookupId,
                LookkupItem = new LookupItemModel
                {
                    Id= lookupItemId,
                    Name="Test",
                    
                }
            };
            var lookupListdto= new List<LookupItemDTO> { new LookupItemDTO {Id= lookupItemId } };
            var lookuitemList = new List<LookupItemModel> { new LookupItemModel { Id = lookupItemId } };

            var lookup = new LookupViewModel { Id = lookupId, Parent = Guid.NewGuid() };
            var lookupItem = new LookupItemModel { Id = lookupItemId };

            _lookupService.GetAllLookupItemsAsync(lookupId).Returns(lookupListdto);
            _mapper.Map<IEnumerable<LookupItemModel>>(lookupListdto).Returns(lookuitemList);

       
            var dto = new LookupItemDTO();
            _mapper.Map<LookupItemDTO>(model.LookkupItem).Returns(dto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _lookupService.Received(1).UpdateLookupItemAsync(model.LookupId, dto);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("LookupList", redirectResult.ActionName);
        }


        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel { LookkupItem = new LookupItemModel() };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
        }


        [Fact]
        public async Task Edit_ShowParentTrue_PopulatesLookupParentList()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                ShowParent = true,
                LookkupItem = new LookupItemModel()
            };
            _controller.ModelState.AddModelError("Error", "Test error");

            var lookupResult = new LookupDTO { Parent = Guid.NewGuid() };
            var lookupViewModel = new LookupViewModel { Parent = Guid.NewGuid() };
            _lookupService.GetLookupsByIdAsync(model.LookupId).Returns(lookupResult);
            _mapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);

            var parentItems = new List<LookupItemDTO>();
            _lookupService.GetLookupItemParentListAsync(Arg.Any<Guid>()).Returns(parentItems);
            _mapper.Map<IEnumerable<LookupItemModel>>(parentItems).Returns(new List<LookupItemModel>());

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var resultModel = Assert.IsType<LookupItemViewModel>(viewResult.Model);
            Assert.NotNull(resultModel.LookupParentList);
        }

        [Fact]
        public async Task Edit_ValidateModelAddsErrors_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookkupItem = new LookupItemModel()
            };

            _lookupService.GetAllLookupItemsAsync(model.LookupId).Returns(new List<LookupItemDTO>());
            _mapper.Map<IEnumerable<LookupItemModel>>(Arg.Any<IEnumerable<LookupItemDTO>>()).Returns(new List<LookupItemModel>());

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Edit_UpdateLookupItemAsyncThrowsException_ReturnsViewResult()
        {
            // Arrange
            var model = new LookupItemViewModel
            {
                LookupId = Guid.NewGuid(),
                LookkupItem = new LookupItemModel()
            };
            var dto = new LookupItemDTO();
            _mapper.Map<LookupItemDTO>(model.LookkupItem).Returns(dto);
            _lookupService.UpdateLookupItemAsync(model.LookupId, dto).Returns(Task.FromException(new Exception("Test exception")));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("EditLookupItem", viewResult.ViewName);
        }
    }
}
