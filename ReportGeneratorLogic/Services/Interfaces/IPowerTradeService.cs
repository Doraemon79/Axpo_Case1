using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axpo;

namespace ReportGeneratorLogic.Services.Interfaces
{
    public interface IPowerTradeService
    {
        Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date);
    }
}
