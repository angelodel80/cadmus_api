﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using CadmusApi.Services;

namespace CadmusApi
{
    /// <summary>
    /// Program.
    /// </summary>
    public sealed class Program
    {
        private static void DumpEnvironmentVars()
        {
            Console.WriteLine("ENVIRONMENT VARIABLES:");
            IDictionary dct = Environment.GetEnvironmentVariables();
            List<string> keys = new List<string>();
            var enumerator = dct.GetEnumerator();
            while (enumerator.MoveNext())
            {
                keys.Add(((DictionaryEntry)enumerator.Current).Key.ToString());
            }

            foreach (string key in keys.OrderBy(s => s))
                Console.WriteLine($"{key} = {dct[key]}");
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSerilog();
                });

        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
#if DEBUG
                .WriteTo.File("cadmus-log.txt", rollingInterval: RollingInterval.Day)
#endif
                .CreateLogger();

            try
            {
                Log.Information("Starting host");
                DumpEnvironmentVars();

                // this is the place for seeding:
                // see https://stackoverflow.com/questions/45148389/how-to-seed-in-entity-framework-core-2
                // and https://docs.microsoft.com/en-us/aspnet/core/migration/1x-to-2x/?view=aspnetcore-2.1#move-database-initialization-code
                CreateHostBuilder(args)
                    .UseSerilog()
                    .Build()
                    .SeedAsync()     // see Services/HostSeedExtension
                    .Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                Debug.WriteLine(ex.ToString());
                Console.WriteLine(ex.ToString());
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
