using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Controller
{
    public class IsolateDispatchControllerTests
    {
        private readonly IIsolateDispatchService _mockIsolateDispatchService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;
        private readonly ILookupService _mockLookupService;

        public IsolateDispatchControllerTests()
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = Substitute.For<ILookupService>();
            _controller = new IsolateDispatchController(_mockIsolateDispatchService, _mockMapper, _mockLookupService);

        }

        [Fact]
        public async Task History_ReturnsViewModelWithData_WhenServiceReturnsValidData()
        {
            // Arrange
            var avNumber = "AV123456-01";
            var isolateId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var isolateDispatchInfoDTOs = new List<IsolateDispatchInfoDTO>
            {
                new IsolateDispatchInfoDTO { Nomenclature = "Test Nomenclature" }
            };
            var isolateDispatchHistories = new List<IsolateDispatchHistory>
            {
                new IsolateDispatchHistory
                {
                    DispatchIsolateId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    IsolateId = isolateId,
                    DispatchId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    NoOfAliquots = 5,
                    PassageNumber = 2,
                    Recipient = "recipient@test.com",
                    RecipientName = "Test Recipient",
                    RecipientAddress = "123 Test Street, Test City",
                    ReasonForDispatch = "Research",
                    DispatchedDate = new DateTime(2024, 1, 1),
                    DispatchedByName = "Test Dispatcher",
                    Avnumber = avNumber,
                    Nomenclature = "Test Nomenclature",
                    LastModified = new byte[] { 1, 2, 3, 4 }
                }
            };

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)isolateDispatchInfoDTOs));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>())
                .Returns(isolateDispatchHistories);

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchHistoryViewModel>(viewResult.Model);
            Assert.Equal("Test Nomenclature", model.Nomenclature);
            Assert.Equal(isolateDispatchHistories, model.DispatchHistoryRecords);
        }

        [Fact]
        public async Task History_ReturnsNullViewModel_WhenServiceReturnsEmptyList()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)new List<IsolateDispatchInfoDTO>()));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDTO>>())
                .Returns(new List<IsolateDispatchHistory>());

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task History_HandlesException_WhenServiceThrowsException()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Throws(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.History(avNumber, isolateId));
        }

        [Fact]
        public async Task History_CallsServiceWithCorrectParameters()
        {
            // Arrange
            var avNumber = "AV000000-01";
            var isolateId = Guid.Parse("2066E698-B746-493A-AB14-B30800CB75A8");

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDTO>)new List<IsolateDispatchInfoDTO>()));

            // Act
            await _controller.History(avNumber, isolateId);

            // Assert
            await _mockIsolateDispatchService.Received(1).GetDispatchesHistoryAsync(
                Arg.Is<string>(s => s == avNumber),
                Arg.Is<Guid>(g => g == isolateId)
            );
        }


        [Theory]
        [InlineData("", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [InlineData("AVN001", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [InlineData("AVN001", "11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000")]
        public async Task Edit_InvalidInput_ReturnsBadRequest(string avNumber, string dispatchId, string dispatchIsolateId)
        {
            // Act
            var result = await _controller.Edit(avNumber, Guid.Parse(dispatchId), Guid.Parse(dispatchIsolateId));

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_ValidInput_InternalRecipient_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var avNumber = "AVN001";
            var dispatchId = Guid.NewGuid();
            var dispatchIsolateId = Guid.NewGuid();

            var isolateDispatchInfoDTO = new IsolateDispatchInfoDTO { RecipientId = Guid.NewGuid() };
            _mockIsolateDispatchService.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId)
            .Returns(isolateDispatchInfoDTO);

            var viabilityLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Viability1" } };
            var workGroupLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "WorkGroup1" } };
            var staffLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff1" } };

            _mockLookupService.GetAllViabilityAsync().Returns(viabilityLookup);
            _mockLookupService.GetAllWorkGroupsAsync().Returns(workGroupLookup);
            _mockLookupService.GetAllStaffAsync().Returns(staffLookup);

            var viewModel = new IsolateDispatchEditViewModel();
            _mockMapper.Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO).Returns(viewModel);

            // Act
            var result = await _controller.Edit(avNumber, dispatchId, dispatchIsolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchEditViewModel>(viewResult.Model);
            Assert.Equal("Internal", model.RecipientLocation);
            Assert.NotNull(model.ViabilityList);
            Assert.NotNull(model.RecipientList);
            Assert.NotNull(model.DispatchedByList);

            await _mockIsolateDispatchService.Received(1).GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId);
            await _mockLookupService.Received(1).GetAllViabilityAsync();
            await _mockLookupService.Received(1).GetAllWorkGroupsAsync();
            await _mockLookupService.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO);
        }

        [Fact]
        public async Task Edit_ValidInput_ExternalRecipient_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var avNumber = "AVN001";
            var dispatchId = Guid.NewGuid();
            var dispatchIsolateId = Guid.NewGuid();

            var isolateDispatchInfoDTO = new IsolateDispatchInfoDTO { RecipientId = null };
            _mockIsolateDispatchService.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId)
            .Returns(isolateDispatchInfoDTO);

            var viabilityLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Viability1" } };
            var workGroupLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "WorkGroup1" } };
            var staffLookup = new List<LookupItemDTO> { new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff1" } };

            _mockLookupService.GetAllViabilityAsync().Returns(viabilityLookup);
            _mockLookupService.GetAllWorkGroupsAsync().Returns(workGroupLookup);
            _mockLookupService.GetAllStaffAsync().Returns(staffLookup);

            var viewModel = new IsolateDispatchEditViewModel();
            _mockMapper.Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO).Returns(viewModel);

            // Act
            var result = await _controller.Edit(avNumber, dispatchId, dispatchIsolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchEditViewModel>(viewResult.Model);
            Assert.Equal("External", model.RecipientLocation);
            Assert.NotNull(model.ViabilityList);
            Assert.NotNull(model.RecipientList);
            Assert.NotNull(model.DispatchedByList);

            await _mockIsolateDispatchService.Received(1).GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId);
            await _mockLookupService.Received(1).GetAllViabilityAsync();
            await _mockLookupService.Received(1).GetAllWorkGroupsAsync();
            await _mockLookupService.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IsolateDispatchEditViewModel>(isolateDispatchInfoDTO);
        }

        [Fact]
        public async Task Edit_ModelStateInvalid_ReturnsViewResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model State is invalid");

            // Act
            var result = await _controller.Edit("AVN001", Guid.NewGuid(), Guid.NewGuid());

            // Assert
            Assert.IsType<ViewResult>(result);
        }




        [Fact]
        public async Task Edit_ValidInput_SuccessfulUpdateAndRedirect()
        {
            // Arrange
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[] { 0x00, 0x01 }
            };

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDTO>());

            _mockMapper.Map<IsolateDispatchInfoDTO>(model).Returns(new IsolateDispatchInfoDTO());

            // Act
            var result = await _controller.Edit(model) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("History", result.ActionName);
            Assert.Equal(model.Avnumber, result.RouteValues["AVNumber"]);
            Assert.Equal(model.DispatchIsolateId, result.RouteValues["IsolateId"]);
            await _mockIsolateDispatchService.Received(1).UpdateDispatchAsync(Arg.Any<IsolateDispatchInfoDTO>(), "TestUser");
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new IsolateDispatchEditViewModel();
            _controller.ModelState.AddModelError("Error", "Sample error");

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.Edit(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public async Task Edit_ExceptionThrownByUpdateDispatchAsync_HandlesErrorAppropriately()
        {
            // Arrange
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = "AV123",
                DispatchIsolateId = Guid.NewGuid(),
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                LastModified = new byte[] { 0x00, 0x01 }
            };

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDTO>());

            _mockMapper.Map<IsolateDispatchInfoDTO>(model).Returns(new IsolateDispatchInfoDTO());

            _mockIsolateDispatchService.UpdateDispatchAsync(Arg.Any<IsolateDispatchInfoDTO>(), Arg.Any<string>())
            .Returns(Task.FromException(new Exception("Update failed")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(model));
        }


    }
}
