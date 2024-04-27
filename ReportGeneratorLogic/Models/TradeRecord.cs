namespace ReportGeneratorLogic.Models
{
    public class TradeRecord
    {
        public int PeriodId { get; set; }
        public required string DateTime { get; set; }
        public double Volume { get; set; }
    }
}
