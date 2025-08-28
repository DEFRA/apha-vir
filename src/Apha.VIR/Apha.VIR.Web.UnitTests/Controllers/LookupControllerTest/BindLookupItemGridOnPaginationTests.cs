using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.Lookup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    public class BindLookupItemGridOnPaginationTests
    {
        private readonly LookupController _controller;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;

        public BindLookupItemGridOnPaginationTests()
        {
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new LookupController(_mockLookupService, _mockMapper);
        }

        [Fact]
        public async Task BindLookupItemGridOnPagination_ValidParameters_ReturnsPartialView()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var pageNo = 1;
            var pageSize = 10;

            var lookupResult = new LookupDTO
            {
                Id = lookupId,
                Parent = Guid.NewGuid(),
                AlternateName = true,
                Smsrelated = false
            };
            var lookupViewModel = new LookupViewModel
            {
                Id = lookupId,
                Parent = lookupResult.Parent,
                AlternateName = lookupResult.AlternateName,
                Smsrelated = lookupResult.Smsrelated
            }; ;
            var lookupEntries = new PaginatedResult<LookupItemDTO> 
            { data = new List<LookupItemDTO> { new LookupItemDTO() }, TotalCount = 0 };
            var lookupItems = new List<LookupItemModel>();

            _mockLookupService.GetLookupsByIdAsync(lookupId).Returns(lookupResult);
            _mockMapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);
            _mockLookupService.GetAllLookupItemsAsync(lookupId, pageNo, pageSize).Returns(lookupEntries);
            _mockMapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data).Returns(lookupItems);

            // Act
            var result = await _controller.BindLookupItemGridOnPagination(lookupId, pageNo, pageSize);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_LookupItemList", partialViewResult.ViewName);
            var model = Assert.IsType<LookupItemListViewModel>(partialViewResult.Model);
            Assert.Equal(lookupId, model.LookupId);
        }
        [Fact]
        public async Task BindLookupItemGridOnPagination_InvalidLookupId_ReturnsBadRequest()
        {
            // Arrange
            var lookupId = Guid.Empty;
            var pageNo = 1;
            var pageSize = 10;

            // Act
            var result = await _controller.BindLookupItemGridOnPagination(lookupId, pageNo, pageSize);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameters.", badRequestResult.Value);
        }

        [Fact]
        public async Task BindLookupItemGridOnPagination_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var pageNo = 1;
            var pageSize = 10;
            _controller.ModelState.AddModelError("error", "Some error");

            // Act
            var result = await _controller.BindLookupItemGridOnPagination(lookupId, pageNo, pageSize);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid parameters.", badRequestResult.Value);
        }

    }
}
