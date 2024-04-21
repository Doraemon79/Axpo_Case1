using ReportGeneratorLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGeneratorLogic.Services.Interfaces
{
    public interface ICsvWriter
    {
        public void WriteCsv(IEnumerable<TradeRecord> records, string filePath);
    }
}
