using MLINTERNSHIP;

namespace FrontEndForecasting.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportToCsvAsync(List<EnhancedForecastResult> results);
        Task<byte[]> ExportToExcelAsync(List<EnhancedForecastResult> results);
        Task<byte[]> ExportToJsonAsync(List<EnhancedForecastResult> results);
    }
}
