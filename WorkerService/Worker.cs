using WorkerService.Configuration;
using WorkerService.Services;
using WorkerService.Interfaces;
namespace WorkerService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly PowerPositionSettings _settings;
    private Timer? _timer;

    public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            PowerPositionSettings settings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Worker] Power Position Extraction Worker starting...");
        _logger.LogInformation($"[Worker] Configuration - Interval: {_settings.IntervalMinutes} minutes, Output: {_settings.OutputFolder}, Logs: {_settings.LoggingDirectory}");

        try
        {
            _logger.LogDebug("[Worker] Performing initial extraction");
            await PerformExtractionAsync();

            var intervalMs = _settings.IntervalMinutes * 60 * 1000;
            _timer = new Timer(
                callback: async state => await PerformExtractionAsync(),
                state: null,
                dueTime: TimeSpan.FromMilliseconds(intervalMs),
                period: TimeSpan.FromMilliseconds(intervalMs)
            );

            _logger.LogInformation($"[Worker] Extractions scheduled every {_settings.IntervalMinutes} minutes");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("[Worker] Extraction Worker stopping (cancellation requested)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Worker] Fatal error in extraction worker");
            throw;
        }
    }

     public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[Worker] Extraction Worker stopping...");
            _timer?.Dispose();
            _logger.LogDebug("[Worker] Timer disposed");
            await base.StopAsync(cancellationToken);
        }

        private async Task PerformExtractionAsync()
        {
            var executionTime = DateTime.Now;
            _logger.LogInformation($"[Worker] [{executionTime:yyyy-MM-dd HH:mm:ss}] Starting extraction cycle");

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var tradeProcessor = scope.ServiceProvider.GetRequiredService<IPowerTradeProcessor>();
                    var csvExportService = scope.ServiceProvider.GetRequiredService<ICsvExportService>();
                    
                    // Get trades for today
                    var today = DateTime.Today;
                    _logger.LogDebug($"[Worker] Processing trades for date: {today:yyyy-MM-dd}");
                    var aggregated = await tradeProcessor.ProcessTradesAsync(today);

                    _logger.LogDebug($"[Worker] Exporting {aggregated.Count} hourly records to {_settings.OutputFolder}");
                    // Export to CSV
                    await csvExportService.ExportAsync(aggregated, _settings.OutputFolder);
                }

                _logger.LogInformation($"[Worker] [{executionTime:yyyy-MM-dd HH:mm:ss}] Extraction cycle completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Worker] [{executionTime:yyyy-MM-dd HH:mm:ss}] Extraction cycle failed");
            }
        }
    
}
