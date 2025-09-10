using System.Security.Claims;

namespace Apha.VIR.Web.Utilities
{
    public static class AuthorisationUtil
    {
        private static IHttpContextAccessor _httpContextAccessor;
        private static List<string> _authMessages = new List<string>();

        public static List<string> AppRoles
        {
            get => _authMessages ??= GetAppRoles();
            set => _authMessages = value;
        }

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public static ClaimsPrincipal? CurrentUser => _httpContextAccessor?.HttpContext?.User;

        public static bool CanGetItem(string role)
        {
            return IsInRole(role);
        }

        public static bool CanAddItem(string role)
        {
            return IsInRole(role);
        }

        public static bool CanEditItem(string role)
        {
            return IsInRole(role);
        }

        public static bool CanDeleteItem(string role)
        {
            return IsInRole(role);
        }

        public static bool IsUserInAnyRole()
        {
            var userroles = GetUserRoles();

            bool hasMatchingRole = userroles.Any(role => AppRoles.Contains(role, StringComparer.OrdinalIgnoreCase));

            return hasMatchingRole;
        }

        public static bool IsInRole(string role)
        {
            var user = CurrentUser;
            return user?.IsInRole(role) ?? false;
        }

        public static string GetUserId()
        {
            return CurrentUser?.Identity?.Name! ?? string.Empty;
        }

        public static string GetUserCurrentRole()
        {
            var roleClaim = CurrentUser?.FindFirst(ClaimTypes.Role)?.Value;
            return roleClaim ?? string.Empty;
        }

        public static List<string> GetUserRoles()
        {
            var roles = CurrentUser?
                //.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                .FindAll(ClaimTypes.Role)
                .Select(c => c.Value).ToList();

            return roles ?? new List<string>();
        }

        private static List<string> GetAppRoles()
        {
            var roles = new List<string>
            {
                AppRoleConstant.IsolateManager,
                AppRoleConstant.IsolateViewer,
                AppRoleConstant.IsolateDeleter,
                AppRoleConstant.LookupDataManager,
                AppRoleConstant.ReportViewer,
                AppRoleConstant.Administrator
            };

            return roles;
        }
    }

    public static class AppRoleConstant
    {
        public const string IsolateManager = "IsolateManager";
        public const string IsolateViewer = "IsolateViewerr";
        public const string IsolateDeleter = "IsolateDeleter";
        public const string LookupDataManager = "LookupManager";
        public const string ReportViewer = "ReportViewer";
        public const string Administrator = "Administrator";
    }
}
