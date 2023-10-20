using Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace API
{
    public static class MiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILoggerManager logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        logger.LogError($"Something went wrong at {contextFeature.Endpoint?.DisplayName}: {contextFeature.Error?.Message}, StackTrace: {contextFeature.Error?.StackTrace}");
                        await context.Response.WriteAsync(new
                        {
                            context.Response.StatusCode,
                            contextFeature.Error?.Message,
                            contextFeature.Error?.Source,
                            contextFeature.Error?.StackTrace,
                            Enpoint = contextFeature.Endpoint?.DisplayName,
                            InnerException = contextFeature.Error?.InnerException?.Message
                        }.ToString());
                    }
                });
            });
        }
    }
}
