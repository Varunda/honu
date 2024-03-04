using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Code.Hubs.Implementations;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Code.Hubs;
using watchtower.Code.Constants;
using watchtower.Code;
using Microsoft.AspNetCore.Mvc;
using watchtower.Code.Tracking;

namespace watchtower.Services {

    /// <summary>
    ///     Background service that updates the world data for the realtime view, but only for worlds that currently have people listening
    /// </summary>
    public class DataBuilderService : BackgroundService {

        private const int _RunDelay = 5; // seconds

        private const string SERVICE_NAME = "data_builder";

        private readonly ILogger<DataBuilderService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly WorldDataRepository _WorldDataRepository;

        private readonly DataBuilderRepository _DataBuilder;

        private List<short> _WorldIDs = new() {
            World.Connery, World.Cobalt, World.Emerald, World.Jaeger, World.Miller, World.SolTech
        };

        private List<int> _Durations = new() {
            60, 120
        };

        public DataBuilderService(ILogger<DataBuilderService> logger,
            DataBuilderRepository dataBuilder, WorldDataRepository worldDataRepo,
            IServiceHealthMonitor healthMon) {

            _Logger = logger;
            _ServiceHealthMonitor = healthMon ?? throw new ArgumentNullException(nameof(healthMon));

            _DataBuilder = dataBuilder ?? throw new ArgumentNullException(nameof(dataBuilder));
            _WorldDataRepository = worldDataRepo ?? throw new ArgumentNullException(nameof(worldDataRepo));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (!stoppingToken.IsCancellationRequested) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();

                    ServiceHealthEntry? entry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                    if (entry == null) {
                        entry = new ServiceHealthEntry() {
                            Name = SERVICE_NAME
                        };
                    }

                    // Useful for debugging on my laptop which can't handle running the queries and run vscode at the same time
                    if (entry.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    string msg = "";

                    foreach (int duration in _Durations) {
                        foreach (short worldID in _WorldIDs) {
                            lock (ConnectionStore.Get().Connections) {
                                int count = ConnectionStore.Get().Connections.Where(iter => iter.Value.WorldID == worldID && iter.Value.Duration == duration).Count();
                                if (count == 0) {
                                    msg += $"{worldID}#{duration} skipped; ";
                                    continue;
                                }
                            }

                            Stopwatch worldTime = Stopwatch.StartNew();

                            WorldData? data = _WorldDataRepository.Get(worldID, duration);
                            try {
                                data = await _DataBuilder.Build(worldID, duration, stoppingToken);
                                data.LastError = null;
                                msg += $"{worldID}#{duration} {worldTime.ElapsedMilliseconds}ms; ";
                            } catch (Exception ex) {
                                msg += $"{worldID}#{duration} {worldTime.ElapsedMilliseconds}ms ERRORED;";
                                if (data != null) {
                                    data.LastError = new ProblemDetails() {
                                        Detail = ex.ToString(),
                                        Title = $"error building {worldID}#{duration}",
                                        Type = ex.Message
                                    };
                                }
                                _Logger.LogError(ex, $"error while building realtime activity for {worldID}#{duration}");
                            }

                            if (data != null) {
                                _WorldDataRepository.Set(worldID, data);
                            }

                            if (stoppingToken.IsCancellationRequested) {
                                _Logger.LogDebug($"Stopping token sent, disabling early");
                                entry.Enabled = false;
                                break;
                            }

                            ServiceHealthEntry? iterEntry = _ServiceHealthMonitor.Get(SERVICE_NAME);
                            if (iterEntry != null && iterEntry.Enabled == false) {
                                _Logger.LogInformation($"{SERVICE_NAME} ended early");
                                entry.Enabled = false;
                                break;
                            }
                        }
                    }

                    long elapsedTime = timer.ElapsedMilliseconds;

                    entry.RunDuration = elapsedTime;
                    entry.LastRan = DateTime.UtcNow;
                    entry.Message = msg;
                    _ServiceHealthMonitor.Set(SERVICE_NAME, entry);

                    long timeToHold = (_RunDelay * 1000) - elapsedTime;

                    // Don't constantly run the data building, not useful, but if it does take awhile start building again so the data is recent
                    if (timeToHold > 5) {
                        await Task.Delay((int)timeToHold, stoppingToken);
                    }
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopped data builder service");
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Exception in DataBuilderService");
                }
            }
        }

    }
}
