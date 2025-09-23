using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Apha.VIR.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.VIR.Web.Components
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly NavigationService _navigationService;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public NavigationViewComponent(NavigationService navigationService,
            ICacheService cacheService,
            IMapper mapper)
        {
            _navigationService = navigationService;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string path = string.Empty;
            string? controllerName = HttpContext.Request.RouteValues["controller"]?.ToString();
            string? actionName = HttpContext.Request.RouteValues["action"]?.ToString();
            if (controllerName?.ToLower() == "isolateandtrayrelocation")
            {
                path = HttpContext.Request.Path.ToString();
            }
            else
            {
                path = $"/{controllerName}/{actionName}";
            }
            path = path.Replace("SearchRepository/Index", "SearchRepository/Search");

            var navItems = await _navigationService.GetNavigationItemsAsync();
            var breadCrumbs = _mapper.Map<List<NavItem>>(navItems);
            var breadcrumbTrail = FindBreadcrumbTrail(breadCrumbs, path);

            var homeNode = breadCrumbs.FirstOrDefault(n => n.Url == "/");
            if (homeNode != null && (breadcrumbTrail.Count == 0 || breadcrumbTrail[0].Url != "/"))
            {
                breadcrumbTrail.Insert(0, homeNode);
            }

            return View(breadcrumbTrail);
        }

        private List<NavItem> FindBreadcrumbTrail(List<NavItem> nodes, string path, List<NavItem> trail = null!)
        {
            trail ??= new List<NavItem>();

            foreach (var node in nodes)
            {
                var newTrail = new List<NavItem>(trail) { node };

                if (string.Equals(node.Url?.TrimEnd('/'), path.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    return newTrail;
                }

                if (node.Children != null && node.Children.Count > 0)
                {
                    var found = FindBreadcrumbTrail(node.Children, path, newTrail);
                    if (found.Count > 0)
                    {
                        found = GetPreviousUpdatedBreadcrumbUrl(found);
                        return found;
                    }

                }
            }

            return new List<NavItem>();
        }

        private List<NavItem> GetPreviousUpdatedBreadcrumbUrl(List<NavItem> found)
        {
            foreach (var item in found.SkipLast(1))
            {
                string? updatedUrl = _cacheService.GetFullUrlFor(item.Url);
                if (!string.IsNullOrEmpty(updatedUrl))
                {
                    item.UpdatedUrl = updatedUrl;
                }
                else
                {
                    item.UpdatedUrl = item.Url;
                }
            }
            return found;
        }
    }
}
