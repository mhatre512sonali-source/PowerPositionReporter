# Power Position Reporter

A .NET worker service for extracting and aggregating power trade positions from the PowerService API, exporting results to CSV format.

## Prerequisites

- .NET 10.0 SDK or later
- Windows, macOS, or Linux

## Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd PowerPositionReporter
```

### 2. Restore Dependencies

```bash
dotnet restore
```

## Required Packages

### WorkerService Project

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.Extensions.Hosting` | .NET worker service framework |
| `Microsoft.Extensions.Configuration` |  Configuration management |
| `Microsoft.Extensions.Logging` |  Logging framework |
| `NodaTime` |  Timezone and date/time handling |
| `Axpo` |  Axpo Powerservice API client library |
| `Serilog` |  Logging library |

### PowerPositionTest Project

| Package | Version | Purpose |
|---------|---------|---------|
| `xunit` | 2.9.3 | Unit testing framework |
| `xunit.runner.visualstudio` | 3.1.4 | Visual Studio test runner |
| `Microsoft.NET.Test.Sdk` | 17.14.1 | Test SDK |
| `Moq` | 4.20.70 | Mocking framework for unit tests |
| `coverlet.collector` | 6.0.4 | Code coverage collection |

## Building

Build the solution:

```bash
dotnet build
```

Build a specific project:

```bash
# Build the worker service
dotnet build WorkerService/WorkerService.csproj

# Build the test project
dotnet build PowerPositionTest/PowerPositionTest.csproj
```

## Running

### Run the Worker Service

```bash
cd WorkerService
dotnet run
```

### Command-Line Arguments

```bash
dotnet run -- --interval 5 --output /path/to/output
```

- `--interval`: Extraction interval in minutes (default: 1)
- `--output`: Output folder path for CSV files (default: ./output)

## Testing

Run all unit tests:

```bash
cd PowerPositionTest
dotnet test
```

## Configuration

Configuration can be set via:

1. **appsettings.json** - Default configuration file
2. **appsettings.Development.json** - Development-specific settings
3. **Command-line arguments** - Override settings at runtime

### Example appsettings.json

```json
{
  "PowerPositionSettings": {
    "IntervalMinutes": 1,
    "OutputFolder": "./output",
    "TimeZoneId": "Europe/London"
  }
}
```

## Project Structure

```
PowerPositionReporter/
├── WorkerService/
│   ├── Program.cs              # Application entry point and DI setup
│   ├── Worker.cs               # Background worker service
│   ├── appsettings.json        # Configuration
│   ├── Configuration/
│   │   └── PowerPositionSettings.cs
│   └── Services/
│       ├── PowerTradeProcessor.cs
│       └── CsvExportService.cs
└── PowerPositionTest/
    └── UnitTest1.cs            # Unit tests
```

## Features

- **Trade Aggregation**: Aggregates power trades by hourly periods
- **CSV Export**: Exports aggregated data to timestamped CSV files
- **Timezone Support**: Handles timezone conversions using NodaTime
- **Configurable Intervals**: Run extraction at user-defined intervals
- **Logging**: Comprehensive logging for debugging and monitoring

## Output

CSV files are generated in the configured output folder with the following format:

```
PowerPosition_YYYYMMDD_HHMM.csv
```

Contents:
```
Local Time,Volume
00:00,1250.50
01:00,1840.75
...
```

## Troubleshooting

### Dependency Injection Error

If you encounter a dependency injection error, ensure all required services are registered in `Program.cs`:

```csharp
builder.Services.AddSingleton(settings);
builder.Services.AddScoped<PowerService>();
builder.Services.AddScoped<IPowerTradeProcessor, PowerTradeProcessor>();
builder.Services.AddScoped<ICsvExportService, CsvExportService>();
```

### Missing Output Folder

The service automatically creates the output folder if it doesn't exist.
