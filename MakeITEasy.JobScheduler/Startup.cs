using MakeITeasy.QuartzNetAdminUI;

using MakeITEasy.JobScheduler.Jobs;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Quartz;

using System;

namespace MakeITEasy.JobScheduler
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 10;
                });

                q.ScheduleJob<HelloJob>(trigger => 
                    trigger
                        .WithIdentity("Combined Configuration Trigger")
                        .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(7)))
                        .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
                        .WithDescription("my awesome trigger configured for a job with single call"),
                    job => job
                        .UsingJobData("JobIndex", "1")
                        .WithIdentity("Hello Job", "group 1")
                    );

                q.ScheduleJob<HelloJob>(trigger => 
                    trigger
                        .WithIdentity("Sample 2")
                        .WithDailyTimeIntervalSchedule(x => x.WithInterval(60, IntervalUnit.Second))
                        .WithDescription("trigger 2"),
                    job => job
                        .UsingJobData("JobIndex", "2")
                        .WithIdentity("Hello Job", "group 2")
                    );

                q.ScheduleJob<HelloJob>(trigger =>
                    trigger
                        .WithIdentity("Job Sample 3")
                        .WithDailyTimeIntervalSchedule(x => x.WithInterval(3600, IntervalUnit.Second))
                        .WithDescription("trigger 3"),
                    job => job
                        .WithIdentity("Hello Job 3", "group 2")
                        .UsingJobData("JobIndex", "3")
                        .UsingJobData("Waiting", "10000")
                    );
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(async endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World! 2");
                });

                endpoints.MapQuartzNetAdminUI(
                    options =>
                    {
                        options.UIPath = "/jobs";
                        options.Scripts = @"
   <script src=""https://code.jquery.com/jquery-3.7.1.min.js""
            integrity=""sha256-/JqT3SQfawRcv/BIHPThkBvs0OEvtFFmqPF/lYI/Cxo=""
            crossorigin=""anonymous""></script>
    <script src=""https://cdnjs.cloudflare.com/ajax/libs/datatables/1.10.21/js/jquery.dataTables.min.js"" integrity=""sha512-BkpSL20WETFylMrcirBahHfSnY++H2O1W+UnEEO4yNIl+jI2+zowyoGJpbtk6bx97fBXf++WJHSSK2MV4ghPcg=="" crossorigin=""anonymous"" referrerpolicy=""no-referrer""></script>
";
                        //options.PageTitle = "Test XXXX";
                    });
            });
        }
    }
}
