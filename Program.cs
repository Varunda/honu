using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using watchtower.Code;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Services;

namespace watchtower {

    public class Program {

        // Is set in main
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private static IHost _Host;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public static async Task Main(string[] args) {
            Console.WriteLine($"Honu starting at {DateTime.UtcNow:u}");

            bool hostBuilt = false;

            // Honu must be started in a background thread, as _Host.RunAsync will block until the whole server
            //      shuts down. If we were to await this Task, then it would be blocked until the server is done
            //      running, at which point then the command bus stuff would start
            //
            // That's not useful, because we want to be able to input commands while the server is running,
            //      not after the server is done running
            _ = Task.Run(async () => {
                try {
                    using TracerProvider trace = Sdk.CreateTracerProviderBuilder()
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("npgsql"))
                        .AddNpgsql()
                        .AddJaegerExporter(config => {

                        })
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(HonuActivitySource.ActivitySourceName))
                        .AddSource(HonuActivitySource.ActivitySourceName)
                        .Build();

                    Stopwatch timer = Stopwatch.StartNew();
                    _Host = CreateHostBuilder(args).Build();
                    hostBuilt = true;
                    Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms to build Honu");
                    timer.Stop();
                    await _Host.RunAsync();
                } catch (Exception ex) {
                    Console.WriteLine($"Fatal error starting Honu:\n{ex}");
                }
            });

            for (int i = 0; i < 10; ++i) {
                await Task.Delay(1000);
                if (hostBuilt == true) {
                    break;
                }
            }

            if (_Host == null) {
                Console.Error.WriteLine($"FATAL> _Host was null after construction");
                return;
            }

            CommandBus? commands = _Host.Services.GetService(typeof(CommandBus)) as CommandBus;
            if (commands == null) {
                Console.Error.WriteLine($"Missing ICommandBus");
            }

            Console.WriteLine($"Ran host");

            string? line = "";
            bool fastStop = false;
            while (line != ".close") {
                line = Console.ReadLine();

                if (line == ".close" || line == ".closefast") {
                    if (line == ".closefast") {
                        fastStop = true;
                    }
                    break;
                } else {
                    if (line != null && commands != null) {
                        await commands.Execute(line);
                    }
                }
            }

            CancellationTokenSource cts = new CancellationTokenSource();
            if (fastStop == true) {
                cts.CancelAfter(1000 * 1);
            } else {
                cts.CancelAfter(1000 * 60);
            }

            await _Host.StopAsync(cts.Token);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            IHostBuilder? host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    // i don't like any of the provided default loggers
                    logging.AddConsole(options => options.FormatterName = "HonuLogger")
                        .AddConsoleFormatter<HonuLogger, HonuFormatterOptions>(options => {

                        });
                })
                .ConfigureAppConfiguration(appConfig => {
                    appConfig.AddUserSecrets<Startup>();
                }).ConfigureWebHostDefaults(webBuilder => {
                    webBuilder.UseStartup<Startup>();
                });

            return host;
        }
    }
}
