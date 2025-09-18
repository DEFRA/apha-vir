using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.UnitTests.Controllers.ErrorControllerTest
{
    public class ErrorControllerTests
    {
        [Fact]
        public void Index_ReturnsErrorView()
        {
            // Arrange
            var controller = new ErrorController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
        }

        [Fact]
        public void AccessDenied_ReturnsErrorView()
        {
            // Arrange
            var controller = new ErrorController();

            // Act
            var result = controller.AccessDenied();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Error", viewResult.ViewName);
        }
    }
}
