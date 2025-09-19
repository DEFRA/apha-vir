using System.Security.Claims;
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
    public class SubmissionControllerTests
    {
        private readonly object _lock;
        private readonly SubmissionController _controller;
        private readonly ISubmissionService _submissionService;
        private readonly IMapper _mapper;
        private readonly ILookupService _lookupService;
        private readonly ISenderService _senderService;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public SubmissionControllerTests(AppRolesFixture fixture)
        {
            _submissionService = Substitute.For<ISubmissionService>();
            _lookupService = Substitute.For<ILookupService>();
            _senderService = Substitute.For<ISenderService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SubmissionController(_lookupService, _senderService, _submissionService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task SubmissionLetter_ReturnsViewResult_WithSubmissionLetterViewModel()
        {
            // Arrange
            string avNumber = "AV0000-01";
            string expectedLetterContent = "Test letter content";
            _submissionService.AVNumberExistsInVirAsync(avNumber).Returns(Task.FromResult(true));
            _submissionService.SubmissionLetter(avNumber, "TestUser").Returns(Task.FromResult(expectedLetterContent));
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.SubmissionLetter(avNumber);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SubmissionLetterViewModel>(viewResult.Model);
            Assert.Equal(expectedLetterContent, model.LetterContent);
        }

        [Fact]
        public async Task SubmissionLetter_ThrowsException_WhenServiceFails()
        {
            // Arrange
            string avNumber = "AV0000-01";
            _submissionService.AVNumberExistsInVirAsync(avNumber).Returns(Task.FromResult(true));
            _submissionService.SubmissionLetter(avNumber, "TestUser").Returns(Task.FromException<string>(new Exception("Service error")));
            SetupMockUserAndRoles();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.SubmissionLetter(avNumber));
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager),
                    new Claim(ClaimTypes.Name, "TestUser")
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.IsolateManager, AppRoleConstant.IsolateViewer, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}
