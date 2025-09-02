using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllWorkGroupsAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllWorkGroupsAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }
        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsCorrectData()
        {
            // Arrange
            var workGroups = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Group 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Group 2" }
            };
            var workGroupDTOs = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = workGroups[0].Id, Name = workGroups[0].Name },
            new LookupItemDTO { Id = workGroups[1].Id, Name = workGroups[1].Name }
            };

            _mockLookupRepository.GetAllWorkGroupsAsync().Returns(workGroups);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(workGroupDTOs);

            // Act
            var result = await _mockLookupService.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockLookupRepository.Received(1).GetAllWorkGroupsAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == workGroups));
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ReturnsEmptyList_WhenRepositoryReturnsEmptyList()
        {
            // Arrange
            _mockLookupRepository.GetAllWorkGroupsAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_ThrowsException_WhenRepositoryThrowsException()
        {
            // Arrange
            _mockLookupRepository.GetAllWorkGroupsAsync().ThrowsAsync(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllWorkGroupsAsync());
        }
    }
}
