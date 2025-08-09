using Prometheus;
using System.Diagnostics;

namespace FrontEndForecasting.Services
{
    public interface IPerformanceMonitoringService
    {
        void RecordForecastRequest(string productId, string model, TimeSpan duration);
        void RecordExportRequest(string format, TimeSpan duration);
        void RecordCacheHit(string key);
        void RecordCacheMiss(string key);
        void RecordError(string operation, Exception exception);
        void RecordUserAction(string action, string userId = null);
    }

    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        
        // Prometheus metrics
        private readonly Counter _forecastRequestsTotal;
        private readonly Histogram _forecastDuration;
        private readonly Counter _exportRequestsTotal;
        private readonly Histogram _exportDuration;
        private readonly Counter _cacheHitsTotal;
        private readonly Counter _cacheMissesTotal;
        private readonly Counter _errorsTotal;
        private readonly Counter _userActionsTotal;

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger;

            // Initialize Prometheus metrics
            _forecastRequestsTotal = Metrics.CreateCounter("forecast_requests_total", "Total number of forecast requests", new CounterConfiguration
            {
                LabelNames = new[] { "product_id", "model" }
            });

            _forecastDuration = Metrics.CreateHistogram("forecast_duration_seconds", "Forecast processing duration in seconds", new HistogramConfiguration
            {
                LabelNames = new[] { "product_id", "model" },
                Buckets = new[] { 0.1, 0.5, 1.0, 2.0, 5.0, 10.0, 30.0, 60.0 }
            });

            _exportRequestsTotal = Metrics.CreateCounter("export_requests_total", "Total number of export requests", new CounterConfiguration
            {
                LabelNames = new[] { "format" }
            });

            _exportDuration = Metrics.CreateHistogram("export_duration_seconds", "Export processing duration in seconds", new HistogramConfiguration
            {
                LabelNames = new[] { "format" },
                Buckets = new[] { 0.1, 0.5, 1.0, 2.0, 5.0, 10.0 }
            });

            _cacheHitsTotal = Metrics.CreateCounter("cache_hits_total", "Total number of cache hits");
            _cacheMissesTotal = Metrics.CreateCounter("cache_misses_total", "Total number of cache misses");
            _errorsTotal = Metrics.CreateCounter("errors_total", "Total number of errors", new CounterConfiguration
            {
                LabelNames = new[] { "operation" }
            });

            _userActionsTotal = Metrics.CreateCounter("user_actions_total", "Total number of user actions", new CounterConfiguration
            {
                LabelNames = new[] { "action", "user_id" }
            });
        }

        public void RecordForecastRequest(string productId, string model, TimeSpan duration)
        {
            try
            {
                _forecastRequestsTotal.WithLabels(productId, model).Inc();
                _forecastDuration.WithLabels(productId, model).Observe(duration.TotalSeconds);

                _logger.LogInformation("Forecast request recorded - Product: {ProductId}, Model: {Model}, Duration: {Duration}ms", 
                    productId, model, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording forecast request metrics");
            }
        }

        public void RecordExportRequest(string format, TimeSpan duration)
        {
            try
            {
                _exportRequestsTotal.WithLabels(format).Inc();
                _exportDuration.WithLabels(format).Observe(duration.TotalSeconds);

                _logger.LogInformation("Export request recorded - Format: {Format}, Duration: {Duration}ms", 
                    format, duration.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording export request metrics");
            }
        }

        public void RecordCacheHit(string key)
        {
            try
            {
                _cacheHitsTotal.Inc();
                _logger.LogDebug("Cache hit recorded for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache hit metrics");
            }
        }

        public void RecordCacheMiss(string key)
        {
            try
            {
                _cacheMissesTotal.Inc();
                _logger.LogDebug("Cache miss recorded for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cache miss metrics");
            }
        }

        public void RecordError(string operation, Exception exception)
        {
            try
            {
                _errorsTotal.WithLabels(operation).Inc();
                _logger.LogError(exception, "Error recorded for operation: {Operation}", operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording error metrics");
            }
        }

        public void RecordUserAction(string action, string userId = null)
        {
            try
            {
                _userActionsTotal.WithLabels(action, userId ?? "anonymous").Inc();
                _logger.LogDebug("User action recorded - Action: {Action}, User: {UserId}", action, userId ?? "anonymous");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording user action metrics");
            }
        }
    }
}
