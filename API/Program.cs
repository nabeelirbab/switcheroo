﻿using System;
using System.Threading.Tasks;
 using Infrastructure.Database;
 using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
 using Microsoft.EntityFrameworkCore;
 using Microsoft.Extensions.DependencyInjection;

 namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateWebHostBuilder(args).Build();
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SwitcherooContext>();
                    await db.Database.MigrateAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            await app.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5002";
            
            return WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:" + port)
                .UseStartup<Startup>();
        }
    }
}
