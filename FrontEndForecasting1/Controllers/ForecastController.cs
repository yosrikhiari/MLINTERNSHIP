using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using MLINTERNSHIP;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FrontEndForecasting1.Controllers
{
    public class ForecastController : Controller
    {
        private readonly ILogger<ForecastController> _logger;

        public ForecastController(ILogger<ForecastController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                _logger.LogInformation("Upload method called");

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

                // Create a memory stream to read the file
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                _logger.LogInformation("Loading CSV data");
                var supplyChainData = ImprovedDemandForecaster.LoadCsvData(memoryStream);

                if (supplyChainData == null || supplyChainData.Count == 0)
                {
                    _logger.LogWarning("No data found in CSV file");
                    ViewBag.Error = "The CSV file appears to be empty or invalid.";
                    return View("Errors");
                }

                _logger.LogInformation($"Loaded {supplyChainData.Count} records from CSV");

                // Store data in temporary file instead of TempData to avoid size limits
                var tempFilePath = Path.GetTempFileName();
                var jsonData = JsonSerializer.Serialize(supplyChainData);
                await System.IO.File.WriteAllTextAsync(tempFilePath, jsonData);

                // Store only the file path in TempData (much smaller)
                TempData["CsvDataFilePath"] = tempFilePath;
                TempData["FileName"] = file.FileName;

                _logger.LogInformation($"Data saved to temporary file: {tempFilePath}");

                // Return preview view instead of immediately running forecasting
                return View("Preview", supplyChainData);
            }
            catch (FileLoadException ex)
            {
                _logger.LogError(ex, "Error loading CSV file");
                ViewBag.Error = $"Error reading the CSV file: {ex.Message}";
                return View("Errors");
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument provided");
                ViewBag.Error = $"Invalid data format: {ex.Message}";
                return View("Errors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during upload");
                ViewBag.Error = $"An unexpected error occurred: {ex.Message}";
                return View("Errors");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Predict(int horizon = 7, string unit = "Days", int quantity = 7)
        {
            try
            {
                _logger.LogInformation("Predict method called with horizon: {Horizon}, unit: {Unit}, quantity: {Quantity}",
                    horizon, unit, quantity);

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

                if (string.IsNullOrEmpty(tempFilePath) || !System.IO.File.Exists(tempFilePath))
                {
                    _logger.LogWarning("Temporary file not found. Path: {TempFilePath}", tempFilePath);
                    ViewBag.Error = "Data file not found. Please upload a CSV file again.";
                    return View("Errors");
                }

                _logger.LogInformation("Reading data from temporary file: {TempFilePath}", tempFilePath);

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
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(15)); // Increased timeout for longer forecasts

                var forecaster = new ImprovedDemandForecaster();
                var results = await Task.Run(() =>
                    forecaster.ForecastAllProductsWithTimeUnits(supplyChainData, forecastRequest), cts.Token);

                if (results == null || results.Count == 0)
                {
                    _logger.LogWarning("Forecasting returned no results");
                    ViewBag.Error = "Unable to generate forecasts. The data may not contain sufficient information.";
                    return View("Errors");
                }

                _logger.LogInformation("Forecasting completed successfully with {Count} results", results.Count);

                // Pass additional data to view
                ViewBag.FileName = fileName;
                ViewBag.Horizon = actualHorizon;
                ViewBag.HorizonDisplay = forecastRequest.GetDisplayText();
                ViewBag.RecordCount = supplyChainData.Count;
                ViewBag.ForecastUnit = unit;
                ViewBag.ForecastQuantity = quantity;

                return View("Results", results);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Forecasting operation timed out");
                ViewBag.Error = "Forecasting took too long and was cancelled. Try with a smaller dataset or shorter horizon.";
                return View("Errors");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing data from temporary file");
                ViewBag.Error = "Error processing the data format. Please ensure your CSV file is valid.";
                return View("Errors");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during prediction");
                ViewBag.Error = $"An unexpected error occurred: {ex.Message}";
                return View("Errors");
            }
        }
        // Helper method for safe file cleanup
        private async Task<bool> CleanupOldTempFilesAsync()
        {
            try
            {
                var tempDirectory = Path.GetTempPath();
                var tempFiles = Directory.GetFiles(tempDirectory, "tmp*.tmp");
                var cutoffTime = DateTime.Now.AddHours(-1); // More aggressive cleanup
                var cleanedCount = 0;

                foreach (var file in tempFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTime < cutoffTime)
                        {
                            await Task.Run(() => System.IO.File.Delete(file));
                            cleanedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temp file: {File}", file);
                    }
                }

                if (cleanedCount > 0)
                {
                    _logger.LogInformation("Cleaned up {Count} old temporary files", cleanedCount);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during temp file cleanup");
                return false;
            }
        }

        public IActionResult NewUpload()
        {
            // Clear any existing data and clean up temporary files
            var tempFilePath = TempData["CsvDataFilePath"] as string;
            if (!string.IsNullOrEmpty(tempFilePath) && System.IO.File.Exists(tempFilePath))
            {
                try
                {
                    System.IO.File.Delete(tempFilePath);
                    _logger.LogInformation($"Cleaned up temporary file: {tempFilePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to clean up temporary file: {tempFilePath}");
                }
            }

            TempData.Clear();
            return RedirectToAction("Index");
        }

       
        private void CleanupOldTempFiles()
        {
            try
            {
                var tempDirectory = Path.GetTempPath();
                var tempFiles = Directory.GetFiles(tempDirectory, "tmp*.tmp");
                var cutoffTime = DateTime.Now.AddHours(-2); // Remove files older than 2 hours

                foreach (var file in tempFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffTime)
                    {
                        try
                        {
                            System.IO.File.Delete(file);
                            _logger.LogInformation($"Cleaned up old temporary file: {file}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to delete old temporary file: {file}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during temporary file cleanup");
            }
        }
    }
}