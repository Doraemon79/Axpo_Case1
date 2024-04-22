using ReportGeneratorLogic.Models;

namespace ReportGeneratorLogic.Services.Interfaces
{
    public interface ICsvWriter
    {
        public void WriteCsv(IEnumerable<TradeRecord> records, string filePath);
    }
}
