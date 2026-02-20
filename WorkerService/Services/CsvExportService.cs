using NodaTime;
using Axpo;
using WorkerService.Interfaces;
using WorkerService.Configuration;

namespace WorkerService.Services
{
    public class CsvExportService : ICsvExportService
    {
        private readonly ILogger<CsvExportService> _logger;
        private readonly IDateTimeZoneProvider _timeZoneProvider;
        private readonly PowerPositionSettings _settings;

        public CsvExportService(ILogger<CsvExportService> logger, PowerPositionSettings settings)
        {
            _logger = logger;
            _timeZoneProvider = DateTimeZoneProviders.Tzdb;
            _settings = settings;
        }

        public async Task ExportAsync(Dictionary<int, double> aggregatedTrades, string outputFolder)
        {
            try
            {
                _logger.LogInformation($"[CsvExportService] Starting CSV export with {aggregatedTrades.Count} hourly entries");

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                    _logger.LogInformation($"[CsvExportService] Created output folder: {outputFolder}");
                }
                else
                {
                    _logger.LogDebug($"[CsvExportService] Output folder already exists: {outputFolder}");
                }

                var tz = _timeZoneProvider[_settings.TimeZoneId];
                var now = SystemClock.Instance.GetCurrentInstant().InZone(tz).LocalDateTime;

                var filename = $"PowerPosition_{now.Year:D4}{now.Month:D2}{now.Day:D2}_{now.Hour:D2}{now.Minute:D2}.csv";
                var filepath = Path.Combine(outputFolder, filename);

                _logger.LogDebug($"[CsvExportService] Generating CSV file: {filename}");

                var lines = new List<string> { "Local Time,Volume" };

                // Sort by hour, adjusting for period 1 as hour 23
                foreach (var kvp in aggregatedTrades.OrderBy(x => (x.Key + 1) % 24))
                {
                    var timeKey = $"{kvp.Key:D2}:00";
                    lines.Add($"{timeKey},{kvp.Value}");
                }

                await File.WriteAllLinesAsync(filepath, lines);
                _logger.LogInformation($"[CsvExportService] CSV successfully exported to: {filepath} ({lines.Count} lines)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CsvExportService] Error exporting CSV");
                throw;
            }
        }
    }
}
