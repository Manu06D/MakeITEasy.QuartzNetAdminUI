using MakeITeasy.QuartzNetAdminUI.Models;

using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakeITeasy.QuartzNetAdminUI.Extensions
{
    public static class ISchedulerFactoryExtension
    {
        public static async Task PerformActionOnITrigger(this IScheduler scheduler, string triggerKey, Action<IScheduler, ITrigger> actionToPerform)
        {
            foreach (var group in await scheduler.GetJobGroupNames())
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupContains(group));

                foreach (var jobKey in jobKeys)
                {
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);

                    var trigger = triggers.FirstOrDefault(x => x.Key.ToString().Equals(triggerKey, StringComparison.InvariantCultureIgnoreCase));

                    if (trigger != null)
                    {
                        actionToPerform(scheduler, trigger);
                    }
                }
            }
        }

        public static async Task<List<TriggerInfo>> GetTriggersInfo(this IScheduler scheduler)
        {
            List<TriggerInfo> result = new();

            foreach (var group in await scheduler.GetJobGroupNames())
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupContains(group));

                foreach (var jobKey in jobKeys)
                {
                    //var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);

                    foreach (ITrigger trigger in triggers)
                    {
                        result.Add(new TriggerInfo()
                        {
                            Name = ((AbstractTrigger)trigger).Name,
                            GroupName = group,
                            Description = trigger?.Description,
                            Key = trigger.Key.ToString(),
                            Status = GetStatusFromTriggerStatus(scheduler.GetTriggerState(trigger.Key).Result),
                            LastExecution = trigger.GetPreviousFireTimeUtc().GetValueOrDefault(),
                            NextExecution = trigger.GetNextFireTimeUtc().GetValueOrDefault(),
                        });
                    }
                }
            }

            return result;
        }

        private static Status GetStatusFromTriggerStatus(TriggerState input) =>
        input switch
        {
            TriggerState.Paused => Status.Paused,
            _ => Status.Waiting
        };
    }
}