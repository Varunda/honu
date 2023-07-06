using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.Tracking;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Models.Health;
using watchtower.Models.Report;
using watchtower.Realtime;
using watchtower.Services.Census;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class ReportHub : Hub<IReportHub> {

        private static int _InstanceCount = 0;

        private readonly ILogger<ReportHub> _Logger;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "Report.{0}"; // {0} => Generator hash

        private const string TRACE_KEY = "report -";

        private readonly OutfitRepository _OutfitRepository;
        private readonly CharacterRepository _CharacterRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly SessionDbStore _SessionDb;
        private readonly OutfitReportParameterDbStore _ReportParametersDb;
        private readonly FacilityControlDbStore _ControlDb;
        private readonly FacilityPlayerControlDbStore _PlayerControlDb;
        private readonly IFacilityDbStore _FacilityDb;
        private readonly ItemCategoryRepository _ItemCategoryRepository;
        private readonly RealtimeReconnectDbStore _ReconnectDb;
        private readonly VehicleDestroyDbStore _VehicleDestroyDb;
        private readonly ExperienceTypeRepository _ExperienceTypeRepository;
        private readonly AchievementEarnedDbStore _AchievementEarnedDbStore;
        private readonly AchievementRepository _AchievementRepository;
        private readonly FireGroupToFireModeRepository _FireGroupRepository;
        private readonly IEventHandler _EventHandler;

        private readonly ReportRepository _ReportRepository;

        static ReportHub() {
            // create a listener that cares about these report activites being created
            ActivitySource.AddActivityListener(new ActivityListener() {
                ShouldListenTo = (source) => {
                    return source.Name == HonuActivitySource.ActivitySourceName;
                },
                Sample = (ref ActivityCreationOptions<ActivityContext> context) => {
                    if (context.Name.StartsWith("report - ") == true) {
                        return ActivitySamplingResult.AllDataAndRecorded;
                    } else {
                        return ActivitySamplingResult.None;
                    }
                }
            });
        }

        public ReportHub(ILogger<ReportHub> logger, IMemoryCache cache,
            CharacterRepository charRepo, OutfitRepository outfitRepo,
            SessionDbStore sessionDb, KillEventDbStore killDb,
            ExpEventDbStore expDb, ItemRepository itemRepo,
            OutfitReportParameterDbStore reportDb, FacilityControlDbStore controlDb,
            FacilityPlayerControlDbStore playerControlDb, IFacilityDbStore facDb,
            ReportRepository reportRepo, RealtimeReconnectDbStore reconnectDb,
            ItemCategoryRepository itemCategoryRepository, VehicleDestroyDbStore vehicleDestroyDb,
            IEventHandler eventHandler, ExperienceTypeRepository experienceTypeRepository,
            AchievementEarnedDbStore achievementEarnedDbStore, AchievementRepository achievementRepository,
            FireGroupToFireModeRepository fireGroupRepository) {

            Interlocked.Increment(ref _InstanceCount);

            _Logger = logger;
            _Cache = cache;

            //_Logger.LogDebug($"ReportHub ctor instance count: {_InstanceCount}");

            _OutfitRepository = outfitRepo;
            _CharacterRepository = charRepo;
            _ItemRepository = itemRepo;
            _KillDb = killDb;
            _ExpDb = expDb;
            _SessionDb = sessionDb;
            _ReportParametersDb = reportDb;
            _ControlDb = controlDb;
            _PlayerControlDb = playerControlDb;
            _FacilityDb = facDb;
            _ReportRepository = reportRepo;
            _ReconnectDb = reconnectDb;
            _ItemCategoryRepository = itemCategoryRepository;
            _VehicleDestroyDb = vehicleDestroyDb;
            _EventHandler = eventHandler;
            _ExperienceTypeRepository = experienceTypeRepository;
            _AchievementEarnedDbStore = achievementEarnedDbStore;
            _AchievementRepository = achievementRepository;
            _FireGroupRepository = fireGroupRepository;
        }

        /// <summary>
        ///     Generate a report based on a generator string passed. You can pass a parameter with just an ID,
        ///     and it will get the generator of that ID
        /// </summary>
        /// <param name="generator">String containing the generator</param>
        public async Task GenerateReport(string generator) {

            await Clients.Caller.UpdateState(OutfitReportState.NOT_STARTED);

            OutfitReportParameters parms = new();

            await Clients.Caller.UpdateState(OutfitReportState.PARSING_GENERATOR);

            try {
                parms = await _ReportRepository.ParseGenerator(generator);
                //_Logger.LogTrace($"first parse: {JToken.FromObject(parms)}");
            } catch (Exception ex) {
                _Logger.LogError(ex, $"failed to parse generator '{generator}'");
                await Clients.Caller.SendError(ex.Message);
                return;
            }

            if (parms.ID != Guid.Empty) {
                OutfitReportParameters? dbParms = await _ReportParametersDb.GetByID(parms.ID);
                if (dbParms != null) {
                    //_Logger.LogDebug($"Finding previous parameters from ID {parms.ID}: {JToken.FromObject(dbParms)}");
                    parms = await _ReportRepository.ParseGenerator(dbParms.Generator);
                    //_Logger.LogDebug($"Parms after ID load: {JToken.FromObject(dbParms)}");
                }
            }

            List<ItemCategory> cats = await _ItemCategoryRepository.GetAll();
            await Clients.Caller.UpdateItemCategories(cats);

            List<ExperienceType> expTypes = await _ExperienceTypeRepository.GetAll();
            await Clients.Caller.UpdateExperienceTypes(expTypes);

            string cacheKey = string.Format(CACHE_KEY, parms.Generator);

            if (_Cache.TryGetValue(cacheKey, out OutfitReport? report) == true) {
                if (report != null) {
                    await Clients.Caller.SendMessage($"report is cached");
                    //_Logger.LogDebug($"OutfitReport '{cacheKey}' is cached");
                    await Clients.Caller.SendParameters(report.Parameters);
                    await Clients.Caller.UpdateSessions(report.Sessions);

                    // this will include the revived and teamkill ones as they are modify the data retrieve,
                    //      not what data is retrieved 
                    await Clients.Caller.UpdateKills(report.Kills);
                    await Clients.Caller.UpdateDeaths(report.Deaths);
                    await Clients.Caller.UpdateExp(report.Experience);
                    await Clients.Caller.UpdateVehicleDestroy(report.VehicleDestroy);
                    await Clients.Caller.UpdateItems(report.Items);
                    await Clients.Caller.UpdateCharacters(report.Characters);
                    await Clients.Caller.UpdateOutfits(report.Outfits);
                    await Clients.Caller.UpdateControls(report.Control);
                    await Clients.Caller.UpdatePlayerControls(report.PlayerControl);
                    await Clients.Caller.UpdateFacilities(report.Facilities);
                    await Clients.Caller.UpdateReconnects(report.Reconnects);
                    await Clients.Caller.UpdateFireGroupXrefs(report.FireGroupXref);
                    if (report.Parameters.IncludeAchievementsEarned == true) {
                        await Clients.Caller.UpdateAchievementEarned(report.AchievementsEarned);
                        await Clients.Caller.UpdateAchievements(report.Achievements);
                    }
                    await Clients.Caller.UpdateState(OutfitReportState.DONE);
                    return;
                } else {
                    await Clients.Caller.SendMessage($"report {cacheKey} was cached, but returned null from cache?");
                }
            }

            report = new OutfitReport();
            report.Parameters = parms;

            // will the report be cached after generation? 
            // set to false when a caught exception occurs during generation
            bool doCache = true;

            // these traces will be created in the context of the signalR connection, which is not useful
            // by setting the Current activity to null, then starting our activity, a new root trace is created
            // https://opentelemetry.io/docs/instrumentation/net/manual/#creating-new-root-activities
            //
            // we do have to remember to set Activity.Current back to what is was before
            Activity? previousCurrent = Activity.Current;
            Activity.Current = null;

            Activity? rootTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} start", ActivityKind.Internal);
            report.TraceSpanID = rootTrace?.SpanId.ToString();

            try {
                bool isValid = await IsValid(parms);
                if (isValid == false) {
                    rootTrace?.Stop();
                    Activity.Current = previousCurrent;
                    return;
                }

                if (parms.ID == Guid.Empty) {
                    parms.ID = Guid.NewGuid();
                    parms.Timestamp = DateTime.UtcNow;
                    await _ReportParametersDb.Insert(parms);
                }

                if (rootTrace == null) {
                    _Logger.LogDebug($"no trace created for report");
                } else {
                    _Logger.LogDebug($"tracing ID {rootTrace.Id} span ID {rootTrace.SpanId} parent ID {rootTrace.ParentId} parent span ID {rootTrace.ParentSpanId}");
                }
                rootTrace?.AddTag("honu.generator", parms.Generator);
                rootTrace?.AddTag("honu.parameters.id", parms.ID);
                rootTrace?.AddTag("honu.parameters.include_achievements", parms.IncludeAchievementsEarned);
                rootTrace?.AddTag("honu.parameters.include_revived_deaths", parms.IncludeRevivedDeaths);
                rootTrace?.AddTag("honu.parameters.include_teamkilled", parms.IncludeTeamkilled);
                rootTrace?.AddTag("honu.parameters.include_teamkills", parms.IncludeTeamkills);
                rootTrace?.AddTag("honu.parameters.include_other_id_exp", parms.IncludeOtherIdExpEvents);
                rootTrace?.AddTag("honu.parameters.start", $"{parms.PeriodStart:u}");
                rootTrace?.AddTag("honu.parameters.end", $"{parms.PeriodEnd:u}");
                rootTrace?.AddTag("honu.parameters.teamID", parms.TeamID);
                rootTrace?.AddTag("honu.parameters.characterIDs", $"[{string.Join(", ", parms.CharacterIDs)}]");
                rootTrace?.AddTag("honu.parameters.outfitIDs", $"[{string.Join(", ", parms.OutfitIDs)}]");

                await Clients.Caller.SendParameters(parms);

                await Clients.Caller.UpdateState(OutfitReportState.GETTING_SESSIONS);

                using (Activity? sessionTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} load sessions")) {
                    List<Session> sessions = new();

                    sessionTrace?.AddTag("honu.character_count", parms.CharacterIDs.Count);
                    foreach (string charID in parms.CharacterIDs) {
                        await Clients.Caller.SendMessage($"loading sessions for character {charID} {parms.PeriodStart:u} - {parms.PeriodEnd:u}");
                        List<Session> s = await _SessionDb.GetByRangeAndCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);
                        sessions.AddRange(s);
                        _Logger.LogTrace($"Loaded {s.Count} sessions for char {charID} between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                    }

                    sessionTrace?.AddTag("honu.outfit_count", parms.OutfitIDs.Count);
                    foreach (string outfitID in parms.OutfitIDs) {
                        await Clients.Caller.SendMessage($"loading sessions for outfit {outfitID} {parms.PeriodStart:u} - {parms.PeriodEnd:u}");
                        List<Session> s = await _SessionDb.GetByRangeAndOutfit(outfitID, parms.PeriodStart, parms.PeriodEnd);
                        sessions.AddRange(s);
                        _Logger.LogTrace($"Loaded {s.Count} sessions for outfit {outfitID} between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                    }

                    sessionTrace?.AddTag("honu.session_count", sessions.Count);
                    report.Sessions = sessions;
                    _Logger.LogTrace($"Loaded a TOTAL of {report.Sessions.Count} sessions between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                    await Clients.Caller.UpdateSessions(report.Sessions);
                }

                if (report.Sessions.Count == 0) {
                    await Clients.Caller.SendError($"found 0 sessions, no data to load");
                    rootTrace?.Stop();
                    Activity.Current = previousCurrent;
                    return;
                }

                HashSet<string> chars = new(); // All of the tracked characters, get the kill//death//exp//vehicle destroy//player control events for these
                foreach (Session s in report.Sessions) {
                    chars.Add(s.CharacterID);
                }

                await Clients.Caller.SendCharacterIDs(chars.ToList());

                // Get kills//deaths
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_KILLDEATHS);
                using (Activity? killDeathActivity = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} kill/deaths")) {
                    foreach (string charID in chars) {
                        try {
                            List<KillEvent> killDeaths = await _KillDb.GetKillsByCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);

                            IEnumerable<KillEvent> kills = killDeaths.Where(iter => chars.Contains(iter.AttackerCharacterID));
                            if (parms.IncludeTeamkills == false) {
                                kills = kills.Where(iter => iter.KilledTeamID != parms.TeamID);
                                //_Logger.LogDebug($"not including tks");
                            }

                            report.Kills.AddRange(kills);

                            await Clients.Caller.SendKills(charID, kills.ToList());

                            IEnumerable<KillEvent> deaths = killDeaths.Where(iter => chars.Contains(iter.KilledCharacterID));
                            if (parms.IncludeRevivedDeaths == false) {
                                deaths = deaths.Where(iter => iter.RevivedEventID == null);
                                //_Logger.LogDebug($"not including revived deaths");
                            }
                            if (parms.IncludeTeamkilled == false) {
                                deaths = deaths.Where(iter => iter.KilledTeamID != iter.AttackerTeamID || iter.KilledTeamID == 4);
                                //_Logger.LogDebug($"not including tked");
                            }

                            report.Deaths.AddRange(deaths);

                            await Clients.Caller.SendDeaths(charID, deaths.ToList());
                        } catch (Exception ex) {
                            killDeathActivity?.AddEvent(new ActivityEvent($"exception loading {charID}"));
                            doCache = false;
                            _Logger.LogError(ex, $"error loading kill//death events for {charID}");
                            await Clients.Caller.SendError($"error while getting kill and death events for {charID}: {ex.Message}");
                        }
                    }
                    killDeathActivity?.AddTag("honu.count", report.Kills.Count + report.Deaths.Count);
                }

                // Get exp
                // while other operations are done per character, this one is done with all characters at once.
                // getting the events one by one is often much more time consuming than all at once,
                //      often taking 5 times longer doing it 1 by 1
                // so why don't others take a long time too? i haven't checked it out all the way, but i suspect
                //      it's due to the amount of data per page, and how much filtering is necessary
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_EXP);
                using (Activity? expTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} exp")) {
                    List<ExpEvent> expEvents = await _ExpDb.GetByCharacterIDs(chars.ToList(), parms.PeriodStart, parms.PeriodEnd);
                    report.Experience = expEvents;
                    expTrace?.AddTag("honu.count", expEvents.Count);

                    if (parms.IncludeOtherIdExpEvents == true) {
                        List<ExpEvent> otherExp = await _ExpDb.GetOtherByCharacterIDs(chars.ToList(), parms.PeriodStart, parms.PeriodEnd);

                        // if the event is already included (for example some assists), don't double count it
                        int duplicate = 0;
                        HashSet<ulong> alreadyPresent = new(expEvents.Select(iter => iter.ID));
                        foreach (ExpEvent e in otherExp) {
                            if (alreadyPresent.Contains(e.ID)) {
                                ++duplicate;
                                continue;
                            }

                            report.Experience.Add(e);
                        }

                        expTrace?.AddTag("honu.other_duplicate_count", duplicate);
                        expTrace?.AddTag("honu.other_unique_count", expEvents.Count - duplicate);
                    }

                    await Clients.Caller.UpdateExp(expEvents);
                }

                // Get vehicle destroy
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_VEHICLE_DESTROY);
                using (Activity? vehicleDestroyTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} vehicle destroy")) {
                    foreach (string charID in chars) {
                        try {
                            List<VehicleDestroyEvent> events = await _VehicleDestroyDb.GetByCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);
                            await Clients.Caller.SendVehicleDestroy(charID, events);
                            report.VehicleDestroy.AddRange(events);
                        } catch (Exception ex) {
                            doCache = false;
                            _Logger.LogError(ex, $"error loading vehicle destroy events for {charID}");
                            await Clients.Caller.SendError($"error while getting vehicle destroy events for {charID}: {ex.Message}");
                        }
                    }
                    vehicleDestroyTrace?.AddTag("honu.count", report.VehicleDestroy.Count);
                }
                
                // get achievements earned if requested
                if (report.Parameters.IncludeAchievementsEarned == true) {
                    await Clients.Caller.UpdateState(OutfitReportState.GETTING_ACHIEVEMENT_EARNED);

                    using (Activity? achievementActivity = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} achievement")) {
                        HashSet<int> achievementIDs = new();
                        foreach (string charID in chars) {
                            try {
                                List<AchievementEarnedEvent> events = await _AchievementEarnedDbStore.GetByCharacterIDAndRange(charID, parms.PeriodStart, parms.PeriodEnd);
                                foreach (AchievementEarnedEvent ev in events) {
                                    achievementIDs.Add(ev.AchievementID);
                                }
                                await Clients.Caller.SendAchievementEarned(charID, events);
                                report.AchievementsEarned.AddRange(events);
                            } catch (Exception ex) {
                                doCache = false;
                                _Logger.LogError(ex, $"error loading achievements earned for {charID}");
                                await Clients.Caller.SendError($"error while getting achievements earned for {charID}: {ex.Message}");
                            }
                        }

                        report.Achievements = (await _AchievementRepository.GetAll()).Where(iter => achievementIDs.Contains(iter.ID)).ToList();
                        await Clients.Caller.UpdateAchievements(report.Achievements);
                    }
                }

                // Get player control
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_PLAYER_CONTROL);
                using (Activity? playerControlTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} player control")) {
                    List<PlayerControlEvent> pcEvents = await _PlayerControlDb.GetByCharacterIDsPeriod(chars.ToList(), parms.PeriodStart, parms.PeriodEnd);

                    // Load each FacilityControlEvent that the tracked characters participated in, load the facility,
                    //      and load the characters that were present
                    await Clients.Caller.UpdateState(OutfitReportState.GETTING_FACILITY_CONTROL);
                    List<FacilityControlEvent> control = new();

                    foreach (PlayerControlEvent ev in pcEvents) {
                        // don't load the same facility control event multiple times
                        if (control.FirstOrDefault(iter => iter.ID == ev.ControlID) != null) {
                            continue;
                        }

                        FacilityControlEvent? controlEvent = await _ControlDb.GetByID(ev.ControlID);
                        if (controlEvent != null) {
                            control.Add(controlEvent);
                        } else {
                            _Logger.LogWarning($"Failed to find control event {ev.ControlID}, but had a player control event for it");
                        }

                        List<PlayerControlEvent> playerEvents = await _PlayerControlDb.GetByEventID(ev.ControlID);
                        report.PlayerControl.AddRange(playerEvents);
                    }
                    report.Control = control;

                    await Clients.Caller.UpdateControls(report.Control);
                    await Clients.Caller.UpdatePlayerControls(report.PlayerControl);
                }

                // Get characters
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_CHARACTERS);
                using (Activity? charTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} characters")) {
                    HashSet<string> charsToLoad = GetUniqueCharacters(report);
                    report.Characters = await _CharacterRepository.GetByIDs(charsToLoad.ToList(), CensusEnvironment.PC, fast: true);
                    await Clients.Caller.UpdateCharacters(report.Characters);
                }

                // Get outfits
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_OUTFITS);
                using (Activity? outfitTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} outfit")) {
                    HashSet<string> outfits = GetUniqueOutfits(report);
                    report.Outfits = await _OutfitRepository.GetByIDs(outfits.ToList());
                    await Clients.Caller.UpdateOutfits(report.Outfits);
                }

                // Get the items
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_ITEMS);
                using (Activity? itemTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} items")) {
                    HashSet<int> items = GetUniqueItems(report);
                    report.Items = (await _ItemRepository.GetAll()).Where(iter => items.Contains(iter.ID)).ToList();
                    await Clients.Caller.UpdateItems(report.Items);
                }

                // Get the facilities
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_FACILITIES);
                using (Activity? facTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} facilities")) {
                    HashSet<int> facilities = GetUniqueFacilities(report);
                    foreach (int facID in facilities) {
                        PsFacility? fac = await _FacilityDb.GetByFacilityID(facID);
                        if (fac != null) {
                            report.Facilities.Add(fac);
                        }
                    }
                    await Clients.Caller.UpdateFacilities(report.Facilities);
                }

                // load the world this report is on by using the first character's world ID
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_RECONNETS);
                using (Activity? reconnectTrace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} reconnects")) {
                    short? worldID = null;
                    foreach (PsCharacter c in report.Characters) {
                        worldID = c.WorldID;
                        break;
                    }

                    if (worldID != null) {
                        List<RealtimeReconnectEntry> reconnects = await _ReconnectDb.GetByInterval(worldID.Value, parms.PeriodStart, parms.PeriodEnd);
                        report.Reconnects = reconnects;
                        await Clients.Caller.UpdateReconnects(reconnects);
                    }
                }

                // load fire group xrefs used
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_FIRE_GROUP_XREF);
                using (Activity? trace = HonuActivitySource.Root.StartActivity($"{TRACE_KEY} fire group xref")) {
                    HashSet<int> fireModes = new(report.Kills.Select(iter => iter.AttackerFireModeID).Union(report.Deaths.Select(iter => iter.AttackerFireModeID)));

                    foreach (int fireModeID in fireModes) {
                        List<FireGroupToFireMode> xrefs = await _FireGroupRepository.GetByFireModeID(fireModeID);
                        report.FireGroupXref.AddRange(xrefs);
                    }

                    await Clients.Caller.UpdateFireGroupXrefs(report.FireGroupXref);
                }

                await Clients.Caller.UpdateState(OutfitReportState.DONE);

                // don't cache reports that didn't fully generate, as the data is not complete
                if (doCache == true) {
                    await Clients.Caller.SendMessage($"report will be cached for future calls");
                    _Cache.Set(cacheKey, report, new MemoryCacheEntryOptions() {
                        SlidingExpiration = TimeSpan.FromMinutes(30)
                    });
                } else {
                    await Clients.Caller.SendMessage($"report is not cached: an exception was thrown while loading the data");
                }

                rootTrace?.Stop();
            } catch (Exception ex) {
                _Logger.LogError(ex, $"generic error in report generation. using generator string '{generator}'");
                await Clients.Caller.SendError(ex.Message);
            } finally {
                rootTrace?.Stop();
                Activity.Current = previousCurrent;
            }
        }

        /// <summary>
        ///     Check if the parameters are valid or not. A message will be sent to the client if the parms are invalid
        /// </summary>
        /// <param name="parms">Parameters to validate</param>
        /// <returns>True if the parameters are valid, false if the parameters are invalid</returns>
        private async Task<bool> IsValid(OutfitReportParameters parms) {
            if (parms.PeriodStart >= parms.PeriodEnd) {
                await Clients.Caller.SendError($"The start period at {parms.PeriodStart:u} cannot be after the end {parms.PeriodEnd:u}");
                return false;
            }

            if (parms.PeriodEnd >= DateTime.UtcNow) {
                await Clients.Caller.SendError($"The end of this report is in the future, wait until after that time to generate a report");
                return false;
            }

            if (parms.PeriodEnd >= _EventHandler.MostRecentProcess()) {
                await Clients.Caller.SendError($"Honu is currently lagging, and is getting more events than it can process. "
                    + $"Events at {parms.PeriodEnd:u} have yet to be processed, Honu's most recent event process was at {_EventHandler.MostRecentProcess():u}");
                return false;
            }

            if (parms.PeriodEnd - parms.PeriodStart > TimeSpan.FromHours(12)) {
                await Clients.Caller.SendError($"A report cannot span more than 12 hours");
                return false;
            }

            if (parms.TeamID <= 0) {
                await Clients.Caller.SendError($"The TeamID of the report is currently {parms.TeamID}, it must be above 0. Try setting the faction");
                return false;
            }

            return true;
        }

        Task[] BatchProcess(int count, Func<int, Task?> func) {
            Task[] tasks = new Task[count];
            for (int i = 0; i < count; ++i) {
                tasks[i] = Task.Run(async () => {
                    Task? t = func(i);
                    if (t != null) {
                        await t;
                    }
                });
            }

            return tasks;
        }

        /// <summary>
        ///     Get the unique outfit IDs that appear in a report
        /// </summary>
        /// <param name="report">Report with the filled events</param>
        private HashSet<string> GetUniqueOutfits(OutfitReport report) {
            HashSet<string> outfits = new();

            foreach (PsCharacter c in report.Characters) {
                if (c.OutfitID != null) {
                    outfits.Add(c.OutfitID);
                }
            }

            foreach (FacilityControlEvent ev in report.Control) {
                if (ev.OutfitID != null && ev.OutfitID != "0") {
                    outfits.Add(ev.OutfitID);
                }
            }

            return outfits;
        }

        /// <summary>
        ///     Get the unique character IDs that appear in a report
        /// </summary>
        /// <param name="report">Report with the events</param>
        private HashSet<string> GetUniqueCharacters(OutfitReport report) {
            HashSet<string> chars = new();

            foreach (Session s in report.Sessions) {
                chars.Add(s.CharacterID);
            }

            foreach (KillEvent ev in report.Kills) {
                chars.Add(ev.AttackerCharacterID);
                chars.Add(ev.KilledCharacterID);
            }

            foreach (KillEvent ev in report.Deaths) {
                chars.Add(ev.AttackerCharacterID);
                chars.Add(ev.KilledCharacterID);
            }

            foreach (ExpEvent ev in report.Experience) {
                chars.Add(ev.SourceID);
                if (ev.OtherID.Length == 19) {
                    chars.Add(ev.OtherID);
                }
            }

            foreach (VehicleDestroyEvent ev in report.VehicleDestroy) {
                chars.Add(ev.AttackerCharacterID);
                chars.Add(ev.KilledCharacterID);
            }

            foreach (PlayerControlEvent ev in report.PlayerControl) {
                chars.Add(ev.CharacterID);
            }

            return chars;
        }

        /// <summary>
        ///     Get the unique item IDs that a client could want
        /// </summary>
        /// <param name="report">Filled out outfit report</param>
        /// <returns></returns>
        private HashSet<int> GetUniqueItems(OutfitReport report) {
            HashSet<int> items = new();

            foreach (KillEvent ev in report.Kills) {
                if (ev.WeaponID != 0 && items.Contains(ev.WeaponID) == false) {
                    items.Add(ev.WeaponID);
                }
            }

            foreach (KillEvent ev in report.Deaths) {
                if (ev.WeaponID != 0 && items.Contains(ev.WeaponID) == false) {
                    items.Add(ev.WeaponID);
                }
            }

            return items;
        }

        /// <summary>
        ///     Get the unique facility IDs
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private HashSet<int> GetUniqueFacilities(OutfitReport report) {
            HashSet<int> ids = new();
            
            foreach (FacilityControlEvent ev in report.Control) {
                ids.Add(ev.FacilityID);
            }

            return ids;
        }

    }

}
