using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Db;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    public class HostedBackgroundWeaponPercentileCacheQueue : BackgroundService {

        private const string SERVICE_NAME = "background_weapon_pcache";

        private readonly ILogger<HostedBackgroundWeaponPercentileCacheQueue> _Logger;
        private readonly IBackgroundWeaponPercentileCacheQueue _Queue;

        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;

        public HostedBackgroundWeaponPercentileCacheQueue(ILogger<HostedBackgroundWeaponPercentileCacheQueue> logger,
            IBackgroundWeaponPercentileCacheQueue queue, IWeaponStatPercentileCacheDbStore percentDb) { 

            _Logger = logger;

            _Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            _PercentileDb = percentDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started {SERVICE_NAME}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    string itemID = await _Queue.Dequeue(stoppingToken);
                    _Logger.LogTrace($"{SERVICE_NAME}> Generating {itemID}");

                    WeaponStatPercentileCache? kd = await _PercentileDb.GenerateKd(itemID);
                    if (kd != null) {
                        kd.TypeID = PercentileCacheType.KD;
                        await _PercentileDb.Upsert(itemID, kd);
                    }

                    WeaponStatPercentileCache? kpm = await _PercentileDb.GenerateKpm(itemID);
                    if (kpm != null) {
                        kpm.TypeID = PercentileCacheType.KPM;
                        await _PercentileDb.Upsert(itemID, kpm);
                    }

                    WeaponStatPercentileCache? acc = await _PercentileDb.GenerateAcc(itemID);
                    if (acc != null) {
                        acc.TypeID = PercentileCacheType.ACC;
                        await _PercentileDb.Upsert(itemID, acc);
                    }

                    WeaponStatPercentileCache? hsr = await _PercentileDb.GenerateHsr(itemID);
                    if (hsr != null) {
                        hsr.TypeID = PercentileCacheType.HSR;
                        await _PercentileDb.Upsert(itemID, hsr);
                    }

                    _Logger.LogInformation($"{SERVICE_NAME}> Generated percentile data for {itemID}");

                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, $"{SERVICE_NAME}> Error while generated weapon percentiles");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping {SERVICE_NAME} with {_Queue.Count()} left");
                }
            }
        }


    }
}
