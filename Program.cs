using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using watchtower.Code;
using watchtower.Code.DiscordInteractions;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Services;
using watchtower.Services.Repositories.PSB;

namespace watchtower {

    public class Program {

        // Is set in main
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        private static IHost _Host;
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public static async Task Main(string[] args) {
            Console.WriteLine($"honu starting [timestamp={DateTime.UtcNow:u}] [cwd={Environment.CurrentDirectory}] [CLR version={Environment.Version}]");

            // mitigation for https://github.com/advisories/GHSA-5crp-9r3c-p9vr
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings() {
                MaxDepth = 128
            };

            // see below, where Task.Run calls CreateHostBuilder for why this variable is used
            bool hostBuilt = false;

            CancellationTokenSource stopSource = new();

            using TracerProvider? trace = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("npgsql"))
                .AddAspNetCoreInstrumentation(options => {
                    // only profile api calls
                    options.Filter = (c) => {
                        return c.Request.Path.StartsWithSegments("/api");
                    };
                })
                .AddJaegerExporter(config => {

                })
                // add our activity source
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(HonuActivitySource.ActivitySourceName))
                .AddSource(HonuActivitySource.ActivitySourceName)
                .Build();

            // Honu must be started in a background thread, as _Host.RunAsync will block until the whole server
            //      shuts down. If we were to await this Task, then it would be blocked until the server is done
            //      running, at which point then the command bus stuff would start
            //
            // That's not useful, because we want to be able to input commands while the server is running,
            //      not after the server is done running
            _ = Task.Run(async () => {
                ILogger<Program>? logger = null;
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    _Host = CreateHostBuilder(args).Build();
                    logger = _Host.Services.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
                    Console.WriteLine($"Took {timer.ElapsedMilliseconds}ms to build Honu");
                    timer.Stop();
                } catch (Exception ex) {
                    if (logger != null) {
                        logger.LogError(ex, "fatal error starting Honu");
                    } else {
                        Console.WriteLine($"Fatal error starting Honu:\n{ex}");
                    }
                }

                try {
                    await _Host.RunAsync(stopSource.Token);
                    hostBuilt = true;
                } catch (Exception ex) {
                    if (logger != null) {
                        logger.LogError(ex, $"error while running honu");
                    } else {
                        Console.WriteLine($"error while running honu:\n{ex}");
                    }
                }
            });

            // wait for the host to be started, max 10 seconds
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

            ILogger<Program> logger = _Host.Services.GetRequiredService<ILogger<Program>>();

            CommandBus? commands = _Host.Services.GetService(typeof(CommandBus)) as CommandBus;
            if (commands == null) {
                logger.LogError($"missing CommandBus");
                Console.Error.WriteLine($"Missing ICommandBus");
            }

            // print both incase the logger is misconfigured or something
            logger.LogInformation($"ran host");
            Console.WriteLine($"ran host");

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
                    if (commands == null) {
                        logger.LogError($"Missing {nameof(CommandBus)} from host, cannot execute '{line}'");
                        Console.Error.WriteLine($"Missing {nameof(CommandBus)} from host, cannot execute '{line}'");
                    }
                    if (line != null && commands != null) {
                        await commands.Execute(line);
                    }
                }
            }

            if (fastStop == true) {
                logger.LogInformation($"stopping from 1'000ms");
                Console.WriteLine($"stopping after 1'000ms");

                CancellationTokenSource cts = new();
                cts.CancelAfter(1000 * 1);
                await _Host.StopAsync(cts.Token);
            } else {
                Console.WriteLine($"stopping without a token");
                await _Host.StopAsync();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) {
            IHostBuilder? host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => {
                    // replace the logger with a one line logger instead
                    logging.AddConsole(options => options.FormatterName = "HonuLogger")
                        .AddConsoleFormatter<HonuLogger, HonuFormatterOptions>(options => { });
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
