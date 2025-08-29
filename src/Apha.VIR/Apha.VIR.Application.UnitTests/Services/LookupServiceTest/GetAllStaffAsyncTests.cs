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
    public class GetAllStaffAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllStaffAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ReturnsStaffData()
        {
            // Arrange
            var staffEntities = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Staff 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Staff 2" }
            };
            var staffDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff 1" },
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Staff 2" }
            };

            _mockLookupRepository.GetAllStaffAsync().Returns(staffEntities);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(staffDtos);

            // Act
            var result = await _mockLookupService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == staffEntities));
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            _mockLookupRepository.GetAllStaffAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task Test_GetAllStaffAsync_ThrowsException()
        {
            // Arrange
            object value = _mockLookupRepository.GetAllStaffAsync().Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllStaffAsync());
            await _mockLookupRepository.Received(1).GetAllStaffAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }
    }
}
