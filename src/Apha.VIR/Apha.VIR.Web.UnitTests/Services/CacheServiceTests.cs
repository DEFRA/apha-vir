using System.Text;
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

        public CacheServiceTests()
        {
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _cache = Substitute.For<IDistributedCache>();
            _logger = Substitute.For<ILogger<CacheService>>();
            _service = new CacheService(_httpContextAccessor, _cache, _logger);
            _session = Substitute.For<ISession>();

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
    }
}
