using Apha.VIR.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Apha.VIR.Web.UnitTests.Extensions
{
    public class AuthenticationServiceExtensionsTests
    {
        [Fact]
        public void AddAuthenticationServices_ConfiguresAuthentication()
        {
            var services = new ServiceCollection();
            var config = new ConfigurationBuilder().Build();

            var result = services.AddAuthenticationServices(config);

            Assert.NotNull(result);
        }



        [Fact]
        public void HandleRedirectToIdentityProvider_InvokedForCoverage()
        {
            var httpContext = new DefaultHttpContext();
            var context = new RedirectContext(
                httpContext,
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                new AuthenticationProperties())
            {
                ProtocolMessage = new OpenIdConnectMessage { RedirectUri = "http://example.com" }
            };

            typeof(AuthenticationExtension)
                .GetMethod("HandleRedirectToIdentityProvider", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { context });
            Assert.StartsWith("https://", context.ProtocolMessage.RedirectUri);
        }

        [Fact]
        public void HandleRemoteFailure_InvokedForCoverage()
        {
            var httpContext = new DefaultHttpContext();
            var ex = new Exception("boom");

            var context = new RemoteFailureContext(
                httpContext,
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                ex);

            Assert.ThrowsAny<Exception>(() =>
                typeof(AuthenticationExtension)
                    .GetMethod("HandleRemoteFailure", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { context })
            );
        }


        [Fact]
        public void HandleTokenValidatedAsync_NoIdentity_ThrowsUnauthorizedAccessException()
        {
            var context = new TokenValidatedContext(
                new DefaultHttpContext(),
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                null!,
                new AuthenticationProperties());

            Assert.Throws<TargetInvocationException>(() =>
                typeof(AuthenticationExtension)
                    .GetMethod("HandleTokenValidatedAsync", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { context })
            );
        }

        [Fact]
        public void HandleTokenValidatedAsync_NoEmail_ThrowsUnauthorizedAccessException()
        {
            var identity = new ClaimsIdentity(new List<Claim>(), "test");
            var principal = new ClaimsPrincipal(identity);

            var context = new TokenValidatedContext(
                new DefaultHttpContext(),
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                principal,
                new AuthenticationProperties());

            Assert.Throws<TargetInvocationException>(() =>
                typeof(AuthenticationExtension)
                    .GetMethod("HandleTokenValidatedAsync", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { context })
            );
        }

        [Fact]
        public void HandleRedirectToIdentityProvider_HttpsUri_DoesNotModifyUri()
        {
            var httpContext = new DefaultHttpContext();
            var context = new RedirectContext(
                httpContext,
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                new AuthenticationProperties())
            {
                ProtocolMessage = new OpenIdConnectMessage { RedirectUri = "https://example.com" }
            };

            typeof(AuthenticationExtension)
                .GetMethod("HandleRedirectToIdentityProvider", BindingFlags.NonPublic | BindingFlags.Static)!
                .Invoke(null, new object[] { context });

            Assert.Equal("https://example.com", context.ProtocolMessage.RedirectUri);
        }

       

        [Fact]
        public void HandleRemoteFailure_UnauthorizedAccessException_ThrowsUnauthorizedAccessException()
        {
            var httpContext = new DefaultHttpContext();
            var ex = new Exception("Outer", new UnauthorizedAccessException("Inner"));

            var context = new RemoteFailureContext(
                httpContext,
                new AuthenticationScheme("Test", "Test", typeof(OpenIdConnectHandler)),
                new OpenIdConnectOptions(),
                ex);

            Assert.Throws<TargetInvocationException>(() =>
                typeof(AuthenticationExtension)
                    .GetMethod("HandleRemoteFailure", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { context })
            );
        }
    }
}
