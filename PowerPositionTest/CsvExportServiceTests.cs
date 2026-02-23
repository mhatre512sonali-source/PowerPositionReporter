
using Microsoft.Extensions.Logging;

[Collection("PowerPositionTests")]
public class CsvExportServiceTests
{
    private readonly PowerPositionSettings settings;
    private readonly Mock<ILogger<CsvExportService>> loggerMock;
    private readonly CsvExportService service;

    public CsvExportServiceTests()
    {
        loggerMock = new Mock<ILogger<CsvExportService>>();
        settings = new PowerPositionSettings
        {
            OutputFolder = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}"),
            TimeZoneId = "Europe/London"
        };
        service = new CsvExportService(loggerMock.Object, settings);
    }

    [Fact]
    public async Task ExportAsync_CreatesOutputFolder_WhenFolderDoesNotExist()
    {
        var aggregatedTrades = new Dictionary<int, double>
        {
            { 0, 100 },
            { 1, 150 }
        };

        try
        {
            await service.ExportAsync(aggregatedTrades, settings.OutputFolder);

            Assert.True(Directory.Exists(settings.OutputFolder));
        }
        finally
        {
            if (Directory.Exists(settings.OutputFolder))
            {
                Directory.Delete(settings.OutputFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_CreatesCsvFile_WithCorrectContent()
    {
        Directory.CreateDirectory(settings.OutputFolder);

        var aggregatedTrades = new Dictionary<int, double>
        {
            { 0, 100.5 },
            { 1, 200.75 }
        };
        try
        {
            await service.ExportAsync(aggregatedTrades, settings.OutputFolder);

            var files = Directory.GetFiles(settings.OutputFolder, "PowerPosition_*.csv");
            Assert.Single(files);

            var content = await File.ReadAllLinesAsync(files[0]);
            Assert.NotEmpty(content);
            Assert.Equal("Local Time,Volume", content[0]);
            Assert.Contains("00:00", content[1]);
            Assert.Contains("01:00", content[2]);
        }
        finally
        {
            if (Directory.Exists(settings.OutputFolder))
            {
                Directory.Delete(settings.OutputFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_WithEmptyTrades_CreatesFileWithHeaderOnly()
    {
        Directory.CreateDirectory(settings.OutputFolder);

        var aggregatedTrades = new Dictionary<int, double>();

        try
        {
            await service.ExportAsync(aggregatedTrades, settings.OutputFolder);

            var files = Directory.GetFiles(settings.OutputFolder, "PowerPosition_*.csv");
            Assert.Single(files);

            var content = await File.ReadAllLinesAsync(files[0]);
            Assert.Single(content);
            Assert.Equal("Local Time,Volume", content[0]);
        }
        finally
        {
            if (Directory.Exists(settings.OutputFolder))
            {
                Directory.Delete(settings.OutputFolder, true);
            }
        }
    }
}
