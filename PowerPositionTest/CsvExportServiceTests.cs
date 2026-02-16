
using Microsoft.Extensions.Logging;

public class CsvExportServiceTests
{
    [Fact]
    public async Task ExportAsync_CreatesOutputFolder_WhenFolderDoesNotExist()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CsvExportService>>();
        var outputFolder = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}");

        var aggregatedTrades = new Dictionary<int, double>
        {
            { 0, 100 },
            { 1, 150 }
        };

        var service = new CsvExportService(loggerMock.Object);
        try
        {
            // Act
            await service.ExportAsync(aggregatedTrades, outputFolder);

            // Assert
            Assert.True(Directory.Exists(outputFolder));
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_CreatesCsvFile_WithCorrectContent()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CsvExportService>>();
        var outputFolder = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}");
        Directory.CreateDirectory(outputFolder);

        var aggregatedTrades = new Dictionary<int, double>
        {
            { 0, 100.5 },
            { 1, 200.75 }
        };

        var service = new CsvExportService(loggerMock.Object);

        try
        {
            // Act
            await service.ExportAsync(aggregatedTrades, outputFolder);

            // Assert
            var files = Directory.GetFiles(outputFolder, "PowerPosition_*.csv");
            Assert.Single(files);

            var content = await File.ReadAllLinesAsync(files[0]);
            Assert.NotEmpty(content);
            Assert.Equal("Local Time,Volume", content[0]);
            Assert.Contains("00:00", content[1]);
            Assert.Contains("01:00", content[2]);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }

    [Fact]
    public async Task ExportAsync_WithEmptyTrades_CreatesFileWithHeaderOnly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<CsvExportService>>();
        var outputFolder = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}");
        Directory.CreateDirectory(outputFolder);

        var aggregatedTrades = new Dictionary<int, double>();

        var service = new CsvExportService(loggerMock.Object);

        try
        {
            // Act
            await service.ExportAsync(aggregatedTrades, outputFolder);

            // Assert
            var files = Directory.GetFiles(outputFolder, "PowerPosition_*.csv");
            Assert.Single(files);

            var content = await File.ReadAllLinesAsync(files[0]);
            Assert.Single(content);
            Assert.Equal("Local Time,Volume", content[0]);
        }
        finally
        {
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, true);
            }
        }
    }
}
