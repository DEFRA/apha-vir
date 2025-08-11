using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Web.Models;
using Apha.VIR.Application.Pagination;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apha.VIR.Web.UnitTests.Controllers
{
    public class SearchRepositoryControllerTests
    {
        private readonly IVirusCharacteristicService _mockVirusCharacteristicService;
        private readonly IIsolateSearchService _mockIsolateSearchService;
        private readonly ILookupService _mockLookupService;
        private readonly IMapper _mockMapper;
        private readonly SearchRepositoryController _controller;
        private readonly ITempDataDictionary _tempData;

        private readonly QueryParameters<SearchCriteriaDTO> queryParameters = new QueryParameters<SearchCriteriaDTO>
        {
            Filter = new SearchCriteriaDTO
            {
                AVNumber = "AV000000-01",
                YearOfIsolation = 0,
                CharacteristicSearch = new List<CharacteristicCriteriaDTO>()
            },
            Page = 1,
            PageSize = 10
        };

        public SearchRepositoryControllerTests()
        {
            _mockVirusCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mockIsolateSearchService = Substitute.For<IIsolateSearchService>();
            _mockLookupService = Substitute.For<ILookupService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new SearchRepositoryController(_mockLookupService, _mockVirusCharacteristicService, _mockIsolateSearchService, _mockMapper);
            _tempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
            {
                ["SearchCriteria"] = JsonConvert.SerializeObject(queryParameters)
            };
            _controller.TempData = _tempData;
        }

        [Fact]
        public async Task Index_ReturnsViewResultWithCorrectModelType()
        {
            // Arrange            
            _mockLookupService.GetAllVirusFamiliesAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllVirusTypesAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllHostSpeciesAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllHostBreedsAsync().Returns(new List<LookupItemDTO>());

            _mockLookupService.GetAllCountriesAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllHostPurposesAsync().Returns(new List<LookupItemDTO>());
            _mockLookupService.GetAllSampleTypesAsync().Returns(new List<LookupItemDTO>());
            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("IsolateSearch", viewResult.ViewName);
            Assert.IsType<SearchRepositoryViewModel>(viewResult.Model);

            // Verify that LoadIsolateSearchFilterControlsData was called with null parameter
            await _mockLookupService.Received(1).GetAllVirusFamiliesAsync();
            await _mockLookupService.Received(1).GetAllVirusTypesAsync();
            await _mockLookupService.Received(1).GetAllHostSpeciesAsync();
            await _mockLookupService.Received(1).GetAllHostBreedsAsync();
            await _mockLookupService.Received(1).GetAllCountriesAsync();
            await _mockLookupService.Received(1).GetAllHostPurposesAsync();
            await _mockLookupService.Received(1).GetAllSampleTypesAsync();
            await _mockVirusCharacteristicService.Received(1).GetAllVirusCharacteristicsAsync();
        }

        [Fact]
        public async Task Search_WithValidCriteria_ReturnsCorrectView()
        {
            // Arrange
            var criteria = new SearchCriteria
            {
                AVNumber = "AV001",
                ReceivedFromDate = DateTime.Today.AddDays(-30),
                ReceivedToDate = DateTime.Today,
                CreatedFromDate = DateTime.Today.AddDays(-30),
                CreatedToDate = DateTime.Today
            };
            _mockMapper.Map<SearchCriteria>(Arg.Any<SearchCriteriaDTO>()).Returns(new SearchCriteria());
            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO>(),
                TotalCount = 10
            };

            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
            .Returns(searchResults);

            // Act
            var result = await _controller.Search(criteria) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IsolateSearch", result.ViewName);
            Assert.IsType<SearchRepositoryViewModel>(result.Model);
        }

        [Fact]
        public async Task Search_WithInvalidModelState_ReturnsViewWithEmptyResults()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model state is invalid");
            _mockMapper.Map<SearchCriteria>(Arg.Any<SearchCriteriaDTO>()).Returns(new SearchCriteria());
            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO>(),
                TotalCount = 10
            };
            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>()).Returns(searchResults);

            // Act
            var result = await _controller.Search(new SearchCriteria()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("IsolateSearch", result.ViewName);
            var model = Assert.IsType<SearchRepositoryViewModel>(result.Model);
            if (model != null && model.IsolateSearchGird != null && model.IsolateSearchGird.IsolateSearchResults != null)
                Assert.Empty(model.IsolateSearchGird.IsolateSearchResults);
        }

        [Fact]
        public async Task Search_WhenTempDataIsNull_ReturnsViewWithEmptyResults()
        {
            // Arrange
            _tempData.Remove("SearchCriteria");
            _controller.TempData = _tempData;
            // Act
            var result = await _controller.BindIsolateGridOnPaginationAndSort(1, "CreatedDate", true) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_IsolateSearchResults", result.ViewName);
            var model = Assert.IsType<IsolateSearchGirdViewModel>(result.Model);
            Assert.Null(model.IsolateSearchResults);
            Assert.Null(model.Pagination);
        }

        [Fact]
        public async Task Search_WhenTempDataIsNotEmpty_ReturnsViewWithResults()
        {
            // Arrange           
            _tempData["SearchCriteria"] = JsonConvert.SerializeObject(queryParameters);
            _controller.TempData = _tempData;

            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO> { new IsolateSearchResultDTO() },
                TotalCount = 1
            };

            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
            .Returns(searchResults);
            _mockMapper.Map<List<IsolateSearchResult>>(Arg.Any<List<IsolateSearchResultDTO>>())
            .Returns(new List<IsolateSearchResult> { new IsolateSearchResult() });

            // Act
            var result = await _controller.BindIsolateGridOnPaginationAndSort(1, "CreatedDate", true) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("_IsolateSearchResults", result.ViewName);
            var model = Assert.IsType<IsolateSearchGirdViewModel>(result.Model);
            if (model.IsolateSearchResults != null)
                Assert.Single(model.IsolateSearchResults);
        }

        [Fact]
        public async Task GetVirusTypesByVirusFamily_NullOrEmptyVirusFamilyId_ReturnsAllVirusTypes()
        {
            // Arrange
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 2" }
            };
            _mockLookupService.GetAllVirusTypesAsync().Returns(virusTypes);

            // Act
            var result = await _controller.GetVirusTypesByVirusFamily(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, selectList.Count);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            Assert.Equal(virusTypes[1].Id.ToString(), selectList[1].Value);
            Assert.Equal(virusTypes[1].Name, selectList[1].Text);
            await _mockLookupService.Received(1).GetAllVirusTypesAsync();
            await _mockLookupService.DidNotReceive().GetAllVirusTypesByParentAsync(Arg.Any<Guid>());
        }

        [Fact]
        public async Task GetVirusTypesByVirusFamily_ValidVirusFamilyId_ReturnsFilteredVirusTypes()
        {
            // Arrange
            Guid virusFamilyId = Guid.NewGuid();
            var virusTypes = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Type 1" }
            };
            _mockLookupService.GetAllVirusTypesByParentAsync(virusFamilyId).Returns(virusTypes);

            // Act
            var result = await _controller.GetVirusTypesByVirusFamily(virusFamilyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Single(selectList);
            Assert.Equal(virusTypes[0].Id.ToString(), selectList[0].Value);
            Assert.Equal(virusTypes[0].Name, selectList[0].Text);
            await _mockLookupService.Received(1).GetAllVirusTypesByParentAsync(virusFamilyId);
            await _mockLookupService.DidNotReceive().GetAllVirusTypesAsync();
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithValidHostSpicyId_ReturnsCorrectJsonResult()
        {
            // Arrange
            Guid hostSpicyId = Guid.NewGuid();
            var hostBreeds = new List<LookupItemDTO>
            {
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed1" },
            new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed2" }
            };
            _mockLookupService.GetAllHostBreedsByParentAsync(hostSpicyId).Returns(hostBreeds);

            // Act
            var result = await _controller.GetHostBreedsByGroup(hostSpicyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, selectListItems.Count);
            Assert.Equal(hostBreeds[0].Id.ToString(), selectListItems[0].Value);
            Assert.Equal(hostBreeds[0].Name, selectListItems[0].Text);
            Assert.Equal(hostBreeds[1].Id.ToString(), selectListItems[1].Value);
            Assert.Equal(hostBreeds[1].Name, selectListItems[1].Text);
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithNullHostSpicyId_ReturnsAllHostBreeds()
        {
            // Arrange
            var allHostBreeds = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed2" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed3" }
            };
            _mockLookupService.GetAllHostBreedsAsync().Returns(allHostBreeds);

            // Act
            var result = await _controller.GetHostBreedsByGroup(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(3, selectListItems.Count);
            Assert.Equal(allHostBreeds[0].Id.ToString(), selectListItems[0].Value);
            Assert.Equal(allHostBreeds[0].Name, selectListItems[0].Text);
            Assert.Equal(allHostBreeds[1].Id.ToString(), selectListItems[1].Value);
            Assert.Equal(allHostBreeds[1].Name, selectListItems[1].Text);
            Assert.Equal(allHostBreeds[2].Id.ToString(), selectListItems[2].Value);
            Assert.Equal(allHostBreeds[2].Name, selectListItems[2].Text);
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithEmptyHostSpicyId_ReturnsAllHostBreeds()
        {
            // Arrange
            var allHostBreeds = new List<LookupItemDTO>
            {
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItemDTO { Id = Guid.NewGuid(), Name = "Breed2" }
            };
            _mockLookupService.GetAllHostBreedsAsync().Returns(allHostBreeds);

            // Act
            var result = await _controller.GetHostBreedsByGroup(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, selectListItems.Count);
            Assert.Equal(allHostBreeds[0].Id.ToString(), selectListItems[0].Value);
            Assert.Equal(allHostBreeds[0].Name, selectListItems[0].Text);
            Assert.Equal(allHostBreeds[1].Id.ToString(), selectListItems[1].Value);
            Assert.Equal(allHostBreeds[1].Name, selectListItems[1].Text);
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithValidHostSpicyId_CallsCorrectMethod()
        {
            // Arrange
            Guid hostSpicyId = Guid.NewGuid();
            _mockLookupService.GetAllHostBreedsByParentAsync(hostSpicyId).Returns(new List<LookupItemDTO>());

            // Act
            await _controller.GetHostBreedsByGroup(hostSpicyId);

            // Assert
            await _mockLookupService.Received(1).GetAllHostBreedsByParentAsync(hostSpicyId);
            await _mockLookupService.DidNotReceive().GetAllHostBreedsAsync();
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithNullHostSpicyId_CallsCorrectMethod()
        {
            // Arrange
            _mockLookupService.GetAllHostBreedsAsync().Returns(new List<LookupItemDTO>());

            // Act
            await _controller.GetHostBreedsByGroup(null);

            // Assert
            await _mockLookupService.Received(1).GetAllHostBreedsAsync();
            await _mockLookupService.DidNotReceive().GetAllHostBreedsByParentAsync(Arg.Any<Guid>());
        }

        [Fact]
        public async Task GetHostBreedsByGroup_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            Guid hostSpicyId = Guid.NewGuid();
            _mockLookupService.GetAllHostBreedsByParentAsync(hostSpicyId).Returns(new List<LookupItemDTO>());

            // Act
            var result = await _controller.GetHostBreedsByGroup(hostSpicyId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Empty(selectListItems);
        }

        [Fact]
        public async Task GetVirusCharacteristicsByVirusType_ValidVirusTypeId_ReturnsCorrectJsonResult()
        {
            // Arrange
            Guid virusTypeId = Guid.NewGuid();
            var expectedCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 1", DataType = "Type 1" },
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 2", DataType = "Type 2" }
            };

            _mockVirusCharacteristicService.GetAllVirusCharacteristicsByVirusTypeAsync(virusTypeId, false)
            .Returns(expectedCharacteristics);

            // Act
            var result = await _controller.GetVirusCharacteristicsByVirusType(virusTypeId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<CustomSelectListItem>>(jsonResult.Value);

            Assert.Equal(expectedCharacteristics.Count, selectListItems.Count);
            Assert.All(selectListItems, item =>
            {
                Assert.Contains(expectedCharacteristics, c => c.Id.ToString() == item.Value && c.Name.ToString() == item.Text);
            });

            await _mockVirusCharacteristicService.Received(1).GetAllVirusCharacteristicsByVirusTypeAsync(virusTypeId, false);
        }

        [Fact]
        public async Task GetVirusCharacteristicsByVirusType_NullVirusTypeId_ReturnsAllCharacteristics()
        {
            // Arrange
            Guid? virusTypeId = null;
            var expectedCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 1", DataType = "Type 1" },
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 2", DataType = "Type 2" },
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 3", DataType = "Type 3" }
            };

            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync()
            .Returns(expectedCharacteristics);

            // Act
            var result = await _controller.GetVirusCharacteristicsByVirusType(virusTypeId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<CustomSelectListItem>>(jsonResult.Value);

            Assert.Equal(expectedCharacteristics.Count, selectListItems.Count);
            Assert.All(selectListItems, item =>
            {
                Assert.Contains(expectedCharacteristics, c => c.Id.ToString() == item.Value && c.Name.ToString() == item.Text);
            });

            await _mockVirusCharacteristicService.Received(1).GetAllVirusCharacteristicsAsync();
        }

        [Fact]
        public async Task GetVirusCharacteristicsByVirusType_EmptyVirusTypeId_ReturnsAllCharacteristics()
        {
            // Arrange
            Guid? virusTypeId = null;
            var expectedCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 1", DataType = "Type 1" },
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 2", DataType = "Type 2" },
                new VirusCharacteristicDTO { Id = Guid.NewGuid(), Name = "Characteristic 3", DataType = "Type 3" }
            };

            _mockVirusCharacteristicService.GetAllVirusCharacteristicsAsync()
            .Returns(expectedCharacteristics);

            // Act
            var result = await _controller.GetVirusCharacteristicsByVirusType(virusTypeId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var selectListItems = Assert.IsType<List<CustomSelectListItem>>(jsonResult.Value);

            Assert.Equal(expectedCharacteristics.Count, selectListItems.Count);
            Assert.All(selectListItems, item =>
            {
                Assert.Contains(expectedCharacteristics, c => c.Id.ToString() == item.Value && c.Name.ToString() == item.Text);
            });

            await _mockVirusCharacteristicService.Received(1).GetAllVirusCharacteristicsAsync();
        }

        [Fact]
        public async Task GetComparatorsAndListValues_ValidInput_ReturnsNonEmptyResults()
        {
            // Arrange
            var virusCharacteristicId = Guid.NewGuid();
            var comparators = new List<string> { "Equal", "NotEqual" };
            var listValues = new List<VirusCharacteristicListEntryDTO>
            {
                new VirusCharacteristicListEntryDTO { Id = Guid.NewGuid(), Name = "Value1" },
                new VirusCharacteristicListEntryDTO { Id = Guid.NewGuid(), Name = "Value2" }
            };

            _mockIsolateSearchService.GetComparatorsAndListValuesAsync(virusCharacteristicId)
            .Returns(Task.FromResult(Tuple.Create(comparators, listValues)));

            // Act
            var result = await _controller.GetComparatorsAndListValues(virusCharacteristicId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
            var jObject = JObject.FromObject(jsonResult.Value);
            var jComparators = jObject["Comparators"] as JArray;
            var jListValues = jObject["ListValues"] as JArray;

            Assert.NotNull(jComparators);
            Assert.Equal(2, jComparators.Count());

            Assert.NotNull(jListValues);
            Assert.Equal(2, jListValues.Count());
        }

        [Fact]
        public async Task GetComparatorsAndListValues_ValidInput_ReturnsEmptyResults()
        {
            // Arrange
            var virusCharacteristicId = Guid.NewGuid();
            var comparators = new List<string>();
            var listValues = new List<VirusCharacteristicListEntryDTO>();

            _mockIsolateSearchService.GetComparatorsAndListValuesAsync(virusCharacteristicId)
            .Returns(Task.FromResult(Tuple.Create(comparators, listValues)));

            // Act
            var result = await _controller.GetComparatorsAndListValues(virusCharacteristicId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            Assert.NotNull(jsonResult.Value);
            var jObject = JObject.FromObject(jsonResult.Value);

            Assert.Equal(0, (jObject["Comparators"] as JArray)?.Count ?? -1);
            Assert.Equal(0, (jObject["ListValues"] as JArray)?.Count ?? -1);
        }

        [Fact]
        public async Task BindIsolateGridOnPaginationAndSort_WithValidCriteria_ReturnsPartialView()
        {
            // Arrange            
            var criteriaDto = new QueryParameters<SearchCriteriaDTO>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Avnumber",
                Descending = false,
                Filter = new SearchCriteriaDTO()
            };
            var criteriaString = JsonConvert.SerializeObject(criteriaDto);
            _tempData["SearchCriteria"] = criteriaString;
            _controller.TempData = _tempData;

            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO> { new IsolateSearchResultDTO() },
                TotalCount = 1
            };
            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
            .Returns(searchResults);

            _mockMapper.Map<List<IsolateSearchResult>>(Arg.Any<List<IsolateSearchResultDTO>>())
            .Returns(new List<IsolateSearchResult> { new IsolateSearchResult() });

            // Act
            var result = await _controller.BindIsolateGridOnPaginationAndSort(criteriaDto.Page, criteriaDto.SortBy, criteriaDto.Descending);

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateSearchResults", viewResult.ViewName);
            var model = Assert.IsType<IsolateSearchGirdViewModel>(viewResult.Model);
            if (model.IsolateSearchResults != null)
                Assert.Single(model.IsolateSearchResults);
            Assert.Equal(criteriaDto.Page, model.Pagination!.PageNumber);
            Assert.Equal(criteriaDto.SortBy, model.Pagination.SortColumn);
            Assert.Equal(criteriaDto.Descending, model.Pagination.SortDirection);
        }

        [Fact]
        public async Task BindIsolateGridOnPaginationAndSort_WithInvalidCriteria_ReturnsEmptyPartialView()
        {
            // Arrange
            var criteriaDto = new QueryParameters<SearchCriteriaDTO>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "FreezerName",
                Descending = true,
                Filter = new SearchCriteriaDTO { AVNumber = "XWERWE" }
            };
            var criteriaString = JsonConvert.SerializeObject(criteriaDto);
            _tempData["SearchCriteria"] = criteriaString;
            _controller.TempData = _tempData;

            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO> { new IsolateSearchResultDTO() },
                TotalCount = 1
            };
            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
            .Returns(searchResults);

            _mockMapper.Map<List<IsolateSearchResult>>(Arg.Any<List<IsolateSearchResultDTO>>())
            .Returns(new List<IsolateSearchResult> { });

            // Act
            var result = await _controller.BindIsolateGridOnPaginationAndSort(criteriaDto.Page, criteriaDto.SortBy, criteriaDto.Descending);

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateSearchResults", viewResult.ViewName);
            var model = Assert.IsType<IsolateSearchGirdViewModel>(viewResult.Model);
            Assert.Empty(model.IsolateSearchResults);
        }

        [Fact]
        public async Task BindIsolateGridOnPaginationAndSort_WithZeroPageNo_UpdatesSortingOnly()
        {
            // Arrange
            var pageNo = 0;
            var column = "FreezerName";
            var sortOrder = true;
            var criteriaDto = new QueryParameters<SearchCriteriaDTO>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "FreezerName",
                Descending = false,
                Filter = new SearchCriteriaDTO()
            };
            var criteriaString = JsonConvert.SerializeObject(criteriaDto);

            _tempData["SearchCriteria"] = criteriaString;
            _controller.TempData = _tempData;

            var searchResults = new PaginatedResult<IsolateSearchResultDTO>
            {
                data = new List<IsolateSearchResultDTO> { new IsolateSearchResultDTO() },
                TotalCount = 1
            };
            _mockIsolateSearchService.PerformSearchAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
            .Returns(searchResults);

            _mockMapper.Map<List<IsolateSearchResult>>(Arg.Any<List<IsolateSearchResultDTO>>())
            .Returns(new List<IsolateSearchResult> { new IsolateSearchResult() });

            // Act
            var result = await _controller.BindIsolateGridOnPaginationAndSort(pageNo, column, sortOrder);

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_IsolateSearchResults", viewResult.ViewName);
            var model = Assert.IsType<IsolateSearchGirdViewModel>(viewResult.Model);
            if (model.IsolateSearchResults != null)
                Assert.Single(model.IsolateSearchResults);
            Assert.Equal(1, model.Pagination!.PageNumber);
            Assert.Equal(column, model.Pagination.SortColumn);
            Assert.Equal(sortOrder, model.Pagination.SortDirection);
        }

        [Fact]
        public void BindGirdPagination_ValidInput_ReturnsViewComponentResult()
        {
            // Arrange
            var paginationModel = new PaginationModel
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = 100,
                SortColumn = "FreezerName",
                SortDirection = true
            };

            // Act
            var result = _controller.BindGirdPagination(paginationModel);

            // Assert
            Assert.IsType<ViewComponentResult>(result);
            var viewComponentResult = result as ViewComponentResult;
            Assert.Equal("Pagination", viewComponentResult!.ViewComponentName);
            Assert.Equal(paginationModel, viewComponentResult.Arguments);
        }

        [Fact]
        public void BindGirdPagination_NullInput_ReturnsViewComponentResult()
        {
            // Arrange
            PaginationModel paginationModel = null;

            // Act
            var result = _controller.BindGirdPagination(paginationModel);

            // Assert
            Assert.IsType<ViewComponentResult>(result);
            var viewComponentResult = result as ViewComponentResult;
            Assert.Equal("Pagination", viewComponentResult!.ViewComponentName);
            Assert.Null(viewComponentResult.Arguments);
        }

        [Fact]
        public void BindGirdPagination_VerifyCorrectViewComponentReturned()
        {
            // Arrange
            var paginationModel = new PaginationModel();

            // Act
            var result = _controller.BindGirdPagination(paginationModel);

            // Assert
            Assert.IsType<ViewComponentResult>(result);
            var viewComponentResult = result as ViewComponentResult;
            Assert.Equal("Pagination", viewComponentResult!.ViewComponentName);
        }

        [Fact]
        public async Task ExportToExcel_ValidSearchCriteria_ReturnsFileContentResult()
        {
            // Arrange         
            _tempData["SearchCriteria"] = JsonConvert.SerializeObject(queryParameters);
            _controller.TempData = _tempData;

            var exportResults = SetupValidSearchCriteriaExportResult();
            _mockIsolateSearchService.GetIsolateSearchExportResultAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
           .Returns(exportResults);
            var mappedResults = SetupValidSearchCriteriaExportMappedResult();
            _mockMapper.Map<List<IsolateSearchExportViewModel>>(Arg.Any<List<IsolateSearchExportDto>>())
            .Returns(mappedResults);

            // Act
            var result = await _controller.ExportToExcel();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.NotNull(fileResult);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Contains("VIR SearchResults", fileResult.FileDownloadName);
            Assert.EndsWith(".xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_ValidSearchCriteria_CorrectFileNameAndContentType()
        {
            // Arrange            
            var exportResults = SetupValidSearchCriteriaExportResult();
            _mockIsolateSearchService.GetIsolateSearchExportResultAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
           .Returns(exportResults);
            var mappedResults = SetupValidSearchCriteriaExportMappedResult();
            _mockMapper.Map<List<IsolateSearchExportViewModel>>(Arg.Any<List<IsolateSearchExportDto>>())
            .Returns(mappedResults);

            // Act
            var result = await _controller.ExportToExcel();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Matches(@"VIR SearchResults \d{1,2}[A-Z][a-z]{2,8}\d{4}\.xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task ExportToExcel_ValidSearchCriteria_ProperMappingOfSearchResults()
        {
            // Arrange
            var exportResults = SetupValidSearchCriteriaExportResult();
            _mockIsolateSearchService.GetIsolateSearchExportResultAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
           .Returns(Task.FromResult(exportResults));
            var mappedResults = SetupValidSearchCriteriaExportMappedResult();
            _mockMapper.Map<List<IsolateSearchExportViewModel>>(Arg.Any<List<IsolateSearchExportDto>>())
            .Returns(mappedResults);

            // Act
            await _controller.ExportToExcel();

            // Assert
            _mockMapper.Received(1).Map<List<IsolateSearchExportViewModel>>(Arg.Any<List<IsolateSearchExportDto>>());
        }

        [Fact]
        public async Task ExportToExcel_ValidSearchCriteria_CorrectExcelFormatting()
        {
            // Arrange
            var exportResults = SetupValidSearchCriteriaExportResult();
            _mockIsolateSearchService.GetIsolateSearchExportResultAsync(Arg.Any<QueryParameters<SearchCriteriaDTO>>())
           .Returns(Task.FromResult(exportResults));
            var mappedResults = SetupValidSearchCriteriaExportMappedResult();
            _mockMapper.Map<List<IsolateSearchExportViewModel>>(Arg.Any<List<IsolateSearchExportDto>>())
            .Returns(mappedResults);

            // Act
            var result = await _controller.ExportToExcel();

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.True(fileResult.FileContents.Length > 0);
            // Additional assertions for Excel formatting can be added here
            // This might involve parsing the Excel file and checking its structure
        }

        private List<IsolateSearchExportDto> SetupValidSearchCriteriaExportResult()
        {
            return new List<IsolateSearchExportDto>
            {
                new IsolateSearchExportDto
                {
                    AVNumber = "AV123456",
                    Nomenclature = "Sample Nomenclature",
                    IsMixedIsolate = "No",
                    VirusFamily = "Orthomyxoviridae",
                    VirusType = "Influenza A",
                    HostPurpose = "Research",
                    SampleType = "Nasal Swab",
                    Group = "Group A",
                    Species = "Human",
                    CountryOfOrigin = "United Kingdom",
                    YearOfIsolation = "2023",
                    IsolationMethod = "Cell Culture",
                    ReceivedDate = "2023-06-15",
                    Freezer = "F1",
                    Tray = "T2",
                    Well = "W3",
                    NoOfAliquots = "5",
                    ValidToIssue = "Yes",
                    WhyNotValidToIssue = "",
                    OriginalSampleAvailable = "Yes",
                    FirstViablePassageNumber = "2",
                    AntiserumProduced = "No",
                    AntigenProduced = "Yes",
                    PhylogeneticAnalysis = "Completed",
                    MTA = "Yes",
                    MTALocation = "Lab Storage",
                    SenderReferenceNumber = "SRN789",
                    SMSReferenceNumber = "SMS456",
                    Comment = "Sample comment for testing",
                    DispatchWarning = "None",
                    ViabilityChecks = "Passed",
                    Characteristics = "Characteristic1: Value1, Characteristic2: Value2"
                }
            };
        }

        private List<IsolateSearchExportViewModel> SetupValidSearchCriteriaExportMappedResult()
        {
            return new List<IsolateSearchExportViewModel>
            {
                new IsolateSearchExportViewModel
                {
                    AVNumber = "AV123456",
                    Nomenclature = "Sample Nomenclature",
                    IsMixedIsolate = "No",
                    VirusFamily = "Orthomyxoviridae",
                    VirusType = "Influenza A",
                    HostPurpose = "Research",
                    SampleType = "Nasal Swab",
                    Group = "Group A",
                    Species = "Human",
                    CountryOfOrigin = "United Kingdom",
                    YearOfIsolation = "2023",
                    IsolationMethod = "Cell Culture",
                    ReceivedDate = "2023-06-15",
                    Freezer = "F1",
                    Tray = "T2",
                    Well = "W3",
                    NoOfAliquots = "5",
                    ValidToIssue = "Yes",
                    WhyNotValidToIssue = "",
                    OriginalSampleAvailable = "Yes",
                    FirstViablePassageNumber = "2",
                    AntiserumProduced = "No",
                    AntigenProduced = "Yes",
                    PhylogeneticAnalysis = "Completed",
                    MTA = "Yes",
                    MTALocation = "Lab Storage",
                    SenderReferenceNumber = "SRN789",
                    SMSReferenceNumber = "SMS456",
                    Comment = "Sample comment for testing",
                    DispatchWarning = "None",
                    ViabilityChecks = "Passed",
                    Characteristics = "Characteristic1: Value1, Characteristic2: Value2"
                }
            };
        }
    }
}


