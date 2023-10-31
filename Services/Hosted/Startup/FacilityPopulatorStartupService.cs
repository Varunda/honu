using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    /// <summary>
    /// Runs once at startup, populating the wt_facility table. If an exception occurs, honu can run, so it's just logged
    /// </summary>
    public class FacilityPopulatorStartupService : BackgroundService {

        private readonly ILogger<FacilityPopulatorStartupService> _Logger;
        private readonly FacilityCollection _FacilityCollection;
        private readonly IFacilityDbStore _FacilityDb;
        private readonly MapCollection _MapCollection;
        private readonly IMapDbStore _MapDb;

        public FacilityPopulatorStartupService(ILogger<FacilityPopulatorStartupService> logger,
            FacilityCollection facCollection, IFacilityDbStore facDb,
            MapCollection mapColl, IMapDbStore mapDb) {

            _Logger = logger;

            _FacilityCollection = facCollection ?? throw new ArgumentNullException(nameof(facCollection));
            _FacilityDb = facDb ?? throw new ArgumentNullException(nameof(facDb));
            _MapCollection = mapColl;
            _MapDb = mapDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogDebug($"starting facility populator service");

            try {
                Stopwatch timer = Stopwatch.StartNew();

                List<PsFacility> facilities = await _FacilityCollection.GetAll();
                List<PsFacility> dbFacs = await _FacilityDb.GetAll();

                _Logger.LogInformation($"Census has {facilities.Count} facilities, DB has {dbFacs.Count} facilities");

                foreach (PsFacility fac in facilities) {
                    await _FacilityDb.Upsert(fac.FacilityID, fac);
                }

                List<PsFacilityLink> links = await _MapCollection.GetFacilityLinks();
                List<PsFacilityLink> dbLinks = await _MapDb.GetFacilityLinks();

                _Logger.LogInformation($"Census has {links.Count} facility links, DB has {dbLinks.Count} facility links");

                foreach (PsFacilityLink link in links) {
                    await _MapDb.UpsertLink(link);
                }

                List<PsMapHex> hexes = await _MapCollection.GetHexes();
                List<PsMapHex> dbHexes = await _MapDb.GetHexes();

                _Logger.LogInformation($"Census has {hexes.Count} hexes, DB has {dbHexes.Count} hexes");

                foreach (PsMapHex hex in hexes) {
                    await _MapDb.UpsertHex(hex);
                }

                _Logger.LogDebug($"Finished in {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to populate facility table");
            }
        }
    }
}
