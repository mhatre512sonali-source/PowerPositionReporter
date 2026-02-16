using System;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Axpo;
using NodaTime;
using Microsoft.Extensions.Options;
using Serilog;
using WorkerService;
using WorkerService.Configuration;
using WorkerService.Interfaces;
using WorkerService.Services;

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, loggerConfiguration) =>
        {
            var loggingDirectory = context.Configuration.GetValue<string>("PowerPositionSettings:LoggingDirectory") ?? "./logs";
            Directory.CreateDirectory(loggingDirectory);
            var logFilePath = Path.Combine(loggingDirectory, "PowerPosition.log");

            loggerConfiguration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 50 * 1024 * 1024,
                    retainedFileCountLimit: 10,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
        })
        .ConfigureServices((context, services) =>
        {
            services.Configure<PowerPositionSettings>(context.Configuration.GetSection("PowerPositionSettings"));

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<PowerPositionSettings>>().Value);

            services.AddScoped<IPowerService, PowerService>();
            services.AddScoped<IPowerTradeProcessor, PowerTradeProcessor>();
            services.AddScoped<ICsvExportService, CsvExportService>();
            services.AddHostedService<Worker>();
        })
        .Build();

    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    var settings = host.Services.GetRequiredService<PowerPositionSettings>();
    logger.LogInformation("Starting Power Position Service - Interval: {Interval}min, Output: {Output}, Logs: {Logs}", settings.IntervalMinutes, settings.OutputFolder, settings.LoggingDirectory);

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
