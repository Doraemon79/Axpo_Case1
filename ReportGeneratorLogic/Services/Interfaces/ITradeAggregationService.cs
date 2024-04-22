using Axpo;
using ReportGeneratorLogic.Models;

namespace ReportGeneratorLogic.Services.Interfaces
{
    public interface ITradeAggregationService
    {
        public List<TradeRecord> AggregateTrades(IEnumerable<PowerTrade> trades, DateTime tradeDate);
    }
}
