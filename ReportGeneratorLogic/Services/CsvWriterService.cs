using CsvHelper;
using CsvHelper.Configuration;
using ReportGeneratorLogic.Models;
using ReportGeneratorLogic.Services.Interfaces;

namespace ReportGeneratorLogic.Services
{
    public class CsvWriterService : ICsvWriter
    {
        private readonly CsvConfiguration _csvConfig;

        public CsvWriterService(CsvConfiguration csvConfig)
        {
            _csvConfig = csvConfig;
        }
        public void WriteCsv(IEnumerable<TradeRecord> records, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, _csvConfig);

            csv.Context.RegisterClassMap<TradeRecordMap>();
            csv.WriteHeader<TradeRecord>();
            csv.NextRecord();

            foreach (var record in records)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }

        }
    }
}
