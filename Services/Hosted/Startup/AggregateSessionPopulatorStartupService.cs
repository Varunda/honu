using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    public class AggregateSessionPopulatorStartupService : BackgroundService {

        private readonly ILogger<AggregateSessionPopulatorStartupService> _Logger;
        private readonly SessionDbStore _SessionDb;
        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly VehicleDestroyDbStore _VehicleDestroyDb;

        public AggregateSessionPopulatorStartupService(ILogger<AggregateSessionPopulatorStartupService> logger,
            SessionDbStore sessionDb, KillEventDbStore killDb,
            ExpEventDbStore expDb, VehicleDestroyDbStore vehicleDestroyDb) {

            _Logger = logger;

            _SessionDb = sessionDb;
            _KillDb = killDb;
            _ExpDb = expDb;
            _VehicleDestroyDb = vehicleDestroyDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            Session? first = await _SessionDb.GetFirstSession();
            if (first == null) {
                _Logger.LogInformation($"no sessions created!");
                return;
            }

            DateTime iterDate = first.Start.Date;
            int iterationCount = 0;

            while (stoppingToken.IsCancellationRequested == false) {
                _Logger.LogInformation($"starting iteration... [iterDate={iterDate:u}]");
                Stopwatch timer = Stopwatch.StartNew();
                ++iterationCount;

                try {
                    // i prefer doing a do{} while(false) for states where there can be multiple exists
                    do {
                        Stopwatch loadTimer = Stopwatch.StartNew();
                        List<Session> sessions = await _SessionDb.GetByRange(iterDate, iterDate.AddDays(1));
                        if (sessions.Count == 0) {
                            _Logger.LogDebug($"no sessions found, skipping [iterDate={iterDate:u}]");
                            break;
                        }

                        if (sessions.FirstOrDefault(iter => iter.End == null) == null) {
                            _Logger.LogDebug($"no unfinished sessions found, skipping [iterDate={iterDate:u}] [sessions.Count={sessions.Count}]");
                            break;
                        }

                        long sessionLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();

                        // find the first session that needs its summary calculated, if one doesn't exist, then this range is already done
                        if (sessions.FirstOrDefault(iter => iter.SummaryCalculated == null) == null) {
                            _Logger.LogDebug($"no unfinished sessions found, skipping [iterDate={iterDate:u}] [sessions.Count={sessions.Count}]");
                            break;
                        }

                        List<ExpEvent> expEvents = await _ExpDb.GetByRange(iterDate, iterDate.AddDays(1));
                        long expLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
                        List<KillEvent> killEvents = await _KillDb.GetByRange(iterDate, iterDate.AddDays(1));
                        long killLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
                        List<VehicleDestroyEvent> vehicleEvents = await _VehicleDestroyDb.GetByRange(iterDate, iterDate.AddDays(1));
                        long vkillLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();

                        Dictionary<string, List<ExpEvent>> exp = new Dictionary<string, List<ExpEvent>>();
                        foreach (ExpEvent ev in expEvents) {
                            if (exp.ContainsKey(ev.SourceID) == false) {
                                exp.Add(ev.SourceID, []);
                            }
                            exp[ev.SourceID].Add(ev);
                        }

                        // we have to store the events, not the count, due to multiple sessions per day
                        // and the stats are per session, not per day
                        Dictionary<string, List<KillEvent>> kills = new();
                        Dictionary<string, List<KillEvent>> deaths = new();
                        foreach (KillEvent ev in killEvents) {
                            if (ev.AttackerTeamID == ev.KilledTeamID) {
                                continue;
                            }

                            if (kills.ContainsKey(ev.AttackerCharacterID) == false) {
                                kills.Add(ev.AttackerCharacterID, []);
                            }
                            kills[ev.AttackerCharacterID].Add(ev);

                            if (ev.RevivedEventID != null) {
                                continue;
                            }

                            if (deaths.ContainsKey(ev.KilledCharacterID) == false) {
                                deaths.Add(ev.KilledCharacterID, []);
                            }
                            deaths[ev.KilledCharacterID].Add(ev);
                        }

                        Dictionary<string, List<VehicleDestroyEvent>> vehicleKills = new();
                        foreach (VehicleDestroyEvent ev in vehicleEvents) {
                            if (ev.AttackerTeamID == ev.KilledTeamID) {
                                continue;
                            }

                            if (vehicleKills.ContainsKey(ev.AttackerCharacterID) == false) {
                                vehicleKills.Add(ev.AttackerCharacterID, []);
                            }
                            vehicleKills[ev.AttackerCharacterID].Add(ev);
                        }
                        long dataLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
                        _Logger.LogInformation($"loaded data [iterDate={iterDate:u}] [sessionLoad={sessionLoadMs}ms] [expLoad={expLoadMs}ms] "
                            + $"[killLoad={killLoadMs}ms] [vkill={vkillLoadMs}ms] [dataLoad={dataLoadMs}ms]");

                        int countProcessed = 0;
                        int countUnended = 0;
                        int countDone = 0;
                        int countPartial = 0;
                        int countAlready = 0;

                        Stopwatch updateTimer = Stopwatch.StartNew();
                        foreach (Session s in sessions) {
                            if (s.SummaryCalculated != null) {
                                ++countAlready;
                                continue;
                            }

                            ++countProcessed;

                            if (s.End == null) {
                                ++countUnended;
                                continue;
                            }

                            // default value is -1 for uncalculated, so set this to set if calculated!
                            if (s.Kills == -1) { s.Kills = 0; }
                            if (s.Deaths == -1) { s.Deaths = 0; }
                            if (s.Heals == -1) { s.Heals = 0; }
                            if (s.Revives == -1) { s.Revives = 0; }
                            if (s.VehicleKills == -1) { s.VehicleKills = 0; }
                            if (s.ShieldRepairs == -1) { s.ShieldRepairs = 0; }
                            if (s.Resupplies == -1) { s.Resupplies = 0; }
                            if (s.Repairs == -1) { s.Repairs = 0; }
                            if (s.Spawns == -1) { s.Spawns = 0; }
                            if (s.ExperienceGained == -1) { s.ExperienceGained = 0; }

                            foreach (KillEvent ev in kills.GetValueOrDefault(s.CharacterID) ?? []) {
                                // ignore events outside the range of this session
                                if (s.Start >= ev.Timestamp || ev.Timestamp > s.End.Value) {
                                    continue;
                                }

                                s.Kills += 1;
                            }

                            foreach (KillEvent ev in deaths.GetValueOrDefault(s.CharacterID) ?? []) {
                                if (s.Start >= ev.Timestamp || ev.Timestamp > s.End.Value) {
                                    continue;
                                }

                                s.Deaths += 1;
                            }

                            foreach (VehicleDestroyEvent ev in vehicleKills.GetValueOrDefault(s.CharacterID) ?? []) {
                                if (s.Start >= ev.Timestamp || ev.Timestamp > s.End.Value) {
                                    continue;
                                }

                                s.VehicleKills += 1;
                            }

                            foreach (ExpEvent ev in exp.GetValueOrDefault(s.CharacterID) ?? []) {
                                if (s.Start >= ev.Timestamp || ev.Timestamp > s.End.Value) {
                                    continue;
                                }

                                s.ExperienceGained += ev.Amount;

                                int expID = ev.ExperienceID;
                                if (Experience.IsHeal(expID) == true) {
                                    s.Heals += 1;
                                } else if (Experience.IsShieldRepair(expID) == true) {
                                    s.ShieldRepairs += 1;
                                } else if (Experience.IsRevive(expID) == true) {
                                    s.Revives += 1;
                                } else if (Experience.IsResupply(expID) == true) {
                                    s.Resupplies += 1;
                                } else if (Experience.IsMaxRepair(expID) == true) {
                                    s.Repairs += 1;
                                } else if (Experience.IsSpawn(expID) == true) {
                                    s.Spawns += 1;
                                }
                            }

                            // if the session ends in this interval, there can't be any more events
                            // but if the end isn't within this interval,
                            // then events in the next interval might be part of this session too
                            if (iterDate <= s.End && (iterDate + TimeSpan.FromDays(1)) > s.End) {
                                s.SummaryCalculated = DateTime.UtcNow;
                                ++countDone;
                            } else {
                                ++countPartial;
                            }

                            try {
                                await _SessionDb.UpdateSummary(s.ID, s);
                            } catch (Exception ex) {
                                _Logger.LogError(ex, $"failed to update session [session.ID={s.ID}] [iterDate={iterDate:u}]");
                            }
                        }

                        updateTimer.Stop();
                        timer.Stop();
                        _Logger.LogInformation($"session summary iteration done [iterDate={iterDate:u}] [duration={timer.ElapsedMilliseconds}ms] [updateTImer={updateTimer.ElapsedMilliseconds}ms]" +
                            $" [sessions.Count={sessions.Count}] [processed={countProcessed}] [unend={countUnended}] [partial={countPartial}] [done={countDone}] [already={countAlready}]");
                    } while (false);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to created session aggregate [iterDate={iterDate:u}]");
                }

                iterDate += TimeSpan.FromDays(1);

                if (iterDate >= DateTime.UtcNow.Date) {
                    break;
                }
            }

            _Logger.LogInformation($"finished session aggregation [iterations={iterationCount}]");
        }

    }
}
