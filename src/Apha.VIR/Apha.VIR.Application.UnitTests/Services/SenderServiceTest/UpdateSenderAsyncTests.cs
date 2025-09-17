using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class UpdateSenderAsyncTests
    {
        private readonly ISenderRepository _mockSenderRepository;
        private readonly IMapper _mockMapper;
        private readonly SenderService _senderService;

        public UpdateSenderAsyncTests()
        {
            _mockSenderRepository = Substitute.For<ISenderRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_mockSenderRepository, _mockMapper);
        }

        [Fact]
        public async Task UpdateSenderAsync_ShouldUpdateSuccessfull_WhenValidSendery()
        {
            // Arrange
            var SenderDto = new SenderDto { SenderId = Guid.NewGuid(), SenderName = "Test Sender" };
            var senderEntity = new Sender { SenderId = SenderDto.SenderId, SenderName = SenderDto.SenderName };

            _mockMapper.Map<Sender>(SenderDto).Returns(senderEntity);
            _mockSenderRepository.UpdateSenderAsync(Arg.Any<Sender>()).Returns(Task.CompletedTask);

            // Act
            await _senderService.UpdateSenderAsync(SenderDto);

            // Assert
            await _mockSenderRepository.Received(1).UpdateSenderAsync(Arg.Is<Sender>(s => s.SenderId == senderEntity.SenderId && s.SenderName == senderEntity.SenderName));
        }

        [Fact]
        public async Task UpdateSenderAsync_ShouldPropagateException_WhenRepositoryThrowsException()
        {
            // Arrange
            var SenderDto = new SenderDto { SenderId = Guid.NewGuid(), SenderName = "Test Sender" };
            var senderEntity = new Sender { SenderId = SenderDto.SenderId, SenderName = SenderDto.SenderName };

            _mockMapper.Map<Sender>(SenderDto).Returns(senderEntity);
            _mockSenderRepository.UpdateSenderAsync(Arg.Any<Sender>()).Returns(Task.FromException(new Exception("Repository error")));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _senderService.UpdateSenderAsync(SenderDto));
        }
    }
}
