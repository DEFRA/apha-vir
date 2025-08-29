using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.VirusCharacteristicsControllerTest
{
    public class BackControllerTests
    {
        [Fact]
        public void Back_RedirectsToIndex()
        {
            // Arrange
            var service = Substitute.For<IVirusCharacteristicService>();
            var listEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            var mapper = Substitute.For<IMapper>();
            var controller = new VirusCharacteristicsController(service, listEntryService, mapper);

            // Act
            var result = controller.Back();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("VirusCharacteristics", redirectResult.ControllerName);
        }
    }
}
