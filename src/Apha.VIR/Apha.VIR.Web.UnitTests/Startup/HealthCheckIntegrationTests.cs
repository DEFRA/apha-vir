using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;


namespace Apha.BST.Web.UnitTests.Startup
{
    public class HealthCheckIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public HealthCheckIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Set environment variable for the duration of these tests
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "local");
            Environment.SetEnvironmentVariable("ASPNETCORE_HTTPS_PORT", "7190");

            _factory = factory;
        }

        [Fact]
        public async Task HealthEndpoint_Returns200()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HomePage_Returns200()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Error/AccessDenied");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
