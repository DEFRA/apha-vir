using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class GetAllSenderOrderByOrganisationAsyncTests
    {
        private readonly ISenderRepository _mockSenderRepository;
        private readonly IMapper _mockMapper;
        private readonly SenderService _senderService;

        public GetAllSenderOrderByOrganisationAsyncTests()
        {
            _mockSenderRepository = Substitute.For<ISenderRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_mockSenderRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSenderOrderByOrganisationAsync_ReturnsSenderList_WhenValidCountryId()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var senders = new List<Sender> { new Sender(), new Sender() };
            var SenderDtos = new List<SenderDto> { new SenderDto(), new SenderDto() };

            _mockSenderRepository.GetAllSenderOrderByOrganisationAsync(countryId).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderDto>>(senders).Returns(SenderDtos);

            // Act
            var result = await _senderService.GetAllSenderOrderByOrganisationAsync(countryId);

            // Assert
            Assert.Equal(SenderDtos, result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderByOrganisationAsync(countryId);
            _mockMapper.Received(1).Map<IEnumerable<SenderDto>>(senders);
        }

        [Fact]
        public async Task GetAllSenderOrderByOrganisationAsync_ReturnsList_WhenNullCountryId()
        {
            // Arrange
            var senders = new List<Sender> { new Sender(), new Sender() };
            var SenderDtos = new List<SenderDto> { new SenderDto(), new SenderDto() };

            _mockSenderRepository.GetAllSenderOrderByOrganisationAsync(null).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderDto>>(senders).Returns(SenderDtos);

            // Act
            var result = await _senderService.GetAllSenderOrderByOrganisationAsync(null);

            // Assert
            Assert.Equal(SenderDtos, result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderByOrganisationAsync(null);
            _mockMapper.Received(1).Map<IEnumerable<SenderDto>>(senders);
        }

        [Fact]
        public async Task GetAllSenderOrderByOrganisationAsync_ReturnsEmptyList_WhenNoSendersFound()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            var emptySenders = new List<Sender>();
            var emptySenderDtos = new List<SenderDto>();

            _mockSenderRepository.GetAllSenderOrderByOrganisationAsync(countryId).Returns(emptySenders);
            _mockMapper.Map<IEnumerable<SenderDto>>(emptySenders).Returns(emptySenderDtos);

            // Act
            var result = await _senderService.GetAllSenderOrderByOrganisationAsync(countryId);

            // Assert
            Assert.Empty(result);
            await _mockSenderRepository.Received(1).GetAllSenderOrderByOrganisationAsync(countryId);
            _mockMapper.Received(1).Map<IEnumerable<SenderDto>>(emptySenders);
        }

        [Fact]
        public async Task GetAllSenderOrderByOrganisationAsync_ExceptionIsThrown_WhenRepositoryThrowsException()
        {
            // Arrange
            var countryId = Guid.NewGuid();
            _mockSenderRepository.GetAllSenderOrderByOrganisationAsync(countryId).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _senderService.GetAllSenderOrderByOrganisationAsync(countryId));
            await _mockSenderRepository.Received(1).GetAllSenderOrderByOrganisationAsync(countryId);
        }
    }
}
