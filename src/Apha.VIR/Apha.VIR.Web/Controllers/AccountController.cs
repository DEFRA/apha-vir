using Amazon.Runtime;
using Apha.VIR.Application.Interfaces;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web.UI.Areas.MicrosoftIdentity.Pages.Account;

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
            string message = $"403 Forbidden: User {username} tried to access {returnUrl}";

            if (!ModelState.IsValid)
            { return View(); }

            errorCode = "403 - Forbidden";
            errorType = _configuration["ExceptionTypes:Authorization"] ?? defaultErrorType;
            _logger.LogError(message, "[{ErrorType:l}] Error [{ErrorCode:l}]: {Message}", errorType, errorCode, message);

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
