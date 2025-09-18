using Apha.VIR.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.UnitTests.Controllers.ContactUsControllerTest
{
    public class ContactUsControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult_WithCorrectViewName()
        {
            // Arrange
            var controller = new ContactUsController();

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("ContactUs", viewResult.ViewName);
        }
    }
}
