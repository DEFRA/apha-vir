using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Application.Services;
using Apha.VIR.Core.Entities;
using Apha.VIR.Core.Interfaces;
using Apha.VIR.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.VIR.Application.UnitTests.Services.SenderServiceTest
{
    public class GetAllSenderAsyncTests
    {
        private readonly ISenderRepository _mockSenderRepository;
        private readonly IMapper _mockMapper;
        private readonly SenderService _senderService;

        public GetAllSenderAsyncTests()
        {
            _mockSenderRepository = Substitute.For<ISenderRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _senderService = new SenderService(_mockSenderRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSenderAsync_SuccessfulRetrieval_ReturnsPaginatedResult()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;

            var senderId1 = Guid.NewGuid();
                var senderId2 = Guid.NewGuid();

            var senders = new List<Sender>
            {
                new Sender { SenderId = senderId1, SenderName = "Sender1" },
                new Sender { SenderId = senderId2, SenderName = "Sender2" },
            };

            var mockPaginatedResult = new PagedData<Sender>(senders, 2);

            var expectedPaginatedResult = new PaginatedResult<SenderDTO>
            {
                data = new List<SenderDTO> 
                { new SenderDTO{ SenderId = senderId1, SenderName = "Sender1" },
                 new SenderDTO { SenderId = senderId2, SenderName = "Sender2" } 
                },
                TotalCount = 2
            };

            _mockSenderRepository.GetAllSenderAsync(pageNo, pageSize).Returns(mockPaginatedResult);
            _mockMapper.Map<PaginatedResult<SenderDTO>>(Arg.Any<PagedData<Sender>>()).Returns(expectedPaginatedResult);

            // Act
            var result = await _senderService.GetAllSenderAsync(pageNo, pageSize);

            // Assert
            await _mockSenderRepository.Received(1).GetAllSenderAsync(pageNo, pageSize);
            _mockMapper.Received(1).Map<PaginatedResult<SenderDTO>>(Arg.Any<PagedData<Sender>>());
            Assert.Equal(expectedPaginatedResult, result);
        }


        [Fact]
        public async Task GetAllSenderAsynct_ReturnsEmptyPaginatedResult_WhenEmptyListReturn()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;

            var mockPaginatedResult = new PagedData<Sender>(new List<Sender>(), 0);
       
            var expectedPaginatedResult = new PaginatedResult<SenderDTO>
            {
                data = new List<SenderDTO>(),
                TotalCount = 0,
            };

            _mockSenderRepository.GetAllSenderAsync(pageNo, pageSize).Returns(mockPaginatedResult);
            _mockMapper.Map<PaginatedResult<SenderDTO>>(Arg.Any<PagedData<Sender>>()).Returns(expectedPaginatedResult);

            // Act
            var result = await _senderService.GetAllSenderAsync(pageNo, pageSize);

            // Assert
            await _mockSenderRepository.Received(1).GetAllSenderAsync(pageNo, pageSize);
            _mockMapper.Received(1).Map<PaginatedResult<SenderDTO>>(Arg.Any<PagedData<Sender>>());
            Assert.Empty(result.data);
            Assert.Equal(0, result.TotalCount);
        }
    }
}
