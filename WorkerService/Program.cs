using Axpo;
using WorkerService;
using WorkerService.Configuration;
using WorkerService.Services;
using WorkerService.Interfaces;
using Microsoft.Extensions.Hosting;
using Serilog;



// builder.Services.AddSingleton(settings);
// builder.Services.AddScoped<IPowerService, PowerService>();
// builder.Services.AddScoped<IPowerTradeProcessor, PowerTradeProcessor>();
// builder.Services.AddScoped<ICsvExportService, CsvExportService>();
// builder.Services.AddHostedService<Worker>();

// var host = builder.Build();

// var logger = loggerFactory.CreateLogger<ILogger<Program>>();

//var logger = host.Services.GetRequiredService<ILogger<Program>>();
// logger.LogInformation($"Starting Power Position Service - Interval: {settings.IntervalMinutes}min, Output: {settings.OutputFolder}, Logs: {settings.LoggingDirectory}");

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            var settings = context.Configuration.GetSection("PowerPositionSettings")
                        .Get<PowerPositionSettings>() ?? new PowerPositionSettings();

            services.Configure<PowerPositionSettings>(options =>
            {
                options.IntervalMinutes = settings.IntervalMinutes;
                options.OutputFolder = settings.OutputFolder;
                options.TimeZoneId = settings.TimeZoneId;
                options.LoggingDirectory = settings.LoggingDirectory;
            });

            services.AddSingleton(settings);
            services.AddScoped<IPowerService, PowerService>();
            services.AddScoped<IPowerTradeProcessor, PowerTradeProcessor>();
            services.AddScoped<ICsvExportService, CsvExportService>();
            services.AddHostedService<Worker>();


            //Configure logging
            var loggingDirectory = settings.LoggingDirectory ?? "./logs";
            Directory.CreateDirectory(loggingDirectory);

            var logFilePath = Path.Combine(loggingDirectory, "PowerPosition.log");

            // Configure Serilog

            Directory.CreateDirectory("logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    "logs/PowerPosition.log",
                    rollingInterval: RollingInterval.Hour,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {SourceContext} {Message}{NewLine}{Exception}")
                .CreateLogger();

            // Create LoggerFactory that uses Serilog
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();           
                builder.AddSerilog();               // Add Serilog as provider
            });
            var logger = loggerFactory.CreateLogger<ILogger<Program>>();
            logger.LogInformation($"Starting Power Position Service - Interval: {settings.IntervalMinutes}min, Output: {settings.OutputFolder}, Logs: {settings.LoggingDirectory}");

        })
        .Build();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
