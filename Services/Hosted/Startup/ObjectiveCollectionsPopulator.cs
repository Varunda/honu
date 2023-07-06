using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;
using watchtower.Services.Repositories.Static;

namespace watchtower.Services.Hosted.Startup {

    /// <summary>
    ///     Runs once at startup, populating all the static directive databases
    /// </summary>
    public class ObjectiveCollectionsPopulator : BackgroundService {

        private readonly ILogger<ObjectiveCollectionsPopulator> _Logger;
        private readonly IServiceProvider _Services;

        private readonly ObjectiveCollection _ObjectiveCensus;
        private readonly ObjectiveTypeCollection _ObjectiveTypeCensus;
        private readonly ObjectiveSetCollection _ObjectiveSetCensus;
        private readonly AchievementCollection _AchievementCensus;
        private readonly ItemCollection _ItemCensus;
        private readonly VehicleCollection _VehicleCensus;
        private readonly ExperienceTypeCollection _ExpTypeCensus;
        private readonly FireGroupToFireModeCollection _FireGroupCensus;
        private readonly ItemCategoryCollection _ItemCategoryCensus;

        private readonly ObjectiveDbStore _ObjectiveDb;
        private readonly ObjectiveTypeDbStore _ObjectiveTypeDb;
        private readonly ObjectiveSetDbStore _ObjectiveSetDb;
        private readonly AchievementDbStore _AchievementDb;
        private readonly ItemDbStore _ItemDb;
        private readonly VehicleDbStore _VehicleDb;
        private readonly ExperienceTypeDbStore _ExpTypeDb;
        private readonly FireGroupToFireModeDbStore _FireGroupDb;
        private readonly ItemCategoryDbStore _ItemCategoryDb;

        public ObjectiveCollectionsPopulator(ILogger<ObjectiveCollectionsPopulator> logger,
            IServiceProvider services,
            ObjectiveCollection objCensus, ObjectiveDbStore objDb,
            ObjectiveTypeCollection objTypeCensus, ObjectiveTypeDbStore objTypeDb,
            ObjectiveSetCollection objSetCensus, ObjectiveSetDbStore objSetDb,
            AchievementCollection achCensus, AchievementDbStore achDb,
            ItemCollection itemCensus, ItemDbStore itemDb,
            VehicleCollection vehCensus, VehicleDbStore vehDb,
            ExperienceTypeDbStore expTypeDb, ExperienceTypeCollection expTypeCensus,
            FireGroupToFireModeCollection fireGroupCensus, FireGroupToFireModeDbStore fireGroupDb,
            ItemCategoryCollection itemCategoryCensus, ItemCategoryDbStore itemCategoryDb) {

            _Logger = logger;
            _Services = services;

            _ObjectiveCensus = objCensus ?? throw new ArgumentNullException(nameof(objCensus));
            _ObjectiveDb = objDb ?? throw new ArgumentNullException(nameof(objDb));
            _ObjectiveTypeCensus = objTypeCensus ?? throw new ArgumentNullException(nameof(objTypeCensus));
            _ObjectiveTypeDb = objTypeDb ?? throw new ArgumentNullException(nameof(objTypeDb));
            _ObjectiveSetCensus = objSetCensus ?? throw new ArgumentNullException(nameof(objSetCensus));
            _ObjectiveSetDb = objSetDb ?? throw new ArgumentNullException(nameof(objSetDb));
            _AchievementCensus = achCensus;
            _AchievementDb = achDb;
            _ItemCensus = itemCensus;
            _ItemDb = itemDb;
            _VehicleCensus = vehCensus;
            _VehicleDb = vehDb;
            _ExpTypeDb = expTypeDb;
            _ExpTypeCensus = expTypeCensus;
            _FireGroupCensus = fireGroupCensus;
            _FireGroupDb = fireGroupDb;
            _ItemCategoryCensus = itemCategoryCensus;
            _ItemCategoryDb = itemCategoryDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => typeof(IRefreshableRepository).IsAssignableFrom(p) && p.IsAbstract == false && p.ContainsGenericParameters == false);

                foreach (Type t in types) {
                    _Logger.LogDebug($"Refreshing {t.FullName}");
                    IRefreshableRepository repo = (IRefreshableRepository) _Services.GetRequiredService(t);
                    await repo.Refresh(CancellationToken.None);
                }

