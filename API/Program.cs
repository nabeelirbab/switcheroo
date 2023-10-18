﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace API
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();
            try
            {
                logger.Debug("Init main");
                DotNetEnv.Env.Load();
                return CreateWebHostBuilder(args).Build().RunAsync();
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Application stopped because of an exception.");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
                
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("SERVER_PORT") ?? "5002";

            var logger = NLog.LogManager.LoadConfiguration("NLog.config").GetCurrentClassLogger();

            try
            {
                logger.Debug("Init main");
                return WebHost.CreateDefaultBuilder(args)
                    .UseUrls("http://*:" + port)
                    .UseStartup<Startup>();
            }
            catch (Exception ex)
            {
                // Handle exceptions during host initialization
                logger.Error(ex, "Application stopped because of an exception.");
                throw;
            }
            finally
            {
                // Ensure to flush and stop NLog
                NLog.LogManager.Shutdown();
            }
        }

    }
}
