using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Application.Pagination;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models.VirusCharacteristic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class ListEntriesControllerTests
    {
        private readonly IVirusCharacteristicService _service;
        private readonly IVirusCharacteristicListEntryService _listEntryService;
        private readonly IMapper _mapper;
        private readonly VirusCharacteristicsController _controller;

        public ListEntriesControllerTests()
        {
            _service = Substitute.For<IVirusCharacteristicService>();
            _listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new VirusCharacteristicsController(_service, _listEntryService, _mapper);
        }
        [Fact]
        public async Task ListEntries_Valid_ReturnsViewWithViewModel_AllPropertiesSet()
        {
            // Arrange
            int pageNo = 2;
            int pageSize = 5;
            var charId = Guid.NewGuid();
            var entryId = Guid.NewGuid();
            var lastModified = new byte[] { 1, 2, 3, 4 };
            var entryName = "Test Entry";
            var characteristic = new VirusCharacteristicDTO { Id = charId, Name = "Test" };
            var entryDto = new VirusCharacteristicListEntryDTO
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = entryName,
                LastModified = lastModified
            };
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO> { entryDto },
                TotalCount = 1
            };
            var entryViewModel = new VirusCharacteristicListEntryModel
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = entryName,
                LastModified = lastModified
            };

            _service.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO> { characteristic });
            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data)
                .Returns(new List<VirusCharacteristicListEntryModel> { entryViewModel });

            // Act
            var result = await _controller.ListEntries(charId, pageNo, pageSize);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntries", viewResult.ViewName);
            var model = Assert.IsType<VirusCharacteristicListEntriesViewModel>(viewResult.Model);

            Assert.Equal(charId, model.CharacteristicId);
            Assert.Equal("Test", model.CharacteristicName);
            Assert.NotNull(model.Entries);
            Assert.NotNull(model.Entries.VirusCharacteristics);
            Assert.Single(model.Entries.VirusCharacteristics);
            var entry = model.Entries.VirusCharacteristics[0];
            Assert.Equal(entryId, entry.Id);
            Assert.Equal(charId, entry.VirusCharacteristicId);
            Assert.Equal(entryName, entry.Name);
            Assert.Equal(lastModified, entry.LastModified);
            Assert.NotNull(model.Entries.Pagination);
            Assert.Equal(pageNo, model.Entries.Pagination.PageNumber);
            Assert.Equal(pageSize, model.Entries.Pagination.PageSize);
            Assert.Equal(1, model.Entries.Pagination.TotalCount);
        }
        [Fact]
        public async Task ListEntries_CharacteristicNotFound_ReturnsViewWithEmptyName()
        {
            //Arrange
            int pageNo = 1;
            int pageSize = 10;
            var charId = Guid.NewGuid();
            _service.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO>());
            var PaginatedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO>(),
                TotalCount = 0
            };
            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(PaginatedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(PaginatedResult.data)
                .Returns(new List<VirusCharacteristicListEntryModel>());

            //Act
            var result = await _controller.ListEntries(charId, pageNo, pageSize);

            //Assert

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicListEntriesViewModel>(viewResult.Model);
            Assert.Equal("VirusCharacteristicListEntries", viewResult.ViewName);
            Assert.Equal(charId, model.CharacteristicId);
            Assert.Equal(string.Empty, model.CharacteristicName);
            Assert.NotNull(model.Entries);
            Assert.NotNull(model.Entries.VirusCharacteristics);
            Assert.Empty(model.Entries.VirusCharacteristics);
            Assert.NotNull(model.Entries.Pagination);
            Assert.Equal(pageNo, model.Entries.Pagination.PageNumber);
            Assert.Equal(pageSize, model.Entries.Pagination.PageSize);
            Assert.Equal(0, model.Entries.Pagination.TotalCount);
        }


        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("error", "some error");
            var result = await _controller.BindCharacteristicEntriesGridOnPagination(Guid.NewGuid(), 1, 10);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_NullCharacteristic_ReturnsBadRequest()
        {
            var result = await _controller.BindCharacteristicEntriesGridOnPagination(null, 1, 10);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_EmptyCharacteristic_ReturnsBadRequest()
        {
            var result = await _controller.BindCharacteristicEntriesGridOnPagination(Guid.Empty, 1, 10);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_Valid_ReturnsPartialViewWithModel()
        {
            //Arrange
            int pageNo = 2;
            int pageSize = 5;
            var charId = Guid.NewGuid();
            var entryId = Guid.NewGuid();
            var entryDto = new VirusCharacteristicListEntryDTO
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = "Test Entry"
            };
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO> { entryDto },
                TotalCount = 1
            };
            var entryViewModel = new VirusCharacteristicListEntryModel
            {
                Id = entryId,
                VirusCharacteristicId = charId,
                Name = "Test Entry"
            };

            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data)
                .Returns(new List<VirusCharacteristicListEntryModel> { entryViewModel });

            //Act
            var result = await _controller.BindCharacteristicEntriesGridOnPagination(charId, pageNo, pageSize);

            //Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_VirusCharacteristicListEntry", partialViewResult.ViewName);
            var model = Assert.IsType<VirusCharacteristicListEntryViewModel>(partialViewResult.Model);
            Assert.NotNull(model.VirusCharacteristics);
            Assert.Single(model.VirusCharacteristics);
            Assert.Equal(entryId, model.VirusCharacteristics[0].Id);
            Assert.NotNull(model.Pagination);
            Assert.Equal(pageNo, model.Pagination.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(1, model.Pagination.TotalCount);
        }
        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_ReturnsPartialViewWithEmptyList_WhenEmptyEntryList()
        {
            int pageNo = 1;
            int pageSize = 10;
            var charId = Guid.NewGuid();
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO>(),
                TotalCount = 0
            };
            var entryViewModels = new List<VirusCharacteristicListEntryModel>();

            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data).Returns(entryViewModels);

            var result = await _controller.BindCharacteristicEntriesGridOnPagination(charId, pageNo, pageSize);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicListEntryViewModel>(partialViewResult.Model);
            Assert.Empty(model.VirusCharacteristics);
            Assert.NotNull(model.Pagination);
            Assert.Equal(pageNo, model.Pagination.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(0, model.Pagination.TotalCount);
        }
        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_ReturnsCorrectPagination_WhenDifferentPageSizes()
        {
            int pageNo = 3;
            int pageSize = 15;
            var charId = Guid.NewGuid();
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO>(),
                TotalCount = 30
            };
            var entryViewModels = new List<VirusCharacteristicListEntryModel>();

            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data).Returns(entryViewModels);

            var result = await _controller.BindCharacteristicEntriesGridOnPagination(charId, pageNo, pageSize);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicListEntryViewModel>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
            Assert.Equal(pageNo, model.Pagination.PageNumber);
            Assert.Equal(pageSize, model.Pagination.PageSize);
            Assert.Equal(30, model.Pagination.TotalCount);
        }
        [Fact]
        public async Task BindCharacteristicEntriesGridOnPagination_ReturnsCorrectViewModels_WhenMappingIsCorrect()
        {
            int pageNo = 1;
            int pageSize = 10;
            var charId = Guid.NewGuid();
            var entryDTOs = new List<VirusCharacteristicListEntryDTO>
            {
                new VirusCharacteristicListEntryDTO { Id = Guid.NewGuid(), Name = "Test Entry" }
            };
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = entryDTOs,
                TotalCount = 1
            };

            var entryViewModels = new List<VirusCharacteristicListEntryModel>
            {
                new VirusCharacteristicListEntryModel
                {
                    Id = entryDTOs[0].Id,
                    Name = entryDTOs[0].Name!
                }
            };

            _listEntryService.GetVirusCharacteristicListEntries(charId, pageNo, pageSize).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data).Returns(entryViewModels);

            var result = await _controller.BindCharacteristicEntriesGridOnPagination(charId, pageNo, pageSize);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<VirusCharacteristicListEntryViewModel>(partialViewResult.Model);
            Assert.Single(model.VirusCharacteristics);
            Assert.Equal(entryViewModels[0].Id, model.VirusCharacteristics[0].Id);
            Assert.Equal(entryViewModels[0].Name, model.VirusCharacteristics[0].Name);
        }


        [Fact]
        public async Task ListEntries_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await _controller.ListEntries(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ListEntries_NullCharacteristic_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ListEntries(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public async Task ListEntries_EmptyCharacteristic_ReturnsBadRequest()
        {
            var result = await _controller.ListEntries(Guid.Empty);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task ListEntries_Valid_ReturnsViewWithViewModel()
        {
            // Arrange
            var charId = Guid.NewGuid();
            var characteristic = new VirusCharacteristicDTO { Id = charId, Name = "Test" };
            var pagedResult = new PaginatedResult<VirusCharacteristicListEntryDTO>
            {
                data = new List<VirusCharacteristicListEntryDTO>(),
                TotalCount = 0
            };
            _service.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO> { characteristic });
            _listEntryService.GetVirusCharacteristicListEntries(charId, 1, 10).Returns(pagedResult);
            _mapper.Map<IEnumerable<VirusCharacteristicListEntryModel>>(pagedResult.data)
                .Returns(new List<VirusCharacteristicListEntryModel>());

            // Act
            var result = await _controller.ListEntries(charId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("VirusCharacteristicListEntries", viewResult.ViewName);
            Assert.IsType<VirusCharacteristicListEntriesViewModel>(viewResult.Model);
        }
    }
}
