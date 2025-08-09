using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace FrontEndForecasting.Services
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;
        private readonly IPerformanceMonitoringService _performanceMonitoring;

        public MemoryCacheService(
            IMemoryCache memoryCache,
            ILogger<MemoryCacheService> logger,
            IPerformanceMonitoringService performanceMonitoring)
        {
            _memoryCache = memoryCache;
            _logger = logger;
            _performanceMonitoring = performanceMonitoring;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                _logger.LogDebug("Attempting to get cached item with key: {Key}", key);
                
                if (_memoryCache.TryGetValue(key, out var cachedValue))
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    _performanceMonitoring.RecordCacheHit(key);
                    return Task.FromResult(cachedValue as T);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                _performanceMonitoring.RecordCacheMiss(key);
                return Task.FromResult<T?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cached item with key: {Key}", key);
                return Task.FromResult<T?>(null);
            }
        }

                public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new MemoryCacheEntryOptions();

                if (expiration.HasValue)
                {
                    options.AbsoluteExpirationRelativeToNow = expiration;
                }
                else
                {
                    // Default expiration of 1 hour
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                }

                _memoryCache.Set(key, value, options);
                _logger.LogDebug("Cached item with key: {Key}, expiration: {Expiration}", key, expiration);
                                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cached item with key: {Key}", key);
                             return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _logger.LogDebug("Removed cached item with key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cached item with key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
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
