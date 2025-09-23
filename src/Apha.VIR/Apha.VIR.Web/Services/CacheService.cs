using Apha.VIR.Web.Models;
using Apha.VIR.Web.Utilities;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Apha.VIR.Web.Services
{
    public class CacheService : ICacheService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private const string SessionKey = "BreadcrumbTrail";

        public CacheService(
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            ILogger<CacheService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
            _logger = logger;
        }

        // Session and Cache methods (using Redis)
        public void SetSessionValue(string key, string value)
        {
            try
            {
                _httpContextAccessor.HttpContext?.Session.SetString(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set session value for key {Key}", key);
            }
        }

        public string? GetSessionValue(string key)
        {
            try
            {
                return _httpContextAccessor.HttpContext?.Session.GetString(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get session value for key {Key}", key);
                return null;
            }
        }

        public void RemoveSessionValue(string key)
        {
            try
            {
                _httpContextAccessor.HttpContext?.Session.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove session value for key {Key}", key);
            }
        }
        
        public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(60)
                };

                var json = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set cache value for key {Key}", key);
            }
        }

        public async Task<T?> GetCacheValueAsync<T>(string key)
        {
            try
            {
                var json = await _cache.GetStringAsync(key);
                if (json is null) return default;

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cache value for key {Key}", key);
                return default;
            }
        }

        public async Task RemoveCacheValueAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove cache value for key {Key}", key);
            }
        }        

        public void AddOrUpdateBreadcrumb(string url, Dictionary<string, string> parameters)
        {
            var list = GetBreadcrumbs();

            var existing = list.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Parameters = parameters;
            }
            else
            {
                list.Add(new BreadcrumbEntry { Url = url, Parameters = parameters });
            }

            SetSessionValue(SessionKey, System.Text.Json.JsonSerializer.Serialize(list));
        }
        
        public string? GetFullUrlFor(string? url)
        {
            var list = GetBreadcrumbs();
            var entry = list.FirstOrDefault(x => x.Url.Equals(url, StringComparison.OrdinalIgnoreCase));

            if (entry == null) return null;

            if (entry.Parameters == null || entry.Parameters.Count == 0)
                return entry.Url;

            var query = string.Join("&", entry.Parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
            return $"{entry.Url}?{query}";
        }

        private List<BreadcrumbEntry> GetBreadcrumbs()
        {
            var json = GetSessionValue(SessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<BreadcrumbEntry>()
                : System.Text.Json.JsonSerializer.Deserialize<List<BreadcrumbEntry>>(json)!;
        }
    }

    public interface ICacheService
    {
        void SetSessionValue(string key, string value);
        string? GetSessionValue(string key);
        void RemoveSessionValue(string key);
        Task SetCacheValueAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task<T?> GetCacheValueAsync<T>(string key);
        Task RemoveCacheValueAsync(string key);
        void AddOrUpdateBreadcrumb(string url, Dictionary<string, string> parameters);
        string? GetFullUrlFor(string? url);
    }
}

