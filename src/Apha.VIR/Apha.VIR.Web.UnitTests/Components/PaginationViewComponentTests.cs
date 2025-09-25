using Apha.VIR.Web.Components;
using Apha.VIR.Web.Models;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace Apha.VIR.Web.UnitTests.Components
{
    public class PaginationViewComponentTests
    {
        private readonly PaginationViewComponent _component;

        public PaginationViewComponentTests()
        {
            _component = new PaginationViewComponent();
        }

        [Fact]
        public void Invoke_ReturnsViewComponentResult()
        {
            // Arrange
            var paginationModel = new PaginationModel();

            // Act
            var result = _component.Invoke(paginationModel);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewViewComponentResult>(result);
        }

        [Fact]
        public void Invoke_CallsViewWithCorrectModel()
        {
            // Arrange
            var paginationModel = new PaginationModel
            {
                PageNumber = 2,
                TotalCount = 5
            };

            // Act
            var result = _component.Invoke(paginationModel) as ViewViewComponentResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            Assert.NotNull(result.ViewData.Model);
            var model = Assert.IsType<PaginationModel>(result.ViewData.Model);
            Assert.Equal(paginationModel.PageNumber, model.PageNumber);
            Assert.Equal(paginationModel.TotalCount, model.TotalCount);
        }

       

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 10)]
        [InlineData(5, 10)]
        [InlineData(10, 10)]
        public void Invoke_WithVariousPageConfigurations_ReturnsCorrectModel(int currentPage, int totalPages)
        {
            // Arrange
            var paginationModel = new PaginationModel
            {
                PageNumber = currentPage,
                TotalCount = totalPages
            };

            // Act
            var result = _component.Invoke(paginationModel) as ViewViewComponentResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ViewData);
            Assert.NotNull(result.ViewData.Model);
            var model = Assert.IsType<PaginationModel>(result.ViewData.Model);
            Assert.Equal(currentPage, model.PageNumber);
            Assert.Equal(totalPages, model.TotalCount);
        }

        [Fact]
        public void Invoke_ReturnsViewComponentResult_WithViewName()
        {
            // Arrange
            var paginationModel = new PaginationModel();

            // Act
            var result = _component.Invoke(paginationModel) as ViewViewComponentResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view
        }
    }
}
