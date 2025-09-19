using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System.Security.Claims;


namespace Apha.VIR.Web.UnitTests.Controllers.SystemInformationControllerTest
{
    public class SystemInformationControllerTests
    {
        private readonly ISystemInfoService _mockSysInfoService;
        private readonly IMapper _mockMapper;
        private readonly IConfiguration _mockConfiguration;
        private readonly SystemInformationController _controller;

        public SystemInformationControllerTests()
        {
            _mockSysInfoService = Substitute.For<ISystemInfoService>();
            _mockMapper = Substitute.For<IMapper>();
            _mockConfiguration = Substitute.For<IConfiguration>();
            _controller = new SystemInformationController(_mockSysInfoService, _mockMapper, _mockConfiguration);
        }

        [Fact]
        public async Task SystemInfo_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var sysInfoDTO = new SystemInfoDto
            {
                Id = Guid.NewGuid(),
                SystemName = "VIRLocal",
                DatabaseVersion = "SQL 2022",
                ReleaseDate = DateTime.Now,
                Environment = "Unit Test",
                Live = false,
                ReleaseNotes = "Unit Test release"
            };
            var sysInfoViewModel = new SystemInformationViewModel
            {
                SystemName = sysInfoDTO.SystemName,
                DatabaseVersion = sysInfoDTO.DatabaseVersion,
                ReleaseDate = sysInfoDTO.ReleaseDate,
                Environment = sysInfoDTO.Environment,
                Live = sysInfoDTO.Live,
                ReleaseNotes = sysInfoDTO.ReleaseNotes
            };
            _mockSysInfoService.GetLatestSysInfo().Returns(sysInfoDTO);
            _mockMapper.Map<SystemInformationViewModel>(sysInfoDTO).Returns(sysInfoViewModel);

            _mockConfiguration["URL:LogMonitor"].Returns("http://example.com/logs");

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.AuthenticationMethod, "TestAuth")
            }, "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            _controller.ControllerContext.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SystemInformationViewModel>(viewResult.Model);
            Assert.Equal(sysInfoViewModel, model);
            Assert.Equal("TestUser", model.UserName);
            Assert.Equal("127.0.0.1", model.HostAddress);
            Assert.Equal("TestAuth", model.AuthenticationType);
            Assert.Equal("True", model.IsAuthenticated);
            Assert.Equal(Environment.Version.ToString(), model.FrameworkVersion);
            Assert.Equal("http://example.com/logs", _controller.ViewBag.ErrorLogUrl);
        }

        [Fact]
        public async Task SystemInfo_HandlesNullIdentity()
        {
            // Arrange
            _mockSysInfoService.GetLatestSysInfo().Returns(new SystemInfoDto());
            _mockMapper.Map<SystemInformationViewModel>(Arg.Any<SystemInfoDto>()).Returns(new SystemInformationViewModel());
            _mockConfiguration["URL:LogMonitor"].Returns("http://example.com/logs");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SystemInformationViewModel>(viewResult.Model);
            Assert.Null(model.UserName);
            Assert.Null(model.AuthenticationType);
            Assert.Equal("False", model.IsAuthenticated);
        }

        [Fact]
        public async Task SystemInfo_HandlesNullRemoteIpAddress()
        {
            // Arrange
            _mockSysInfoService.GetLatestSysInfo().Returns(new SystemInfoDto());
            _mockMapper.Map<SystemInformationViewModel>(Arg.Any<SystemInfoDto>()).Returns(new SystemInformationViewModel());
            _mockConfiguration["URL:LogMonitor"].Returns("http://example.com/logs");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SystemInformationViewModel>(viewResult.Model);
            Assert.Null(model.HostAddress);
        }

        [Fact]
        public async Task SystemInfo_ThrowsExceptionForMissingConfiguration()
        {
            // Arrange
            _mockSysInfoService.GetLatestSysInfo().Returns(new SystemInfoDto());
            _mockMapper.Map<SystemInformationViewModel>(Arg.Any<SystemInfoDto>()).Returns(new SystemInformationViewModel());
            _mockConfiguration["URL:LogMonitor"].Returns((string?)null);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index());
        }
    }
}
