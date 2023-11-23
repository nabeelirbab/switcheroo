using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetEnv.Env.Load();

            // Run the web host asynchronously
            var host = CreateWebHostBuilder(args).Build();
            await host.StartAsync();

            // Create a connection to the SignalR hub
            var hubConnection = new HubConnectionBuilder()
                .WithUrl("http://13.50.221.83/chatHub") // Replace with your hub URL
                .Build();
            Console.WriteLine($"<--------Get URL-------->");

            await hubConnection.StartAsync();

            Console.WriteLine($"<--------connection established-------->");

            // Close the connection when done
            await hubConnection.DisposeAsync();

            await host.WaitForShutdownAsync();

            // return CreateWebHostBuilder(args).Build().RunAsync();
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
