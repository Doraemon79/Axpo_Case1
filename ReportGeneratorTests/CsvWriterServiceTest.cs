using CsvHelper.Configuration;
using ReportGeneratorLogic.Models;
using ReportGeneratorLogic.Services;
using System.Globalization;
using System.Text;

namespace ReportGeneratorTests
{
    public class CsvWriterServiceTest
    {
        private readonly CsvWriterService CsvWriterService_Sut;
        private readonly CsvConfiguration _csvConfig;

        public CsvWriterServiceTest()
        {
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                Quote = '"',
                Encoding = Encoding.UTF8
            };
            CsvWriterService_Sut = new CsvWriterService(_csvConfig);
        }

        [Fact]
        public void WriteCsv_RecordsAndFilePath_Should_WriteCsv()
        {
            //Arrange
            var filePath = "TestFilePath.csv";
            var records = new List<TradeRecord>
            {
                new() {
                PeriodId = 1,
                DateTime = "01-01-2021",
                Volume = 1.1,
                }
            };

            //Act
            CsvWriterService_Sut.WriteCsv(records, filePath);

            //Assert
            Assert.True(File.Exists(filePath));
            File.Delete(filePath);
            Assert.False(File.Exists(filePath));

        }
    }
}
