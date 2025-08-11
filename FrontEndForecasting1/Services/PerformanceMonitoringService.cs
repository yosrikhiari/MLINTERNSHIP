using System.Diagnostics;

namespace FrontEndForecasting.Services
{
    public interface IPerformanceMonitoringService
    {
        void RecordForecastRequest(string productId, string model, TimeSpan duration);
        void RecordExportRequest(string format, TimeSpan duration);
        void RecordCacheHit(string key);
        void RecordCacheMiss(string key);
        void RecordRedisOperation(string operation, TimeSpan duration, bool success);
        void RecordError(string operation, Exception exception);
        void RecordUserAction(string action, string userId = null);
    }

    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;
        }

        public void RecordForecastRequest(string productId, string model, TimeSpan duration)
        {
            try
            {
                _logger.LogInformation("Forecast request completed - Product: {ProductId}, Model: {Model}, Duration: {Duration}ms", 
                    productId, model, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording forecast request");
            }
        }

        public void RecordExportRequest(string format, TimeSpan duration)
        {
            try
            {
                _logger.LogInformation("Export request completed - Format: {Format}, Duration: {Duration}ms", 
                    format, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording export request");
            }
        }

        public void RecordCacheHit(string key)
        {
            try
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit");
            }
        }

        public void RecordCacheMiss(string key)
        {
            try
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache miss");
            }
        }

        public void RecordError(string operation, Exception exception)
        {
            try
            {
                _logger.LogError(exception, "Error occurred during operation: {Operation}", operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording error");
            }
        }

        public void RecordRedisOperation(string operation, TimeSpan duration, bool success)
        {
            try
            {
                var status = success ? "success" : "failure";
                _logger.LogInformation("Redis operation - Operation: {Operation}, Duration: {Duration}ms, Status: {Status}", 
                    operation, duration.TotalMilliseconds, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording Redis operation");
            }
        }

        public void RecordUserAction(string action, string userId = null)
        {
            try
            {
                _logger.LogDebug("User action - Action: {Action}, User: {UserId}", action, userId ?? "anonymous");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording user action");
            }
        }
    }
}
