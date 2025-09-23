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
            Assert.Equal(paginationModel, result.ViewData.Model);
        }

        [Fact]
        public void Invoke_WithNullModel_ReturnsViewComponentResult()
        {
            // Act
            var result = _component.Invoke(null!);

            // Assert
            Assert.IsType<ViewViewComponentResult>(result);
        }

        [Fact]
        public void Invoke_WithEmptyModel_ReturnsViewComponentResult()
        {
            // Arrange
            var emptyModel = new PaginationModel();

            // Act
            var result = _component.Invoke(emptyModel);

            // Assert
            Assert.IsType<ViewViewComponentResult>(result);
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
            Assert.NotNull(result.ViewData); // Ensure ViewData is not null
            Assert.NotNull(result.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<PaginationModel>(result.ViewData.Model);
            Assert.Equal(currentPage, model.PageNumber);
            Assert.Equal(totalPages, model.TotalCount);
        }
    }
}
