using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class GetAllSenderOrderBySenderAsyncTests
    {
        private readonly ISenderRepository _mockSenderRepository;
        private readonly IMapper _mockMapper;
        private readonly SenderService _senderService;

        public GetAllSenderOrderBySenderAsyncTests()
        {
            _mockSenderRepository = Substitute.For<ISenderRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_mockSenderRepository, _mockMapper);
        }

           [Fact]
        public async Task GetAllSenderOrderBySenderAsync_ShouldReturnMappedSenders_WhenSendersExist()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var senders = new List<Sender> { new Sender(), new Sender() };
            var senderDTOs = new List<SenderDTO> { new SenderDTO(), new SenderDTO() };

            _mockSenderRepository.GetAllSenderOrderBySenderAsync(countryId).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderDTO>>(senders).Returns(senderDTOs);

            // Act
            var result = await _senderService.GetAllSenderOrderBySenderAsync(countryId);

            // Assert
            Assert.Equal(senderDTOs, result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderBySenderAsync(countryId);
            _mockMapper.Received(1).Map<IEnumerable<SenderDTO>>(senders);
        }

        [Fact]
        public async Task GetAllSenderOrderBySenderAsync_ShouldReturnEmptyList_WhenNoSendersFound()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var emptySenders = new List<Sender>();
            var emptySenderDTOs = new List<SenderDTO>();

            _mockSenderRepository.GetAllSenderOrderBySenderAsync(countryId).Returns(emptySenders);
            _mockMapper.Map<IEnumerable<SenderDTO>>(emptySenders).Returns(emptySenderDTOs);

            // Act
            var result = await _senderService.GetAllSenderOrderBySenderAsync(countryId);

            // Assert
            Assert.Empty(result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderBySenderAsync(countryId);
            _mockMapper.Received(1).Map<IEnumerable<SenderDTO>>(emptySenders);
        }

        [Fact]
        public async Task GetAllSenderOrderBySenderAsync_ShouldHandleNull_WhenCountryIdNull()
        {
            // Arrange
            Guid? countryId = null;
            var senders = new List<Sender> { new Sender() };
            var senderDTOs = new List<SenderDTO> { new SenderDTO() };

            _mockSenderRepository.GetAllSenderOrderBySenderAsync(countryId).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderDTO>>(senders).Returns(senderDTOs);

            // Act
            var result = await _senderService.GetAllSenderOrderBySenderAsync(countryId);

            // Assert
            Assert.Single(result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderBySenderAsync(countryId);
            _mockMapper.Received(1).Map<IEnumerable<SenderDTO>>(senders);
        }

        [Fact]
        public async Task GetAllSenderOrderBySenderAsync_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            _mockSenderRepository.GetAllSenderOrderBySenderAsync(countryId).Returns<Task<IEnumerable<Sender>>>(_ => throw new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _senderService.GetAllSenderOrderBySenderAsync(countryId));
            await _mockSenderRepository.Received(1).GetAllSenderOrderBySenderAsync(countryId);
        }

       
    }
}
