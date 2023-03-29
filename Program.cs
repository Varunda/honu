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

        private static void PrintD(string name, DateTime d) {
            DateTimeOffset doo = new DateTimeOffset(d);
            Console.WriteLine($"{name} => {d:u} ({d.Kind}) {doo:u} ({doo.Offset}) {doo.ToUnixTimeSeconds()} {d.GetDiscordFullTimestamp()}");
        }

        public static async Task Main(string[] args) {
            Console.WriteLine($"Honu starting at {DateTime.UtcNow:u}");

            /* i have no tests so i put them here lol
            string[] dates = new string[] {
                "Thursday Feb 2, 01:00 - 03:00",
                "Thursday Feb 2, 01 - 03",
                "Thursday, Feb. 2, 01:00 - 03:00",
                "Thursday, Feb. 2nd, 01 - 03",
                "Thursday, Feb. 2nd, 01:00 - 03:00",
                "2023-02-02 01:00 - 03:00",
                "2023-02-02 01 - 03",
                "2023-02-02 1 - 3",
                "February 4th 19:00  - 21:00 UTC",
                "Thursday, February 2nd 19:00-21:00 UTC",
                "February 3rd @ 00:30-02:00 UTC",
                "Saturday, February 4th @ 1:30 - 3:30 UTC "
            };

            foreach (string d in dates) {
                (DateTime? start, DateTime? end) = PsbReservationRepository.ParseVeryInexact(d, out string feedback);
                if (start != null && end != null) {
                    Console.WriteLine($"{d} => {start:u} to {end:u}");
                }

                Console.WriteLine(feedback);
                Console.WriteLine("\n========================================================");
            }
            */

            /*
            string d = "1/28/2023 21:00:00";

            DateTime.TryParse(d, null, DateTimeStyles.AssumeLocal, out DateTime d1);
            DateTime.TryParse(d, null, DateTimeStyles.AssumeUniversal, out DateTime d2);
            DateTime.TryParse(d, out DateTime d3);
            DateTime d4 = DateTime.SpecifyKind(d3, DateTimeKind.Utc);
            DateTime d5 = DateTime.SpecifyKind(d3, DateTimeKind.Local);
            DateTime d6 = DateTime.SpecifyKind(d3, DateTimeKind.Unspecified);
            DateTime d7 = DateTime.SpecifyKind(d2, DateTimeKind.Utc);
            DateTime d8 = DateTime.SpecifyKind(d2, DateTimeKind.Local);
            DateTime d9 = DateTime.SpecifyKind(d2, DateTimeKind.Unspecified);

            PrintD("d1", d1);
            PrintD("d2", d2);
            PrintD("d3", d3);
            PrintD("d4", d4);
            PrintD("d5", d5);
            PrintD("d6", d6);
            PrintD("d7", d7);
            PrintD("d8", d8);
            PrintD("d9", d9);

            return;
            */

            bool hostBuilt = false;

            CancellationTokenSource stopSource = new();

            /*
            using TracerProvider? trace = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("npgsql"))
                .AddAspNetCoreInstrumentation(options => {
                    // only profile api calls
                    options.Filter = (c) => {
                        return c.Request.Path.StartsWithSegments("/api");
                    };
                })
                //.AddNpgsql()
                .AddJaegerExporter(config => {

                })
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(HonuActivitySource.ActivitySourceName))
                .AddSource(HonuActivitySource.ActivitySourceName)
                .Build();

            Task hostTask = CreateHostBuilder(args).RunConsoleAsync();
            */

            // Honu must be started in a background thread, as _Host.RunAsync will block until the whole server
            //      shuts down. If we were to await this Task, then it would be blocked until the server is done
            //      running, at which point then the command bus stuff would start
            //
            // That's not useful, because we want to be able to input commands while the server is running,
            //      not after the server is done running
            _ = Task.Run(async () => {
                ILogger<Program>? logger = null;
                try {
                    using TracerProvider? trace = Sdk.CreateTracerProviderBuilder()
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("npgsql"))
                        .AddAspNetCoreInstrumentation(options => {
                            // only profile api calls
                            options.Filter = (c) => {
                                return c.Request.Path.StartsWithSegments("/api");
                            };
                        })
                        //.AddNpgsql()
                        .AddJaegerExporter(config => {

                        })
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(HonuActivitySource.ActivitySourceName))
                        .AddSource(HonuActivitySource.ActivitySourceName)
                        .Build();

                    Stopwatch timer = Stopwatch.StartNew();

                    _Host = CreateHostBuilder(args).Build();
                    logger = _Host.Services.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
                    hostBuilt = true;
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
                    //await _Host.RunConsoleAsync();
                    await _Host.RunAsync(stopSource.Token);
                } catch (Exception ex) {
                    if (logger != null) {
                        logger.LogError(ex, $"error while running honu");
                    } else {
                        Console.WriteLine($"error while running honu:\n{ex}");
                    }
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

            ILogger<Program> logger = _Host.Services.GetRequiredService<ILogger<Program>>();

            CommandBus? commands = _Host.Services.GetService(typeof(CommandBus)) as CommandBus;
            if (commands == null) {
                logger.LogError($"missing CommandBus");
                Console.Error.WriteLine($"Missing ICommandBus");
            }

            logger.LogInformation($"ran host");
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
                //stopSource.CancelAfter(1 * 1000);
            } else {
                Console.WriteLine($"stopping without a token");
                //stopSource.Cancel();
                await _Host.StopAsync();
            }

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
