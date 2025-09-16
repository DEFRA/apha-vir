using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class VirusCharacteristicsControllerTests
    {
        private readonly object _lock;
        private readonly VirusCharacteristicsController _controller;
        private readonly IVirusCharacteristicService _mockVirusCharacteristicService;
        private readonly IMapper _mockMapper;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public VirusCharacteristicsControllerTests(AppRolesFixture fixture)
        {
            _mockVirusCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_mockVirusCharacteristicService, _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public void Index_ReturnsVirusCharacteristicManagementView()
        {
            // Arrange
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.LookupDataManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicManagement", viewResult.ViewName);
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
        public async Task BindCharacteristicsGridOnPagination_ValidInput_ReturnsPartialView()
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
        public async Task BindCharacteristicsGridOnPagination_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.BindCharacteristicsGridOnPagination(1, 10);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAsync_Get_ReturnsViewResultWithCorrectModel()
        {
            // Arrange
            var expectedTypes = new List<VirusCharacteristicDataTypeDTO>
            {
                new VirusCharacteristicDataTypeDTO { Id = Guid.NewGuid(), DataType = "Type1" },
                new VirusCharacteristicDataTypeDTO { Id = Guid.NewGuid(), DataType = "Type2" }
            };

            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(expectedTypes);

            // Act
            var result = await _controller.CreateAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicsModel>(viewResult.Model);
            Assert.NotNull(model.CharacteristicTypeNameList);
            Assert.Empty(model.CharacteristicTypeNameList);
        }

        [Fact]
        public async Task CreateAsync_Get_PopulatesCharacteristicTypeNameListCorrectly()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            // Arrange
            var expectedTypes = new List<VirusCharacteristicDataTypeDTO>
            {
                new VirusCharacteristicDataTypeDTO { Id = id1, DataType = "Type1" },
                new VirusCharacteristicDataTypeDTO { Id = id2, DataType = "Type2" }
            };

            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(expectedTypes);

            // Act
            var result = await _controller.CreateAsync();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicsModel>(viewResult.Model);
            Assert.Equal(2, expectedTypes.Count);
            Assert.Equal(id1, expectedTypes[0].Id);
            Assert.Equal("Type1", expectedTypes[0].DataType);
        }

        [Fact]
        public async Task CreateAsync_Get_HandlesExceptionFromService()
        {
            // Arrange
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.CreateAsync());
        }
        
        [Fact]
        public async Task Create_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new VirusCharacteristicsModel
            {
                Name = "test",
                DisplayOnSearch = true,
                CharacteristicIndex = 0,
            };
 
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
        public async Task EditAsync_Get_ValidId_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var id = Guid.NewGuid();
            var virusTypeId = Guid.NewGuid();
            var virusCharacteristicDto = new VirusCharacteristicDTO { Id = id, Name = "Test Characteristic" };
            var virusCharacteristicModel = new VirusCharacteristicsModel { Id = id, Name = "Test Characteristic" };
            var virusTypesDto = new List<VirusCharacteristicDataTypeDTO> { new VirusCharacteristicDataTypeDTO { Id = virusTypeId, DataType = "Type1" } };
            var virusTypes = new List<VirusCharacteristicDataType> { new VirusCharacteristicDataType { Id = virusTypeId, DataType = "Type1" } };

            _mockVirusCharacteristicService.GetVirusCharacteristicsByIdAsync(id).Returns(virusCharacteristicDto);
            _mockMapper.Map<VirusCharacteristicsModel>(virusCharacteristicDto).Returns(virusCharacteristicModel);
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(virusTypesDto);
            _mockMapper.Map<List<VirusCharacteristicDataType>>(virusTypesDto).Returns(virusTypes);

            // Act
            var result = await _controller.EditAsync(id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<VirusCharacteristicsModel>(viewResult.Model);
            Assert.Equal(id, model.Id);
            Assert.Equal("Test Characteristic", model.Name);
            Assert.NotEmpty(model.CharacteristicTypeNameList!);
            Assert.Single(model.CharacteristicTypeNameList!);
            Assert.Equal(virusTypeId.ToString(), model.CharacteristicTypeNameList![0].Value.ToString());
            Assert.Equal("Type1", model.CharacteristicTypeNameList[0].Text);

            await _mockVirusCharacteristicService.Received(1).GetVirusCharacteristicsByIdAsync(id);
            await _mockVirusCharacteristicService.Received(1).GetAllVirusCharactersticsTypeNamesAsync();
        }

        [Fact]
        public async Task EditAsync_Get_NoDataReturned_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockVirusCharacteristicService.GetVirusCharacteristicsByIdAsync(id).Returns((VirusCharacteristicDTO?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _controller.EditAsync(id));
        }

        [Fact]
        public async Task EditAsync_Get_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var id = Guid.Empty;

            // Act
            var result = await _controller.EditAsync(id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsViewResult()
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
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var model = new VirusCharacteristicsModel 
            { 
                Id = id, 
                Name = "Test",
                CharacteristicType=Guid.NewGuid(),
                NumericSort=true,
                DisplayOnSearch=false,
                CharacteristicIndex=1,
                LastModified = new byte[8]
            };

            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync()
            .Returns(new[] { new VirusCharacteristicDTO { Id = id } });
     
            _mockMapper.Map<VirusCharacteristicDTO>(model).Returns(new VirusCharacteristicDTO());
   
            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("List", redirectToActionResult.ActionName);
            await _mockVirusCharacteristicService.Received(1).UpdateEntryAsync(Arg.Any<VirusCharacteristicDTO>());
        }

        [Fact]
        public async Task Edit_Post_ValidationErrors_ReturnsViewWithErrors()
        {
            // Arrange
            var model = new VirusCharacteristicsModel { Id = Guid.NewGuid(), Name = "Test" };
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO>());
            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(new List<VirusCharacteristicDataTypeDTO>());

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.True(_controller.ViewBag.showsummary);
            Assert.NotEmpty(_controller.ModelState);
        }

        [Fact]
        public async Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
        {
            // Arrange
            var model = new VirusCharacteristicsModel { Id = Guid.NewGuid(), Name = "Test" };
            _mockMapper.Map<VirusCharacteristicDTO>(model).Returns(new VirusCharacteristicDTO());
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO>());
            _mockVirusCharacteristicService.UpdateEntryAsync(Arg.Any<VirusCharacteristicDTO>()).Throws(new Exception("Test exception"));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.NotEmpty(_controller.ModelState);
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
            var expectedTypes = new List<VirusCharacteristicDataTypeDTO>();
            var id = Guid.NewGuid();
            var model = new VirusCharacteristicsModel
            {
                Id = id,
                Name = "Test",
                CharacteristicType = Guid.NewGuid(),
                NumericSort = true,
                DisplayOnSearch = false,
                CharacteristicIndex = 1,
                LastModified = new byte[8]
            };

            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync()
            .Returns(new[] { new VirusCharacteristicDTO { Id = id } });

            _mockMapper.Map<VirusCharacteristicDTO>(model).Returns(new VirusCharacteristicDTO());

            _mockVirusCharacteristicService.GetAllVirusCharactersticsTypeNamesAsync().Returns(expectedTypes);
      
            // Act
            var result = await _controller.Delete(model, id);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("List", redirectToActionResult.ActionName);
            await _mockVirusCharacteristicService.Received(1).DeleteVirusCharactersticsAsync(id, model.LastModified);
        }
    }
}
