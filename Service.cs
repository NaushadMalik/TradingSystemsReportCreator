using Services;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading.Tasks;
using TradingSystems_ReportCreator;

namespace TradingSystems
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var intervalInMinutes = int.Parse(ConfigurationManager.AppSettings["Interval"]);
            RunTask(intervalInMinutes);
        }

        static void RunTask(int minutes)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    IPowerService powerService = new PowerService();
                    var reportGenerator = new ReportGenerator(powerService);

                    await reportGenerator.RunAsync(DateTime.Now);

                    await Task.Delay(TimeSpan.FromMinutes(minutes));
                }
            });
        }

        protected override void OnStop()
        {
        }
    }
}
