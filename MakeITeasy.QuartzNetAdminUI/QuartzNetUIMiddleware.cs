using MakeITeasy.QuartzNetAdminUI.Helpers;
using MakeITeasy.QuartzNetAdminUI.Models;

using Microsoft.AspNetCore.Http;

using Quartz;
using Quartz.Impl.Triggers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using MakeITeasy.QuartzNetAdminUI.Extensions;

namespace MakeITeasy.QuartzNetAdminUI
{
    internal class QuartzNetUIMiddleware
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzNetUIMiddleware(RequestDelegate next, ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        private readonly Dictionary<string, Func<string, HttpContext, Task>> actionsByName = new()
        {
            { Const.QueryAction, async (resourceName, context) => await ResourceHandler(resourceName, context) }
        };

        private readonly static Dictionary<string, Func<string, ISchedulerFactory, Task<string>>> ApiActions = new()
        {
            { "GetJobs",  async (x, y) => await GetJobs(x, y) },
            { "runJob",   async (x, y) => await RunJob(x, y) },
            { "pauseJob", async (x, y) => await PauseJob(x, y) }
        };

        /// <summary>
        /// Here start the game
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var queries = context.Request.Query;

            var actionQuery = queries.FirstOrDefault(x => x.Key.Equals(Const.QueryAction, StringComparison.InvariantCultureIgnoreCase));

            //todo check HasValue
            string lookupQueryActionName = Const.ConstDictionnary[nameof(Const.ActionRessourceGet)];

            if (actionQuery.IsDefault())
            {
                await RenderDefaultPage(context);
            }
            else if (actionQuery.Value.FirstOrDefault()?.Equals(Const.ConstDictionnary[nameof(Const.ActionRessourceGet)], StringComparison.InvariantCultureIgnoreCase) == true)
            {
                await ResourceHandler(queries[Const.ActionRessourceNameGet], context);
            }
            else if (actionQuery.Value.FirstOrDefault()?.Equals(Const.ConstDictionnary[nameof(Const.ActionApiGet)], StringComparison.InvariantCultureIgnoreCase) == true)
            {
                await ApiHandler(queries[Const.ActionApiNameGet], context);
            }
        }

        private static async Task RenderDefaultPage(HttpContext context)
        {
            string indexFile = await FileHelper.GetResourceFileAsync("index.html");

            Dictionary<string, string> variables = new();

            foreach (var dicoValue in Const.ConstDictionnary)
            {
                variables.TryAdd(dicoValue.Key, dicoValue.Value);
            }

            if (!string.IsNullOrEmpty(indexFile))
            {
                indexFile = StringHelpers.ReplacePercent(indexFile, variables);

                await context.Response.WriteAsync(indexFile);
            }
        }

        private static async Task ResourceHandler(string ressourceName, HttpContext httpContext)
        {
            if (!string.IsNullOrEmpty(ressourceName))
            {
                httpContext.Response.ContentType = FileHelper.ContentTypeByExtension[ressourceName[ressourceName.LastIndexOf(".")..]];

                await httpContext.Response.WriteAsync(await FileHelper.GetResourceFileAsync(ressourceName));
            }
        }

        private async Task ApiHandler(string apiName, HttpContext httpContext)
        {
            if (! string.IsNullOrWhiteSpace(apiName))
            {
                var actionArgument = httpContext.Request.Query.FirstOrDefault(x => x.Key.Equals(Const.ActionApiArgementNameGet, StringComparison.InvariantCultureIgnoreCase)).Value.ToString();

                var output = await ApiActions[apiName].Invoke(actionArgument, _schedulerFactory);

                httpContext.Response.ContentType = FileHelper.ContentTypeByExtension[".json"];

                await httpContext.Response.WriteAsync(output);
            }
        }

        private static async Task<string> GetJobs(string argument, ISchedulerFactory schedulerFactory)
        {
            ScheduleInfo si = new();

            var scheduler = await schedulerFactory.GetScheduler();

            var executingJobs = await scheduler.GetCurrentlyExecutingJobs();

            List<TriggerInfo> triggers = await scheduler.GetJobs();

            si.Groups.AddRange(triggers.GroupBy(x => x.GroupName, (key, group) => new ScheduleGroupInfo() { Name = key, Jobs = group.ToList() }).ToList());

            var runningJobs = executingJobs.Select(x => ((AbstractTrigger)x.Trigger).Name);

            var runningTriggerInfo = si.Groups.SelectMany(x => x.Jobs.Where(y => runningJobs.Any(z => z.Equals(y.Name, StringComparison.InvariantCultureIgnoreCase))));

            foreach (var triggerInfo in runningTriggerInfo)
            {
                triggerInfo.Status = Status.Running;
            }

            return JsonSerializer.Serialize(si, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });
        }

        private static async Task<string> RunJob(string triggerKey, ISchedulerFactory schedulerFactory)
        {
            var scheduler = await schedulerFactory.GetScheduler();

            await scheduler.PerformActionOnITrigger(triggerKey, async (x, y) => await x.TriggerJob(y.JobKey, y.JobDataMap));

            return JsonSerializer.Serialize(new { Result = "ok" });
        }

        private static async Task<string> PauseJob(string triggerKey, ISchedulerFactory schedulerFactory)
        {
            var scheduler = await schedulerFactory.GetScheduler();

            await scheduler.PerformActionOnITrigger(triggerKey, async (x, y) => await x.PauseTrigger(y.Key));

            return JsonSerializer.Serialize(new { Result = "ok" });
        }
    }
}