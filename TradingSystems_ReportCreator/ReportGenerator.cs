using log4net;
using Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TradingSystems_FileWriter;

namespace TradingSystems_ReportCreator
{
    public interface IReportGenerator
    {
        void Run(DateTime date);
        Task RunAsync(DateTime date);
    }
    public class ReportGenerator : IReportGenerator
    {
        private IPowerService powerService = null;
        private readonly string location = ConfigurationManager.AppSettings["Location"];
        private IWriter fileWriter;
        private static readonly ILog log = LogManager.GetLogger(typeof(IReportGenerator));

        public ReportGenerator(IPowerService powerService)
        {
            log4net.Config.XmlConfigurator.Configure();
            this.powerService = powerService;
        }

        public void Run(DateTime date)
        {
            try
            {
                log.Info($"Report Generation started for {date}");
                //Get Power Trades
                var powerTrades = GetTrades(date);

                // Aggregate Positions
                var tradePositions = AggregatePosition(powerTrades);

                // Build CSV File
                fileWriter = new CSVWriter();
                fileWriter.Write(tradePositions, Path.Combine(location, $"PowerPosition_{date:yyyyMMdd_HHmm}.csv"));

                log.Info($"Report Generation finished for {date}");
            }
            catch (Exception e)
            {
                log.Error($"Report Generation Exception for {date}", e);
            }
        }

        public async Task RunAsync(DateTime date)
        {
            try
            {
                log.Info($"Report Generation started for {date}");

                //Get Power Trades
                var powerTrades = await GetTradesAsync(date);

                // Aggregate Positions
                var tradePositions = AggregatePosition(powerTrades);

                log.Info($"Trades received for {date}, AggregatePositions taken care off and creating CSV file.");

                // Build CSV File
                fileWriter = new CSVWriter();
                fileWriter.Write(tradePositions, Path.Combine(location, $"PowerPosition_{date:yyyyMMdd_HHmm}.csv"));

                log.Info($"Report Generation finished for {date}");
            }
            catch (Exception e)
            {
                log.Error($"Report Generation Exception for {date}", e);
            }
        }

        private async Task<IEnumerable<PowerTrade>> GetTradesAsync(DateTime date)
        {
            return await powerService.GetTradesAsync(date);
        }

        private IEnumerable<PowerTrade> GetTrades(DateTime date)
        {
            return powerService.GetTrades(date);
        }

        private Dictionary<int, double> AggregatePosition(IEnumerable<PowerTrade> powerTrades)
        {
            Dictionary<int, double> tradePositions = new Dictionary<int, double>();
            powerTrades
                .SelectMany(x => x.Periods)
                .GroupBy(x => x.Period)
                .ToList()
                .ForEach(x => tradePositions[x.Key] = x.Sum(p => p.Volume));

            return tradePositions;
        }
    }
}
