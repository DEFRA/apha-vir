using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Models.Lookup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SenderControllerTest
{
    public class BindSenderGridOnPaginationTests
    {
        private readonly ISenderService _mockSenderService;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;
        private readonly SenderController _controller;

        public BindSenderGridOnPaginationTests()
        {
            _mockSenderService = Substitute.For<ISenderService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SenderController(_mockSenderService, _mockLookupService, _mockMapper);
        }

        [Fact]
        public async Task Index_ResultView()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;
            var mockSenders = new PaginatedResult<SenderDTO>
            {
                data = new List<SenderDTO> { new SenderDTO() },
                TotalCount = 1
            };

            var mockMappedSenders = new List<SenderMViewModel>
            {
                new SenderMViewModel
                {
                    SenderAddress="TestAddr",
                    SenderName="TestSender",
                    SenderOrganisation="TestCountry"
                }
            };

            _mockSenderService.GetAllSenderAsync(pageNo, pageSize).Returns(mockSenders);
            _mockMapper.Map<IEnumerable<SenderMViewModel>>(mockSenders.data).Returns(mockMappedSenders);

            // Act
            var result = await _controller.Index();

            // Assert
            await _mockSenderService.Received(1).GetAllSenderAsync(pageNo, pageSize);
            _mockMapper.Received(1).Map<IEnumerable<SenderMViewModel>>(mockSenders.data);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<SenderListViewModel>(viewResult.Model);
            Assert.Equal("Sender", viewResult.ViewName);
            Assert.Equal(mockMappedSenders, model.Senders);
            Assert.Equal(pageNo, model.Pagination!.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(mockSenders.TotalCount, model.Pagination.TotalCount);
        }

        [Fact]
        public async Task BindSenderGridOnPagination_ReturnsPartialView_whenValidInput()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;
            var senders = new PaginatedResult<SenderDTO> { data = new List<SenderDTO>(), TotalCount = 20 };
            var senderViewModels = new List<SenderMViewModel>();

            _mockSenderService.GetAllSenderAsync(pageNo, pageSize).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderMViewModel>>(senders.data).Returns(senderViewModels);

            // Act
            var result = await _controller.BindSenderGridOnPagination(pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_SenderList", partialViewResult.ViewName);
            var model = Assert.IsType<SenderListViewModel>(partialViewResult.Model);
            Assert.Equal(senderViewModels, model.Senders);
            Assert.Equal(pageNo, model.Pagination!.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(20, model.Pagination.TotalCount);
        }

        [Fact]
        public async Task BindSenderGridOnPagination_ReturnsBadRequest_WhenInvalidModelState()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.BindSenderGridOnPagination(1, 10);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameters.", badRequestResult.Value);
        }

        [Fact]
        public async Task BindSenderGridOnPagination_ReturnsPartialViewWithEmptyList_WhenEmptySenderList()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;
            var senders = new PaginatedResult<SenderDTO> { data = new List<SenderDTO>(), TotalCount = 0 };
            var senderViewModels = new List<SenderMViewModel>();

            _mockSenderService.GetAllSenderAsync(pageNo, pageSize).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderMViewModel>>(senders.data).Returns(senderViewModels);

            // Act
            var result = await _controller.BindSenderGridOnPagination(pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<SenderListViewModel>(partialViewResult.Model);
            Assert.Empty(model.Senders);
            Assert.Equal(0, model.Pagination!.TotalCount);
        }

        [Fact]
        public async Task BindSenderGridOnPagination_ReturnsCorrectPagination_WhenDifferentPageSizes()
        {
            // Arrange
            int pageNo = 2;
            int pageSize = 15;
            var senders = new PaginatedResult<SenderDTO> { data = new List<SenderDTO>(), TotalCount = 30 };
            var senderViewModels = new List<SenderMViewModel>();

            _mockSenderService.GetAllSenderAsync(pageNo, pageSize).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderMViewModel>>(senders.data).Returns(senderViewModels);

            // Act
            var result = await _controller.BindSenderGridOnPagination(pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<SenderListViewModel>(partialViewResult.Model);
            Assert.Equal(pageNo, model.Pagination!.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(30, model.Pagination.TotalCount);
        }

        [Fact]
        public async Task BindSenderGridOnPagination_ReturnsCorrectViewModels_WhenMappingIsCorrect()
        {
            // Arrange
            int pageNo = 1;
            int pageSize = 10;
            var senderDTOs = new List<SenderDTO> { new SenderDTO { SenderId = Guid.NewGuid(), SenderName = "Test Sender" } };
            var senders = new PaginatedResult<SenderDTO> { data = senderDTOs, TotalCount = 1 };

            var senderViewModels = new List<SenderMViewModel>
            {
                new SenderMViewModel
                {
                    SenderId = senderDTOs[0].SenderId,
                    SenderName = senderDTOs[0].SenderName!,
                    SenderAddress="test",
                    SenderOrganisation="India"
                }
            };
 
            _mockSenderService.GetAllSenderAsync(pageNo, pageSize).Returns(senders);
            _mockMapper.Map<IEnumerable<SenderMViewModel>>(senders.data).Returns(senderViewModels);

            // Act
            var result = await _controller.BindSenderGridOnPagination(pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<SenderListViewModel>(partialViewResult.Model);
            Assert.Single(model.Senders);
            Assert.Equal(senderViewModels[0].SenderId, model.Senders[0].SenderId);
            Assert.Equal(senderViewModels[0].SenderName, model.Senders[0].SenderName);
        }
    }
}
