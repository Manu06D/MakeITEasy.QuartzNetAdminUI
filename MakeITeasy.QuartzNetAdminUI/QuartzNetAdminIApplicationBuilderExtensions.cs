using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using System;

namespace MakeITeasy.QuartzNetAdminUI
{
    public static class QuartzNetAdminIApplicationBuilderExtensions
    {
        public static void MapQuartzNetAdminUI(this IEndpointRouteBuilder builder, Action<QuartzNetAdminUIOptions> setupOptions = null)
        {
            var options = new QuartzNetAdminUIOptions();
            setupOptions?.Invoke(options);

            var apiDelegate =
                    builder.CreateApplicationBuilder()
                        //.UseAuthentication()
                        .UseMiddleware<QuartzNetUIMiddleware>()
                        .Build();

            builder.Map(options.UIPath, apiDelegate);
        }
    }
}