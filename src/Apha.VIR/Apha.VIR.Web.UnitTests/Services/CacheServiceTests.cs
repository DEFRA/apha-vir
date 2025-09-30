using System.Text;
using System.Text.Json;
using Apha.VIR.Web.Models;
using Apha.VIR.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.VIR.Web.UnitTests.Services
{
    public class CacheServiceTests
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;
        private readonly CacheService _service;
        private readonly ISession _session;
        // Simulated session storage
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public CacheServiceTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _cache = Substitute.For<IDistributedCache>();
            _logger = Substitute.For<ILogger<CacheService>>();
            _service = new CacheService(_httpContextAccessor, _cache, _logger);
            _session = Substitute.For<ISession>();

            // Simulated session storage behavior
            _session.When(x => x.Set(Arg.Any<string>(), Arg.Any<byte[]>()))
                .Do(call =>
                {
                    var key = call.Arg<string>();
                    var value = call.Arg<byte[]>();
                    _sessionStorage[key] = value;
                });

            _session.TryGetValue(Arg.Any<string>(), out Arg.Any<byte[]?>())
                .Returns(call =>
                {
                    var key = call.Arg<string>();
                    if (_sessionStorage.TryGetValue(key, out var value))
                    {
                        call[1] = value; // Set the out parameter
                        return true;
                    }
                    call[1] = null;
                    return false;
                });

            _session.When(x => x.Remove(Arg.Any<string>()))
                .Do(call =>
                {
                    var key = call.Arg<string>();
                    _sessionStorage.Remove(key);
                });

            var context = Substitute.For<HttpContext>();
            context.Session.Returns(_session);
            _httpContextAccessor.HttpContext.Returns(context);
        }

        [Fact]
        public void SetSessionValue_SetsValue()
        {
            _service.SetSessionValue("key", "value");

            _session.Received().Set(
                "key",
                Arg.Is<byte[]>(b => Encoding.UTF8.GetString(b) == "value"));
        }

        [Fact]
        public void SetSessionValue_Throws_LogsError()
        {
            _session.When(x => x.Set("key", Arg.Any<byte[]>()))
                .Do(_ => throw new Exception("fail"));

            _service.SetSessionValue("key", "value");

            _logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("Failed to set session value for key")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void GetSessionValue_ReturnsValue()
        {
            var bytes = Encoding.UTF8.GetBytes("val");
            _session.TryGetValue("key", out Arg.Any<byte[]?>())
                .Returns(x =>
                {
                    x[1] = bytes; 
                    return true;
                });

            var result = _service.GetSessionValue("key");

            Assert.Equal("val", result);
        }

        [Fact]
        public void GetSessionValue_Throws_LogsErrorAndReturnsNull()
        {
            _session.When(x => x.TryGetValue("key", out Arg.Any<byte[]?>()))
                .Do(_ => throw new Exception("fail"));

            var result = _service.GetSessionValue("key");

            Assert.Null(result);

            _logger.Received().Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Is<object>(v => v.ToString()!.Contains("Failed to get session value for key")),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public void RemoveSessionValue_RemovesValue()
        {
            _service.RemoveSessionValue("key");
            _session.Received().Remove("key");
        }

        [Fact]
        public void RemoveSessionValue_Throws_LogsError()
        {
            _session.When(x => x.Remove("key")).Do(_ => throw new Exception("fail"));

            _service.RemoveSessionValue("key");

            _logger.Received().Log(
               LogLevel.Error,
               Arg.Any<EventId>(),
               Arg.Is<object>(v => v.ToString()!.Contains("Failed to remove session value for key")),
               Arg.Any<Exception>(),
               Arg.Any<Func<object, Exception?, string>>());           
        }

        [Fact]
        public async Task SetCacheValueAsync_SetsValue()
        {
            await _service.SetCacheValueAsync("key", 123);

            await _cache.Received().SetAsync(
                "key",
                Arg.Any<byte[]>(),
                Arg.Any<DistributedCacheEntryOptions>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SetCacheValueAsync_Throws_LogsError()
        {
            _cache.When(x => x.SetAsync(
                    "key",
                    Arg.Any<byte[]>(),
                    Arg.Any<DistributedCacheEntryOptions>(),
                    Arg.Any<CancellationToken>()))
                .Do(_ => throw new Exception("fail"));

            await _service.SetCacheValueAsync("key", 123);

            _logger.Received().Log(
              LogLevel.Error,
              Arg.Any<EventId>(),
              Arg.Is<object>(v => v.ToString()!.Contains("Failed to set cache value for key")),
              Arg.Any<Exception>(),
              Arg.Any<Func<object, Exception?, string>>());          
        }

        [Fact]
        public async Task GetCacheValueAsync_ReturnsValue()
        {
            var json = System.Text.Json.JsonSerializer.Serialize(123);
            var bytes = Encoding.UTF8.GetBytes(json);

            _cache.GetAsync("key", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<byte[]?>(bytes)!); 

            var result = await _service.GetCacheValueAsync<int>("key");

            Assert.Equal(123, result);
        }

        [Fact]
        public async Task GetCacheValueAsync_ReturnsDefaultIfNull()
        {
            _cache.GetAsync("key", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<byte[]?>(null)!); 

            var result = await _service.GetCacheValueAsync<int>("key");

            Assert.Equal(0, result);
        }

        [Fact]
        public async Task GetCacheValueAsync_Throws_LogsErrorAndReturnsDefault()
        {
            _cache.GetAsync("key", Arg.Any<CancellationToken>())
                .Returns<Task<byte[]?>>(_ => throw new Exception("fail"));

            var result = await _service.GetCacheValueAsync<int>("key");

            Assert.Equal(0, result);

            _logger.Received().Log(
              LogLevel.Error,
              Arg.Any<EventId>(),
              Arg.Is<object>(v => v.ToString()!.Contains("Failed to get cache value for key")),
              Arg.Any<Exception>(),
              Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task RemoveCacheValueAsync_RemovesValue()
        {
            await _service.RemoveCacheValueAsync("key");

            await _cache.Received().RemoveAsync("key", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RemoveCacheValueAsync_Throws_LogsError()
        {
            _cache.When(x => x.RemoveAsync("key", Arg.Any<CancellationToken>()))
                .Do(_ => throw new Exception("fail"));

            await _service.RemoveCacheValueAsync("key");

            _logger.Received().Log(
             LogLevel.Error,
             Arg.Any<EventId>(),
             Arg.Is<object>(v => v.ToString()!.Contains("Failed to remove cache value for key")),
             Arg.Any<Exception>(),
             Arg.Any<Func<object, Exception?, string>>());            
        }

        [Fact]
        public async Task SetCacheValueAsync_UsesProvidedExpiration()
        {
            // Arrange
            var expiration = TimeSpan.FromMinutes(5);
            DistributedCacheEntryOptions? capturedOptions = null;

            _cache.When(x => x.SetAsync(
                    "key",
                    Arg.Any<byte[]>(),
                    Arg.Any<DistributedCacheEntryOptions>(),
                    Arg.Any<CancellationToken>()))
                .Do(ci => capturedOptions = ci.Arg<DistributedCacheEntryOptions>());

            // Act
            await _service.SetCacheValueAsync("key", "value", expiration);

            // Assert
            Assert.NotNull(capturedOptions);
            Assert.Equal(expiration, capturedOptions!.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public async Task SetCacheValueAsync_UsesDefaultExpirationWhenNull()
        {
            // Arrange
            DistributedCacheEntryOptions? capturedOptions = null;

            _cache.When(x => x.SetAsync(
                    "key",
                    Arg.Any<byte[]>(),
                    Arg.Any<DistributedCacheEntryOptions>(),
                    Arg.Any<CancellationToken>()))
                .Do(ci => capturedOptions = ci.Arg<DistributedCacheEntryOptions>());

            // Act
            await _service.SetCacheValueAsync("key", "value", null);

            // Assert
            Assert.NotNull(capturedOptions);
            Assert.Equal(TimeSpan.FromMinutes(60), capturedOptions!.AbsoluteExpirationRelativeToNow);
        }

        [Fact]
        public void AddOrUpdateBreadcrumb_AddsNewEntry_WhenUrlNotExists()
        {
            // Arrange
            _service.RemoveSessionValue("BreadcrumbTrail");
            var parameters = new Dictionary<string, string> { { "id", "1" } };

            // Act
            _service.AddOrUpdateBreadcrumb("/test", parameters);

            // Assert
            var json = _service.GetSessionValue("BreadcrumbTrail");
            var list = JsonSerializer.Deserialize<List<BreadcrumbEntry>>(json!);
            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Equal("/test", list[0].Url);
            Assert.Equal(parameters, list[0].Parameters);
        }

        [Fact]
        public void AddOrUpdateBreadcrumb_UpdatesParameters_WhenUrlExists()
        {
            // Arrange
            var initial = new List<BreadcrumbEntry>
            {
                new BreadcrumbEntry { Url = "/test", Parameters = new Dictionary<string, string> { { "old", "x" } } }
            };
            _service.SetSessionValue("BreadcrumbTrail", JsonSerializer.Serialize(initial));
            var newParams = new Dictionary<string, string> { { "new", "y" } };

            // Act
            _service.AddOrUpdateBreadcrumb("/test", newParams);

            // Assert
            var json = _service.GetSessionValue("BreadcrumbTrail");
            var list = JsonSerializer.Deserialize<List<BreadcrumbEntry>>(json!);
            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Equal(newParams, list[0].Parameters);
        }

        [Fact]
        public void GetFullUrlFor_ReturnsNull_WhenEntryNotFound()
        {
            // Arrange
            _service.RemoveSessionValue("BreadcrumbTrail");

            // Act
            var result = _service.GetFullUrlFor("/notfound");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetFullUrlFor_ReturnsUrl_WhenNoParameters()
        {
            // Arrange
            var entries = new List<BreadcrumbEntry>
            {
                new BreadcrumbEntry { Url = "/simple", Parameters = null! }
            };
            _service.SetSessionValue("BreadcrumbTrail", JsonSerializer.Serialize(entries));

            // Act
            var result = _service.GetFullUrlFor("/simple");

            // Assert
            Assert.Equal("/simple", result);
        }

        [Fact]
        public void GetFullUrlFor_ReturnsUrlWithQuery_WhenParametersExist()
        {
            // Arrange
            var entries = new List<BreadcrumbEntry>
            {
                new BreadcrumbEntry
                {
                    Url = "/complex",
                    Parameters = new Dictionary<string, string>
                    {
                        { "a", "b" },
                        { "c", "d e" } // space will be URL-encoded
                    }
                }
            };
            _service.SetSessionValue("BreadcrumbTrail", JsonSerializer.Serialize(entries));

            // Act
            var result = _service.GetFullUrlFor("/complex");

            // Assert
            Assert.StartsWith("/complex?", result);
            Assert.Contains("a=b", result);
            Assert.Contains("c=d%20e", result); // Space encoded as %20
        }

        [Fact]
        public void GetBreadcrumbs_ReturnsEmptyList_WhenSessionValueIsNullOrEmpty()
        {
            // Arrange
            _service.RemoveSessionValue("BreadcrumbTrail");

            // Act
            var method = typeof(CacheService).GetMethod("GetBreadcrumbs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method!.Invoke(_service, null) as List<BreadcrumbEntry>;

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result!);
        }

        [Fact]
        public void GetBreadcrumbs_ReturnsDeserializedList_WhenSessionValueExists()
        {
            // Arrange
            var entries = new List<BreadcrumbEntry>
            {
                new BreadcrumbEntry
                {
                    Url = "/x",
                    Parameters = new Dictionary<string, string> { { "p", "q" } }
                }
            };
            _service.SetSessionValue("BreadcrumbTrail", JsonSerializer.Serialize(entries));

            // Act
            var method = typeof(CacheService).GetMethod("GetBreadcrumbs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = method!.Invoke(_service, null) as List<BreadcrumbEntry>;

            // Assert
            Assert.NotNull(result);
            Assert.Single(result!);
            Assert.Equal("/x", result![0].Url);
            Assert.Equal("q", result[0].Parameters["p"]);
        }
    }
}
