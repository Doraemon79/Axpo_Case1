using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportGeneratorLogic.Models
{
    public class TradeRecord
    {
        public int PeriodId { get; set; }
        public string dateTime { get; set; }
        public double Volume { get; set; }
    }
}
