using System.Security.Claims;
using Apha.VIR.Application.DTOs;
using Apha.VIR.Application.Interfaces;
using Apha.VIR.Web.Controllers;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Controllers.SampleControllerTest
{
    [Collection("UserAppRolesValidationTests")]
    public class SampleControllerTests
    {
        private readonly object _lock;
        private readonly SampleController _controller;
        private readonly ISampleService _sampleService;
        private readonly ILookupService _lookupService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _mockHttpContextAccessor;

        public SampleControllerTests(AppRolesFixture fixture)
        {
            _sampleService = Substitute.For<ISampleService>();
            _lookupService = Substitute.For<ILookupService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new SampleController(_sampleService, _lookupService, _mapper);
            _mockHttpContextAccessor = Substitute.For<IHttpContextAccessor>();
            AuthorisationUtil.Configure(_mockHttpContextAccessor);
            _lock = fixture.LockObject;
        }

        [Fact]
        public async Task Index_WithValidSample_ReturnsViewWithViewModel()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var SampleDto = new SampleDto { SampleId = sampleId };
            var viewModel = new SampleViewModel { SampleId = sampleId };

            _sampleService.GetSampleAsync(avNumber, sampleId).Returns(SampleDto);
            _mapper.Map<SampleViewModel>(SampleDto).Returns(viewModel);

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); // Ensure the model is not null
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure the model is not null before dereferencing                       
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithNullSample_ReturnsViewWithNewViewModel()
        {
            // Arrange
            var avNumber = "AV123";

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model); // Ensure the model is not null before dereferencing
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure the model is not null before dereferencing            
            Assert.Equal(avNumber, model.AVNumber);
        }

        [Fact]
        public async Task Index_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Sample error");

            // Act
            var result = await _controller.Create("AV123");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Index_CorrectMappingAndViewModelPopulation()
        {
            // Arrange
            var avNumber = "AV123";
            var sampleId = Guid.NewGuid();
            var SampleDto = new SampleDto { SampleId = sampleId };
            var viewModel = new SampleViewModel
            {
                SampleId = sampleId,
                SampleTypeList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostSpeciesList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostBreedList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>(), // Initialize to avoid null
                HostPurposeList = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>() // Initialize to avoid null
            };

            _sampleService.GetSampleAsync(avNumber, sampleId).Returns(SampleDto);
            _mapper.Map<SampleViewModel>(SampleDto).Returns(viewModel);

            _lookupService.GetAllSampleTypesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDto>>(new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Type1" }
            }));
            _lookupService.GetAllHostSpeciesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDto>>(new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Species1" }
            }));
            _lookupService.GetAllHostPurposesAsync().Returns(Task.FromResult<IEnumerable<LookupItemDto>>(new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Purpose1" }
            }));
            _lookupService.GetAllHostBreedsAsync().Returns(Task.FromResult<IEnumerable<LookupItemDto>>(new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Breed1" }
            }));

            // Act
            var result = await _controller.Create(avNumber) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<SampleViewModel>(result.Model);
            var model = result.Model as SampleViewModel;
            Assert.NotNull(model); // Ensure model is not null before dereferencing
            Assert.NotNull(model.SampleTypeList); // Ensure SampleTypeList is not null
            Assert.NotEmpty(model.SampleTypeList);
            Assert.NotNull(model.HostSpeciesList); // Ensure HostSpeciesList is not null
            Assert.NotEmpty(model.HostSpeciesList);
            Assert.NotNull(model.HostBreedList); // Ensure HostBreedList is not null
            Assert.NotEmpty(model.HostBreedList);
            Assert.NotNull(model.HostPurposeList); // Ensure HostPurposeList is not null
            Assert.NotEmpty(model.HostPurposeList);
        }

        [Fact]
        public async Task Insert_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "TEST123" };
            _mapper.Map<SampleDto>(model).Returns(new SampleDto());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(model);

            // Assert
            await _sampleService.Received(1).AddSample(Arg.Any<SampleDto>(), "TEST123", "TestUser");
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;

            // Fix for CS8602: Ensure RouteValues is not null before accessing
            Assert.NotNull(redirectResult.RouteValues); // Ensure RouteValues is not null
            Assert.Equal("TEST123", redirectResult.RouteValues["AVNumber"]);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Insert_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var model = new SampleViewModel();
            _controller.ModelState.AddModelError("Error", "Test error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting
            Assert.NotNull(viewResult.Model);
            var viewModel = (SampleViewModel)viewResult.Model;

            Assert.Equal(model, viewModel);
            await _lookupService.Received(1).GetAllSampleTypesAsync();
            await _lookupService.Received(1).GetAllHostSpeciesAsync();
            await _lookupService.Received(1).GetAllHostPurposesAsync();
            await _lookupService.Received(1).GetAllHostBreedsAsync();
        }

        [Fact]
        public async Task Update_ValidModel_ReturnsRedirectToActionResult()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "TEST123" };
            var SampleDto = new SampleDto();
            _mapper.Map<SampleDto>(model).Returns(SampleDto);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Edit(model);

            // Assert
            await _sampleService.Received(1).UpdateSample(SampleDto, "TestUser");
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = (RedirectToActionResult)result;

            // Fix for CS8602: Ensure RouteValues is not null before accessing
            Assert.NotNull(redirectResult.RouteValues); // Ensure RouteValues is not null
            Assert.Equal("TEST123", redirectResult.RouteValues["AVNumber"]);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("SubmissionSamples", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Update_InvalidModel_ReturnsViewResult()
        {
            // Arrange
            var model = new SampleViewModel();
            _controller.ModelState.AddModelError("Error", "Model error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = (ViewResult)result;

            // Fix for CS8600 and CS8602: Ensure viewResult.Model is not null before casting and dereferencing
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task Update_SuccessfulUpdate_CallsUpdateSample()
        {
            // Arrange
            var model = new SampleViewModel();
            var SampleDto = new SampleDto();
            _mapper.Map<SampleDto>(model).Returns(SampleDto);
            SetupMockUserAndRoles();
            // Act
            await _controller.Edit(model);

            // Assert
            await _sampleService.Received(1).UpdateSample(SampleDto, "TestUser");
        }

        [Fact]
        public async Task Update_ExceptionThrown_ReturnsViewResult()
        {
            // Arrange
            var model = new SampleViewModel();
            var SampleDto = new SampleDto();
            _mapper.Map<SampleDto>(model).Returns(SampleDto);
            _sampleService
                .UpdateSample(SampleDto, "TestUser")
                .Returns(Task.FromException(new Exception("Test exception")));
            SetupMockUserAndRoles();
            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Edit(model));
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_ValidSpeciesId_ReturnsBreedList()
        {
            // Arrange
            var speciesId = Guid.NewGuid();
            var breeds = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Breed1" },
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Breed2" }
            };
            _lookupService.GetAllHostBreedsByParentAsync(speciesId).Returns(breeds);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetBreedsBySpecies(speciesId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var breedList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Equal(2, breedList.Count);
            Assert.Equal("Breed1", breedList[0].Text);
            Assert.Equal("Breed2", breedList[1].Text);
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_NullSpeciesId_ReturnsEmptyList()
        {
            // Arrange
            _lookupService.GetAllHostBreedsByParentAsync(null).Returns(new List<LookupItemDto>());
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetBreedsBySpecies(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var breedList = Assert.IsType<List<SelectListItem>>(jsonResult.Value);
            Assert.Empty(breedList);
        }

        [Fact]
        public async Task Test_GetBreedsBySpecies_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Some error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetBreedsBySpecies(Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsPartialView_WhenModelStateIsValid()
        {
            // Arrange
            var latinBreedDtos = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Breed", ParentName = "Test Species", AlternateName = "Test Alt", Active = true, Sms = true, Smscode = "123" }
            };
            _lookupService.GetAllHostBreedsAltNameAsync().Returns(latinBreedDtos);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_LatinBreed", viewResult.ViewName);
            var model = Assert.IsAssignableFrom<List<LatinBreedModel>>(viewResult.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_GetLatinBreadList_ReturnsCorrectData()
        {
            // Arrange
            var latinBreedDtos = new List<LookupItemDto>
            {
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Breed 1", ParentName = "Test Species 1", AlternateName = "Test Alt 1", Active = true, Sms = true, Smscode = "123" },
                new LookupItemDto { Id = Guid.NewGuid(), Name = "Test Breed 2", ParentName = "Test Species 2", AlternateName = "Test Alt 2", Active = false, Sms = false, Smscode = "456" }
            };
            _lookupService.GetAllHostBreedsAltNameAsync().Returns(latinBreedDtos);
            SetupMockUserAndRoles();
            // Act
            var result = await _controller.GetLatinBreadList();

            // Assert
            var viewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsAssignableFrom<List<LatinBreedModel>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Test Breed 1", model[0].Name);
            Assert.Equal("Test Breed 2", model[1].Name);
            Assert.True(model[0].Active);
            Assert.False(model[1].Active);
        }

        [Fact]
        public async Task Create_Get_WithNullAVNumber_RedirectsToHomeIndex()
        {
            // Act
            var result = await _controller.Create((string)null!);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Create_Get_WithEmptyAVNumber_RedirectsToHomeIndex()
        {
            // Act
            var result = await _controller.Create((string)string.Empty);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Create_Post_WhenUserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "AV123" };
            AuthorisationUtil.AppRoles = new List<string>(); 

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Create(model));
        }

        [Fact]
        public async Task Edit_Get_WithNullAVNumber_RedirectsToHomeIndex()
        {
            // Act
            var result = await _controller.Edit(null!, Guid.NewGuid());

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }


        [Fact]
        public async Task Edit_Get_WithEmptyAVNumber_RedirectsToHomeIndex()
        {
            // Act
            var result = await _controller.Edit(string.Empty, Guid.NewGuid());

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Edit_Get_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = await _controller.Edit("AV123", Guid.NewGuid());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WhenUserNotAuthorized_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var model = new SampleViewModel { AVNumber = "AV123" };
            AuthorisationUtil.AppRoles = new List<string>(); 

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Edit(model));
        }

        [Fact]
        public async Task GetBreedsBySpecies_WhenUserNotInAnyRole_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            AuthorisationUtil.AppRoles = new List<string>(); 

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetBreedsBySpecies(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetLatinBreadList_WhenUserNotInAnyRole_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            AuthorisationUtil.AppRoles = new List<string>(); 

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetLatinBreadList());
        }

        private void SetupMockUserAndRoles()
        {
            lock (_lock)
            {
                var claims = new List<Claim>
                {    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager)
                };
                var user = new ClaimsPrincipal(new ClaimsIdentity(claims));
                _mockHttpContextAccessor?.HttpContext?.User.Returns(user);

                var appRoles = new List<string> { AppRoleConstant.LookupDataManager, AppRoleConstant.IsolateManager, AppRoleConstant.Administrator };
                AuthorisationUtil.AppRoles = appRoles;
            }
        }
    }
}

