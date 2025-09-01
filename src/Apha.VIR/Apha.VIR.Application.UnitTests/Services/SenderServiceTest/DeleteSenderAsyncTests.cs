using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class DeleteSenderAsyncTests
    {
        private readonly ISenderRepository _senderRepository;
        private readonly IMapper _mapper;
        private readonly SenderService _senderService;

        public DeleteSenderAsyncTests()
        {
            _senderRepository = Substitute.For<ISenderRepository>();
            _mapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_senderRepository, _mapper);
        }


        [Fact]
        public async Task DeleteSenderAsync_SenderDeleted_WhenCorrectSenderId()
        {
            // Arrange
            var senderId = Guid.NewGuid();

            // Act
            await _senderService.DeleteSenderAsync(senderId);

            // Assert
            await _senderRepository.Received(1).DeleteSenderAsync(senderId);
        }

        [Fact]
        public async Task DeleteSenderAsync_PropagatesException_WhenRepositoryThrows()
        {
            // Arrange
            var senderId = Guid.NewGuid();
            var expectedException = new Exception("Repository error");
            _senderRepository.DeleteSenderAsync(senderId).ThrowsAsync(expectedException);

            // Act & Assert
            var actualException = await Assert.ThrowsAsync<Exception>(() => _senderService.DeleteSenderAsync(senderId));
            Assert.Same(expectedException, actualException);
        }
    }
}
