using Apha.VIR.Application.Validation;

namespace Apha.VIR.Web.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Continue down the pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred");

                context.Response.ContentType = "application/json";

                switch (ex)
                {
                    case BusinessValidationErrorException validationEx:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(validationEx.Errors);
                        break;

                    // Handle more custom exception types here as needed
                    default:
                        context.Response.Redirect("/Error");
                        break;
                }
            }
        }
    }
}
