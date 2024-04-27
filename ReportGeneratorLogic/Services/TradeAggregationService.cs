using Axpo;
using ReportGeneratorLogic.Models;
using ReportGeneratorLogic.Services.Interfaces;

namespace ReportGeneratorLogic.Services
{
    public class TradeAggregationService(string timeZoneId = "GMT Standard Time") : ITradeAggregationService
    {
        private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        public List<TradeRecord> AggregateTrades(IEnumerable<PowerTrade> trades, DateTime tradeDate)
        {
            TimeSpan utcOffset = _timeZoneInfo.GetUtcOffset(tradeDate);
            DateTime startDateTime = new(tradeDate.Year, tradeDate.Month, tradeDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
            if (utcOffset.Hours < 0) { startDateTime = startDateTime.AddDays(1); }
            startDateTime = startDateTime.Date.AddHours(-utcOffset.Hours);


            var aggregatedRecords = new Dictionary<int, TradeRecord>();

            foreach (var trade in trades)
            {
                foreach (var period in trade.Periods)
                {
                    if (aggregatedRecords.TryGetValue(period.Period, out TradeRecord existingRecord))
                    {
                        existingRecord.Volume += period.Volume;
                    }
                    else
                    {
                        DateTime dt = new(startDateTime.Ticks, DateTimeKind.Unspecified);
                        if (utcOffset.Hours > 0) { dt = dt.AddHours(period.Period - 1); }
                        else if (utcOffset.Hours < 0) { dt = dt.AddHours(-(period.Period - 1)); };

                        aggregatedRecords[period.Period] = new TradeRecord
                        {
                            PeriodId = period.Period,
                            DateTime = dt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            Volume = period.Volume
                        };
                    }
                }
            }

            return [.. aggregatedRecords.Values.OrderBy(r => r.PeriodId)];
        }

    }
}
