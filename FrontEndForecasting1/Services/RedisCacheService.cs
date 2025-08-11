using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace FrontEndForecasting.Services
{
    public class RedisCacheService : ICacheService
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IPerformanceMonitoringService _performanceMonitoringService;

        public RedisCacheService(
            IDistributedCache distributedCache,
            ILogger<RedisCacheService> logger,
            IPerformanceMonitoringService performanceMonitoringService)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _performanceMonitoringService = performanceMonitoringService;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                _logger.LogDebug("[Redis] Get key: {Key}", key);
                var bytes = await _distributedCache.GetAsync(key);
                if (bytes is null || bytes.Length == 0)
                {
                    _performanceMonitoringService.RecordCacheMiss(key);
                    return null;
                }

                _performanceMonitoringService.RecordCacheHit(key);
                var value = JsonSerializer.Deserialize<T>(bytes, JsonOptions);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] Error getting key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
                };

                await _distributedCache.SetAsync(key, bytes, options);
                _logger.LogDebug("[Redis] Set key: {Key} exp: {Exp}", key, options.AbsoluteExpirationRelativeToNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] Error setting key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("[Redis] Removed key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] Error removing key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var bytes = await _distributedCache.GetAsync(key);
                return bytes is not null && bytes.Length > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] Error checking existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            var cached = await GetAsync<T>(key);
            if (cached is not null)
            {
                return cached;
            }

            var value = await factory();
            await SetAsync(key, value, expiration);
            return value;
        }
    }
}


