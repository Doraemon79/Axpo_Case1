using Axpo;

namespace ReportGeneratorLogic.Services.Interfaces
{
    public interface IPowerTradeService
    {
        Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
    }
}
