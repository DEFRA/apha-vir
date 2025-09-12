using Apha.VIR.DataAccess.Data;
using Apha.VIR.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.VIR.Web.UnitTests.Controllers.Startup
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public void ConfigureServices_RegistersExpectedServices()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            // Replace real DB context with in-memory or mock
            builder.Services.AddDbContext<VIRDbContext>(options =>
                options.UseInMemoryDatabase("VIRTestDb"));

            // Act
            builder.ConfigureServices();
            var provider = builder.Services.BuildServiceProvider();

            // Assert – just a couple of examples:
            Assert.NotNull(provider.GetService<IHttpContextAccessor>()); // from AddHttpContextAccessor
            Assert.NotNull(provider.GetService<Microsoft.AspNetCore.Mvc.Controllers.IControllerFactory>()); // from MVC registration
        }
    }
}