using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    /// <summary>
    ///     Runs once at startup, populating all the static directive databases
    /// </summary>
    public class ObjectiveCollectionsPopulator : BackgroundService {

        private readonly ILogger<ObjectiveCollectionsPopulator> _Logger;

        private readonly ObjectiveCollection _ObjectiveCensus;
        private readonly ObjectiveTypeCollection _ObjectiveTypeCensus;
        private readonly ObjectiveSetCollection _ObjectiveSetCensus;

        private readonly ObjectiveDbStore _ObjectiveDb;
        private readonly ObjectiveTypeDbStore _ObjectiveTypeDb;
        private readonly ObjectiveSetDbStore _ObjectiveSetDb;

        public ObjectiveCollectionsPopulator(ILogger<ObjectiveCollectionsPopulator> logger,
            ObjectiveCollection objCensus, ObjectiveDbStore objDb,
            ObjectiveTypeCollection objTypeCensus, ObjectiveTypeDbStore objTypeDb,
            ObjectiveSetCollection objSetCensus, ObjectiveSetDbStore objSetDb) {

            _Logger = logger;

            _ObjectiveCensus = objCensus ?? throw new ArgumentNullException(nameof(objCensus));
            _ObjectiveDb = objDb ?? throw new ArgumentNullException(nameof(objDb));
            _ObjectiveTypeCensus = objTypeCensus ?? throw new ArgumentNullException(nameof(objTypeCensus));
            _ObjectiveTypeDb = objTypeDb ?? throw new ArgumentNullException(nameof(objTypeDb));
            _ObjectiveSetCensus = objSetCensus ?? throw new ArgumentNullException(nameof(objSetCensus));
            _ObjectiveSetDb = objSetDb ?? throw new ArgumentNullException(nameof(objSetDb));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                List<PsObjective> censusObjs = await _ObjectiveCensus.GetAll();
                List<PsObjective> dbObjs = await _ObjectiveDb.GetAll();

                _Logger.LogDebug($"Objectives: got {censusObjs.Count} from Census, have {dbObjs.Count} in DB");
                if (censusObjs.Count > dbObjs.Count) {
                    foreach (PsObjective dir in censusObjs) {
                        await _ObjectiveDb.Upsert(dir);
                    }
                }

                List<ObjectiveType> censusTypes = await _ObjectiveTypeCensus.GetAll();
                List<ObjectiveType> dbTypes = await _ObjectiveTypeDb.GetAll();

                _Logger.LogDebug($"ObjectiveTypes: got {censusTypes.Count} from Census, have {dbTypes.Count} in DB");
                if (censusTypes.Count > dbTypes.Count) {
                    foreach (ObjectiveType type in censusTypes) {
                        await _ObjectiveTypeDb.Upsert(type);
                    }
                }

                List<ObjectiveSet> censusSets = await _ObjectiveSetCensus.GetAll();
                List<ObjectiveSet> dbSets = await _ObjectiveSetDb.GetAll();

                _Logger.LogDebug($"ObjectiveSets: got {censusSets.Count} from Census, have {dbSets.Count} in DB");
                if (censusSets.Count > dbSets.Count) {
                    foreach (ObjectiveSet type in censusSets) {
                        await _ObjectiveSetDb.Upsert(type);
                    }
                }

                _Logger.LogDebug($"Finished objective populator in {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to populate directive tables");
            }
        }

    }
}
