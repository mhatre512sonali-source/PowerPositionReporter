namespace WorkerService.Interfaces;

public interface ICsvExportService
{
    Task ExportAsync(Dictionary<int, double> aggregatedTrades, string outputFolder);
}