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
using watchtower.Models.Watchtower;
using watchtower.Realtime;

namespace watchtower.Services.Hosted {

    public class RealtimeNetworkBuilderService : BackgroundService {

        private const int _RunDelay = 5; // seconds

        private const string SERVICE_NAME = "network_builder";

        private readonly ILogger<RealtimeNetworkBuilderService> _Logger;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;
        private readonly IEventHandler _EventHandler;

        private readonly RealtimeNetworkBuilder _Builder;
        private readonly RealtimeNetworkRepository _Repository;

        public RealtimeNetworkBuilderService(ILogger<RealtimeNetworkBuilderService> logger,
            IServiceHealthMonitor serviceHealthMonitor, RealtimeNetworkBuilder builder,
            RealtimeNetworkRepository repository, IEventHandler eventHandler) {

            _Logger = logger;

            _ServiceHealthMonitor = serviceHealthMonitor;
            _Builder = builder;
            _Repository = repository;
            _EventHandler = eventHandler;
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

                    if (DateTime.UtcNow - _EventHandler.MostRecentProcess() > TimeSpan.FromMinutes(3)) {
                        msg = $"Realtime is behind {DateTime.UtcNow - _EventHandler.MostRecentProcess()} (which is greater than 3m), not running network builder";
                    } else {
                        foreach (short worldID in World.All) {
                            Stopwatch worldTime = Stopwatch.StartNew();
                            RealtimeNetwork data = await _Builder.Build(worldID);
                            msg += $"{worldID} {worldTime.ElapsedMilliseconds}ms; ";
                            _Repository.Set(worldID, data);

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
