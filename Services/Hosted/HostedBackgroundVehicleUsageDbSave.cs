using Google.Apis.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models.Api;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundVehicleUsageDbSave : BackgroundService {

        private readonly ILogger<HostedBackgroundVehicleUsageDbSave> _Logger;
        private readonly VehicleUsageRepository _VehicleUsageRepository;
        private readonly VehicleUsageDbStore _VehicleUsageDb;

        public HostedBackgroundVehicleUsageDbSave(ILogger<HostedBackgroundVehicleUsageDbSave> logger,
            VehicleUsageRepository vehicleUsageRepository, VehicleUsageDbStore vehicleUsageDb) {

            _Logger = logger;
            _VehicleUsageRepository = vehicleUsageRepository;
            _VehicleUsageDb = vehicleUsageDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"starting background service");

            while (stoppingToken.IsCancellationRequested == false) {

                Stopwatch timer = Stopwatch.StartNew();
                int skipped = 0;
                int inserted = 0;
                try {

                    foreach (short worldID in World.PcStreams) {
                        foreach (uint zoneID in Zone.StaticZones) {
                            stoppingToken.ThrowIfCancellationRequested();

                            VehicleUsageData usage = await _VehicleUsageRepository.Get(worldID, zoneID, false);

                            if (usage.Total == 0) {
                                ++skipped;
                                continue;
                            }

                            try {
                                await _VehicleUsageDb.Insert(usage, stoppingToken);
                                ++inserted;
                            } catch (Exception ex2) {
                                _Logger.LogError(ex2, $"failed to insert vehicle usage [worldID={worldID}] [zoneID={zoneID}]");
                            }
                        }
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to perform vehicle usage db save");
                }

                _Logger.LogInformation($"vehicle usage data saved [timer={timer.ElapsedMilliseconds}ms] [skipped={skipped}] [inserted={inserted}]");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

    }
}
