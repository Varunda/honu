using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly ILogger<ReportHub> _Logger;

        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "Report.{0}"; // {0} => Generator hash

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
        private readonly IEventHandler _EventHandler;

        private readonly ReportRepository _ReportRepository;

        public ReportHub(ILogger<ReportHub> logger, IMemoryCache cache,
            CharacterRepository charRepo, OutfitRepository outfitRepo,
            SessionDbStore sessionDb, KillEventDbStore killDb,
            ExpEventDbStore expDb, ItemRepository itemRepo,
            OutfitReportParameterDbStore reportDb, FacilityControlDbStore controlDb,
            FacilityPlayerControlDbStore playerControlDb, IFacilityDbStore facDb,
            ReportRepository reportRepo, RealtimeReconnectDbStore reconnectDb,
            ItemCategoryRepository itemCategoryRepository, VehicleDestroyDbStore vehicleDestroyDb,
            IEventHandler eventHandler, ExperienceTypeRepository experienceTypeRepository) {

            _Logger = logger;
            _Cache = cache;

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
        }

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
                    //_Logger.LogDebug($"OutfitReport '{cacheKey}' is cached");
                    await Clients.Caller.SendParameters(report.Parameters);
                    await Clients.Caller.UpdateSessions(report.Sessions);
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
                    await Clients.Caller.UpdateState(OutfitReportState.DONE);
                    return;
                }
            }

            report = new OutfitReport();
            report.Parameters = parms;

            try {
                bool isValid = await IsValid(parms);
                if (isValid == false) {
                    return;
                }

                if (parms.ID == Guid.Empty) {
                    parms.ID = Guid.NewGuid();
                    parms.Timestamp = DateTime.UtcNow;
                    await _ReportParametersDb.Insert(parms);
                }

                await Clients.Caller.SendParameters(parms);

                await Clients.Caller.UpdateState(OutfitReportState.GETTING_SESSIONS);

                List<Session> sessions = new();

                foreach (string charID in parms.CharacterIDs) {
                    List<Session> s = await _SessionDb.GetByRangeAndCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);
                    sessions.AddRange(s);
                    _Logger.LogTrace($"Loaded {s.Count} sessions for {charID} between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                }

                foreach (string outfitID in parms.OutfitIDs) {
                    List<Session> s = await _SessionDb.GetByRangeAndOutfit(outfitID, parms.PeriodStart, parms.PeriodEnd);
                    sessions.AddRange(s);
                    _Logger.LogTrace($"Loaded {s.Count} sessions for {outfitID} between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                }

                report.Sessions = sessions;
                _Logger.LogTrace($"Loaded a TOTAL of {report.Sessions.Count} sessions between {parms.PeriodStart:u} and {parms.PeriodEnd:u}");
                await Clients.Caller.UpdateSessions(report.Sessions);

                if (report.Sessions.Count == 0) {
                    await Clients.Caller.SendError($"found 0 sessions, not data to load");
                    return;
                }

                HashSet<string> chars = new(); // All of the tracked characters, get the kill//death//exp//vehicle destroy//player control events for these
                foreach (Session s in report.Sessions) {
                    chars.Add(s.CharacterID);
                }

                await Clients.Caller.SendCharacterIDs(chars.ToList());

                // Get kills//deaths
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_KILLDEATHS);
                foreach (string charID in chars) {
                    try {
                        List<KillEvent> killDeaths = await _KillDb.GetKillsByCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);

                        List<KillEvent> kills = killDeaths.Where(iter => {
                            return iter.AttackerTeamID == parms.TeamID
                                && chars.Contains(iter.AttackerCharacterID)
                                && iter.KilledTeamID != parms.TeamID;
                        }).ToList();
                        report.Kills.AddRange(kills);

                        await Clients.Caller.SendKills(charID, kills);

                        List<KillEvent> deaths = killDeaths.Where(iter => {
                            return (iter.KilledTeamID == parms.TeamID || iter.KilledTeamID == 4)
                                && chars.Contains(iter.KilledCharacterID)
                                && (iter.KilledTeamID != iter.AttackerTeamID || iter.KilledTeamID == 4)
                                && iter.RevivedEventID == null;
                        }).ToList();
                        report.Deaths.AddRange(deaths);

                        await Clients.Caller.SendDeaths(charID, deaths);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error loading kill//death events for {charID}");
                        await Clients.Caller.SendError($"error while getting kill and death events for {charID}: {ex.Message}");
                    }
                }

                // Get exp
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_EXP);
                List<ExpEvent> expEvents = await _ExpDb.GetByCharacterIDs(chars.ToList(), parms.PeriodStart, parms.PeriodEnd);
                report.Experience = expEvents;
                await Clients.Caller.UpdateExp(expEvents);
                /*
                foreach (string charID in chars) {
                    try {
                        List<ExpEvent> exp = await _ExpDb.GetByCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);
                        await Clients.Caller.SendExp(charID, exp);
                        report.Experience.AddRange(exp);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error loading exp events for {charID}");
                        await Clients.Caller.SendError($"error while getting exp events for {charID}: {ex.Message}");
                    }
                }
                */

                // Get vehicle destroy
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_VEHICLE_DESTROY);
                foreach (string charID in chars) {
                    try {
                        List<VehicleDestroyEvent> events = await _VehicleDestroyDb.GetByCharacterID(charID, parms.PeriodStart, parms.PeriodEnd);
                        await Clients.Caller.SendVehicleDestroy(charID, events);
                        report.VehicleDestroy.AddRange(events);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error loading vehicle destroy events for {charID}");
                        await Clients.Caller.SendError($"error while getting vehicle destroy events for {charID}: {ex.Message}");
                    }
                }

                // Get player control
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_PLAYER_CONTROL);
                try {
                    List<PlayerControlEvent> pcEvents = await _PlayerControlDb.GetByCharacterIDsPeriod(chars.ToList(), parms.PeriodStart, parms.PeriodEnd);
                    await Clients.Caller.UpdatePlayerControls(pcEvents);
                    report.PlayerControl = pcEvents;
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error getting player control events");
                }
                /*
                foreach (string charID in chars) {
                    try {
                        List<PlayerControlEvent> events = await _PlayerControlDb.GetByCharacterIDPeriod(charID, parms.PeriodStart, parms.PeriodEnd);
                        await Clients.Caller.SendPlayerControl(charID, events);
                        report.PlayerControl.AddRange(events);
                    } catch (Exception ex) {
                        _Logger.LogError(ex, $"error loading player control events for {charID}");
                        await Clients.Caller.SendError($"error while getting player control events for {charID}: {ex.Message}");
                    }
                }
                */

                // Load each FacilityControlEvent that the tracked characters participated in, load the facility,
                //      and load the characters that were present
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_FACILITY_CONTROL);
                List<FacilityControlEvent> control = new List<FacilityControlEvent>();
                List<PlayerControlEvent> controlEvents = new List<PlayerControlEvent>(report.PlayerControl);
                foreach (PlayerControlEvent ev in controlEvents) {
                    if (control.FirstOrDefault(iter => iter.ID == ev.ControlID) != null) {
                        continue;
                    }

                    FacilityControlEvent? controlEvent = await _ControlDb.GetByID(ev.ControlID);
                    if (controlEvent != null) {
                        control.Add(controlEvent);
                    }

                    List<PlayerControlEvent> playerEvents = await _PlayerControlDb.GetByEventID(ev.ControlID);
                    report.PlayerControl.AddRange(playerEvents);
                }
                report.Control = control;
                await Clients.Caller.UpdateControls(report.Control);
                await Clients.Caller.UpdatePlayerControls(report.PlayerControl);

                // Get characters
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_CHARACTERS);
                HashSet<string> charsToLoad = GetUniqueCharacters(report);
                report.Characters = await _CharacterRepository.GetByIDs(charsToLoad.ToList());
                await Clients.Caller.UpdateCharacters(report.Characters);

                // Get outfits
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_OUTFITS);
                HashSet<string> outfits = GetUniqueOutfits(report);
                report.Outfits = await _OutfitRepository.GetByIDs(outfits.ToList());
                await Clients.Caller.UpdateOutfits(report.Outfits);

                // Get the items
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_ITEMS);
                HashSet<int> items = GetUniqueItems(report);
                report.Items = (await _ItemRepository.GetAll()).Where(iter => items.Contains(iter.ID)).ToList();
                await Clients.Caller.UpdateItems(report.Items);

                // Get the facilities
                await Clients.Caller.UpdateState(OutfitReportState.GETTING_FACILITIES);
                HashSet<int> facilities = GetUniqueFacilities(report);
                foreach (int facID in facilities) {
                    PsFacility? fac = await _FacilityDb.GetByFacilityID(facID);
                    if (fac != null) {
                        report.Facilities.Add(fac);
                    }
                }
                await Clients.Caller.UpdateFacilities(report.Facilities);

                await Clients.Caller.UpdateState(OutfitReportState.GETTING_RECONNETS);
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

                await Clients.Caller.UpdateState(OutfitReportState.DONE);

                _Cache.Set(cacheKey, report, new MemoryCacheEntryOptions() {
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                });
            } catch (Exception ex) {
                _Logger.LogError(ex, $"generic error in report generation. using generator string '{generator}'");
                await Clients.Caller.SendError(ex.Message);
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

            if (parms.PeriodEnd - parms.PeriodStart >= TimeSpan.FromHours(8)) {
                await Clients.Caller.SendError($"A report cannot span more than 8 hours");
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
