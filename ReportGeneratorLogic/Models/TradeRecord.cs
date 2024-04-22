namespace ReportGeneratorLogic.Models
{
    public class TradeRecord
    {
        public int PeriodId { get; set; }
        public required string dateTime { get; set; }
        public double Volume { get; set; }
    }
}
