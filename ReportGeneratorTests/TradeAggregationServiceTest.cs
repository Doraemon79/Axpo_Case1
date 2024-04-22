using Axpo;
using ReportGeneratorLogic.Services;

namespace ReportGeneratorTests
{
    public class TradeAggregationServiceTest
    {
        string timeZoneId = "GMT Standard Time";
        private readonly TradeAggregationService TradeAggregationService_Sut;

        public TradeAggregationServiceTest()
        {
            TradeAggregationService_Sut = new TradeAggregationService();
        }

        [Fact]
        public void AggregateTradesTest_PeriodCount_ShouldMatch()
        {
            //Arrange
            DateTime dateTest = new DateTime(2021, 1, 1);
            PowerTrade pw0 = PowerTrade.Create(dateTest, 3);
            PowerTrade pw1 = PowerTrade.Create(dateTest, 3);

            IEnumerable<PowerTrade> powerTrades = new List<PowerTrade> { pw0, pw1 };
            TradeAggregationService_Sut.AggregateTrades(new List<PowerTrade>(), new DateTime(2021, 1, 1));
            var date = new DateTime(2021, 1, 1);

            //Act
            var service = new TradeAggregationService();
            var trades = service.AggregateTrades(powerTrades, date);

            //Assert
            Assert.Equal(3, trades.Count);
            Assert.Equal(2.2, trades[0].Volume);

        }

        [Theory]
        [InlineData(1.1, 2.2, 3.3, 1.1, 2.2, 3.3, 2.2, 4.4, 6.6)]
        public void AggregateTradesTest_PeriodVolumes_ShouldAdd(double volume01, double volume02, double volume03,
                                                                double volume11, double volume12, double volume13,
                                                                double expected1, double expected2, double expected3)
        {
            //Arrange
            PowerPeriod period01 = new PowerPeriod(1);
            PowerPeriod period02 = new PowerPeriod(2);
            PowerPeriod period03 = new PowerPeriod(3);
            PowerPeriod period11 = new PowerPeriod(1);
            PowerPeriod period12 = new PowerPeriod(2);
            PowerPeriod period13 = new PowerPeriod(3);
            period01.SetVolume(volume01);
            period02.SetVolume(volume02);
            period03.SetVolume(volume03);
            period11.SetVolume(volume11);
            period12.SetVolume(volume12);
            period13.SetVolume(volume13);
            PowerPeriod[] periods0 = { period01, period02, period03 };
            PowerPeriod[] periods1 = { period11, period12, period13 };
            DateTime dateTest = new DateTime(2021, 1, 1);
            PowerTrade pw0 = PowerTrade.Create(dateTest, 3);
            PowerTrade pw1 = PowerTrade.Create(dateTest, 3);
            pw0.Periods[0] = periods0[0];
            pw0.Periods[1] = periods0[1];
            pw0.Periods[2] = periods0[2];
            pw1.Periods[0] = periods1[0];
            pw1.Periods[1] = periods1[1];
            pw1.Periods[2] = periods1[2];


            IEnumerable<PowerTrade> powerTrades = new List<PowerTrade> { pw0, pw1 };
            TradeAggregationService_Sut.AggregateTrades(new List<PowerTrade>(), new DateTime(2021, 1, 1));
            var date = new DateTime(2021, 1, 1);

            //Act
            var service = new TradeAggregationService();
            var trades = service.AggregateTrades(powerTrades, date);

            //Assert
            Assert.Equal(expected1, trades[0].Volume);
            Assert.Equal(expected2, trades[1].Volume);
            Assert.Equal(expected3, trades[2].Volume);
        }
    }
}
