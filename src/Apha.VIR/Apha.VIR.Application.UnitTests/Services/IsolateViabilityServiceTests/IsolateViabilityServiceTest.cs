using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using Microsoft.Data.SqlClient;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.IsolateViabilityServiceTests
{
    public class IsolateViabilityServiceTest: AbstractIsolateViabilityServiceTest
    {
        [Fact]
        public async Task GetViabilityHistoryAsync_SuccessfulRetrieval_ReturnsViabilityHistory()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            GetViabilityHistoryAsyncSuccessfulRetrievalArrange(avNumber, isolateId);
            // Act
            var result = await _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.NotEmpty(result);
            await _mockIsolateRepository.Received(1).GetIsolateInfoByAVNumberAsync(avNumber);
            await _mockIsolateViabilityRepository.Received(1).GetViabilityHistoryAsync(isolateId);
        }

        [Fact]
        public async Task GetViabilityHistoryAsync_NoMatchingIsolate_ReturnsEmptyResult()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();
            var isolateList = new List<IsolateInfo>();

            _mockIsolateRepository.GetIsolateInfoByAVNumberAsync(avNumber).Returns(isolateList);

            // Act
            var result = await _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetViabilityHistoryAsync__SuccessfulRetrievaWithCorrectEnrichDetail_ReturnsViabilityHistorydDTO()
        {
            // Arrange
            var avNumber = "AV123";
            var isolateId = Guid.NewGuid();

            GetViabilityHistoryAsyncSuccessfulRetrievalEnrichArrange(avNumber, isolateId);

            // Act
            var result = await _isolateViabilityService.GetViabilityHistoryAsync(avNumber, isolateId);

            // Assert
            Assert.NotEmpty(result);
            var dto = result.First();
            Assert.Equal("PTest", dto.Nomenclature);
            Assert.Equal("John Doe", dto.CheckedByName);
            Assert.Equal("Viable", dto.ViableName);
        }

        [Theory]
        [InlineData(null, "00000000-0000-0000-0000-000000000000")]
        [InlineData("", "00000000-0000-0000-0000-000000000000")]
        [InlineData("AV123", "00000000-0000-0000-0000-000000000000")]
        public async Task GetViabilityHistoryAsync_NullOrEmptyInput_ReturnsEmptyResult(string avNumber, string isolateId)
        {
            // Act
            var result = await _isolateViabilityService.GetViabilityHistoryAsync(avNumber, Guid.Parse(isolateId));

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteIsolateViabilityAsync_WithValidInput_SuccessfulDeletion()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var userId = "testUser";

            // Act
            await _isolateViabilityService.DeleteIsolateViabilityAsync(isolateId, lastModified, userId);

            // Assert
            await _mockIsolateViabilityRepository.Received(1).DeleteIsolateViabilityAsync(
            Arg.Is<Guid>(id => id == isolateId),
            Arg.Is<byte[]>(lm => lm.SequenceEqual(lastModified)),
            Arg.Is<string>(u => u == userId)
            );
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task DeleteIsolateViabilityAsync_InvalidUserId_NoDeletion(string invalidUserId)
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };

            // Act
            await _isolateViabilityService.DeleteIsolateViabilityAsync(isolateId, lastModified, invalidUserId);

            // Assert
            await _mockIsolateViabilityRepository.Received(1).DeleteIsolateViabilityAsync(
            Arg.Is<Guid>(id => id == isolateId),
            Arg.Is<byte[]>(lm => lm.SequenceEqual(lastModified)),
            Arg.Is<string>(u => u == invalidUserId)
            );
        }

        [Fact]
        public async Task DeleteIsolateViabilityAsync_EmptyIsolateId_NoDeletion()
        {
            // Arrange
            var isolateId = Guid.Empty;
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var userId = "testUser";

            // Act
            await _isolateViabilityService.DeleteIsolateViabilityAsync(isolateId, lastModified, userId);

            // Assert
            await _mockIsolateViabilityRepository.Received(1).DeleteIsolateViabilityAsync(
            Arg.Is<Guid>(id => id == isolateId),
            Arg.Is<byte[]>(lm => lm.SequenceEqual(lastModified)),
            Arg.Is<string>(u => u == userId)
            );
        }

        [Fact]
        public async Task DeleteIsolateViabilityAsync_EmptyLastModified_ShouldThrowException()
        {
            // Arrange
            var isolateId = Guid.NewGuid();
            byte[] lastModified = Array.Empty<byte>();
            var userId = "testUser";

            // Arrange

            _mockIsolateViabilityRepository.DeleteIsolateViabilityAsync(isolateId, lastModified, userId)
             .Returns(Task.FromException(new ArgumentException("lastModified")));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _isolateViabilityService.DeleteIsolateViabilityAsync(isolateId, lastModified, userId));

            await _mockIsolateViabilityRepository.Received(1).DeleteIsolateViabilityAsync(isolateId, lastModified, userId);

        }
    }
}
