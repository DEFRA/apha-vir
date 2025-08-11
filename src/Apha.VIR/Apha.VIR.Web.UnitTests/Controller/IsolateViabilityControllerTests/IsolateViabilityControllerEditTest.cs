using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateViabilityControllerTests
{
    public class IsolateViabilityControllerEditTest
    {
        private readonly IIsolateViabilityService _isolateViabilityService;
        private readonly IMapper _mapper;
        private readonly IsolateViabilityController _controller;
        private readonly ILookupService _lookupService;
        public IsolateViabilityControllerEditTest()
        {
            _lookupService = Substitute.For<ILookupService>();
            _isolateViabilityService = Substitute.For<IIsolateViabilityService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateViabilityController(_isolateViabilityService, _lookupService, _mapper);
        }

        [Fact]
        public async Task Edit_Get_ValidInput_ReturnsViewResult()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            var viabilityHistory = new List<IsolateViabilityInfoDTO> { new IsolateViabilityInfoDTO { IsolateViabilityId = isolateViabilityId } };
            var isolateViabilityModelList = new List<IsolateViabilityModel> { new IsolateViabilityModel { IsolateViabilityId = isolateViabilityId } };
            var MapviabilityList = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Viability" } };
            var MapStaffList = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Test Staff" } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(viabilityHistory);
            _lookupService.GetAllViabilityAsync().Returns(MapviabilityList);
            _lookupService.GetAllStaffAsync().Returns(MapStaffList);

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViabilityInfoDTO>>())
           .Returns(isolateViabilityModelList);

            _mapper.Map<IsolateViabilityModel>(Arg.Any<IsolateViabilityModel>()).Returns(isolateViabilityModelList.First());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViability>>()).Returns(isolateViabilityModelList);

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            var model = Assert.IsType<IsolateViabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.IsolateViability);
            Assert.NotNull(model.ViabilityList);
            Assert.NotEmpty(model.ViabilityList);
            Assert.NotEmpty(model.ViabilityList);
            Assert.NotNull(model.CheckedByList);
            Assert.NotEmpty(model.CheckedByList);
            Assert.NotEmpty(model.CheckedByList);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Edit_Get_InvalidAVNumber_ReturnsBadRequest(string avNumber)
        {
            // Arrange
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_InvalidIsolate_ReturnsBadRequest()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.Empty;
            var isolateViabilityId = Guid.NewGuid();

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task Edit_Get_MultipleViabilityHistoriesFound_ReturnsViewResultWithFirstHistory()
        {
            // Arrange
            var avNumber = "AV123";
            var isolate = Guid.NewGuid();
            var isolateViabilityId = Guid.NewGuid();
            var viabilityHistory = new List<IsolateViabilityInfoDTO>
                                    {
                                    new IsolateViabilityInfoDTO { IsolateViabilityId = isolateViabilityId },
                                    new IsolateViabilityInfoDTO { IsolateViabilityId = Guid.NewGuid() }
                                    };
            var isolateViabilityModelList = new List<IsolateViabilityModel>
            { new IsolateViabilityModel { IsolateViabilityId = isolateViabilityId } };

            _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolate).Returns(viabilityHistory);
            _lookupService.GetAllViabilityAsync().Returns(new List<LookupItemDTO>());
            _lookupService.GetAllStaffAsync().Returns(new List<LookupItemDTO>());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViabilityInfoDTO>>())
            .Returns(isolateViabilityModelList);

            _mapper.Map<IsolateViabilityModel>(Arg.Any<IsolateViabilityModel>()).Returns(isolateViabilityModelList.First());

            _mapper.Map<IEnumerable<IsolateViabilityModel>>(Arg.Any<IEnumerable<IsolateViability>>()).Returns(isolateViabilityModelList);

            // Act
            var result = await _controller.Edit(avNumber, isolate, isolateViabilityId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            var model = Assert.IsType<IsolateViabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.IsolateViability);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new IsolateViabilityViewModel
            {
                IsolateViability = new IsolateViabilityModel
                {
                    AVNumber = "AV123",
                    IsolateViabilityIsolateId = Guid.NewGuid()
                }
            };

            var dto = new IsolateViabilityInfoDTO();
            _mapper.Map<IsolateViabilityInfoDTO>(model.IsolateViability).Returns(dto);

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(IsolateViabilityController.History), redirectResult.ActionName);
            Assert.NotNull(redirectResult);
            Assert.NotNull(redirectResult.RouteValues);
            Assert.True(redirectResult.RouteValues.ContainsKey("AVNumber"));
            Assert.Equal(model.IsolateViability.AVNumber, redirectResult.RouteValues["AVNumber"]);
            Assert.Equal(model.IsolateViability.IsolateViabilityIsolateId, redirectResult.RouteValues["Isolate"]);

            await _isolateViabilityService.Received(1).UpdateIsolateViabilityAsync(dto, "TestUser");
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsBadRequestResult()
        {
            // Arrange
            var model = new IsolateViabilityViewModel { IsolateViability= new IsolateViabilityModel
            { IsolateViabilityId=Guid.NewGuid()} };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
         
            Assert.NotNull(badRequestResult);
            Assert.NotNull(badRequestResult.Value);

            var serializableError = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(serializableError.ContainsKey("ModelError"));

            var error = serializableError["ModelError"];
            Assert.NotNull(error);
            var errormsg = ((string[])error)[0];
            Assert.Equal("Invalid parameters.", errormsg);
        }
    }
}