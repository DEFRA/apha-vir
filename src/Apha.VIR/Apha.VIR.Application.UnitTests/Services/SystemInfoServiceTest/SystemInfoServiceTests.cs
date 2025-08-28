using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using YourNamespace.Services;

namespace Apha.VIR.Application.UnitTests.Services.SystemInfoServiceTest
{
    public class SystemInfoServiceTests
    {
        private readonly ISystemInfoRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly SystemInfoService _systemInfoService;

        public SystemInfoServiceTests()
        {
            _mockRepository = Substitute.For<ISystemInfoRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _systemInfoService = new SystemInfoService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetLatestSysInfo_ReturnsValidSystemInfoDTO()
        {
            // Arrange
            var mockSystemInfo = new SystemInfo
            {
                Id = Guid.NewGuid(),
                SystemName = "VIRLocal",
                DatabaseVersion = "SQL 2022",
                ReleaseDate = DateTime.Now,
                Environment = "Unit Test",
                Live = false,
                ReleaseNotes = "Unit Test release"
            };

            var expectedDto = new SystemInfoDTO
            {
                Id = mockSystemInfo.Id,
                SystemName = mockSystemInfo.SystemName,
                DatabaseVersion = mockSystemInfo.DatabaseVersion,
                ReleaseDate = mockSystemInfo.ReleaseDate,
                Environment = mockSystemInfo.Environment,
                Live = mockSystemInfo.Live,
                ReleaseNotes = mockSystemInfo.ReleaseNotes
            };

            _mockRepository.GetLatestSysInfoAsync().Returns(mockSystemInfo);
            _mockMapper.Map<SystemInfoDTO>(Arg.Any<SystemInfo>()).Returns(expectedDto);

            // Act
            var result = await _systemInfoService.GetLatestSysInfo();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto.Id, result.Id);
            Assert.Equal(expectedDto.SystemName, result.SystemName);
            Assert.Equal(expectedDto.DatabaseVersion, result.DatabaseVersion);
            Assert.Equal(expectedDto.ReleaseDate, result.ReleaseDate);
            Assert.Equal(expectedDto.Environment, result.Environment);
            Assert.Equal(expectedDto.Live, result.Live);
            Assert.Equal(expectedDto.ReleaseNotes, result.ReleaseNotes);
        }

        [Fact]
        public async Task GetLatestSysInfo_RepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetLatestSysInfoAsync()
                .Returns(Task.FromResult<SystemInfo>(null!));  // use null! to silence warning

            _mockMapper.Map<SystemInfoDTO>(Arg.Any<SystemInfo>())
                .Returns((SystemInfoDTO?)null);

            // Act
            var result = await _systemInfoService.GetLatestSysInfo();

            // Assert
            Assert.Null(result);
        }
    }
}
