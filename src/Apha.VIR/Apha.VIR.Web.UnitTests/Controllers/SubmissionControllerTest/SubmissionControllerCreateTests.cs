using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SubmissionControllerTest
{
    [Collection("UserAppRolesValidationTests")]    
    public class SubmissionControllerCreateTests
    {
        private readonly object _lock;
        private readonly ILookupService _mockLookupService;
        private readonly ISenderService _mockSenderService;
        private readonly ISubmissionService _mockSubmissionService;
        private readonly ISampleService _mockSampleService;
        private readonly IIsolatesService _mockIsolatedService;
        private readonly IMapper _mockMapper;
        private readonly SubmissionController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public SubmissionControllerCreateTests(AppRolesFixture fixture)
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockSenderService = Substitute.For<ISenderService>();
            _mockSubmissionService = Substitute.For<ISubmissionService>();
            _mockSampleService = Substitute.For<ISampleService>();
            _mockIsolatedService = Substitute.For<IIsolatesService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SubmissionController(_mockLookupService, 
                _mockSenderService, 
                _mockSubmissionService, 
                _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithCorrectModel()
        {
            // Arrange
            const string avNumber = "TEST123";
            var ddldata = new List<LookupItemDto>
            {
                new LookupItemDto{ Id = Guid.NewGuid(), Name = "lookitem_1" },
                new LookupItemDto{ Id = Guid.NewGuid(), Name = "lookitem_2" }
            };

            _mockLookupService.GetAllCountriesAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmittingLabAsync().Returns(ddldata.AsEnumerable());
            _mockLookupService.GetAllSubmissionReasonAsync().Returns(ddldata.AsEnumerable());

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<SubmissionCreateViewModel>(result.Model);
            Assert.Equal(avNumber, model.AVNumber);
            Assert.NotEmpty(model.CountryList!);
            Assert.NotEmpty(model.SubmittingLabList!);
            Assert.NotEmpty(model.SubmissionReasonList!);
            Assert.Empty(model.Senders!);
            Assert.Empty(model.Organisations!);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var submission = new SubmissionCreateViewModel();
            _controller.ModelState.AddModelError("Error", "Test error");
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Create(submission) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SubmissionCreateViewModel>(result.Model);
            await _mockLookupService.Received(1).GetAllCountriesAsync();
            await _mockLookupService.Received(1).GetAllSubmittingLabAsync();
            await _mockLookupService.Received(1).GetAllSubmissionReasonAsync();
        }

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var submission = new SubmissionCreateViewModel();
            var SubmissionDto = new SubmissionDto();
            _mockMapper.Map<SubmissionCreateViewModel>(SubmissionDto).Returns(submission);
            SetupMockUserAndRoles();

            // Act
            var result = await _controller.Create(submission) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("SubmissionSamples", result.ControllerName);
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
