using System.Text.Json;
using Apha.VIR.Web.Models;

namespace Apha.VIR.Web.Services
{
    public class NavigationService
    {
        private readonly Task<List<NavItem>> _navItemsTask;
        private static readonly JsonSerializerOptions _defaultJsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true          
        };

        public NavigationService(IWebHostEnvironment env)
        {
            _navItemsTask = LoadNavigationDataAsync(env.WebRootPath);
        }

        private static async Task<List<NavItem>> LoadNavigationDataAsync(string webRootPath)
        {
            var jsonPath = Path.Combine(webRootPath, "NavigationData", "navigation.json");
            if (!File.Exists(jsonPath))
                return new List<NavItem>();

            var json = await File.ReadAllTextAsync(jsonPath);
            return JsonSerializer.Deserialize<List<NavItem>>(json, _defaultJsonOptions) ?? new List<NavItem>();
        }

        public Task<List<NavItem>> GetNavigationItemsAsync()
        {
            return _navItemsTask;
        }
    }
}
