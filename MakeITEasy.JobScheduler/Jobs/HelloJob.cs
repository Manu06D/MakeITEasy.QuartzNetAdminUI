using Microsoft.Extensions.Logging;

using Quartz;

using System;
using System.Threading.Tasks;

namespace MakeITEasy.JobScheduler.Jobs
{
    public class HelloJob : IJob
    {
        private readonly ILogger<HelloJob> _logger;

        public HelloJob(ILogger<HelloJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            if(!int.TryParse(dataMap.GetString("Waiting"), out int millisecondsDelay))
            {
                millisecondsDelay = new Random().Next(5000, 50000);
            }

            _logger.LogInformation($"starting job {dataMap.GetString("JobIndex")}. waiting for {millisecondsDelay}");

            await Task.Delay(millisecondsDelay);
            
            _logger.LogInformation("Finishing Hello Job");
        }
    }
}