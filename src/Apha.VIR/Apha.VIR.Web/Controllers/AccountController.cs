using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        [Route("AccessDenied")]
        public IActionResult AccessDenied(string returnUrl)
        {
            string username = User.Identity?.Name!;
            string errorCode;
            string defaultErrorType = "VIR.GENERAL_EXCEPTION";
            string errorType = string.Empty;
            string message = $"403 Forbidden: User {username} not authorised to access {returnUrl}";

            var ex = new UnauthorizedAccessException(message);

            if (!ModelState.IsValid)
            { return View(); }

            errorCode = "403 - Forbidden";
            errorType = _configuration["ExceptionTypes:Authorization"] ?? defaultErrorType;
            _logger.LogError(ex, "[{ErrorType:l}] Error [{ErrorCode:l}]: {Message}", errorType, errorCode, ex.Message);

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
