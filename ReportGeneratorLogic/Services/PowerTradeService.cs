using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Axpo;
using ReportGeneratorLogic.Services.Interfaces;

namespace ReportGeneratorLogic.Services
{
    public class PowerTradeService : IPowerTradeService
    {
        public async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
        {
            var powerService = new PowerService();
            return await powerService.GetTradesAsync(date);
        }
    }
}
