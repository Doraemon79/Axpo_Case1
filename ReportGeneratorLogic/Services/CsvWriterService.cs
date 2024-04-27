using CsvHelper;
using CsvHelper.Configuration;
using ReportGeneratorLogic.Models;
using ReportGeneratorLogic.Services.Interfaces;
using Serilog;

namespace ReportGeneratorLogic.Services
{
    public class CsvWriterService(CsvConfiguration csvConfig) : ICsvWriter
    {
        private readonly CsvConfiguration _csvConfig = csvConfig;

        public void WriteCsv(IEnumerable<TradeRecord> records, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, _csvConfig))
                {
                    csv.Context.RegisterClassMap<TradeRecordMap>();
                    csv.WriteRecords(records);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to write records to CSV at {FilePath}", filePath);
                throw;
            }
        }
    }
}
