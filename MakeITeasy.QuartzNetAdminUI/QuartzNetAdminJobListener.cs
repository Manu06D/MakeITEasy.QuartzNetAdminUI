using Microsoft.Extensions.Logging;

using Quartz;

using System.Threading;
using System.Threading.Tasks;

namespace MakeITeasy.QuartzNetAdminUI
{
    public class QuartzNetAdminJobListener : IJobListener
    {
        private readonly ILogger<QuartzNetAdminJobListener> _logger;

        public QuartzNetAdminJobListener(ILogger<QuartzNetAdminJobListener> logger)
        {
            _logger = logger;
        }

        public string Name => "MyJobLister";

        public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("job cancelled");

            return Task.CompletedTask;
        }

        public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("job going to be executed");
            return Task.CompletedTask;
        }

        public Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("job executed");
            return Task.CompletedTask;
        }
    }
}
