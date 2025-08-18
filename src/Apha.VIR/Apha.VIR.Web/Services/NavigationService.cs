using System.Text.Json;
using Apha.VIR.Web.Models;

namespace Apha.VIR.Web.Services
{
    public class NavigationService
    {
        private readonly Task<List<NavItem>> _navItemsTask;

        public NavigationService(IWebHostEnvironment env)
        {
            _navItemsTask = LoadNavigationDataAsync(env.WebRootPath);
        }

        private async Task<List<NavItem>> LoadNavigationDataAsync(string webRootPath)
        {
            var jsonPath = Path.Combine(webRootPath, "NavigationData", "navigation.json");
            if (!File.Exists(jsonPath))
                return new List<NavItem>();

            var json = await File.ReadAllTextAsync(jsonPath);
            return JsonSerializer.Deserialize<List<NavItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<NavItem>();
        }

        public Task<List<NavItem>> GetNavigationItemsAsync()
        {
            return _navItemsTask;
        }
    }
}
