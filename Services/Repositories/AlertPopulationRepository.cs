using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Alert;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Models.Events;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class AlertPopulationRepository {

        private readonly ILogger<AlertPopulationRepository> _Logger;
        private readonly AlertPopulationDbStore _Db;
        private readonly AlertRepository _AlertRepository;
        private readonly SessionDbStore _SessionDb;
        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly AlertDbStore _AlertDb;

        private Dictionary<long, Task<List<AlertPopulation>>> _PendingCreate = new();

        public AlertPopulationRepository(ILogger<AlertPopulationRepository> logger,
            AlertPopulationDbStore db, AlertRepository alertRepository,
            SessionDbStore sessionDb, KillEventDbStore killDb,
            ExpEventDbStore expDb, AlertDbStore alertDb) {

            _Logger = logger;
            _Db = db;
            _AlertRepository = alertRepository;
            _SessionDb = sessionDb;
            _KillDb = killDb;
            _ExpDb = expDb;
            _AlertDb = alertDb;
        }

        /// <summary>
        ///     Get the population over the course of an alert
        /// </summary>
        /// <remarks>
        ///     This data will be saved in the DB if it hasn't already been generated
        /// </remarks>
        /// <param name="alertID">ID of the alert to get the population of</param>
        /// <param name="cancel">Cancellation token</param>
        /// <returns>
        ///     A list of <see cref="AlertPopulation"/> that represents the changing population of players over time
        /// </returns>
        public async Task<List<AlertPopulation>> GetByAlertID(long alertID, CancellationToken cancel) {
            Task<List<AlertPopulation>>? pending = null;
            lock (_PendingCreate) {
                _ = _PendingCreate.TryGetValue(alertID, out pending);
            }

            if (pending != null) {
                _Logger.LogDebug($"{alertID} is pending creation, returning the pending entry instead");
                return await pending;
            }

            List<AlertPopulation> pop = await _Db.GetByAlertID(alertID, cancel);

            if (pop.Count > 0) {
                return pop;
            }

            PsAlert? alert = await _AlertRepository.GetByID(alertID);
            if (alert == null) {
                return new List<AlertPopulation>();
            }

            pending = CreateAndInsertData(alert, cancel);
            lock (_PendingCreate) { _PendingCreate.TryAdd(alert.ID, pending); }
            try {
                pop = await pending;
            } finally {
                lock (_PendingCreate) { _PendingCreate.Remove(alert.ID); }
            }

            return pop;
        }

        private async Task<List<AlertPopulation>> CreateAndInsertData(PsAlert alert, CancellationToken cancel) {
            Stopwatch timer = Stopwatch.StartNew();

            DateTime start = alert.Timestamp;
            DateTime end = alert.Timestamp + TimeSpan.FromSeconds(alert.Duration);

            // Events not in the alert's zone are there to get when a player joins/leaves the zone
            List<KillEvent> kills = await _KillDb.GetByRange(start - TimeSpan.FromMinutes(5), end, null, alert.WorldID);
            List<ExpEvent> exp = await _ExpDb.GetByRange(start - TimeSpan.FromMinutes(5), end, null, alert.WorldID);
            List<AlertPlayer> players = await _AlertDb.GetParticipants(alert);
            List<Session> sessions = await _SessionDb.GetByRange(start, end);

            long loadData = timer.ElapsedMilliseconds;

            int samples = (int)Math.Ceiling((end - start).TotalMinutes);
            _Logger.LogDebug($"Using {samples} samples from alert period {start:u} to {end:u}");

            List<AlertPopulation> pops = new List<AlertPopulation>(samples);
            for (int i = 0; i < samples; ++i) {
                AlertPopulation pop = new AlertPopulation();
                pop.AlertID = alert.ID;
                pop.Timestamp = alert.Timestamp + TimeSpan.FromMinutes(i);
                pops.Add(pop);
            }

            Dictionary<string, List<TimestampZoneEvent>> timestampedEvents = new Dictionary<string, List<TimestampZoneEvent>>();
            foreach (AlertPlayer p in players) {
                timestampedEvents.Add(p.CharacterID, new List<TimestampZoneEvent>());
            }
            
            // Add each death event to the players
            foreach (KillEvent ev in kills) {
                if (timestampedEvents.TryGetValue(ev.AttackerCharacterID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.AttackerLoadoutID, Type = "event", TeamID = ev.AttackerTeamID });
                    timestampedEvents[ev.AttackerCharacterID] = events;
                }

                if (timestampedEvents.TryGetValue(ev.KilledCharacterID, out events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.KilledLoadoutID, Type = "event", TeamID = ev.KilledTeamID });
                    timestampedEvents[ev.KilledCharacterID] = events;
                }
            }

            // Add each exp event to the players
            foreach (ExpEvent ev in exp) {
                if (timestampedEvents.TryGetValue(ev.SourceID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = ev.Timestamp, ZoneID = ev.ZoneID, LoadoutID = ev.LoadoutID, Type = "event", TeamID = ev.TeamID });
                    timestampedEvents[ev.SourceID] = events;
                }
            }

            foreach (Session s in sessions) {
                if (timestampedEvents.TryGetValue(s.CharacterID, out List<TimestampZoneEvent>? events) == true) {
                    events.Add(new TimestampZoneEvent() { Timestamp = s.Start, ZoneID = 0, Type = "login" });

                    if (s.End != null) {
                        events.Add(new TimestampZoneEvent() { Timestamp = s.End.Value, ZoneID = 0, Type = "logout" });
                    }
                }
            }

            long sortData = timer.ElapsedMilliseconds - loadData;

            // For each player, an iteration over the samples is done, and check
            foreach (AlertPlayer p in players) {
                if (timestampedEvents.TryGetValue(p.CharacterID, out List<TimestampZoneEvent>? events) == false) {
                    continue;
                }

                List<TimestampZoneEvent> sorted = events.OrderBy(iter => iter.Timestamp).ToList();
                if (sorted.Count <= 0) {
                    continue;
                }

                uint lastZoneID = sorted[0].ZoneID;
                short? teamID = sorted[0].TeamID;
                TimestampZoneEvent lastEvent = sorted[0];

                for (int i = 0; i < samples; ++i) {
                    List<TimestampZoneEvent> sampledEvents = new List<TimestampZoneEvent>();

                    DateTime sampleStart = start + TimeSpan.FromMinutes(i);
                    DateTime sampleEnd = start + TimeSpan.FromMinutes(i + 1);

                    foreach (TimestampZoneEvent ev in sorted) {
                        if (ev.Timestamp < sampleStart) { // Event came too early
                            continue;
                        }

                        if (ev.Timestamp <= sampleEnd) {
                            lastZoneID = ev.ZoneID;
                            teamID = ev.TeamID;
                            lastEvent = ev;
                            break;
                        }
                    }

                    if ((sampleStart - lastEvent.Timestamp) > TimeSpan.FromMinutes(5)) {
                        //_Logger.LogDebug($"ITERATION {i}: Player {p.CharacterID} was AFK. Last event was at {lastEvent.Timestamp:u}, sample start: {sampleStart:u}, span: {sampleStart - lastEvent.Timestamp}");
                        continue;
                    }

                    if (alert.ZoneID == 0 || lastZoneID == alert.ZoneID) {
                        //_Logger.LogDebug($"ITERATION {i}: Player {p.CharacterID} was in zone {lastZoneID} and on team {teamID} at {sampleStart:u}");

                        AlertPopulation sample = pops[i];
                        if (teamID == 1) {
                            ++sample.CountVS;
                        } else if (teamID == 2) {
                            ++sample.CountNC;
                        } else if (teamID == 3) {
                            ++sample.CountTR;
                        } else {
                            ++sample.CountUnknown;
                        }
                    }
                }
            }

            long computeData = timer.ElapsedMilliseconds - sortData - loadData;

            foreach (AlertPopulation pop in pops) {
                await _Db.Insert(alert.ID, pop, cancel);
            }

            long insertData = timer.ElapsedMilliseconds - computeData - sortData - loadData;

            _Logger.LogDebug(
                $"Took {timer.ElapsedMilliseconds}ms to generate alert population data for alert {alert.ID}/{alert.Name}\n"
                + $"\tTime to load data: {loadData}ms\n"
                + $"\tSort data: {sortData}ms\n"
                + $"\tCompute data: {computeData}ms\n"
                + $"\tInsert data: {insertData}ms\n"
            );

            return pops;
        }

    }
}
