using Apha.VIR.Web.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Apha.BST.Web.UnitTests.Startup
{
    public class AuthenticationExtensionTests
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        public AuthenticationExtensionTests()
        {
            _services = new ServiceCollection();
            _configuration = Substitute.For<IConfiguration>();
        }

        [Fact]
        public void AddAuthenticationServices_ShouldRegisterAuthentication()
        {
            // Act
            var result = AuthenticationExtension.AddAuthenticationServices(_services, _configuration);

            // Assert
            Assert.NotNull(result);
            var serviceProvider = _services.BuildServiceProvider();
            var authService = serviceProvider.GetService<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
            Assert.NotNull(authService); // Basic check - deeper checks can be added
        }
    }

}
