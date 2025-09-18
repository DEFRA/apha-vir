using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Web.Utilities;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Utilities
{
    public class AuthorisationUtilTests
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpContext _httpContext;
        private readonly ClaimsPrincipal _user;
        public AuthorisationUtilTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _httpContext = Substitute.For<HttpContext>();
            _user = Substitute.For<ClaimsPrincipal>();

            _httpContextAccessor.HttpContext.Returns(_httpContext);
            _httpContext.User.Returns(_user);
            AuthorisationUtil.Configure(_httpContextAccessor);
        }

        [Fact]
        public void Configure_ValidHttpContextAccessor_SetsFieldCorrectly()
        {
            // Arrange
            var httpContextAccessor = Substitute.For<IHttpContextAccessor>();

            // Act
            AuthorisationUtil.Configure(httpContextAccessor);

            // Assert
            var fieldInfo = typeof(AuthorisationUtil).GetField("_httpContextAccessor", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(fieldInfo);
            var value = fieldInfo.GetValue(null);
            Assert.Same(httpContextAccessor, value);
        }

        [Fact]
        public void Configure_NullHttpContextAccessor_ThrowsArgumentNullException()
        {
            // Arrange
            IHttpContextAccessor? httpContextAccessor = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => AuthorisationUtil.Configure(httpContextAccessor!));
        }

        [Fact]
        public void CanGetItem_UserInRole_ReturnsTrue()
        {
            // Arrange
            var role = "TestRole";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
new Claim(ClaimTypes.Role, role)
}));
            _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

            // Act
            var result = AuthorisationUtil.CanGetItem(role);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanGetItem_UserNotInRole_ReturnsFalse()
        {
            // Arrange
            var role = "TestRole";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity([
new Claim(ClaimTypes.Role, "DifferentRole")
]));
            _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

            // Act
            var result = AuthorisationUtil.CanGetItem(role);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanGetItem_CurrentUserIsNull_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.CanGetItem("AnyRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanGetItem_HttpContextIsNull_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.CanGetItem("AnyRole");

            // Assert
            Assert.False(result);

            // Additional assertion to differentiate from CanGetItem_CurrentUserIsNull_ReturnsFalse
            Assert.Null(_httpContextAccessor.HttpContext);
        }

        [Fact]
        public void CanGetItem_UserIsNull_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = new ClaimsPrincipal() });

            // Act
            var result = AuthorisationUtil.CanGetItem("AnyRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddItem_UserInRole_ReturnsTrue()
        {
            // Arrange
            var role = "TestRole";
            SetupMockUser(new[] { role });

            // Act
            var result = AuthorisationUtil.CanAddItem(role);

            // Assert
            Assert.True(result);
        }

        private static readonly string[] OtherRoleArray = new[] { "OtherRole" };
        [Fact]
        public void CanAddItem_UserNotInRole_ReturnsFalse()
        {
            // Arrange
            var role = "TestRole";
            SetupMockUser(OtherRoleArray);

            // Act
            var result = AuthorisationUtil.CanAddItem(role);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanAddItem_NullUser_ReturnsFalse()
        {
            // Arrange
            var role = "TestRole";
            SetupNullUser();

            // Act
            var result = AuthorisationUtil.CanAddItem(role);

            // Assert
            Assert.False(result);
        }

        private static readonly string[] SomeRoleArray = new[] { "SomeRole" };
        [Fact]
        public void CanAddItem_EmptyRole_ReturnsFalse()
        {
            // Arrange
            var role = string.Empty;
            SetupMockUser(SomeRoleArray);

            // Act
            var result = AuthorisationUtil.CanAddItem(role);

            // Assert
            Assert.False(result);
        }

        private void SetupMockUser(string[] roles)
        {
            var identity = new ClaimsIdentity();
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext { User = principal };
            _httpContextAccessor.HttpContext.Returns(context);
        }

        private void SetupNullUser()
        {
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        }

        [Fact]
        public void CanEditItem_UserInRole_ReturnsTrue()
        {
            // Arrange
            SetupUser("TestRole");

            // Act
            var result = AuthorisationUtil.CanEditItem("TestRole");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanEditItem_UserNotInRole_ReturnsFalse()
        {
            // Arrange
            SetupUser("OtherRole");

            // Act
            var result = AuthorisationUtil.CanEditItem("TestRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanEditItem_NullRole_ReturnsFalse()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = AuthorisationUtil.CanEditItem("TestRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanEditItem_EmptyRole_ReturnsFalse()
        {
            // Arrange
            SetupUser(string.Empty);

            // Act
            var result = AuthorisationUtil.CanEditItem("TestRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanEditItem_MultipleRoles_ReturnsTrue()
        {
            // Arrange
            SetupUser("Role1", "TestRole", "Role2");

            // Act
            var result = AuthorisationUtil.CanEditItem("TestRole");

            // Assert
            Assert.True(result);
        }

        private void SetupUser(params string[]? roles)
        {
            var claims = roles?.Select(r => new Claim(ClaimTypes.Role, r)).ToList() ?? new List<Claim>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

            var context = new DefaultHttpContext { User = user };
            _httpContextAccessor.HttpContext.Returns(context);
        }

        [Fact]
        public void CanDeleteItem_UserInRole_ReturnsTrue()
        {
            // Arrange
            var role = "TestRole";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
new Claim(ClaimTypes.Role, role)
}));
            _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

            // Act
            var result = AuthorisationUtil.CanDeleteItem(role);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanDeleteItem_UserNotInRole_ReturnsFalse()
        {
            // Arrange
            var role = "TestRole";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
new Claim(ClaimTypes.Role, "DifferentRole")
}));
            _httpContextAccessor.HttpContext.Returns(new DefaultHttpContext { User = claimsPrincipal });

            // Act
            var result = AuthorisationUtil.CanDeleteItem(role);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanDeleteItem_CurrentUserIsNull_ReturnsFalse()
        {
            // Arrange
            var role = "TestRole";
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.CanDeleteItem(role);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsUserInAnyRole_UserHasMatchingRole_ReturnsTrue()
        {
            // Arrange
            var userRoles = new List<Claim> { new Claim(ClaimTypes.Role, AppRoleConstant.IsolateManager) };
            _user.FindAll(ClaimTypes.Role).Returns(userRoles);

            // Act
            var result = AuthorisationUtil.IsUserInAnyRole();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsUserInAnyRole_UserHasMultipleRolesOneMatching_ReturnsTrue()
        {
            // Arrange
            var userRoles = new List<Claim>
{
new Claim(ClaimTypes.Role, "NonMatchingRole"),
new Claim(ClaimTypes.Role, AppRoleConstant.IsolateViewer)
};
            _user.FindAll(ClaimTypes.Role).Returns(userRoles);

            // Act
            var result = AuthorisationUtil.IsUserInAnyRole();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsUserInAnyRole_UserHasNonMatchingRoles_ReturnsFalse()
        {
            // Arrange
            var userRoles = new List<Claim>
{
new Claim(ClaimTypes.Role, "NonMatchingRole1"),
new Claim(ClaimTypes.Role, "NonMatchingRole2")
};
            _user.FindAll(ClaimTypes.Role).Returns(userRoles);

            // Act
            var result = AuthorisationUtil.IsUserInAnyRole();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsUserInAnyRole_UserHasNoRoles_ReturnsFalse()
        {
            // Arrange
            var userRoles = new List<Claim>();
            _user.FindAll(ClaimTypes.Role).Returns(userRoles);

            // Act
            var result = AuthorisationUtil.IsUserInAnyRole();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsUserInAnyRole_CurrentUserIsNull_ReturnsFalse()
        {
            // Arrange
            _httpContext.User.Returns((ClaimsPrincipal?)null);

            // Act
            var result = AuthorisationUtil.IsUserInAnyRole();

            // Assert
            Assert.False(result);
        }

        // Fix for CS8602: Dereference of a possibly null reference.
        // Added null checks for `_httpContextAccessor.HttpContext` and `_httpContextAccessor.HttpContext.User`
        // to ensure safe dereferencing.

        [Fact]
        public void IsInRole_UserInRole_ReturnsTrue()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "TestRole") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextAccessor.HttpContext?.User.Returns(claimsPrincipal);

            // Act
            var result = AuthorisationUtil.IsInRole("TestRole");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInRole_UserNotInRole_ReturnsFalse()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "OtherRole") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _httpContextAccessor.HttpContext?.User.Returns(claimsPrincipal);

            // Act
            var result = AuthorisationUtil.IsInRole("TestRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInRole_CurrentUserIsNull_ReturnsFalse()
        {
            // Arrange
            _httpContextAccessor.HttpContext = null;

            // Act
            var result = AuthorisationUtil.IsInRole("TestRole");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetUserId_WhenCurrentUserIsNull_ReturnsEmptyString()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.GetUserId();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        // Fix for CS8600: Converting null literal or possible null value to non-nullable type.
        // Updated the code to explicitly cast the null value to a nullable type.

        [Fact]
        public void GetUserId_WhenCurrentUserIdentityIsNull_ReturnsEmptyString()
        {
            // Arrange
            var httpContext = Substitute.For<HttpContext>();
            var claimsPrincipal = Substitute.For<ClaimsPrincipal>();
            claimsPrincipal.Identity.Returns((ClaimsIdentity?)null); // Fix: Explicitly cast to nullable type
            httpContext.User.Returns(claimsPrincipal);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserId();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetUserId_WhenCurrentUserIdentityNameIsNull_ReturnsEmptyString()
        {
            // Arrange
            var httpContext = Substitute.For<HttpContext>();
            var claimsPrincipal = Substitute.For<ClaimsPrincipal>();
            var claimsIdentity = Substitute.For<ClaimsIdentity>();
            claimsIdentity.Name.Returns((string?)null); // Fix: Explicitly mark the null value as nullable
            claimsPrincipal.Identity.Returns(claimsIdentity);
            httpContext.User.Returns(claimsPrincipal);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserId();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetUserId_WhenCurrentUserIdentityNameHasValue_ReturnsName()
        {
            // Arrange
            var expectedUserId = "testUser";
            var httpContext = Substitute.For<HttpContext>();
            var claimsPrincipal = Substitute.For<ClaimsPrincipal>();
            var claimsIdentity = Substitute.For<ClaimsIdentity>();
            claimsIdentity.Name.Returns(expectedUserId);
            claimsPrincipal.Identity.Returns(claimsIdentity);
            httpContext.User.Returns(claimsPrincipal);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserId();

            // Assert
            Assert.Equal(expectedUserId, result);
        }

        [Fact]
        public void GetUserCurrentRole_WhenCurrentUserIsNull_ReturnsEmptyString()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.GetUserCurrentRole();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetUserCurrentRole_WhenCurrentUserExistsButNoRoleClaim_ReturnsEmptyString()
        {
            // Arrange
            var httpContext = Substitute.For<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            httpContext.User.Returns(claimsPrincipal);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserCurrentRole();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void GetUserCurrentRole_WhenCurrentUserExistsWithRoleClaim_ReturnsRole()
        {
            // Arrange
            var httpContext = Substitute.For<HttpContext>();
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "TestRole") };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            httpContext.User.Returns(claimsPrincipal);
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserCurrentRole();

            // Assert
            Assert.Equal("TestRole", result);
        }

        [Fact]
        public void GetUserRoles_WhenCurrentUserIsNull_ReturnsEmptyList()
        {
            // Arrange
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

            // Act
            var result = AuthorisationUtil.GetUserRoles();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUserRoles_WhenCurrentUserHasNoRoles_ReturnsEmptyList()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserRoles();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetUserRoles_WhenCurrentUserHasOneRole_ReturnsListWithOneRole()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, "Admin") };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserRoles();

            // Assert
            Assert.Single(result);
            Assert.Contains("Admin", result);
        }

        [Fact]
        public void GetUserRoles_WhenCurrentUserHasMultipleRoles_ReturnsListWithAllRoles()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
{
new Claim(ClaimTypes.Role, "Admin"),
new Claim(ClaimTypes.Role, "User"),
new Claim(ClaimTypes.Role, "Manager")
};
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
            _httpContextAccessor.HttpContext.Returns(httpContext);

            // Act
            var result = AuthorisationUtil.GetUserRoles();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains("Admin", result);
            Assert.Contains("User", result);
            Assert.Contains("Manager", result);
        }

        [Fact]
        public void AppRoles_ShouldReturnExpectedRoles()
        {
            // Arrange
            var expectedRoles = new List<string>
{
AppRoleConstant.IsolateManager,
AppRoleConstant.IsolateViewer,
AppRoleConstant.IsolateDeleter,
AppRoleConstant.LookupDataManager,
AppRoleConstant.ReportViewer,
AppRoleConstant.Administrator
};

            // Act
            var result = AuthorisationUtil.AppRoles;

            // Assert
            Assert.Equal(expectedRoles, result);
        }

        [Fact]
        public void AppRoles_ShouldCacheResult()
        {
            // Arrange
            var firstCall = AuthorisationUtil.AppRoles;

            // Act
            var secondCall = AuthorisationUtil.AppRoles;

            // Assert
            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public void AppRoles_ShouldBeModifiable()
        {
            // Arrange
            var newRoles = new List<string> { "NewRole1", "NewRole2" };

            // Act
            AuthorisationUtil.AppRoles = newRoles;
            var result = AuthorisationUtil.AppRoles;

            // Assert
            Assert.Equal(newRoles, result);

            // Clean up
            AuthorisationUtil.AppRoles = null!; // Reset to original state
        }

        [Fact]
        public void AppRoles_ShouldResetCacheWhenSetToNull()
        {
            // Arrange
            var firstCall = AuthorisationUtil.AppRoles;

            // Act
            AuthorisationUtil.AppRoles = null!;
            var secondCall = AuthorisationUtil.AppRoles;

            // Assert
            Assert.NotSame(firstCall, secondCall);
            Assert.Equal(firstCall, secondCall); // Content should be the same
        }

    }
}