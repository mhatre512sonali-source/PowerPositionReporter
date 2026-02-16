namespace WorkerService.Configuration
{
    public class PowerPositionSettings
    {
        public int IntervalMinutes { get; set; } = 60;
        public string OutputFolder { get; set; } = "./output";
        public string TimeZoneId { get; set; } = "Europe/London";

        public string LoggingDirectory { get; set; } = "./logs";
    }
}
