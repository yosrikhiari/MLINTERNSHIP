using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using System.Text.Json;
using MLINTERNSHIP;

namespace FrontEndForecasting1.Services
{
    public class ExportService : IExportService
    {
        private readonly ILogger<ExportService> _logger;

        public ExportService(ILogger<ExportService> logger)
        {
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> ExportToCsvAsync(List<EnhancedForecastResult> results)
        {
            try
            {
                using var memoryStream = new MemoryStream();
                using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

                // Write header
                csv.WriteField("Product ID");
                csv.WriteField("Selected Model");
                csv.WriteField("Forecast Period");
                csv.WriteField("Day/Period");
                csv.WriteField("Forecast Value");
                csv.WriteField("Confidence Interval");
                csv.WriteField("Prophet Forecast");
                csv.WriteField("XGBoost Forecast");
                csv.WriteField("SMAPE");
                csv.WriteField("MAPE");
                csv.WriteField("R²");
                csv.WriteField("RMSE");
                csv.WriteField("MAE");
                csv.NextRecord();

                // Write data
                foreach (var result in results)
                {
                    var useAggregated = result.AggregatedPredictions?.Count > 0 && 
                                      result.Request?.Unit != ForecastUnit.Days;

                    var predictions = useAggregated ? result.AggregatedPredictions : result.Predictions;
                    var confidenceIntervals = useAggregated ? result.AggregatedConfidenceIntervals : result.ConfidenceIntervals;
                    var labels = useAggregated ? result.AggregatedLabels : 
                                Enumerable.Range(1, result.Predictions.Count).Select(i => $"Day {i}").ToList();

                    for (int i = 0; i < predictions.Count; i++)
                    {
                        csv.WriteField(result.ProductId);
                        csv.WriteField(result.SelectedModel);
                        csv.WriteField(result.Request?.GetDisplayText() ?? "7 days");
                        csv.WriteField(labels[i]);
                        csv.WriteField(predictions[i].ToString("F2"));
                        csv.WriteField((confidenceIntervals?.Count > i ? confidenceIntervals[i] : 0).ToString("F2"));
                        csv.WriteField(result.ProphetPredictions?.Count > i ? result.ProphetPredictions[i].ToString("F2") : "");
                        csv.WriteField(result.XgBoostPredictions?.Count > i ? result.XgBoostPredictions[i].ToString("F2") : "");
                        csv.WriteField(result.Metrics?.MAPE.ToString("F2") ?? "0.00");
                        csv.WriteField(result.Metrics?.R2.ToString("F3") ?? "0.000");
                        csv.WriteField(result.Metrics?.RMSE.ToString("F2") ?? "0.00");
                        csv.WriteField(result.Metrics?.MAE.ToString("F2") ?? "0.00");
                        csv.NextRecord();
                    }
                }

                await writer.FlushAsync();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to CSV");
                throw;
            }
        }

        public async Task<byte[]> ExportToExcelAsync(List<EnhancedForecastResult> results)
        {
            try
            {
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Forecast Results");

                // Add headers
                var headers = new[]
                {
                    "Product ID", "Selected Model", "Forecast Period", "Day/Period", 
                    "Forecast Value", "Confidence Interval", "Prophet Forecast", 
                    "XGBoost Forecast", "SMAPE", "MAPE", "R²", "RMSE", "MAE"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                }

                // Add data
                int row = 2;
                foreach (var result in results)
                {
                    var useAggregated = result.AggregatedPredictions?.Count > 0 && 
                                      result.Request?.Unit != ForecastUnit.Days;

                    var predictions = useAggregated ? result.AggregatedPredictions : result.Predictions;
                    var confidenceIntervals = useAggregated ? result.AggregatedConfidenceIntervals : result.ConfidenceIntervals;
                    var labels = useAggregated ? result.AggregatedLabels : 
                                Enumerable.Range(1, result.Predictions.Count).Select(i => $"Day {i}").ToList();

                    for (int i = 0; i < predictions.Count; i++)
                    {
                        worksheet.Cells[row, 1].Value = result.ProductId;
                        worksheet.Cells[row, 2].Value = result.SelectedModel;
                        worksheet.Cells[row, 3].Value = result.Request?.GetDisplayText() ?? "7 days";
                        worksheet.Cells[row, 4].Value = labels[i];
                        worksheet.Cells[row, 5].Value = predictions[i];
                        worksheet.Cells[row, 6].Value = confidenceIntervals?.Count > i ? confidenceIntervals[i] : 0;
                        worksheet.Cells[row, 7].Value = result.ProphetPredictions?.Count > i ? result.ProphetPredictions[i] : null;
                        worksheet.Cells[row, 8].Value = result.XgBoostPredictions?.Count > i ? result.XgBoostPredictions[i] : null;
                        worksheet.Cells[row, 10].Value = result.Metrics?.MAPE ?? 0;
                        worksheet.Cells[row, 11].Value = result.Metrics?.R2 ?? 0;
                        worksheet.Cells[row, 12].Value = result.Metrics?.RMSE ?? 0;
                        worksheet.Cells[row, 13].Value = result.Metrics?.MAE ?? 0;
                        row++;
                    }
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to Excel");
                throw;
            }
        }

        public async Task<byte[]> ExportToJsonAsync(List<EnhancedForecastResult> results)
        {
            try
            {
                var exportData = results.Select(r => new
                {
                    ProductId = r.ProductId,
                    SelectedModel = r.SelectedModel,
                    ForecastPeriod = r.Request?.GetDisplayText() ?? "7 days",
                    ForecastUnit = r.Request?.Unit.ToString() ?? "Days",
                    Predictions = r.Predictions,
                    AggregatedPredictions = r.AggregatedPredictions,
                    ConfidenceIntervals = r.ConfidenceIntervals,
                    AggregatedConfidenceIntervals = r.AggregatedConfidenceIntervals,
                    ProphetPredictions = r.ProphetPredictions,
                    XgBoostPredictions = r.XgBoostPredictions,
                    Metrics = r.Metrics,
                    ProphetSMAPE = r.ProphetSMAPE,
                    XgBoostSMAPE = r.XgBoostSMAPE,
                    ForecastDates = r.ForecastDates?.Select(d => d.ToString("yyyy-MM-dd")).ToList()
                });

                var jsonString = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return Encoding.UTF8.GetBytes(jsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting to JSON");
                throw;
            }
        }
    }
}
