using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;
using System;

namespace API.GraphQL
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            await context.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var requestBody = Encoding.UTF8.GetString(buffer);
            context.Request.Body.Position = 0;

            Console.WriteLine($"Request Body: {requestBody}");

            await _next(context);
        }
    }
}
