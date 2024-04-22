using ReportGeneratorLogic.Services;

namespace ReportGeneratorTests
{
    public class PowerTradeServiceTest
    {
        private readonly PowerTradeService PowerTradeService_Sut;

        public PowerTradeServiceTest()
        {
            PowerTradeService_Sut = new PowerTradeService();
        }

        [Fact]
        public void Test1()
        {
            //Arrange
            var date = new DateTime(2021, 1, 1);

            //Act
            var service = new PowerTradeService();
            var trades = service.GetTradesAsync(date);

            //Assert
            Assert.Equal(24, trades.Result.First().Periods.Count());
            Assert.Equal(date, trades.Result.First().Date);

        }
    }
}