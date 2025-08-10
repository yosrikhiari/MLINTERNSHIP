using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using MLINTERNSHIP;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;
using FrontEndForecasting.Services;

namespace FrontEndForecasting.Controllers
{

    /// <summary>
    /// Controller for handling demand forecasting operations including file upload, prediction, and export functionality.
    /// </summary>
    public class ForecastController : Controller
    {
        private readonly ILogger<ForecastController> _logger;
        private readonly ICacheService _cacheService;
        private readonly IExportService _exportService;
        private readonly IPerformanceMonitoringService _performanceMonitoring;

        /// <summary>
        /// Initializes a new instance of the ForecastController with required services.
        /// </summary>
        /// <param name="logger">The logger service for recording application events.</param>
        /// <param name="cacheService">The cache service for improving performance.</param>
        /// <param name="exportService">The export service for data export functionality.</param>
        /// <param name="performanceMonitoring">The performance monitoring service for tracking metrics.</param>
        public ForecastController(
            ILogger<ForecastController> logger,
            ICacheService cacheService,
            IExportService exportService,
            IPerformanceMonitoringService performanceMonitoring)
        {
            _logger = logger;
            _cacheService = cacheService;
            _exportService = exportService;
            _performanceMonitoring = performanceMonitoring;
        }

        /// <summary>
        /// Displays the main forecasting dashboard.
        /// </summary>
        /// <returns>The Index view for the forecasting dashboard.</returns>
        public IActionResult Index()
        {
            _performanceMonitoring.RecordUserAction("view_index");
            return this.View();
        }

        /// <summary>
        /// Handles CSV file upload for demand forecasting.
        /// </summary>
        /// <param name="file">The CSV file containing supply chain data.</param>
        /// <returns>
        /// Returns a preview of the uploaded data if successful, or an error view if the upload fails.
        /// The uploaded data is cached for 1 hour to improve performance.
        /// </returns>
        /// <remarks>
        /// The method supports the following features:
        /// - File validation (CSV format only)
        /// - Data caching for improved performance
        /// - Error handling and logging
        /// - Performance monitoring
        /// 
        /// Expected CSV format:
        /// - Date, SKU, Product type, Price, Availability, Number of products sold, Revenue generated,
        ///   Customer demographics, Stock levels, Procurement lead time, Shipping times, Shipping carriers,
        ///   Shipping costs, Supplier name, Location, Manufacturing lead time, Manufacturing costs,
        ///   is_weekend, day_of_week, month, quarter, year
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Upload method called");
                _performanceMonitoring.RecordUserAction("upload_file");

                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("No file uploaded or file is empty");
                    ViewBag.Error = "Please select a valid CSV file.";
                    return View("Errors");
                }

                _logger.LogInformation($"File uploaded: {file.FileName}, Size: {file.Length} bytes");

                // Validate file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (fileExtension != ".csv")
                {
                    _logger.LogWarning($"Invalid file type: {fileExtension}");
                    ViewBag.Error = "Please upload a CSV file only.";
                    return View("Errors");
                }

