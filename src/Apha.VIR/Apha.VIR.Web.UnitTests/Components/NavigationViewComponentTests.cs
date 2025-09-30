using System.Reflection;
using Apha.VIR.Web.Components;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Components
{
    public class NavigationViewComponentTests
    { // Helper to create the component with injected dependencies and set the private _navItemsTask
        private NavigationViewComponent CreateComponent(List<NavItem> navItems,
            DefaultHttpContext httpContext,
            out ICacheService cacheService,
            out IMapper mapper)
        {
            // mock environment (NavigationService requires IWebHostEnvironment in ctor)
            var env = Substitute.For<IWebHostEnvironment>();
            env.WebRootPath.Returns(Path.GetTempPath());

            // create real NavigationService and replace its private _navItemsTask with our navItems
            var navService = new NavigationService(env);
            var field = typeof(NavigationService).GetField("_navItemsTask", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?? throw new InvalidOperationException("_navItemsTask field not found. Check NavigationService implementation.");
            field.SetValue(navService, Task.FromResult(navItems));

            // NSubstitute mocks (as requested)
            cacheService = Substitute.For<ICacheService>();
            mapper = Substitute.For<IMapper>();
            // Map<List<NavItem>>(navItems) should return the same list so the component works on a mapped copy
            mapper.Map<List<NavItem>>(Arg.Any<List<NavItem>>()).Returns(ci => ci.ArgAt<List<NavItem>>(0));

            var component = new NavigationViewComponent(navService, cacheService, mapper);

            // Attach HttpContext for the ViewComponent (so component.HttpContext.Request.RouteValues etc. work)
            component.ViewComponentContext = new ViewComponentContext
            {
                ViewContext = new ViewContext
                {
                    HttpContext = httpContext,
                    ViewData = new ViewDataDictionary(new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary())
                }
            };

            return component;
        }

        [Fact]
        public async Task InvokeAsync_Inserts_HomeNode_When_NoTrailFound()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["controller"] = "Other";
            httpContext.Request.RouteValues["action"] = "Index";
            httpContext.Request.Path = "/Other/Index";

            var navItems = new List<NavItem>
            {
                new NavItem{ Url = "/", Title = "Home" },
                new NavItem{ Url = "/unrelated", Title = "Unrelated" }
            };

            var component = CreateComponent(navItems, httpContext, out var cacheService, out var mapper);

            // Act
            var result = await component.InvokeAsync();

            // Assert
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData); // Ensure ViewData is not null
            Assert.NotNull(viewResult.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<List<NavItem>>(viewResult.ViewData.Model);

            // Because nothing matched the path, component inserts the home node (only)
            Assert.Single(model);
            Assert.Equal("/", model[0].Url);
        }

        [Fact]
        public async Task InvokeAsync_Isolates_Create_ChangesActionTo_Edit_And_FindsBreadcrumb()
        {
            // Arrange for isolates/create -> actionName becomes "Edit"
            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["controller"] = "Isolates";
            httpContext.Request.RouteValues["action"] = "Create";
            httpContext.Request.Path = "/Isolates/Create";

            var navItems = new List<NavItem>
            {
                new NavItem{ Url = "/", Title = "Home" },
                new NavItem{ Url = "/Isolates/Edit", Title = "Edit isolate" }
            };

            var component = CreateComponent(navItems, httpContext, out var cacheService, out var mapper);

            // Act
            var result = await component.InvokeAsync();

            // Assert
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData); // Ensure ViewData is not null
            Assert.NotNull(viewResult.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<List<NavItem>>(viewResult.ViewData.Model);

            // Home node must be inserted and the found Edit node must be present
            Assert.Equal(2, model.Count);
            Assert.Equal("/", model[0].Url);                // home inserted
            Assert.Equal("/Isolates/Edit", model[1].Url);   // matched node after actionName changed to Edit
        }

        [Fact]
        public async Task InvokeAsync_Finds_Recursive_Trail_And_Updates_PreviousUrl_When_CacheReturnsValue()
        {
            // Arrange: parent -> child structure so FindBreadcrumbTrail returns [parent, child]
            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["controller"] = "Parent";
            httpContext.Request.RouteValues["action"] = "Child";
            httpContext.Request.Path = "/Parent/Child";

            var child = new NavItem { Url = "/parent/child", Title = "Child" };
            var parent = new NavItem { Url = "/parent", Title = "Parent", Children = new List<NavItem> { child } };
            var navItems = new List<NavItem> { new NavItem { Url = "/", Title = "Home" }, parent };

            var component = CreateComponent(navItems, httpContext, out var cacheService, out var mapper);

            // Cache returns a full URL for parent -> GetPreviousUpdatedBreadcrumbUrl should set UpdatedUrl
            cacheService.GetFullUrlFor("/parent").Returns("https://example.com/parent-updated");

            // Act
            var result = await component.InvokeAsync();

            // Assert
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData); // Ensure ViewData is not null
            Assert.NotNull(viewResult.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<List<NavItem>>(viewResult.ViewData.Model);

            // Model should be [home, parent, child] and parent.UpdatedUrl should be the cached full URL
            Assert.Equal(3, model.Count);
            Assert.Equal("/", model[0].Url);
            Assert.Equal("/parent", model[1].Url);
            Assert.Equal("https://example.com/parent-updated", model[1].UpdatedUrl);
            Assert.Equal("/parent/child", model[2].Url);

            // verify cache was consulted exactly once for the parent
            cacheService.Received(1).GetFullUrlFor("/parent");
        }

        [Fact]
        public async Task InvokeAsync_Sets_UpdatedUrl_To_Url_When_CacheReturnsEmpty()
        {
            // Arrange: same structure, but cache returns null/empty for parent
            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["controller"] = "Parent";
            httpContext.Request.RouteValues["action"] = "Child";
            httpContext.Request.Path = "/Parent/Child";

            var child = new NavItem { Url = "/parent/child", Title = "Child" };
            var parent = new NavItem { Url = "/parent", Title = "Parent", Children = new List<NavItem> { child } };
            var navItems = new List<NavItem> { new NavItem { Url = "/", Title = "Home" }, parent };

            var component = CreateComponent(navItems, httpContext, out var cacheService, out var mapper);

            // Cache returns null -> UpdatedUrl should fallback to item.Url
            cacheService.GetFullUrlFor("/parent").Returns((string?)null);

            // Act
            var result = await component.InvokeAsync();

            // Assert
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData); // Ensure ViewData is not null
            Assert.NotNull(viewResult.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<List<NavItem>>(viewResult.ViewData.Model);

            Assert.Equal(3, model.Count);
            Assert.Equal("/parent", model[1].Url);
            Assert.Equal("/parent", model[1].UpdatedUrl); // fallback to Url
            cacheService.Received(1).GetFullUrlFor("/parent");
        }

        [Fact]
        public async Task InvokeAsync_Replaces_SearchRepository_Index_With_Search()
        {
            // Arrange: controller SearchRepository + Index should be replaced to SearchRepository/Search
            var httpContext = new DefaultHttpContext();
            httpContext.Request.RouteValues["controller"] = "SearchRepository";
            httpContext.Request.RouteValues["action"] = "Index";
            httpContext.Request.Path = "/SearchRepository/Index";

            var navItems = new List<NavItem>
            {
                new NavItem { Url = "/", Title = "Home" },
                new NavItem { Url = "/SearchRepository/Search", Title = "Search" }
            };

            var component = CreateComponent(navItems, httpContext, out var cacheService, out var mapper);

            // Act
            var result = await component.InvokeAsync();

            // Assert that the SearchRepository/Search node is matched
            var viewResult = Assert.IsType<ViewViewComponentResult>(result);
            Assert.NotNull(viewResult.ViewData); // Ensure ViewData is not null
            Assert.NotNull(viewResult.ViewData.Model); // Ensure Model is not null
            var model = Assert.IsType<List<NavItem>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("/SearchRepository/Search", model[1].Url);
        }
    }
}
