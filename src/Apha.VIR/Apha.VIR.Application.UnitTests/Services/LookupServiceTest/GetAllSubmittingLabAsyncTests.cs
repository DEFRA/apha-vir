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
    public class GetAllSubmittingLabAsyncTests
    {
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllSubmittingLabAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldReturnMappedResults()
        {
            // Arrange
            var submittingLabs = new List<LookupItem>
            {
            new LookupItem { Id = Guid.NewGuid(), Name = "Lab 1" },
            new LookupItem { Id = Guid.NewGuid(), Name = "Lab 2" }
            };
            var expectedDtos = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = submittingLabs[0].Id, Name = submittingLabs[0].Name },
            new LookupItemDTO { Id = submittingLabs[1].Id, Name = submittingLabs[1].Name }
            };

            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(submittingLabs);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllSubmittingLabAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == submittingLabs));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldReturnEmptyList_WhenNoLabsExist()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllSubmittingLabAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => !x.Any()));
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(Task.FromException<IEnumerable<LookupItem>>(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllSubmittingLabAsync());
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
        }

        [Fact]
        public async Task GetAllSubmittingLabAsync_ShouldThrowException_WhenMapperFails()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmittingLabAsync().Returns(new List<LookupItem>());
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Throws(new AutoMapperMappingException("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _mockLookupService.GetAllSubmittingLabAsync());
            await _mockLookupRepository.Received(1).GetAllSubmittingLabAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>());
        }

    }
}
