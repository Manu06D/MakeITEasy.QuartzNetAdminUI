# MakeITEasy.QuartzNetAdminUI

Provide a simple web interface on top of quartz.net. This interface allows you to check all jobs and their state.

You can individually trigger jobs and pause all of them.

There's rest endpoints that provide all jobs information (specifically their status) and the capabilities of pausing all of them. This fit in an automatic deployment scenario. 


![image](https://user-images.githubusercontent.com/23009730/222890085-e68d8e73-83fc-44a6-bf26-c6d5b930dae6.png)

## Configuration

Open your startup.cs file and add the endpoint.

```
using MakeITeasy.QuartzNetAdminUI;
...
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseEndpoints(async endpoints =>
            {
                ....
                endpoints.MapQuartzNetAdminUI(
                    options =>
                    {
                        options.UIPath = "/jobs";
                        options.PageTitle = "My scheduled jobs";
                    });
            });
        }

```
