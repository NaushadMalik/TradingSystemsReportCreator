using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TradingSystems_ReportCreator;

namespace TradingSystem.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private Mock<IPowerService> mockPowerService;
        private ReportGenerator reportGenerator;

        [TestInitialize()]
        public void Startup()
        {
            mockPowerService = new Mock<IPowerService>();
        }

        [TestMethod]
        public async Task Test_ReportGeneratorAsync()
        {
            var date = DateTime.Parse("21/01/2022 20:49");

            var powerTrade_1 = PowerTrade.Create(date, 2);
            powerTrade_1.Periods[0].Volume = 999.9;
            powerTrade_1.Periods[1].Volume = 0.1;

            var powerTrade_2 = PowerTrade.Create(date, 2);
            powerTrade_2.Periods[0].Volume = 0.1;
            powerTrade_2.Periods[1].Volume = 9999.9;

            var powerTrades = Task.FromResult<IEnumerable<PowerTrade>>(new List<PowerTrade> { powerTrade_1, powerTrade_2 });

            // Convert object to Dictonary so as to compare and assert
            Dictionary<int, double> actualTrades = new Dictionary<int, double>();
            powerTrades.Result
                .SelectMany(x => x.Periods)
                .GroupBy(x => x.Period)
                .ToList()
                .ForEach(x => actualTrades[x.Key] = x.Sum(p => p.Volume));

            mockPowerService
                .Setup(x => x.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(powerTrades);

            reportGenerator = new ReportGenerator(mockPowerService.Object);
            await reportGenerator.RunAsync(date);

            //Read Values from generated CSV file so as to compare and assert
            Dictionary<int, double> expectedTrades = new Dictionary<int, double>();
            File.ReadLines($@"{ConfigurationManager.AppSettings["Location"]}/PowerPosition_{date:yyyyMMdd_HHmm}.csv").ToList().Skip(1).ToList().ForEach(x =>
            {
                var periodArray = x.Split(',');
                expectedTrades.Add(int.Parse(periodArray[0]), double.Parse(periodArray[1]));
            });

            // Assert
            actualTrades.Should().BeEquivalentTo(expectedTrades);
        }

        [TestMethod]
        public void Test_ReportGenerator()
        {
            reportGenerator = new ReportGenerator(new PowerService());
            reportGenerator.Run(System.DateTime.Now);
        }
    }
}
