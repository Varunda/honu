using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Constants;
using watchtower.Models;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted {

    public class HostedSessionSummaryProcess(
            ILogger<HostedSessionSummaryProcess> logger, IServiceHealthMonitor healthMonitor,
            SessionDbStore sessionDb, KillEventDbStore killDb,
            ExpEventDbStore expDb, VehicleDestroyDbStore vehicleDestroyDb
        )
        : ManagedPeriodTask("HostedSessionSummaryProcess", TimeSpan.FromMinutes(30), logger, healthMonitor) {

        private readonly ILogger<HostedSessionSummaryProcess> _Logger = logger;
        private readonly SessionDbStore _SessionDb = sessionDb;
        private readonly KillEventDbStore _KillDb = killDb;
        private readonly ExpEventDbStore _ExpDb = expDb;
        private readonly VehicleDestroyDbStore _VehicleDestroyDb = vehicleDestroyDb;

        protected override async Task Run(CancellationToken stoppingToken) {
            _Logger.LogInformation($"starting session summary update");
            List<SessionBlock> sessionBlocks = await _GetBlocks(stoppingToken);

            foreach (SessionBlock block in sessionBlocks) {
                stoppingToken.ThrowIfCancellationRequested();
                await _UpdateBlock(block, stoppingToken);
            }
        }

        /// <summary>
        ///     get all session blocks that have un-summarized summary stats
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        private async Task<List<SessionBlock>> _GetBlocks(CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();

            List<Session> sessions = await _SessionDb.GetNeedsSummary(cancel);
            _Logger.LogDebug($"loaded sessions [sessions.Count={sessions.Count}] [timer={timer.ElapsedMilliseconds}ms]");
            if (sessions.Count == 0) {
                return new List<SessionBlock>();
            }

            List<SessionBlock> sessionBlocks = [];
            sessions.Sort((a, b) => {
                return DateTime.Compare(a.Start, b.Start);
            });

            SessionBlock currentBlock = _MakeBlock(sessions[0]);
            _Logger.LogDebug($"first block [block.Start={currentBlock.Start:u}] [block.End={currentBlock.End:u}]");

            foreach (Session s in sessions.Skip(1)) {
                if (s.End == null) {
                    _Logger.LogDebug($"not sure why this session is in here, supposed to only be unfinished sessions! [s.ID={s.ID}] [s.Start={s.Start:u}]");
                    continue;
                }
                if (s.SummaryCalculated != null) {
                    _Logger.LogDebug($"not sure why this session is in here, supposed to only be uncalculated sessions! [s.ID={s.ID}] [s.Start={s.Start:u}]");
                    continue;
                }

                if (s.Start.Between(currentBlock.Start, currentBlock.End)) {
                    currentBlock.Sessions.Add(s);
                    if (s.End > currentBlock.End) {
                        currentBlock.End = s.End.Value;
                    }
                } else {
                    _Logger.LogDebug($"new session block found [block={currentBlock}] [s.Start={s.Start:u}]");
                    sessionBlocks.Add(currentBlock);
                    currentBlock = _MakeBlock(s);
                }
            }

            if (currentBlock.Sessions.Count > 0) {
                _Logger.LogDebug($"new session block found (no sessions left) [block={currentBlock}]");
                sessionBlocks.Add(currentBlock);
            }

            int sessionsInBlocks = sessionBlocks.Sum(iter => iter.Sessions.Count);

            _Logger.LogDebug($"found session blocks for aggregate [sessions.Count={sessions.Count}] [sessionsInBlocks={sessionsInBlocks}]"
                + $" [sessionBlocks.Count={sessionBlocks.Count}] [timer={timer.ElapsedMilliseconds}ms]");

            return sessionBlocks;
        }

        private SessionBlock _MakeBlock(Session s) {
            SessionBlock block = new SessionBlock();
            block.Start = s.Start;
            block.End = s.End!.Value;
            block.Sessions.Add(s);

            return block;
        }

        /// <summary>
        ///     update all sessions with a <see cref="SessionBlock"/>
        /// </summary>
        /// <param name="sessions"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        private async Task _UpdateBlock(SessionBlock sessions, CancellationToken cancel) {
            if (sessions.Sessions.Count == 0) {
                return;
            }

            Stopwatch loadTimer = Stopwatch.StartNew();

            DateTime firstSessionStart = sessions.Start;
            DateTime lastSessionEnd = sessions.End;

            _Logger.LogDebug($"updating sessions with aggregate stats [start={firstSessionStart:u}] [end={lastSessionEnd:u}]");

            // load all events that occur within this block
            List<SmallerExpEvent> expEvents = await _ExpDb.GetSmallerByRange(firstSessionStart, lastSessionEnd, cancel);
            long expLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
            List<KillEvent> killEvents = await _KillDb.GetByRange(firstSessionStart, lastSessionEnd, null, null, cancel);
            long killLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
            List<VehicleDestroyEvent> vehicleEvents = await _VehicleDestroyDb.GetByRange(firstSessionStart, lastSessionEnd, cancel);
            long vkillLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();

            Dictionary<ulong, List<SmallerExpEvent>> exp = new Dictionary<ulong, List<SmallerExpEvent>>();
            foreach (SmallerExpEvent ev in expEvents) {
                if (exp.ContainsKey(ev.SourceID) == false) {
                    exp.Add(ev.SourceID, []);
                }
                exp[ev.SourceID].Add(ev);
            }

            // it's possible to have sessions for the same character within the same block
            // due to this, we need store the events, as we need to filter the events
            // to the sessions they exist in
            Dictionary<string, List<DateTime>> kills = new();
            Dictionary<string, List<DateTime>> deaths = new();
            foreach (KillEvent ev in killEvents) {
                if (ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (kills.ContainsKey(ev.AttackerCharacterID) == false) {
                    kills.Add(ev.AttackerCharacterID, []);
                }
                kills[ev.AttackerCharacterID].Add(ev.Timestamp);

                if (ev.RevivedEventID != null) {
                    continue;
                }

                if (deaths.ContainsKey(ev.KilledCharacterID) == false) {
                    deaths.Add(ev.KilledCharacterID, []);
                }
                deaths[ev.KilledCharacterID].Add(ev.Timestamp);
            }

            Dictionary<string, List<DateTime>> vehicleKills = new();
            foreach (VehicleDestroyEvent ev in vehicleEvents) {
                if (ev.AttackerTeamID == ev.KilledTeamID) {
                    continue;
                }

                if (vehicleKills.ContainsKey(ev.AttackerCharacterID) == false) {
                    vehicleKills.Add(ev.AttackerCharacterID, []);
                }
                vehicleKills[ev.AttackerCharacterID].Add(ev.Timestamp);
            }
            long dataLoadMs = loadTimer.ElapsedMilliseconds; loadTimer.Restart();
            _Logger.LogInformation($"loaded data [start={firstSessionStart:u}] [end={lastSessionEnd:u}] [expLoad={expLoadMs}ms] "
                + $"[killLoad={killLoadMs}ms] [vkill={vkillLoadMs}ms] [dataLoad={dataLoadMs}ms]");

            int countProcessed = 0;
            int countUnended = 0;
            int countDone = 0;
            int countPartial = 0;
            int countAlready = 0;

            // now all data is loaded and in dictionaries to load, sound them all
            Stopwatch updateTimer = Stopwatch.StartNew();
            foreach (Session s in sessions.Sessions) {
                if (s.SummaryCalculated != null) {
                    ++countAlready;
                    continue;
                }

                ++countProcessed;

                if (s.End == null) {
                    ++countUnended;
                    continue;
                }

                // in case somehow these values were already set, change them back to zero
                s.Kills = 0;
                s.Deaths = 0;
                s.Heals = 0;
                s.Revives = 0;
                s.VehicleKills = 0;
                s.ShieldRepairs = 0;
                s.Resupplies = 0;
                s.Repairs = 0;
                s.Spawns = 0;
                s.ExperienceGained = 0;

                foreach (DateTime ev in kills.GetValueOrDefault(s.CharacterID) ?? []) {
                    // ignore events outside the range of this session
                    if (s.Start >= ev || ev > s.End.Value) {
                        continue;
                    }

                    s.Kills += 1;
                }

                foreach (DateTime ev in deaths.GetValueOrDefault(s.CharacterID) ?? []) {
                    if (s.Start >= ev || ev > s.End.Value) {
                        continue;
                    }

                    s.Deaths += 1;
                }

                foreach (DateTime ev in vehicleKills.GetValueOrDefault(s.CharacterID) ?? []) {
                    if (s.Start >= ev || ev > s.End.Value) {
                        continue;
                    }

                    s.VehicleKills += 1;
                }

                foreach (SmallerExpEvent ev in exp.GetValueOrDefault(ulong.Parse(s.CharacterID)) ?? []) {
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

                try {
                    s.SummaryCalculated = DateTime.UtcNow;
                    await _SessionDb.UpdateSummary(s.ID, s);
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to update session [session.ID={s.ID}]");
                }
            }

            updateTimer.Stop();
            _Logger.LogInformation($"session summary iteration done [duration={loadTimer.ElapsedMilliseconds}ms] [updateTimer={updateTimer.ElapsedMilliseconds}ms]" +
                $" [sessions.Count={sessions.Sessions.Count}] [processed={countProcessed}] [unend={countUnended}] [partial={countPartial}] [done={countDone}] [already={countAlready}]");
        }

        private class SessionBlock {

            public DateTime Start { get; set; } = DateTime.MaxValue;

            public DateTime End { get; set; } = DateTime.MinValue;

            public List<Session> Sessions { get; set; } = new();

            public override string ToString() {
                return $"<{nameof(SessionBlock)} [Start={Start:u}] [End={End:u}] [Sessions.Count={Sessions.Count}]>";
            }

        }

    }
}
