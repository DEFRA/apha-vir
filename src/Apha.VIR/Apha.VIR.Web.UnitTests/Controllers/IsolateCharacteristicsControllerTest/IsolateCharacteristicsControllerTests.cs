using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.IsolateCharacteristicsControllerTest
{
    public class IsolateCharacteristicsControllerTests
    {
        private readonly IIsolatesService _isolatesService;
        private readonly IVirusCharacteristicListEntryService _virusCharacteristicListEntryService;
        private readonly IVirusCharacteristicService _virusCharacteristicService;
        private readonly IMapper _mapper;
        private readonly IsolateCharacteristicsController _controller;

        public IsolateCharacteristicsControllerTests()
        {
            _isolatesService = Substitute.For<IIsolatesService>();
            _virusCharacteristicListEntryService = Substitute.For<IVirusCharacteristicListEntryService>();
            _virusCharacteristicService = Substitute.For<IVirusCharacteristicService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new IsolateCharacteristicsController(_isolatesService, _virusCharacteristicListEntryService, _virusCharacteristicService, _mapper);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "some error");
            // Act
            var result = await _controller.Edit("AVNumber", Guid.NewGuid());
            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // Replaced IsInstanceOf with IsType
        }

        [Fact]
        public async Task Edit_ValidModelState_EmptyIsolateCharacteristicInfoList_ReturnsViewWithEmptyModel()
        {
            // Arrange
            var isolate = Guid.NewGuid();
            _isolatesService.GetIsolateCharacteristicInfoAsync(isolate).Returns(new List<IsolateCharacteristicDTO>());
            _mapper.Map<List<IsolateCharacteristicViewModel>>(Arg.Any<List<IsolateCharacteristicDTO>>()).Returns(new List<IsolateCharacteristicViewModel>());

            // Act
            var result = await _controller.Edit("AVNumber", isolate) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<IsolateCharacteristicViewModel>>(result.Model); // Replaced IsInstanceOf with IsType
            Assert.Empty((List<IsolateCharacteristicViewModel>)result.Model);
        }

        [Fact]
        public async Task Edit_ValidModelState_WithIsolateCharacteristicInfoList_ReturnsViewWithModel()
        {
            // Arrange
            var isolate = Guid.NewGuid();
            var dtoList = new List<IsolateCharacteristicDTO>
            {
                new IsolateCharacteristicDTO { CharacteristicType = "Text" },
                new IsolateCharacteristicDTO { CharacteristicType = "SingleList", VirusCharacteristicId = Guid.NewGuid(), CharacteristicValue = "Value" }
            };
            var modelList = new List<IsolateCharacteristicViewModel>
            {
                new IsolateCharacteristicViewModel { CharacteristicType = "Text" },
                new IsolateCharacteristicViewModel { CharacteristicType = "SingleList", VirusCharacteristicId = Guid.NewGuid(), CharacteristicValue = "Value" }
            };

            _isolatesService.GetIsolateCharacteristicInfoAsync(isolate).Returns(dtoList);
            _mapper.Map<List<IsolateCharacteristicViewModel>>(Arg.Any<List<IsolateCharacteristicDTO>>()).Returns(modelList);

            // Act
            var result = await _controller.Edit("AVNumber", isolate) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<List<IsolateCharacteristicViewModel>>(result.Model); // Replaced IsInstanceOf with IsType
            Assert.Equal(2, ((List<IsolateCharacteristicViewModel>)result.Model).Count);
        }

        [Fact]
        public void Edit_GetIsolateCharacteristicInfoAsyncThrowsException_ThrowsException()
        {
            // Arrange
            var isolate = Guid.NewGuid();
            _isolatesService.GetIsolateCharacteristicInfoAsync(isolate)
                .Returns(callInfo => Task.FromException<IEnumerable<IsolateCharacteristicDTO>>(new Exception("Test exception")));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(() => _controller.Edit("AVNumber", isolate));
        }

        [Fact]
        public async Task Edit_ValidInput_ReturnsRedirectToActionResult()
        {
            // Arrange
            var characteristics = new List<IsolateCharacteristicViewModel>
            {
                new IsolateCharacteristicViewModel { AVNumber = "AV001", VirusCharacteristicId = Guid.NewGuid() }
            };
            _virusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(new List<VirusCharacteristicDTO>());
            _mapper.Map<IsolateCharacteristicDTO>(Arg.Any<IsolateCharacteristicViewModel>()).Returns(new IsolateCharacteristicDTO());

            // Act
            var result = await _controller.Edit(characteristics);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.NotNull(redirectResult); // Ensure redirectResult is not null
            Assert.Equal("Index", redirectResult!.ActionName); // Use null-forgiving operator
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Edit_InvalidModelState_ReturnsViewResult()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");
            var characteristics = new List<IsolateCharacteristicViewModel>();

            // Act
            var result = await _controller.Edit(characteristics);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_ValidationErrors_ReturnsViewResultWithModelErrors()
        {
            // Arrange
            var characteristics = new List<IsolateCharacteristicViewModel>
            {
                new IsolateCharacteristicViewModel
                {
                    AVNumber = "AV001",
                    VirusCharacteristicId = Guid.NewGuid(),
                    CharacteristicType = "Text",
                    CharacteristicValue = "Invalid"
                }
            };
            var virusCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO
                {
                    Id = characteristics[0].VirusCharacteristicId!.Value, // Use the null-forgiving operator (!) to suppress the nullable warning
                    Length = 5
                }
            };
            _virusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(virusCharacteristics);

            // Act
            var result = await _controller.Edit(characteristics);

            // Assert
            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_ValidInput_SuccessfulUpdate()
        {
            // Arrange
            var characteristics = new List<IsolateCharacteristicViewModel>
            {
                new IsolateCharacteristicViewModel { VirusCharacteristicId = Guid.NewGuid(), CharacteristicType = "Text", CharacteristicValue = "Test" }
            };
            // Updated the code to handle the nullable value type warning (CS8629) by using the null-coalescing operator.
            var existingCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = characteristics[0].VirusCharacteristicId ?? Guid.Empty }
            };
            _virusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(existingCharacteristics);
            _mapper.Map<IsolateCharacteristicDTO>(Arg.Any<IsolateCharacteristicViewModel>()).Returns(new IsolateCharacteristicDTO());

            // Act
            var result = await _controller.Edit(characteristics);

            // Assert
            await _isolatesService.Received(1).UpdateIsolateCharacteristicsAsync(Arg.Any<IsolateCharacteristicDTO>(), Arg.Any<string>());
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Edit_InvalidInput_ReturnsViewWithErrors()
        {
            // Arrange
            var characteristics = new List<IsolateCharacteristicViewModel>
            {
                new IsolateCharacteristicViewModel { VirusCharacteristicId = Guid.NewGuid(), CharacteristicType = "Numeric", CharacteristicValue = "InvalidNumber" }
            };
            // Updated the code to handle the nullable value type warning (CS8629) by using the null-coalescing operator.
            var existingCharacteristics = new List<VirusCharacteristicDTO>
            {
                new VirusCharacteristicDTO { Id = characteristics[0].VirusCharacteristicId ?? Guid.Empty }
            };
            _virusCharacteristicService.GetAllVirusCharacteristicsAsync().Returns(existingCharacteristics);
            _controller.ModelState.AddModelError("Error", "Invalid input");

            // Act
            var result = await _controller.Edit(characteristics);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.NotNull(viewResult); // Ensure viewResult is not null
            Assert.Equal(characteristics, viewResult!.Model); // Use null-forgiving operator to suppress warning
        }

        [Fact]
        public async Task GetDropDownList_ValidInput_ReturnsListOfSelectListItems()
        {
            // Arrange
            var virusCharacteristicId = Guid.NewGuid();
            var characteristicValue = "TestValue";
            var options = new List<VirusCharacteristicListEntryDTO>
            {
            new VirusCharacteristicListEntryDTO { Name = "TestValue" },
            new VirusCharacteristicListEntryDTO { Name = "OtherValue" }
            };
            _virusCharacteristicListEntryService.GetEntriesByCharacteristicIdAsync(virusCharacteristicId).Returns(options);

            // Act
            var result = await _controller.GetDropDownList(virusCharacteristicId, characteristicValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, item => item.Value == "TestValue" && item.Selected);
            Assert.Contains(result, item => item.Value == "OtherValue" && !item.Selected);
        }

        [Fact]
        public async Task GetDropDownList_InvalidModelState_ReturnsEmptyList()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = await _controller.GetDropDownList(Guid.NewGuid(), "TestValue");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDropDownList_NoMatchingEntries_ReturnsEmptyList()
        {
            // Arrange
            var virusCharacteristicId = Guid.NewGuid();
            _virusCharacteristicListEntryService.GetEntriesByCharacteristicIdAsync(virusCharacteristicId).Returns(new List<VirusCharacteristicListEntryDTO>());

            // Act
            var result = await _controller.GetDropDownList(virusCharacteristicId, "TestValue");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDropDownList_MatchingEntries_VerifySelectedItem()
        {
            // Arrange
            var virusCharacteristicId = Guid.NewGuid();
            var characteristicValue = "SelectedValue";
            var options = new List<VirusCharacteristicListEntryDTO>
{
new VirusCharacteristicListEntryDTO { Name = "SelectedValue" },
new VirusCharacteristicListEntryDTO { Name = "OtherValue" }
};
            _virusCharacteristicListEntryService.GetEntriesByCharacteristicIdAsync(virusCharacteristicId).Returns(options);

            // Act
            var result = await _controller.GetDropDownList(virusCharacteristicId, characteristicValue);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, item => item.Value == "SelectedValue" && item.Selected);
            Assert.Contains(result, item => item.Value == "OtherValue" && !item.Selected);
        }

        [Fact]
        public void ValidateCharacteristic_ValidTextInput_ReturnsEmptyString()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Text",
                CharacteristicName = "TestCharacteristic",
                CharacteristicValue = "ValidText"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                Length = 10
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Empty(result);
        }

        [Fact]
        public void ValidateCharacteristic_InvalidTextInput_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Text",
                CharacteristicName = "TestCharacteristic",
                CharacteristicValue = "ThisTextIsTooLong"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                Length = 10
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("exceeds maximum length requirement", result);
        }

        [Fact]
        public void ValidateCharacteristic_ValidNumericInput_ReturnsEmptyString()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Numeric",
                CharacteristicName = "TestNumeric",
                CharacteristicValue = "5.00"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                MinValue = 0,
                MaxValue = 10,
                DecimalPlaces = 2
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Empty(result);
        }

        [Fact]
        public void ValidateCharacteristic_NumericInputBelowMinValue_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Numeric",
                CharacteristicName = "TestNumeric",
                CharacteristicValue = "-1"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                MinValue = 0,
                MaxValue = 10
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("below the minimum value requirement", result);
        }

        [Fact]
        public void ValidateCharacteristic_NumericInputAboveMaxValue_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Numeric",
                CharacteristicName = "TestNumeric",
                CharacteristicValue = "11"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                MinValue = 0,
                MaxValue = 10
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("exceeds the maximum value requirement", result);
        }

        [Fact]
        public void ValidateCharacteristic_NumericInputIncorrectDecimalPlaces_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Numeric",
                CharacteristicName = "TestNumeric",
                CharacteristicValue = "5.1"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                MinValue = 0,
                MaxValue = 10,
                DecimalPlaces = 2
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("does not include the required number of decimal places", result);
        }

        [Fact]
        public void ValidateCharacteristic_NonNumericValue_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.NewGuid(),
                CharacteristicType = "Numeric",
                CharacteristicName = "TestNumeric",
                CharacteristicValue = "NotANumber"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO
            {
                MinValue = 0,
                MaxValue = 10
            };

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("is not a valid number", result);
        }

        [Fact]
        public void ValidateCharacteristic_EmptyVirusCharacteristicId_ReturnsErrorMessage()
        {
            var characteristicViewModel = new IsolateCharacteristicViewModel
            {
                VirusCharacteristicId = Guid.Empty,
                CharacteristicType = "Text",
                CharacteristicName = "TestCharacteristic",
                CharacteristicValue = "ValidText"
            };
            var virusCharacteristicDTO = new VirusCharacteristicDTO();

            var result = IsolateCharacteristicsController.ValidateCharacteristic(characteristicViewModel, virusCharacteristicDTO);

            Assert.Contains("Id not specified for this item", result);
        }        
    }
}
