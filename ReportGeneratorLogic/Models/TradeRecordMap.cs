using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGeneratorLogic.Models
{
    public class TradeRecordMap : ClassMap<TradeRecord>
    {
        public TradeRecordMap()
        {
            Map(m => m.dateTime).Name("datetime").Index(0);
            Map(m => m.Volume).Name("Volume").Index(1);
        }
    }
}
