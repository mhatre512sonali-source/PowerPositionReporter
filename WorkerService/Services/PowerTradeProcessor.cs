using Axpo;
using NodaTime;
using WorkerService.Interfaces;
using System.Globalization;
using WorkerService.Configuration;

namespace WorkerService.Services
{
    public class PowerTradeProcessor : IPowerTradeProcessor
    {
        private readonly IPowerService _powerService;
        private readonly ILogger<PowerTradeProcessor> _logger;
        private readonly PowerPositionSettings _settings;
        private readonly IDateTimeZoneProvider _timeZoneProvider;

        public PowerTradeProcessor(
            IPowerService powerService,
            ILogger<PowerTradeProcessor> logger,
            PowerPositionSettings settings)
        {
            _powerService = powerService;
            _logger = logger;
            _timeZoneProvider = DateTimeZoneProviders.Tzdb;
            _settings = settings;
        }

        public async Task<Dictionary<int, double>> ProcessTradesAsync(DateTime date)
        { 
            try
            {
                _logger.LogInformation($"[PowerTradeProcessor] Starting trade processing for {date:yyyy-MM-dd}");

                var trades = await _powerService.GetTradesAsync(date);
                var tradeList = trades.ToList();

                _logger.LogInformation($"[PowerTradeProcessor] Retrieved {tradeList.Count} trades");
                _logger.LogInformation($"[PowerTradeProcessor] Trade IDs: {string.Join(", ", tradeList.Select(t => t.TradeId))}");

                var aggregated = new Dictionary<int, double>();
                //var timeZone = _timeZoneProvider[_settings.TimeZoneId];
               
                var startOfTradingDay = date.Date.AddHours(-1);

                foreach (var trade in tradeList)
                {
                    //_logger.LogDebug($"[PowerTradeProcessor] Processing trade {trade.TradeId} with {trade.Periods.Count} periods");
                    foreach (var period in trade.Periods)
                    {
                        // Period 1 = 23:00, Period 2 = 00:00, ..., Period 24 = 22:00
                        var timeKey = startOfTradingDay.AddHours(period.Period - 1);

                        if (!aggregated.ContainsKey(timeKey.Hour))
                        {
                            aggregated[timeKey.Hour] = 0;
                        }
                        aggregated[timeKey.Hour] += period.Volume;
                        //_logger.LogDebug($"[PowerTradeProcessor] Trade {trade.TradeId}, Period {period.Period}, Volume {period.Volume} -> Hour {timeKey.Hour}");
                    }
                }

                _logger.LogInformation($"[PowerTradeProcessor] Aggregation complete: {aggregated.Count} hourly buckets with total volume {aggregated.Values.Sum()}");
                return aggregated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[PowerTradeProcessor] Error processing trades");
                throw;
            }
        }
    }
}
