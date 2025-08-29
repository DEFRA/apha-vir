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

namespace Apha.VIR.Application.UnitTests.Services.LookupServiceTest
{
    public class GetAllSubmissionReasonAsyncTests
    { 
        private readonly ILookupRepository _mockLookupRepository;
        private readonly IMapper _mockMapper;
        private readonly LookupService _mockLookupService;

        public GetAllSubmissionReasonAsyncTests()
        {
            _mockLookupRepository = Substitute.For<ILookupRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _mockLookupService = new LookupService(_mockLookupRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldReturnMappedDTOs_WhenReasonExist()
        {
            // Arrange
            var mockReasons = new List<LookupItem> { new LookupItem(), new LookupItem() };
            var expectedDtos = new List<LookupItemDTO> { new LookupItemDTO(), new LookupItemDTO() };

            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(mockReasons);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(expectedDtos);

            // Act
            var result = await _mockLookupService.GetAllSubmissionReasonAsync();

            // Assert
            Assert.Equal(expectedDtos, result);
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == mockReasons));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldReturnEmptyList_WhenNoReasonsExist()
        {
            // Arrange
            var emptyList = new List<LookupItem>();
            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(emptyList);
            _mockMapper.Map<IEnumerable<LookupItemDTO>>(Arg.Any<IEnumerable<LookupItem>>()).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _mockLookupService.GetAllSubmissionReasonAsync();

            // Assert
            Assert.Empty(result);
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
            _mockMapper.Received(1).Map<IEnumerable<LookupItemDTO>>(Arg.Is<IEnumerable<LookupItem>>(x => x == emptyList));
        }

        [Fact]
        public async Task GetAllSubmissionReasonAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockLookupRepository.GetAllSubmissionReasonAsync().Returns(Task.FromException<IEnumerable<LookupItem>>(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _mockLookupService.GetAllSubmissionReasonAsync());
            await _mockLookupRepository.Received(1).GetAllSubmissionReasonAsync();
        }
    }
}