                // Read stream once and compute a stable content hash for caching
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                string contentHash;
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    contentHash = Convert.ToHexString(sha256.ComputeHash(memoryStream));
                }
                memoryStream.Position = 0;

                // Stable cache key based on content hash
                var cacheKey = $"upload_{contentHash}";

                var cachedData = await _cacheService.GetAsync<List<SupplyChainData>>(cacheKey);

                List<SupplyChainData> supplyChainData;
                if (cachedData != null)
                {
                    _logger.LogInformation("Using cached data for file: {FileName}", file.FileName);
                    supplyChainData = cachedData;
                }
                else
                {
                    _logger.LogInformation("Loading CSV data");
                    supplyChainData = ImprovedDemandForecaster.LoadCsvData(memoryStream);

                    if (supplyChainData == null || supplyChainData.Count == 0)
                    {
                        _logger.LogWarning("No data found in CSV file");
                        ViewBag.Error = "The CSV file appears to be empty or invalid.";
                        return View("Errors");
                    }

                    // Cache the processed data for 1 hour
                    await _cacheService.SetAsync(cacheKey, supplyChainData, TimeSpan.FromHours(1));
                }

                _logger.LogInformation($"Loaded {supplyChainData.Count} records from CSV");

                // Store data in temporary file instead of TempData to avoid size limits
                var tempFilePath = Path.GetTempFileName();
                var jsonData = JsonSerializer.Serialize(supplyChainData);
                await System.IO.File.WriteAllTextAsync(tempFilePath, jsonData);

                // Store only the file path in TempData (much smaller)
                TempData["CsvDataFilePath"] = tempFilePath;
                TempData["FileName"] = file.FileName;
                TempData["UploadContentHash"] = contentHash;

                _logger.LogInformation($"Data saved to temporary file: {tempFilePath}");

                // Return preview view instead of immediately running forecasting
                return View("Preview", supplyChainData);
            }
            catch (FileLoadException ex)
            {
                _performanceMonitoring.RecordError("upload_file", ex);
                _logger.LogError(ex, "Error loading CSV file");
                ViewBag.Error = $"Error reading the CSV file: {ex.Message}";
                return View("Errors");
            }
            catch (ArgumentException ex)
            {
                _performanceMonitoring.RecordError("upload_file", ex);
                _logger.LogError(ex, "Invalid argument provided");
                ViewBag.Error = $"Invalid data format: {ex.Message}";
                return View("Errors");
            }
            catch (Exception ex)
            {
                _performanceMonitoring.RecordError("upload_file", ex);
                _logger.LogError(ex, "Unexpected error during upload");
                ViewBag.Error = $"An unexpected error occurred: {ex.Message}";
                return View("Errors");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitoring.RecordForecastRequest("upload", "file_processing", stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Generates demand forecasts based on uploaded data and specified parameters.
        /// </summary>
        /// <param name="horizon">The forecast horizon in days (default: 7).</param>
        /// <param name="unit">The time unit for forecasting - Days, Weeks, or Months (default: "Days").</param>
        /// <param name="quantity">The quantity of time units to forecast (default: 7).</param>
        /// <returns>
        /// Returns the forecast results view with enhanced forecasting data, or an error view if the prediction fails.
        /// Results are cached for 30 minutes to improve performance.
        /// </returns>
        /// <remarks>
        /// The method supports the following features:
        /// - Multiple time units (Days, Weeks, Months)
        /// - Caching of forecast results
        /// - Performance monitoring and metrics
        /// - Error handling and logging
        /// - Timeout protection (15 minutes)
        /// 
        /// The forecasting process uses:
        /// - Prophet model for time series forecasting
        /// - XGBoost model for machine learning predictions
        /// - Automatic model selection based on performance
        /// - Confidence intervals and performance metrics
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Predict(int horizon = 7, string unit = "Days", int quantity = 7)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Predict method called with horizon: {Horizon}, unit: {Unit}, quantity: {Quantity}",
                    horizon, unit, quantity);

                _performanceMonitoring.RecordUserAction("predict_forecast");

                // Validate unit parameter
                if (!Enum.TryParse<ForecastUnit>(unit, out var forecastUnit))
                {
                    _logger.LogWarning("Invalid unit value: {Unit}", unit);
                    ViewBag.Error = "Invalid forecast unit specified.";
                    return View("Errors");
                }

                // Create forecast request
                var forecastRequest = new ForecastRequest
                {
                    Unit = forecastUnit,
                    Quantity = quantity
                };

                // Validate the forecast request
                var (isValid, errorMessage) = ImprovedDemandForecaster.ValidateForecastRequest(forecastRequest);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid forecast request: {ErrorMessage}", errorMessage);
                    ViewBag.Error = errorMessage;
                    return View("Errors");
                }

                // Use the calculated horizon from the request
                var actualHorizon = forecastRequest.HorizonInDays;

                _logger.LogInformation("Using forecast horizon of {ActualHorizon} days for {Quantity} {Unit}",
                    actualHorizon, quantity, unit);

                // Retrieve file path from TempData
                var tempFilePath = TempData["CsvDataFilePath"] as string;
                var fileName = TempData["FileName"] as string;
                var contentHash = TempData["UploadContentHash"] as string;

                if (string.IsNullOrEmpty(tempFilePath) || !System.IO.File.Exists(tempFilePath))
                {
                    _logger.LogWarning("Temporary file not found. Path: {TempFilePath}", tempFilePath);
                    ViewBag.Error = "Data file not found. Please upload a CSV file again.";
                    return View("Errors");
                }

                _logger.LogInformation("Reading data from temporary file: {TempFilePath}", tempFilePath);

                // Check cache for forecast results
                var cacheKey = !string.IsNullOrEmpty(contentHash)
                    ? $"forecast_{contentHash}_{horizon}_{unit}_{quantity}"
                    : $"forecast_{fileName}_{horizon}_{unit}_{quantity}";
                var cachedResults = await _cacheService.GetAsync<List<EnhancedForecastResult>>(cacheKey);

                List<EnhancedForecastResult> results;
                if (cachedResults != null)
                {
                    _logger.LogInformation("Using cached forecast results for key: {CacheKey}", cacheKey);
                    results = cachedResults;
                }
                else
                {
                    // Read and deserialize data
                    var csvDataJson = await System.IO.File.ReadAllTextAsync(tempFilePath);
                    var supplyChainData = JsonSerializer.Deserialize<List<SupplyChainData>>(csvDataJson);

                    // Clean up temporary file immediately after reading
                    _ = Task.Run(CleanupOldTempFilesAsync);

                    if (supplyChainData == null || supplyChainData.Count == 0)
                    {
                        _logger.LogWarning("Failed to deserialize CSV data from temporary file");
                        ViewBag.Error = "Failed to process the uploaded data. Please try uploading again.";
                        return View("Errors");
                    }

                    _logger.LogInformation("Processing {Count} records for prediction", supplyChainData.Count);

                    // Add timeout for forecasting
                    using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15));

                    var forecaster = new ImprovedDemandForecaster();
                    results = await Task.Run(() =>
                        forecaster.ForecastAllProductsWithTimeUnits(supplyChainData, forecastRequest), cts.Token);

                    if (results == null || results.Count == 0)
                    {
                        _logger.LogWarning("Forecasting returned no results");
                        ViewBag.Error = "Unable to generate forecasts. The data may not contain sufficient information.";
                        return View("Errors");
                    }

                    // Cache the results for 30 minutes
                    await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(30));
                }

                _logger.LogInformation("Forecasting completed successfully with {Count} results", results.Count);

                // Pass additional data to view
                ViewBag.FileName = fileName;
                ViewBag.Horizon = actualHorizon;
                ViewBag.HorizonDisplay = forecastRequest.GetDisplayText();
                ViewBag.RecordCount = results.Count;
                ViewBag.ForecastUnit = unit;
                ViewBag.ForecastQuantity = quantity;

                return View("Results", results);
            }
            catch (OperationCanceledException)
            {
                _performanceMonitoring.RecordError("predict_forecast", new TimeoutException("Forecasting operation timed out"));
                _logger.LogWarning("Forecasting operation timed out");
                ViewBag.Error = "Forecasting took too long and was cancelled. Try with a smaller dataset or shorter horizon.";
                return View("Errors");
            }
            catch (Exception ex)
            {
                _performanceMonitoring.RecordError("predict_forecast", ex);
                _logger.LogError(ex, "Unexpected error during prediction");
                ViewBag.Error = $"An unexpected error occurred: {ex.Message}";
                return View("Errors");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitoring.RecordForecastRequest("prediction", unit, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Exports forecast results in various formats (CSV, Excel, JSON).
        /// </summary>
        /// <param name="format">The export format - "csv", "excel", or "json" (default: "csv").</param>
        /// <returns>
        /// Returns a file download with the exported data in the specified format.
        /// </returns>
        /// <remarks>
        /// Supported export formats:
        /// - CSV: Comma-separated values format for spreadsheet applications
        /// - Excel: Microsoft Excel format (.xlsx) with formatted data
        /// - JSON: JavaScript Object Notation format for API integration
        /// 
        /// The exported data includes:
        /// - Product ID and selected model
        /// - Forecast values and confidence intervals
        /// - Performance metrics (SMAPE, MAPE, R², RMSE, MAE)
        /// - Time period information
        /// </remarks>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Export(string format = "csv")
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _performanceMonitoring.RecordUserAction("export_results");

                // Get results from TempData or session
                var results = TempData["ForecastResults"] as List<EnhancedForecastResult>;
                if (results == null || !results.Any())
                {
                    ViewBag.Error = "No forecast results available for export.";
                    return View("Errors");
                }

                byte[] exportData;
                string contentType;
                string fileName;

                switch (format.ToLower())
                {
                    case "excel":
                        exportData = await _exportService.ExportToExcelAsync(results);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName = $"forecast_results_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        break;
                    case "json":
                        exportData = await _exportService.ExportToJsonAsync(results);
                        contentType = "application/json";
                        fileName = $"forecast_results_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        break;
                    default:
                        exportData = await _exportService.ExportToCsvAsync(results);
                        contentType = "text/csv";
                        fileName = $"forecast_results_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        break;
                }

                _logger.LogInformation("Export completed successfully - Format: {Format}, Records: {Count}", format, results.Count);

                return new FileContentResult(exportData, contentType) { FileDownloadName = fileName };
            }
            catch (Exception ex)
            {
                _performanceMonitoring.RecordError("export_results", ex);
                _logger.LogError(ex, "Error during export");
                ViewBag.Error = $"Error during export: {ex.Message}";
                return View("Errors");
            }
            finally
            {
                stopwatch.Stop();
                _performanceMonitoring.RecordExportRequest(format, stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Cleans up old temporary files to free up disk space.
        /// </summary>
        /// <returns>True if cleanup was successful, false otherwise.</returns>
        private async Task<bool> CleanupOldTempFilesAsync()
        {
            try
            {
                var tempDir = Path.GetTempPath();
                var tempFiles = Directory.GetFiles(tempDir, "temp_forecast_*");
                var cutoffTime = DateTime.Now.AddHours(-1);

                foreach (var file in tempFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < cutoffTime)
                        {
                            System.IO.File.Delete(file);
                            _logger.LogDebug("Cleaned up old temp file: {File}", file);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up temp file: {File}", file);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp file cleanup");
                return false;
            }
        }

        /// <summary>
        /// Displays the new upload page.
        /// </summary>
        /// <returns>The Index view for starting a new upload.</returns>
        public IActionResult NewUpload()
        {
            _performanceMonitoring.RecordUserAction("new_upload");
            return View("Index");
        }

        /// <summary>
        /// Cleans up old temporary files synchronously.
        /// </summary>
        private void CleanupOldTempFiles()
        {
            try
            {
                var tempDir = Path.GetTempPath();
                var tempFiles = Directory.GetFiles(tempDir, "temp_forecast_*");
                var cutoffTime = DateTime.Now.AddHours(-1);

                foreach (var file in tempFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < cutoffTime)
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to clean up temp file: {File}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during temp file cleanup");
            }
        }
    }
}