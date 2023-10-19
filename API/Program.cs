using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace API
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            DotNetEnv.Env.Load();
            return CreateWebHostBuilder(args).Build().RunAsync();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5002";

            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:" + port)
                .UseStartup<Startup>();
        }

    }
}
