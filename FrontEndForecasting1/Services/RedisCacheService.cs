using Microsoft.Extensions.Caching.Distributed;
using System.Diagnostics;
using System.Text.Json;

namespace FrontEndForecasting.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IPerformanceMonitoringService _performanceMonitoring;

        public RedisCacheService(
            IDistributedCache distributedCache,
            ILogger<RedisCacheService> logger,
            IPerformanceMonitoringService performanceMonitoring)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _performanceMonitoring = performanceMonitoring;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Attempting to get cached item with key: {Key}", key);
                
                var cachedValue = await _distributedCache.GetStringAsync(key);
                
                if (!string.IsNullOrEmpty(cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    _performanceMonitoring.RecordCacheHit(key);
                    _performanceMonitoring.RecordRedisOperation("get", stopwatch.Elapsed, true);
                    
                    try
                    {
                        return JsonSerializer.Deserialize<T>(cachedValue);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize cached value for key: {Key}", key);
                        await _distributedCache.RemoveAsync(key);
                        _performanceMonitoring.RecordRedisOperation("get", stopwatch.Elapsed, false);
                        return null;
                    }
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                _performanceMonitoring.RecordCacheMiss(key);
                _performanceMonitoring.RecordRedisOperation("get", stopwatch.Elapsed, true);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached item with key: {Key}", key);
                _performanceMonitoring.RecordRedisOperation("get", stopwatch.Elapsed, false);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var options = new DistributedCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration of 1 hour
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                }

                var serializedValue = JsonSerializer.Serialize(value);
                await _distributedCache.SetStringAsync(key, serializedValue, options);
                
                _logger.LogDebug("Cached item with key: {Key}, expiration: {Expiration}", key, expiration);
                _performanceMonitoring.RecordRedisOperation("set", stopwatch.Elapsed, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cached item with key: {Key}", key);
                _performanceMonitoring.RecordRedisOperation("set", stopwatch.Elapsed, false);
            }
        }

        public async Task RemoveAsync(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Removed cached item with key: {Key}", key);
                _performanceMonitoring.RecordRedisOperation("remove", stopwatch.Elapsed, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached item with key: {Key}", key);
                _performanceMonitoring.RecordRedisOperation("remove", stopwatch.Elapsed, false);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var value = await _distributedCache.GetStringAsync(key);
                var exists = !string.IsNullOrEmpty(value);
                _performanceMonitoring.RecordRedisOperation("exists", stopwatch.Elapsed, true);
                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of cached item with key: {Key}", key);
                _performanceMonitoring.RecordRedisOperation("exists", stopwatch.Elapsed, false);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            var newValue = await factory();
            await SetAsync(key, newValue, expiration);
            return newValue;
        }
    }
}
