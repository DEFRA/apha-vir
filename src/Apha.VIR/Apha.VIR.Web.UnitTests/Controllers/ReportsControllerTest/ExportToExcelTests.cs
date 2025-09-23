using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;

namespace Apha.VIR.Web.UnitTests.Controllers.ReportsControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class ExportToExcelTests
    {
        private readonly object _lock;
        private readonly IReportService _mockReportService;
        private readonly IMapper _mockMapper;
        private readonly ReportsController _controller;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public ExportToExcelTests(AppRolesFixture fixture)
        {
            _mockReportService = Substitute.For<IReportService>();
            _mockMapper = Substitute.For<IMapper>();
            _controller = new ReportsController(_mockReportService, _mockMapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task ExportToExcel_ValidInputWithData_ReturnsFileResultWithCorrectContentTypeAndFileName()
        {
            // Arrange
            _controller.ModelState.Clear();

            var dateFrom = new DateTime(2023, 5, 1);
            var dateTo = new DateTime(2023, 5, 10);

            var inputModel = new IsolateDispatchReportViewModel
            {
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var serviceData = new List<IsolateDispatchReportDto>
                                    {
                                        new IsolateDispatchReportDto
                                        {
                                            AVNumber = "AV001",
                                            Nomenclature = "Virus X",
                                            NoOfAliquots = 10,
                                            PassageNumber = 2,
                                            Recipient = "Lab A",
                                            RecipientName = "Dr. Smith",
                                            RecipientAddress = "123 Research Way",
                                            ReasonForDispatch = "Testing",
                                            DispatchedDate = new DateTime(2023, 5, 5),
                                            DispatchedByName = "Technician A"
                                        }
                                    };

            var mappedData = serviceData.Select(dto => new IsolateDispatchReportModel
            {
                AVNumber = dto.AVNumber!,
                Nomenclature = dto.Nomenclature!,
                NoOfAliquots = dto.NoOfAliquots,
                PassageNumber = dto.PassageNumber,
                Recipient = dto.Recipient,
                RecipientName = dto.RecipientName,
                RecipientAddress = dto.RecipientAddress,
                ReasonForDispatch = dto.ReasonForDispatch,
                DispatchedDate = dto.DispatchedDate,
                DispatchedByName = dto.DispatchedByName
            }).ToList();

            _mockReportService.GetDispatchesReportAsync(dateFrom, dateTo).Returns(serviceData);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceData).Returns(mappedData);

            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(inputModel.DateFrom, inputModel.DateTo);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.NotNull(fileResult.FileContents);
            Assert.NotEmpty(fileResult.FileContents);

            // Optional: Validate file name
            var expectedFileNamePart = "VIR IsolateDispatchReport";
            Assert.Contains(expectedFileNamePart, fileResult.FileDownloadName);
        }
     
        [Fact]
        public async Task ExportToExcel_PropertiesWithoutDisplayAttribute_UsesPropertyName()
        {
            // Arrange
            _controller.ModelState.Clear();

            var dateFrom = new DateTime(2023, 5, 1);
            var dateTo = new DateTime(2023, 5, 10);

            var serviceData = new List<IsolateDispatchReportDto>
    {
        new IsolateDispatchReportDto
        {
            AVNumber = "AV001",
            Nomenclature = "Virus X"
        }
    };

            var mappedData = serviceData.Select(dto => new IsolateDispatchReportModel
            {
                AVNumber = dto.AVNumber!,
                Nomenclature = dto.Nomenclature!
            }).ToList();

            _mockReportService.GetDispatchesReportAsync(dateFrom, dateTo).Returns(serviceData);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceData).Returns(mappedData);

            lock (_lock)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
        };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(dateFrom, dateTo);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);

            // Verify Excel content uses property names when DisplayAttribute is null
            using (var stream = new MemoryStream(fileResult.FileContents))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.First();

                // Get properties without DisplayAttribute to verify fallback to property name
                var properties = typeof(IsolateDispatchReportModel).GetProperties()
                    .Where(p => p.Name != "DispatchedBy").ToList();

                // Verify that property names are used when DisplayAttribute is null (tests line 41)
                for (int i = 0; i < properties.Count; i++)
                {
                    var displayAttr = properties[i].GetCustomAttribute<DisplayAttribute>();
                    var expectedHeaderValue = displayAttr?.Name ?? properties[i].Name;
                    var actualHeaderValue = worksheet.Cell(1, i + 1).Value.ToString();
                    Assert.Equal(expectedHeaderValue, actualHeaderValue);
                }
            }
        }
      

        [Fact]
        public async Task ExportToExcel_ValidInputWithNoData_ReturnsFileResultWithEmptyWorksheet()
        {
            // Arrange
            var today = DateTime.Today;
            var expectedFileName = $"VIR IsolateDispatchReport {today.Day}{today:MMMM}{today.Year}.xlsx";

            _controller.ModelState.Clear();

            var model = new IsolateDispatchReportViewModel
            {
                DateFrom = new DateTime(2023, 1, 1),
                DateTo = new DateTime(2023, 1, 31)
            };

            _mockReportService.GetDispatchesReportAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>())
            .Returns(Task.FromResult<IEnumerable<IsolateDispatchReportDto>>(new List<IsolateDispatchReportDto>()));

            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(Arg.Any<IEnumerable<IsolateDispatchReportDto>>())
            .Returns(new List<IsolateDispatchReportModel>());

            lock (_lock)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(model.DateFrom, model.DateTo);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal(expectedFileName, fileResult.FileDownloadName);
            Assert.True(fileResult.FileContents.Length > 0);
        }
       
        [Fact]
        public async Task ExportToExcel_PropertiesWithDisplayAttributes_UsesDisplayAttributeNames()
        {
            // Arrange
            _controller.ModelState.Clear();

            var dateFrom = new DateTime(2023, 5, 1);
            var dateTo = new DateTime(2023, 5, 10);

            // Create test data that will exercise the DisplayAttribute logic
            var serviceData = new List<IsolateDispatchReportDto>
    {
        new IsolateDispatchReportDto
        {
            AVNumber = "AV001",
            Nomenclature = "Virus X",
            NoOfAliquots = 10,
            PassageNumber = 2,
            Recipient = "Lab A",
            RecipientName = "Dr. Smith",
            RecipientAddress = "123 Research Way",
            ReasonForDispatch = "Testing",
            DispatchedDate = new DateTime(2023, 5, 5),
            DispatchedByName = "Technician A"
        }
    };

            var mappedData = serviceData.Select(dto => new IsolateDispatchReportModel
            {
                AVNumber = dto.AVNumber!,
                Nomenclature = dto.Nomenclature!,
                NoOfAliquots = dto.NoOfAliquots,
                PassageNumber = dto.PassageNumber,
                Recipient = dto.Recipient,
                RecipientName = dto.RecipientName,
                RecipientAddress = dto.RecipientAddress,
                ReasonForDispatch = dto.ReasonForDispatch,
                DispatchedDate = dto.DispatchedDate,
                DispatchedByName = dto.DispatchedByName
            }).ToList();

            _mockReportService.GetDispatchesReportAsync(dateFrom, dateTo).Returns(serviceData);
            _mockMapper.Map<IEnumerable<IsolateDispatchReportModel>>(serviceData).Returns(mappedData);

            lock (_lock)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
        };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(dateFrom, dateTo);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.NotNull(fileResult.FileContents);
            Assert.NotEmpty(fileResult.FileContents);

            // Verify the Excel content can be read (this exercises both line 41 and 63)
            using (var stream = new MemoryStream(fileResult.FileContents))
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.First();

                // Verify headers are populated (tests line 41 - displayAttr?.Name ?? properties[i].Name)
                Assert.True(worksheet.Cell(1, 1).Value.ToString().Length > 0);

                // Verify data cells are populated (tests line 63 - value?.ToString() ?? string.Empty)
                Assert.True(worksheet.Cell(2, 1).Value.ToString().Length > 0);
            }
        }
       
        [Fact]
        public async Task ExportToExcel_BothDatesAreNull_ReturnsViewWithBothModelErrors()
        {
            // Arrange
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            lock (_lock)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
        };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(dateFrom, dateTo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchReportViewModel>(viewResult.Model);
            Assert.Null(model.DateFrom);
            Assert.Null(model.DateTo);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(dateFrom)));
            Assert.True(_controller.ModelState.ContainsKey(nameof(dateTo)));
            Assert.Equal("Date From must be entered", _controller.ModelState[nameof(dateFrom)]!.Errors[0].ErrorMessage);
            Assert.Equal("Date To must be entered", _controller.ModelState[nameof(dateTo)]!.Errors[0].ErrorMessage);
        }
      
        [Fact]
        public async Task ExportToExcel_DateToIsNull_ReturnsViewWithModelError()
        {
            // Arrange
            var dateFrom = new DateTime(2023, 5, 1);
            DateTime? dateTo = null;

            lock (_lock)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
        };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(dateFrom, dateTo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchReportViewModel>(viewResult.Model);
            Assert.Equal(dateFrom, model.DateFrom);
            Assert.Null(model.DateTo);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(dateTo)));
            Assert.Equal("Date To must be entered", _controller.ModelState[nameof(dateTo)]!.Errors[0].ErrorMessage);
        }
      
        [Fact]
        public async Task ExportToExcel_DateFromIsNull_ReturnsViewWithModelError()
        {
            // Arrange
            DateTime? dateFrom = null;
            var dateTo = new DateTime(2023, 5, 10);

            lock (_lock)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, AppRoleConstant.Administrator)
        };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act
            var result = await _controller.ExportToExcel(dateFrom, dateTo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<IsolateDispatchReportViewModel>(viewResult.Model);
            Assert.Null(model.DateFrom);
            Assert.Equal(dateTo, model.DateTo);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey(nameof(dateFrom)));
            Assert.Equal("Date From must be entered", _controller.ModelState[nameof(dateFrom)]!.Errors[0].ErrorMessage);
        }
      
        [Fact]
        public async Task ExportToExcel_UserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var dateFrom = new DateTime(2023, 5, 1,0,0,0,DateTimeKind.Utc);
            var dateTo = new DateTime(2023, 5, 10, 0, 0, 0, DateTimeKind.Utc); // or DateTimeKind.Local or DateTimeKind.Unspecified


            lock (_lock)
            {
                // Simulate a user with no roles
                var claimsIdentity = new ClaimsIdentity();
                var user = new ClaimsPrincipal(claimsIdentity);
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _controller.ExportToExcel(dateFrom, dateTo));
            Assert.Equal("User not authorised to retrieve this list", exception.Message);
        }
        // ...existing code...
    }
}