                /*
                // check
                List<PsObjective> censusObjs = await _ObjectiveCensus.GetAll();
                List<PsObjective> dbObjs = await _ObjectiveDb.GetAll();

                _Logger.LogDebug($"Objectives: got {censusObjs.Count} from Census, have {dbObjs.Count} in DB");
                if (censusObjs.Count > dbObjs.Count) {
                    foreach (PsObjective dir in censusObjs) {
                        await _ObjectiveDb.Upsert(dir);
                    }
                }

                // check
                List<ObjectiveType> censusTypes = await _ObjectiveTypeCensus.GetAll();
                List<ObjectiveType> dbTypes = await _ObjectiveTypeDb.GetAll();

                _Logger.LogDebug($"ObjectiveTypes: got {censusTypes.Count} from Census, have {dbTypes.Count} in DB");
                if (censusTypes.Count > dbTypes.Count) {
                    foreach (ObjectiveType type in censusTypes) {
                        await _ObjectiveTypeDb.Upsert(type);
                    }
                }

                // check
                List<ObjectiveSet> censusSets = await _ObjectiveSetCensus.GetAll();
                List<ObjectiveSet> dbSets = await _ObjectiveSetDb.GetAll();

                _Logger.LogDebug($"ObjectiveSets: got {censusSets.Count} from Census, have {dbSets.Count} in DB");
                if (censusSets.Count > dbSets.Count) {
                    foreach (ObjectiveSet type in censusSets) {
                        await _ObjectiveSetDb.Upsert(type);
                    }
                }

                // check
                List<Achievement> censusAchs = await _AchievementCensus.GetAll();
                List<Achievement> dbAchs = await _AchievementDb.GetAll();

                _Logger.LogDebug($"Achievement: got {censusAchs.Count} from Census, have {dbAchs.Count} in DB");
                if (censusAchs.Count > dbAchs.Count) {
                    foreach (Achievement ach in censusAchs) {
                        await _AchievementDb.Upsert(ach);
                    }
                }

                // check
                List<PsItem> censusItems = await _ItemCensus.GetAll();
                List<PsItem> dbItems = await _ItemDb.GetAll();

                Dictionary<int, PsItem> dbdb = new Dictionary<int, PsItem>();
                foreach (PsItem i in dbItems) {
                    dbdb[i.ID] = i;
                }

                _Logger.LogDebug($"Item: got {censusItems.Count} from Census, have {dbItems.Count} in DB");
                //if (censusItems.Count > dbItems.Count) {
                foreach (PsItem item in censusItems) {
                    _ = dbdb.TryGetValue(item.ID, out PsItem? db);
                    if (db == null) {
                        _Logger.LogDebug($"NEW >> item from Census {item.Name}");
                    }
                    await _ItemDb.Upsert(item);
                }
                //}

                // check
                List<PsVehicle> censusVehs = await _VehicleCensus.GetAll();
                List<PsVehicle> dbVehs = await _VehicleDb.GetAll();

                _Logger.LogDebug($"Vehicle: got {censusVehs.Count} from Census, have {dbVehs.Count} in DB");
                if (censusVehs.Count > dbVehs.Count) {
                    foreach (PsVehicle veh in censusVehs) {
                        await _VehicleDb.Upsert(veh);
                    }
                }

                // check
                List<ExperienceType> censusExpType = await _ExpTypeCensus.GetAll();
                List<ExperienceType> dbExpType = await _ExpTypeDb.GetAll();

                _Logger.LogDebug($"ExperienceType: got {censusExpType.Count} from Census, have {dbExpType.Count} in DB");
                foreach (ExperienceType expt in censusExpType) {
                    await _ExpTypeDb.Upsert(expt);
                }

                List<FireGroupToFireMode> censusFireGroup = await _FireGroupCensus.GetAll();
                List<FireGroupToFireMode> dbFireGroup = await _FireGroupDb.GetAll();

                _Logger.LogDebug($"FireGroupToFireMode: got {censusFireGroup.Count} from Census, have {dbFireGroup.Count} in DB");
                foreach (FireGroupToFireMode fireGroup in censusFireGroup) {
                    await _FireGroupDb.Upsert(fireGroup);
                }

                // check
                List<ItemCategory> censusItemCategory = await _ItemCategoryCensus.GetAll();
                List<ItemCategory> dbItemCategory = await _ItemCategoryDb.GetAll();

                _Logger.LogDebug($"ItemCategory: got {censusItemCategory.Count} from Census, have {dbItemCategory.Count} in DB");
                foreach (ItemCategory itemCat in censusItemCategory) {
                    await _ItemCategoryDb.Upsert(itemCat);
                }
                */

                _Logger.LogDebug($"Finished objective populator in {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to populate directive tables");
            }
        }

    }
}
