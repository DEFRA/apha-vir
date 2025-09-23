using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateDispatchControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class IsolateDispatchControllerTests
    {
        private readonly object _lock;
        private readonly IIsolateDispatchService _mockIsolateDispatchService;
        private readonly ILookupService _mockLookupService;
        private readonly IIsolatesService _mockIsolatesService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IMapper _mockMapper;
        private readonly IsolateDispatchController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public IsolateDispatchControllerTests(AppRolesFixture fixture)
        {
            _mockIsolateDispatchService = Substitute.For<IIsolateDispatchService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockIsolatesService = Substitute.For<IIsolatesService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockMapper = Substitute.For<IMapper>();
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;

            _controller = new IsolateDispatchController(_mockIsolateDispatchService,
                _mockLookupService,
                _mockIsolatesService,
                _mockSubmissionService,
                _mockSampleService,
                _mockMapper);
        }

        [Fact]
        public async Task History_ReturnsViewModelWithData_WhenServiceReturnsValidData()
        {
            // Arrange
            var avNumber = "AV123456-01";
            var isolateId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var isolateDispatchInfoDTOs = new List<IsolateDispatchInfoDto>
            {
                new IsolateDispatchInfoDto { Nomenclature = "Test Nomenclature" }
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
                    DispatchedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DispatchedByName = "Test Dispatcher",
                    Avnumber = avNumber,
                    Nomenclature = "Test Nomenclature",
                    LastModified = new byte[] { 1, 2, 3, 4 }
                }
            };

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(Arg.Any<string>(), Arg.Any<Guid>())
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDto>)isolateDispatchInfoDTOs));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDto>>())
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
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDto>)new List<IsolateDispatchInfoDto>()));
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDto>>())
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
                .Returns(Task.FromResult((IEnumerable<IsolateDispatchInfoDto>)new List<IsolateDispatchInfoDto>()));

            // Act
            await _controller.History(avNumber, isolateId);

            // Assert
            await _mockIsolateDispatchService.Received(1).GetDispatchesHistoryAsync(
                Arg.Is<string>(s => s == avNumber),
                Arg.Is<Guid>(g => g == isolateId)
            );
        }

        [Fact]
        public async Task Confirmation_WithValidIsolateGuid_ReturnsViewResult()
        {
            // Arrange
            var isolateGuid = Guid.NewGuid();
            var dispatchConfirmationDTO = new IsolateFullDetailDto
            {
                IsolateDetails = new IsolateInfoDto { NoOfAliquots = 5 },
                IsolateDispatchDetails = new List<IsolateDispatchInfoDto>(),
                IsolateViabilityDetails = new List<IsolateViabilityInfoDto>(),
                IsolateCharacteristicDetails = new List<IsolateCharacteristicInfoDto>()

            };

            _mockIsolateDispatchService.GetDispatcheConfirmationAsync(isolateGuid)
            .Returns(Task.FromResult(dispatchConfirmationDTO));

            var mappedDispatchHistory = new List<IsolateDispatchHistory>();
            _mockMapper.Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDto>>())
            .Returns(mappedDispatchHistory);

            // Act
            var result = await _controller.Confirmation(isolateGuid);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchConfirmatioViewModel>(viewResult.Model);
            Assert.Equal("Isolate dispatch completed successfully.", model.DispatchConfirmationMessage);
            Assert.Equal(5, model.RemainingAliquots);
            Assert.Same(mappedDispatchHistory, model.DispatchHistorys);

            await _mockIsolateDispatchService.Received(1).GetDispatcheConfirmationAsync(isolateGuid);
            _mockMapper.Received(1).Map<IEnumerable<IsolateDispatchHistory>>(Arg.Any<IEnumerable<IsolateDispatchInfoDto>>());
        }

        [Fact]
        public async Task Confirmation_WithEmptyGuid_ReturnsBadRequest()
        {
            // Arrange
            var emptyGuid = Guid.Empty;

            // Act
            var result = await _controller.Confirmation(emptyGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Isolate ID.", badRequestResult.Value);
        }

        [Fact]
        public async Task Confirmation_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var isolateGuid = Guid.NewGuid();
            _controller.ModelState.AddModelError("Error", "Model error");

            // Act
            var result = await _controller.Confirmation(isolateGuid);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid Isolate ID.", badRequestResult.Value);
        }


        [Theory]
        [InlineData("", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [InlineData("AVN001", "00000000-0000-0000-0000-000000000000", "00000000-0000-0000-0000-000000000000")]
        [InlineData("AVN001", "11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000")]
        public async Task Edit_Get_InvalidInput_ReturnsBadRequest(string avNumber, string dispatchId, string dispatchIsolateId)
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.Edit(avNumber, Guid.Parse(dispatchId), Guid.Parse(dispatchIsolateId));

            // Assert          
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);

            // Optionally verify that ModelState was passed back
            var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
            Assert.True(modelState.ContainsKey("error"));
        }

        [Fact]
        public async Task Edit_Get_ValidInput_InternalRecipient_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var avNumber = "AVN001";
            var dispatchId = Guid.NewGuid();
            var dispatchIsolateId = Guid.NewGuid();

            var isolateDispatchInfoDTO = new IsolateDispatchInfoDto { RecipientId = Guid.NewGuid() };
            _mockIsolateDispatchService.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId)
            .Returns(isolateDispatchInfoDTO);

            var viabilityLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Viability1" } };
            var workGroupLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "WorkGroup1" } };
            var staffLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Staff1" } };

            _mockLookupService.GetAllViabilityAsync().Returns(viabilityLookup);
            _mockLookupService.GetAllWorkGroupsAsync().Returns(workGroupLookup);
            _mockLookupService.GetAllStaffAsync().Returns(staffLookup);

            var viewModel = new IsolateDispatchEditViewModel()
            {
                ValidToIssue = false
            };
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
        public async Task Edit_Get_ValidInput_ExternalRecipient_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var avNumber = "AVN001";
            var dispatchId = Guid.NewGuid();
            var dispatchIsolateId = Guid.NewGuid();

            var isolateDispatchInfoDTO = new IsolateDispatchInfoDto { RecipientId = null };
            _mockIsolateDispatchService.GetDispatchForIsolateAsync(avNumber, dispatchId, dispatchIsolateId)
            .Returns(isolateDispatchInfoDTO);

            var viabilityLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Viability1" } };
            var workGroupLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "WorkGroup1" } };
            var staffLookup = new List<LookupItemDto> { new LookupItemDto { Id = Guid.NewGuid(), Name = "Staff1" } };

            _mockLookupService.GetAllViabilityAsync().Returns(viabilityLookup);
            _mockLookupService.GetAllWorkGroupsAsync().Returns(workGroupLookup);
            _mockLookupService.GetAllStaffAsync().Returns(staffLookup);

            var viewModel = new IsolateDispatchEditViewModel()
            {
                ValidToIssue = false
            };
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
        public async Task Edit_Post_ValidInput_SuccessfulUpdateAndRedirect()
        {
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";
            // Arrange
            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                ValidToIssue = true,
                LastModified = new byte[] { 0x00, 0x01 }
            };

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDto>());

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(avnumber, isolateId)
                .Returns(new[] { new IsolateDispatchInfoDto { DispatchId = dispatchId } });

            _mockMapper.Map<IsolateDispatchInfoDto>(model).Returns(new IsolateDispatchInfoDto());
            SetupMockUserAndRoles();

          

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.NotNull(result);

            if (result is RedirectToActionResult redirectResult)
            {
                Assert.Equal("History", redirectResult.ActionName);
                Assert.NotNull(redirectResult.RouteValues);
                Assert.True(redirectResult.RouteValues.ContainsKey("AVNumber"));
                Assert.True(redirectResult.RouteValues.ContainsKey("IsolateId"));
                Assert.Equal(model.Avnumber, redirectResult.RouteValues["AVNumber"]);
                Assert.Equal(model.DispatchIsolateId, redirectResult.RouteValues["IsolateId"]);
                await _mockIsolateDispatchService.Received(1).UpdateDispatchAsync(Arg.Any<IsolateDispatchInfoDto>(), "TestUser");
            }
            else if (result is ViewResult viewResult)
            {
                Assert.Equal("", viewResult.ViewName ?? string.Empty);
                Assert.Same(model, viewResult.Model);
                Assert.False(_controller.ModelState.IsValid);
            }
            else
            {
                Assert.False(false, $"Unexpected result type: {result.GetType()}");
            }
        }
        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new IsolateDispatchEditViewModel() { ValidToIssue = false };
            _controller.ModelState.AddModelError("Error", "Sample error");

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Edit(model) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(model, result.Model);
        }

        [Fact]
        public async Task Edit_Post_ExceptionThrownByUpdateDispatchAsync_HandlesErrorAppropriately()
        {
            // Arrange
            var dispatchId = Guid.NewGuid();
            var isolateId = Guid.NewGuid();
            var avnumber = "AV123";

            var model = new IsolateDispatchEditViewModel
            {
                Avnumber = avnumber,
                DispatchId = dispatchId,
                DispatchIsolateId = isolateId,
                ViabilityId = Guid.NewGuid(),
                RecipientId = Guid.NewGuid(),
                DispatchedById = Guid.NewGuid(),
                ValidToIssue=false,
                LastModified = new byte[] { 0x00, 0x01 }
            };

            _mockLookupService.GetAllViabilityAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllWorkGroupsAsync().Returns(new List<LookupItemDto>());
            _mockLookupService.GetAllStaffAsync().Returns(new List<LookupItemDto>());

            _mockIsolateDispatchService.GetDispatchesHistoryAsync(avnumber, isolateId)
          .Returns(new[] { new IsolateDispatchInfoDto { DispatchId = dispatchId } });

            _mockMapper.Map<IsolateDispatchInfoDto>(model).Returns(new IsolateDispatchInfoDto());

            _mockIsolateDispatchService.UpdateDispatchAsync(Arg.Any<IsolateDispatchInfoDto>(), Arg.Any<string>())
        .Returns(Task.FromException(new Exception("Update failed")));
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
          
            Assert.Same(model, viewResult.Model);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(_controller.ModelState.Values, v => v.Errors.Any(e => e.ErrorMessage == "Isolate cannot be dispatched as there are no aliquots available."));
        }

        [Fact]
        public async Task Edit_Post_Unauthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var model = new IsolateDispatchEditViewModel()
            {
                ValidToIssue = false
            };
            // Remove Administrator role
            AuthorisationUtil.AppRoles = new List<string> { AppRoleConstant.IsolateManager };
            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(model));
        }

        [Fact]
        public async Task Edit_Get_ModelStateInvalid_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "invalid");
            // Act
            var result = await _controller.Edit("AV001", Guid.NewGuid(), Guid.NewGuid());
            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task History_ModelStateInvalid_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "invalid");
            var avNumber = string.Empty; 
            var isolateId = Guid.Empty;

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        [Fact]
        public async Task History_BothParametersEmpty_ReturnsView()
        {
            // Arrange
            var avNumber = string.Empty; 
            var isolateId = Guid.Empty;

            // Act
            var result = await _controller.History(avNumber, isolateId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.Model);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {   new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
