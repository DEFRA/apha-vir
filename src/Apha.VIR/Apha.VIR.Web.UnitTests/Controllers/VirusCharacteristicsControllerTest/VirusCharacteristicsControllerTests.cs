using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Application.Services;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class VirusCharacteristicsControllerTests
    {
        private readonly VirusCharacteristicsController _controller;
        private readonly IVirusCharacteristicService _mockVirusCharacteristicService;
        private readonly IMapper _mockMapper;

        public VirusCharacteristicsControllerTests()
        {
            _mockVirusCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_mockVirusCharacteristicService, _mockMapper);
        }

        [Fact]
        public async Task EditAsync_ReturnsViewResultWithCorrectModel()
        {
            // Arrange
            var expectedTypes = new List<VirusCharacteristicDataTypeDTO>
{
new VirusCharacteristicDataTypeDTO { Id = new Guid(), DataType = "Type1" },
new VirusCharacteristicDataTypeDTO { Id = new Guid(), DataType = "Type2" }
};
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(expectedTypes);

            // Act
            var result = await _controller.CreateAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicsModel>(viewResult.Model);
            Assert.NotNull(model.CharacteristicTypeNameList);
            Assert.Equal(2, model.CharacteristicTypeNameList.Count);
        }

        [Fact]
        public async Task EditAsync_PopulatesCharacteristicTypeNameListCorrectly()
        {
            // Arrange
            var expectedTypes = new List<VirusCharacteristicDataTypeDTO>
{
new VirusCharacteristicDataTypeDTO { Id = new Guid(), DataType = "Type1" },
new VirusCharacteristicDataTypeDTO { Id = new Guid(), DataType = "Type2" }
};
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(expectedTypes);

            // Act
            var result = await _controller.CreateAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicsModel>(viewResult.Model);
            Assert.Equal(2, model.CharacteristicTypeNameList?.Count);
            Assert.Equal(new Guid().ToString(), model.CharacteristicTypeNameList?[0].Value);
            Assert.Equal("Type1", model.CharacteristicTypeNameList?[0].Text);
            Assert.Equal(new Guid().ToString(), model.CharacteristicTypeNameList?[1].Value);
            Assert.Equal("Type2", model.CharacteristicTypeNameList?[1].Text);
        }

        [Fact]
        public async Task EditAsync_HandlesExceptionFromService()
        {
            // Arrange
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateAsync());
        }
        [Fact]
        public async Task Test_Edit_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel();
            var dto = new VirusCharacteristicDTO();
            _mockMapper.Map<VirusCharacteristicDTO>(model).Returns(dto);

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("List", redirectResult.ActionName);
            await _mockVirusCharacteristicService.Received(1).AddEntryAsync(Arg.Any<VirusCharacteristicDTO>());
            _mockMapper.Received(1).Map<VirusCharacteristicDTO>(model);
        }

        [Fact]
        public async Task Test_Edit_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel();
            _controller.ModelState.AddModelError("Error", "Model error");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.Equal(model, viewResult.Model);
        }

        [Fact]
        public async Task List_WithDefaultParameters_ReturnsCorrectViewModel()
        {
            // Arrange
            var expectedData = new PaginatedResult<VirusCharacteristicDTO>();
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync(1, 10).Returns(expectedData);
            _mockMapper.Map<List<VirusCharacteristicsModel>>(expectedData).Returns(new List<VirusCharacteristicsModel>());
                
            // Act
            var result = await _controller.List() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VirusCharacteristicsViewModel>(result.Model);
            var model = result.Model as VirusCharacteristicsViewModel;
            Assert.Equal(1, model?.Pagination?.PageNumber);
            Assert.Equal(10, model?.Pagination?.PageSize);
            Assert.Equal(0, model?.Pagination?.TotalCount);
        }

        [Fact]
        public async Task List_WithCustomParameters_ReturnsCorrectViewModel()
        {
            // Arrange
            var expectedData = new PaginatedResult<VirusCharacteristicDTO>();
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync(2, 20).Returns(expectedData);
            _mockMapper.Map<List<VirusCharacteristicsModel>>(expectedData).Returns(new List<VirusCharacteristicsModel>());

            // Act
            var result = await _controller.List(2, 20) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VirusCharacteristicsViewModel>(result.Model);
            var model = result.Model as VirusCharacteristicsViewModel;
            Assert.NotNull(model?.Pagination);
            Assert.Equal(2, model?.Pagination.PageNumber);
            Assert.Equal(20, model?.Pagination.PageSize);
        }

        [Fact]
        public async Task List_WithInvalidParameters_ReturnsCorrectViewModel()
        {
            // Arrange
            var expectedData = new PaginatedResult<VirusCharacteristicDTO>();
            //var expectedResult = (data: expectedData, TotalCount: 0);
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync(1, 10).Returns(expectedData);
            _mockMapper.Map<List<VirusCharacteristicsModel>>(expectedData).Returns(new List<VirusCharacteristicsModel>());

            // Act
            var result = await _controller.List(1, 10) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<VirusCharacteristicsViewModel>(result.Model);
            var model = result.Model as VirusCharacteristicsViewModel;
            Assert.NotNull(model?.Pagination);
            Assert.Equal(1, model?.Pagination.PageNumber);
            Assert.Equal(10, model?.Pagination.PageSize);
            Assert.Equal(0, model?.Pagination.TotalCount);
        }
        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_ValidInput_ReturnsPartialView()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;
            var mockResult = new PaginatedResult<VirusCharacteristicDTO>
            {
                data = new List<VirusCharacteristicDTO> { new VirusCharacteristicDTO() },
                TotalCount = 1
            };
            var entryViewModel = new VirusCharacteristicsModel();
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync(pageNo, pageSize).Returns(mockResult);
            _mockMapper.Map<IEnumerable<VirusCharacteristicsModel>>(mockResult.data)
                .Returns(new List<VirusCharacteristicsModel> { entryViewModel });

            // Act
            var result = await _controller.BindCharacteristicsGridOnPagination(pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_VirusCharatersticsList", partialViewResult.ViewName);
            var model = Assert.IsType<VirusCharacteristicsViewModel>(partialViewResult.Model);
            Assert.Equal(pageNo, model?.Pagination?.PageNumber);
            Assert.Equal(pageSize, model?.Pagination?.PageSize);
            Assert.Equal(1, model?.Pagination?.TotalCount);
        }

        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.BindCharacteristicsGridOnPagination(1, 10);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task Delete_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel();
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Delete(model, Guid.NewGuid());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
        }

        [Fact]
        public async Task Delete_EmptyGuid_ReturnsViewResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel();

            // Act
            var result = await _controller.Delete(model, Guid.Empty);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
        }

        [Fact]
        public async Task Delete_ValidModelAndId_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel { LastModified = new byte[8] };
            var id = Guid.NewGuid();

            // Act
            var result = await _controller.Delete(model, id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("List", redirectToActionResult.ActionName);
            await _mockVirusCharacteristicService.Received(1).DeleteVirusCharactersticsAsync(id, model.LastModified);
        }
    }


}
