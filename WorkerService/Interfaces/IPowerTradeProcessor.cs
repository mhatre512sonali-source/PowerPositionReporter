namespace WorkerService.Interfaces
{
    public interface IPowerTradeProcessor
    {
        Task<Dictionary<int, double>> ProcessTradesAsync(DateTime date);
    }
}