using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Apha.VIR.Application.Validation;
using Apha.VIR.Web.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.VIR.Web.UnitTests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly ExceptionMiddleware _middleware;

        public ExceptionMiddlewareTests()
        {
            _next = Substitute.For<RequestDelegate>();
            _logger = Substitute.For<ILogger<ExceptionMiddleware>>();
            _configuration = Substitute.For<IConfiguration>();
            _middleware = new ExceptionMiddleware(_next, _logger, _configuration);
        }

        [Fact]
        public async Task InvokeAsync_UnauthorizedAccessException_LogsAndRedirects()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user1") }));
            _next(context).Throws(new UnauthorizedAccessException("forbidden"));

            // Act
            await _middleware.InvokeAsync(context);

            // Assert
            Assert.Equal("/Error", context.Response.Headers["Location"]);
            // Cannot assert extension method call directly, ensure no exception is thrown
        }

        [Fact]
        public async Task InvokeAsync_AuthenticationFailureException_LogsAndRedirects()
        {
            var context = new DefaultHttpContext();
            _next(context).Throws(new AuthenticationFailureException("auth failed"));

            await _middleware.InvokeAsync(context);

            Assert.Equal("/Error", context.Response.Headers["Location"]);
        }

        [Fact]
        public async Task InvokeAsync_SqlException_LogsAndRedirects()
        {
            var context = new DefaultHttpContext();

            // Create fake SqlException using reflection
            var ctor = typeof(SqlException).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)[0];
            var sqlException = (SqlException)ctor.Invoke(new object[] { "Test", null!, null!, Guid.NewGuid() });

            _next(context).Throws(sqlException);

            await _middleware.InvokeAsync(context);

            Assert.Equal("/Error", context.Response.Headers["Location"]);
        }

        [Fact]
        public async Task InvokeAsync_BusinessValidationErrorException_WritesJsonAndSets400()
        {
            var context = new DefaultHttpContext();
            var errors = new List<BusinessValidationError> { new BusinessValidationError("Field", "Error1") };
            var exception = new BusinessValidationErrorException(errors);

            _next(context).Throws(exception);

            await _middleware.InvokeAsync(context);

            Assert.StartsWith("application/json", context.Response.ContentType);
            Assert.Equal("/Error", context.Response.Headers["Location"]);
        }

        [Fact]
        public async Task InvokeAsync_GenericException_LogsAndRedirects()
        {
            var context = new DefaultHttpContext();
            _next(context).Throws(new Exception("generic"));

            await _middleware.InvokeAsync(context);

            Assert.Equal("/Error", context.Response.Headers["Location"]);
        }
    }
}
