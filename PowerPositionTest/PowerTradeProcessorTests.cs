using Microsoft.Extensions.Logging;
using Axpo;
using WorkerService.Services;
using WorkerService.Interfaces;
using NodaTime;
using Xunit.Sdk;

namespace PowerPositionTest;

[Collection("PowerPositionTests")]
public class PowerTradeProcessorTests
{
    private readonly PowerPositionSettings settings;
    private readonly Mock<ILogger<PowerTradeProcessor>> loggerMock;
    private readonly Mock<IPowerService> powerServiceMock;
    private readonly PowerTradeProcessor processor;

    public PowerTradeProcessorTests()
    {
        settings = new PowerPositionSettings
        {
            OutputFolder = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}"),
            TimeZoneId = "Europe/London"
        };
        loggerMock = new Mock<ILogger<PowerTradeProcessor>>();
        powerServiceMock = new Mock<IPowerService>();
        processor = new PowerTradeProcessor(powerServiceMock.Object, loggerMock.Object, settings);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(15)]
    public async Task ProcessTradesAsync_WithValidTrades_ReturnsAggregatedVolumes(int period)
    {

        List<PowerTrade> listPowerTrades = GetListOfPowerTrades(period);

        powerServiceMock
            .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(listPowerTrades);

        var result = await processor.ProcessTradesAsync(DateTime.Now);

        Assert.NotNull(result);
        Assert.Equal(period, result.Count);

        for (int i = 0; i < result.Count; i++)
        {
            if (result.ContainsKey(i))
            {
                Assert.Equal(15, result[i]);
            }
        }
    }

    [Fact]
    public async Task ProcessTradesAsync_WithEmptyTrades_ReturnsEmptyDictionary()
    {
        powerServiceMock
            .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<PowerTrade>());

        var result = await processor.ProcessTradesAsync(DateTime.Now);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ProcessTradesAsync_LogsInformation()
    {
        var trades = new List<PowerTrade>();
        trades.Add(PowerTrade.Create(DateTime.Now.Date, 10));

        powerServiceMock
            .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(trades);

        await processor.ProcessTradesAsync(DateTime.Now);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private List<PowerTrade> GetListOfPowerTrades(int period)
    {
        var listPowerTrades = new List<PowerTrade>();
        PowerTrade pt1 = PowerTrade.Create(DateTime.Now.Date, period);
        for (int i = 0; i < pt1.Periods.Length; i++)
        {
            pt1.Periods[i].SetVolume(10);
        }
        listPowerTrades.Add(pt1);

        PowerTrade pt2 = PowerTrade.Create(DateTime.Now.Date, period);
        for (int i = 0; i < pt2.Periods.Length; i++)
        {
            pt2.Periods[i].SetVolume(5);
        }
        listPowerTrades.Add(pt2);
        return listPowerTrades;
    }
}

public class PowerPositionSettingsTests
{

    [Fact]
    public void PowerPositionSettings_CanSetProperties()
    {
        // Arrange
        var settings = new PowerPositionSettings();

        // Act
        settings.IntervalMinutes = 5;
        settings.OutputFolder = "/custom/path";
        settings.TimeZoneId = "UTC";

        // Assert
        Assert.Equal(5, settings.IntervalMinutes);
        Assert.Equal("/custom/path", settings.OutputFolder);
        Assert.Equal("UTC", settings.TimeZoneId);
    }
}
