using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class GetSenderAsynTests
    {
        private readonly ISenderRepository _mockSenderRepository;
        private readonly IMapper _mockMapper;
        private readonly SenderService _senderService;

        public GetSenderAsynTests()
        {
            _mockSenderRepository = Substitute.For<ISenderRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_mockSenderRepository, _mockMapper);
        }


        [Fact]
        public async Task GetSenderAsync_ReturnsSenderDto_WhenValidSenderId()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var sender = new Sender { SenderId = senderId, SenderName = "Test Sender" };
            var SenderDto = new SenderDto { SenderId = senderId, SenderName = "Test Sender" };

            _mockSenderRepository.GetSenderAsync(senderId).Returns(sender);
            _mockMapper.Map<SenderDto>(sender).Returns(SenderDto);

            // Act
            var result = await _senderService.GetSenderAsync(senderId);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SenderDto>(result);
            Assert.Equal(senderId, result.SenderId);
            Assert.Equal("Test Sender", result.SenderName);
        }

        [Fact]
        public async Task GetSenderAsync_ReturnsNull_WhenNonExistentId()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            _mockSenderRepository.GetSenderAsync(nonExistentId).Returns((new Sender()));

            // Act
            var result = await _senderService.GetSenderAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSenderAsync_ReturnCorrectlyMappedDTO_WhenMapped()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var sender = new Sender { SenderId = senderId };
            _mockSenderRepository.GetSenderAsync(senderId).Returns(sender);

            // Act
            await _senderService.GetSenderAsync(senderId);

            // Assert
            _mockMapper.Received(1).Map<SenderDto>(Arg.Is<Sender>(s => s.SenderId == senderId));
        }
    }
}
