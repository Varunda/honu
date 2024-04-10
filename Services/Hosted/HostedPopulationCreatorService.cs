using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    /// <summary>
    ///     Background service that generates population data using session data honu has
    /// </summary>
    public class HostedPopulationCreatorService : BackgroundService {

        private const string SERVICE_NAME = "population_creator";

        private readonly ILogger<HostedPopulationCreatorService> _Logger;
        private readonly CharacterRepository _CharacterRepository;
        private readonly SessionDbStore _SessionDb;
        private readonly PopulationDbStore _PopulationDb;

        private readonly List<short> _Worlds = new();
        private readonly List<short> _Factions = new();

        private readonly HashSet<string> _MissingCharacters = new();

        private const int TASK_DELAY = 60 * 5; // 5 minutes between runs

        public HostedPopulationCreatorService(ILogger<HostedPopulationCreatorService> logger,
            SessionDbStore sessionDb, PopulationDbStore populationDb,
            CharacterRepository characterRepository) {

            _Logger = logger;
            _SessionDb = sessionDb;
            _PopulationDb = populationDb;
            _CharacterRepository = characterRepository;

            _Worlds.Add(0);
            _Worlds.AddRange(World.PcStreams);
            _Factions.Add(0);
            _Factions.AddRange(Faction.All);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"Started at {DateTime.UtcNow:u}");

            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    Stopwatch timer = Stopwatch.StartNew();
                    int generatedCount = 0;

                    HashSet<DateTime> alreadyPopulated = new();
                    List<PopulationCount> counts = await _PopulationDb.GetCounts();
                    foreach (PopulationCount count in counts) {
                        alreadyPopulated.Add(count.Timestamp);
                    }

                    Session? firstSession = await _SessionDb.GetFirstSession();
                    if (firstSession == null) {
                        _Logger.LogWarning($"No sessions have occured, cannot create population data");
                        continue;
                    }

                    DateTime s = firstSession.Start;
                    DateTime start = new(s.Year, s.Month, s.Day, s.Hour, 0, 0);

                    // Get the final date, which is the current hour, minus 1 minute
                    // As the current hour hasn't finished, don't want to generate population on it yet
                    DateTime f = DateTime.UtcNow;
                    DateTime finish = new(f.Year, f.Month, f.Day, f.Hour, 0, 0);
                    finish -= TimeSpan.FromMinutes(1);

                    _Logger.LogInformation($"Start at {start:u}, finish at {finish:u}");

                    for (DateTime i = start; i < finish; i += TimeSpan.FromHours(1)) {
                        stoppingToken.ThrowIfCancellationRequested();
                        DateTime rangeEnd = i + TimeSpan.FromHours(1);

                        try {
                            if (alreadyPopulated.Contains(i)) {
                                continue;
                            }

                            await Generate(i, 60 * 60);
                            ++generatedCount;
                        } catch (Exception ex) {
                            _Logger.LogError(ex, $"error generating population data at {i:u}");
                        }
                    }

                    _Logger.LogInformation($"Took {timer.ElapsedMilliseconds}ms to run. Generated {generatedCount} entries. Waiting {TASK_DELAY} seconds to re-run");

                    _Logger.LogInformation($"Missed {_MissingCharacters.Count} characters this run");
                    _MissingCharacters.Clear();

                    await Task.Delay(TASK_DELAY * 1000, stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "error creating population data");
                }
            }
        }

        /// <summary>
        ///     Generate and store the population data for a timestamp over an interval
        /// </summary>
        /// <param name="timestamp">Lower bound of when sessions will be included</param>
        /// <param name="duration">How many seconds after <paramref name="duration"/> will the upper bound be</param>
        private async Task Generate(DateTime timestamp, int duration) {
            DateTime rangeEnd = timestamp + TimeSpan.FromSeconds(duration);
            _Logger.LogDebug($"Generate({timestamp:u}) Started, end at {rangeEnd:u}");

            Stopwatch stepTimer = Stopwatch.StartNew();

            List<Session> sessions = await _SessionDb.GetByRange(timestamp, rangeEnd);
            _Logger.LogDebug($"Generate({timestamp:u}) Loaded {sessions.Count} sessions in {stepTimer.ElapsedMilliseconds}ms");
            stepTimer.Restart();

            List<string> charIDs = sessions.Select(iter => iter.CharacterID).Distinct().ToList();
            _Logger.LogDebug($"Generate({timestamp:u}) loading {charIDs.Count} characters");
            Dictionary<string, PsCharacter> chars = (await _CharacterRepository.GetByIDs(charIDs, CensusEnvironment.PC, fast: true))
                .ToDictionary(iter => iter.ID);

            _Logger.LogDebug($"Generate({timestamp:u}) Loaded {chars.Count}/{charIDs.Count} characters in {stepTimer.ElapsedMilliseconds}ms");
            stepTimer.Restart();

            // Generate the data for each world and faction combination
            foreach (short worldID in _Worlds) {
                foreach (short factionID in _Factions) {
                    List<Session> filtered = sessions.Where(iter => {
                        if (chars.TryGetValue(iter.CharacterID, out PsCharacter? c) == false) {
                            if (_MissingCharacters.Contains(iter.CharacterID) == false) {
                                //_Logger.LogWarning($"Missing {iter.CharacterID}");
                                _MissingCharacters.Add(iter.CharacterID);
                            }
                            return false;
                        }

                        if (worldID != 0 && c.WorldID != worldID) {
                            return false;
                        }

                        if (factionID != 0 && c.FactionID != factionID) {
                            return false;
                        }

                        return true;
                    }).ToList();

                    PopulationEntry entry = new();
                    entry.WorldID = worldID;
                    entry.FactionID = factionID;
                    entry.Timestamp = timestamp;
                    entry.Duration = duration;
                    entry.Total = filtered.Count;
                    entry.Logins = filtered.Count(iter => iter.Start >= timestamp);
                    entry.Logouts = filtered.Count(iter => iter != null && iter.End <= rangeEnd);
                    entry.UniqueCharacters = filtered.Select(iter => iter.CharacterID).Distinct().Count();

                    List<int> sessionLengths = new(filtered.Count);
                    foreach (Session session in filtered) {
                        DateTime end = session.End ?? DateTime.UtcNow;

                        DateTime b = (session.Start < timestamp) ? timestamp : session.Start;
                        DateTime e = (end > rangeEnd) ? rangeEnd : end;

                        int length = (int)(e - b).TotalSeconds;

                        entry.SecondsPlayed += length;
                        sessionLengths.Add((int)(e - session.Start).TotalSeconds);
                    }

                    entry.AverageSessionLength = (sessionLengths.Count == 0) ? 0 : (int)sessionLengths.Average();

                    await _PopulationDb.Insert(entry);
                }
            }

        }

    }
}
