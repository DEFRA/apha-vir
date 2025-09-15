using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsListEntryControllerTest
{
    public class BackTests
    {
        [Fact]
        public void Back_RedirectsToIndex()
        {
            // Arrange
            var service = Substitute.For<IVirusCharacteristicService>();
            var listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            var mapper = Substitute.For<IMapper>();
            var controller = new VirusCharacteristicsListEntryController(service, listEntryService, mapper);

            // Act
            var result = controller.Back();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("VirusCharacteristicsListEntry", redirectResult.ControllerName);
        }
    }
}
