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
using Apha.VIR.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.LookupControllerTest
{
    public class LookupListTests
    {
        private readonly ILookupService _lookupService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly LookupController _controller;

        public LookupListTests()
        {
            _lookupService = Substitute.For<ILookupService>();
            _cacheService = Substitute.For<ICacheService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new LookupController(_lookupService, 
                _cacheService,
                _mapper);
        }

        [Fact]
        public async Task LookupList_ValidParameters_ReturnsViewResult()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupResult = new LookupDto
            {
                Id = lookupId,
                Name = "Test Lookup",
                ReadOnly = true,
                Parent = Guid.NewGuid(),
                AlternateName = true,
                Smsrelated = true
            };
            var lookupViewModel = new LookupViewModel
            {
                Id = lookupId,
                Name = "Test Lookup",
                ReadOnly = true,
                Parent = lookupResult.Parent,
                AlternateName = true,
                Smsrelated = true
            };
            var lookupEntries = new PaginatedResult<LookupItemDto>
            {
                data = new List<LookupItemDto> { new LookupItemDto() },
                TotalCount = 1
            };
            var lookupItems = new List<LookupItemModel> { new LookupItemModel() };

            _lookupService.GetLookupByIdAsync(lookupId).Returns(lookupResult);
            _mapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);

            _lookupService.GetAllLookupItemsAsync(lookupId, 1, 10).Returns(lookupEntries);
            _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data).Returns(lookupItems);

            // Act
            var result = await _controller.LookupList(lookupId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("LookupItem", viewResult.ViewName);
            var model = Assert.IsType<LookupListViewModel>(viewResult.Model);

            // Assert LookupListViewModel properties
            Assert.Equal(lookupId, model.LookupId);
            Assert.Equal("Test Lookup Look-up List", model.LookupName);
            Assert.True(model.IsReadOnly);

            // Assert LookupItemListViewModel properties
            Assert.NotNull(model.LookupItemResult);
            Assert.Equal(lookupId, model.LookupItemResult.LookupId);
            Assert.True(model.LookupItemResult.ShowParent);
            Assert.True(model.LookupItemResult.ShowAlternateName);
            Assert.True(model.LookupItemResult.ShowSMSRelated);
            Assert.Single(model.LookupItemResult.LookupItems);

            // Assert Pagination
            Assert.NotNull(model.LookupItemResult.Pagination);
            Assert.Equal(1, model.LookupItemResult.Pagination.PageNumber);
            Assert.Equal(10, model.LookupItemResult.Pagination.PageSize);
            Assert.Equal(1, model.LookupItemResult.Pagination.TotalCount);
        }

        [Fact]
        public async Task LookupList_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            var lookupResult = new LookupDto
            {
                Id = lookupId,
                Name = "Test Lookup",
                ReadOnly = true,
                Parent = Guid.NewGuid(),
                AlternateName = true,
                Smsrelated = true
            };
            var lookupViewModel = new LookupViewModel
            {
                Id = lookupId,
                Name = "Test Lookup",
                ReadOnly = true,
                Parent = lookupResult.Parent,
                AlternateName = true,
                Smsrelated = true
            };
            var lookupEntries = new PaginatedResult<LookupItemDto> { data = new List<LookupItemDto> { new LookupItemDto() }, TotalCount = 100 };
            var lookupItems = new List<LookupItemModel> { new LookupItemModel() };

            _lookupService.GetLookupByIdAsync(lookupId).Returns(lookupResult);
            _mapper.Map<LookupViewModel>(lookupResult).Returns(lookupViewModel);
            _lookupService.GetAllLookupItemsAsync(lookupId, 2, 20).Returns(lookupEntries);
            _mapper.Map<IEnumerable<LookupItemModel>>(lookupEntries.data).Returns(lookupItems);

            // Act
            var result = await _controller.LookupList(lookupId, 2, 20);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LookupListViewModel>(viewResult.Model);

            // Assert LookupListViewModel properties
            Assert.Equal(lookupId, model.LookupId);
            Assert.Equal("Test Lookup Look-up List", model.LookupName);
            Assert.True(model.IsReadOnly);

            // Assert LookupItemListViewModel properties
            Assert.NotNull(model.LookupItemResult);
            Assert.Equal(lookupId, model.LookupItemResult.LookupId);
            Assert.True(model.LookupItemResult.ShowParent);
            Assert.True(model.LookupItemResult.ShowAlternateName);
            Assert.True(model.LookupItemResult.ShowSMSRelated);
            Assert.Single(model.LookupItemResult.LookupItems);

            // Assert Pagination
            Assert.NotNull(model.LookupItemResult.Pagination);
            Assert.Equal(2, model.LookupItemResult.Pagination.PageNumber);
            Assert.Equal(20, model.LookupItemResult.Pagination.PageSize);
            Assert.Equal(100, model.LookupItemResult.Pagination.TotalCount);
        }

        [Fact]
        public async Task LookupList_InvalidLookupId_ReturnsBadRequest()
        {
            // Arrange
            var lookupId = Guid.Empty;

            // Act
            var result = await _controller.LookupList(lookupId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LookupList_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var lookupId = Guid.NewGuid();
            _controller.ModelState.AddModelError("Error", "Model state is invalid");

            // Act
            var result = await _controller.LookupList(lookupId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }


        [Fact]
        public async Task Index_ReturnsViewResult_WithLookupViewModels()
        {
            // Arrange
            var mockLookupDtos = new List<LookupDto> { new LookupDto(), new LookupDto() };
            _lookupService.GetAllLookupsAsync().Returns(mockLookupDtos);

            var mockLookupViewModels = new List<LookupViewModel> { new LookupViewModel(), new LookupViewModel() };
            _mapper.Map<IEnumerable<LookupViewModel>>(Arg.Any<IEnumerable<LookupDto>>()).Returns(mockLookupViewModels);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Lookup", viewResult.ViewName);
            Assert.IsAssignableFrom<IEnumerable<LookupViewModel>>(viewResult.Model);
        }
    }
}
