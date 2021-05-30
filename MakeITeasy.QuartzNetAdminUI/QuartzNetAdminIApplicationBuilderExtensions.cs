using MakeITeasy.QuartzNetAdminUI.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MakeITeasy.QuartzNetAdminUI
{
    public static class QuartzNetAdminIApplicationBuilderExtensions
    {
        private readonly static Dictionary<string, string> ContentTypeByExtension = new()
        {
            { ".js", "text/javascript" },
            { ".css", "text/css" },
            { ".json", "application/json" }
        };

        private readonly static Dictionary<string, Func<string, ISchedulerFactory, Task<string>>> ApiActions = new()
        {
            { "GetJobs", async (x, y) => await GetJobs(x, y) },
            { "RunJob", async (x, y) => await RunJob(x, y) }
        };

        private static readonly string DynamicStringPrefix = "%%";

        private static async Task<string> GetJobs(string argument, ISchedulerFactory schedulerFactory)
        {
            ScheduleInfo si = new();

            var scheduler = await schedulerFactory.GetScheduler();

            var executingJobs = await scheduler.GetCurrentlyExecutingJobs();

            List<TriggerInfo> triggers = new();

            await PerformActionOnTriggers(schedulerFactory, (groupName, trigger) => triggers.Add(new TriggerInfo()
            {
                Name = ((AbstractTrigger)trigger).Name,
                GroupName = groupName,
                Description = trigger?.Description,
                Key = trigger.Key.ToString(),
                LastExecution = trigger.GetPreviousFireTimeUtc().GetValueOrDefault(),
                NextExecution = trigger.GetNextFireTimeUtc().GetValueOrDefault(),
            }));

            si.Groups.AddRange(triggers.GroupBy(x => x.GroupName, (key, group) => new ScheduleGroupInfo() { Name = key, Jobs = group.ToList() }).ToList());

            var runningJobs = executingJobs.Select(x => ((AbstractTrigger)x.Trigger).Name);

            var runningTriggerInfo = si.Groups.SelectMany(x => x.Jobs.Where(y => runningJobs.Any(z => z.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase))));
            
            foreach(var triggerInfo in runningTriggerInfo)
            {
                triggerInfo.Status = Status.Running;
            }

            return JsonSerializer.Serialize(si, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)}
            });
        }


        private static async Task PerformActionOnTriggers(ISchedulerFactory schedulerFactory, Action<string, ITrigger> action)
        {
            var scheduler = await schedulerFactory.GetScheduler();
            var jobGroups = await scheduler.GetJobGroupNames();

            foreach (var group in jobGroups)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = await scheduler.GetJobKeys(groupMatcher);

                foreach (var jobKey in jobKeys)
                {
                    var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);

                    foreach (ITrigger trigger in triggers)
                    {
                        action(group, trigger);
                    }
                }
            }
        }

        private static async Task<string> RunJob(string triggerKey, ISchedulerFactory schedulerFactory)
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var jobGroups = await scheduler.GetJobGroupNames();

            foreach (var group in jobGroups)
            {
                ScheduleGroupInfo sgi = new();
                sgi.Name = group;

                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobKeys = await scheduler.GetJobKeys(groupMatcher);

                foreach (var jobKey in jobKeys)
                {
                    var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);

                    var trigger = triggers.FirstOrDefault(x => x.Key.ToString().Equals(triggerKey, StringComparison.InvariantCultureIgnoreCase));

                    if(trigger != null)
                    {
                        await scheduler.TriggerJob(trigger.JobKey, trigger.JobDataMap);
                    }
                }
            }

            return JsonSerializer.Serialize(new { Result = "ok" });
        }

        public static async Task MapQuartzNetAdminUI(this IEndpointRouteBuilder builder, Action<QuartzNetAdminUIOptions> setupOptions = null)
        {
            //https://dev.to/nikiforovall/asp-net-core-endpoints-add-endpoint-enabled-middleware-by-using-iendpointroutebuilder-extension-method-2e52

            var options = new QuartzNetAdminUIOptions();
            setupOptions?.Invoke(options);

            builder.Map("/quartzAdmin-ressources", async context =>
            {
                var getQueryParam = context.Request.Query.FirstOrDefault(x => x.Key.Equals(Const.RessourceGetParameter, StringComparison.InvariantCultureIgnoreCase));

                string requestedFileName = getQueryParam.Value.ToString();

                if (!string.IsNullOrEmpty(requestedFileName))
                {
                    context.Response.ContentType = ContentTypeByExtension[requestedFileName[requestedFileName.LastIndexOf(".")..]];

                    await context.Response.WriteAsync(await GetResourceFileAsync(getQueryParam.Value));
                }
            });

            builder.Map("/quartzAdmin-api", async context =>
            {
                var getQueryParam = context.Request.Query.FirstOrDefault(x => x.Key.Equals(Const.APIActionName, StringComparison.InvariantCultureIgnoreCase));

                string apiActionName = getQueryParam.Value.ToString();

                if (!string.IsNullOrEmpty(apiActionName))
                {
                    var actionArgument = context.Request.Query.FirstOrDefault(x => x.Key.Equals(Const.APIArgumentName, StringComparison.InvariantCultureIgnoreCase)).Value.ToString();

                    var output = await ApiActions[apiActionName].Invoke(actionArgument, context.RequestServices.GetRequiredService<ISchedulerFactory>());

                    context.Response.ContentType = ContentTypeByExtension[".json"];

                    await context.Response.WriteAsync(output);
                }
            });

            builder.Map(options.UIPath, async context =>
            {
                string indexFile = await QuartzNetAdminIApplicationBuilderExtensions.GetResourceFileAsync("index.html");

                Dictionary<string, string> variables = new();

                foreach (var dicoValue in Const.ConstDictionnary)
                {
                    variables.TryAdd(dicoValue.Key, dicoValue.Value);
                }

                if (!string.IsNullOrEmpty(indexFile))
                {
                    indexFile = ReplacePercent(indexFile, variables);

                    await context.Response.WriteAsync(indexFile);
                }
            });

            try
            {
                await RegisterJobListener(builder);
            }
            catch (Exception e)
            {
                string s = e.Message;
            }
        }

        private static async Task RegisterJobListener(IEndpointRouteBuilder builder)
        {
            var schedulerFactory = builder.ServiceProvider.GetService<ISchedulerFactory>();

            if (schedulerFactory != null)
            {
                var logger = builder.ServiceProvider.GetService<ILogger<QuartzNetAdminJobListener>>();
                var schedulers = await schedulerFactory.GetAllSchedulers();

                foreach (var scheduler in schedulers.ToList())
                {
                    scheduler.ListenerManager.AddJobListener(new QuartzNetAdminJobListener(logger), GroupMatcher<JobKey>.AnyGroup());
                }
            }
        }

        public async static Task<string> GetResourceFileAsync(string fileName)
        {
            var assembly = typeof(QuartzNetAdminIApplicationBuilderExtensions).Assembly;

            string assemblyName = assembly.GetName().Name;

            string assetsPath = $"{assemblyName}.assets.";

            string[] resourceNames = assembly.GetManifestResourceNames();

            string indexResourceFile = resourceNames.FirstOrDefault(x => x[assetsPath.Length..].Equals(fileName));

            if (!string.IsNullOrEmpty(indexResourceFile))
            {
                using (var stream = assembly.GetManifestResourceStream(indexResourceFile))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }

            return string.Empty;
        }

        private static string ReplacePercent(string input, Dictionary<string, string> values, int startIndex = 0)
        {
            var index1 = input.IndexOf(DynamicStringPrefix, startIndex);

            if (index1 > startIndex)
            {
                var index2 = input.IndexOf(DynamicStringPrefix, index1 + 1);
                if (index2 > index1)
                {
                    var output = input.Substring(0, index1);
                    output += values[input.Substring(index1 + DynamicStringPrefix.Length, index2 - index1 - DynamicStringPrefix.Length)];
                    output += input[(index2 + DynamicStringPrefix.Length)..];

                    return ReplacePercent(output, values, index2);
                }
            }

            return input;
        }
    }
}
