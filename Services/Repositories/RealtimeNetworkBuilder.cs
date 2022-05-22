using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Watchtower;
using watchtower.Services.Db;

namespace watchtower.Services.Repositories {

    public class RealtimeNetworkBuilder {

        private readonly ILogger<RealtimeNetworkBuilder> _Logger;
        private readonly IMemoryCache _Cache;
        private const string CACHE_KEY = "RealtimeNetwork.{0}"; // {0} => WorldID

        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;

        private readonly CharacterRepository _CharacterRepository;

        private const int MINUTES_BACK = 3;

        public RealtimeNetworkBuilder(ILogger<RealtimeNetworkBuilder> logger, IMemoryCache cache,
            KillEventDbStore killDb, ExpEventDbStore expDb,
            CharacterRepository characterRepository) {

            _Logger = logger;
            _Cache = cache;

            _KillDb = killDb;
            _ExpDb = expDb;
            _CharacterRepository = characterRepository;
        }

        public async Task<RealtimeNetwork> Build(short worldID) {
            if (_Cache.TryGetValue(string.Format(CACHE_KEY, worldID), out RealtimeNetwork? cached) == true) {
                return cached!;
            }

            DateTime startPeriod = DateTime.UtcNow - TimeSpan.FromMinutes(MINUTES_BACK);

            Stopwatch timer = Stopwatch.StartNew();
            Stopwatch overall = Stopwatch.StartNew();

            List<ExpEvent> expEvents = await _ExpDb.GetByRange(startPeriod, DateTime.UtcNow, zoneID: null, worldID: worldID);
            long expMs = timer.ElapsedMilliseconds; timer.Restart();

            List<KillEvent> killEvents = await _KillDb.GetByRange(startPeriod, DateTime.UtcNow, zoneID: null, worldID: worldID);
            long killMs = timer.ElapsedMilliseconds; timer.Restart();

            Dictionary<string, Dictionary<string, double>> data = new(); // <char id, <other id, strength>>

            void IncreaseStrength(string charID, string otherID, DateTime when, double amount) {
                // Scale based on how long ago the event took place
                double secondsAgo = (double) (DateTime.UtcNow - when).TotalSeconds;
                double scaleFactor = Math.Pow(1d - (secondsAgo / (MINUTES_BACK * 60d)), 10d);
                //double scaleFactor = (0.5d) * (1d - (secondsAgo / (MINUTES_BACK * 60d)));
                amount = scaleFactor * amount;

                if (data.ContainsKey(charID) == false) {
                    data.Add(charID, new Dictionary<string, double>());
                }

                if (data[charID].ContainsKey(otherID) == false) {
                    data[charID][otherID] = amount;
                } else {
                    data[charID][otherID] += amount;
                }
            }

            foreach (ExpEvent exp in expEvents) {
                if (Experience.OtherIDIsCharacterID(exp.ExperienceID) == false) {
                    continue;
                }

                if (exp.OtherID == "0") {
                    continue;
                }

                double str = 0;

                if (Experience.IsHeal(exp.ExperienceID)) {
                    str = 0.1d;
                } else if (Experience.IsResupply(exp.ExperienceID)) {
                    str = 0.05d;
                } else if (Experience.IsRevive(exp.ExperienceID)) {
                    str = 0.2d;
                } else if (Experience.IsShieldRepair(exp.ExperienceID)) {
                    str = 0.05d;
                } else if (Experience.IsMaxRepair(exp.ExperienceID)) {
                    str = 0.1d;
                } else if (Experience.IsAssist(exp.ExperienceID)) {
                    str = 0.25d;
                }

                if (str > 0) {
                    IncreaseStrength(exp.SourceID, exp.OtherID, exp.Timestamp, str);
                }
            }

            long processExpMs = timer.ElapsedMilliseconds; timer.Restart();

            foreach (KillEvent kill in killEvents) {
                if (kill.AttackerCharacterID == "0" || kill.KilledCharacterID == "0") {
                    continue;
                }

                if (kill.AttackerCharacterID != kill.KilledCharacterID) {
                    IncreaseStrength(kill.AttackerCharacterID, kill.KilledCharacterID, kill.Timestamp, 0.5d);
                }
            }

            long processKillMs = timer.ElapsedMilliseconds; timer.Restart();

            RealtimeNetwork network = new RealtimeNetwork();
            network.WorldID = worldID;
            network.Timestamp = DateTime.UtcNow;

            foreach (KeyValuePair<string, Dictionary<string, double>> iter in data) {
                //_Logger.LogDebug($"{iter.Key} has interacted with {iter.Value.Count} characters");

                List<RealtimeNetworkInteraction> interactions = new List<RealtimeNetworkInteraction>();

                foreach (KeyValuePair<string, double> playerIter in iter.Value) {
                    //_Logger.LogTrace($"{iter.Key} has interacted with {playerIter.Key} with a strength of {playerIter.Value}");

                    /*
                    if (playerIter.Value < 0.025d) {
                        continue;
                    }
                    */

                    interactions.Add(new RealtimeNetworkInteraction() {
                        OtherID = playerIter.Key,
                        Strength = playerIter.Value
                    });
                }

                // Not enough interesting interactions
                if (interactions.Count < 1) {
                    continue;
                }

                network.Players.Add(new RealtimeNetworkPlayer() {
                    CharacterID = iter.Key,
                    Interactions = interactions
                });
            }

            HashSet<string> charIDs = new HashSet<string>();

            foreach (RealtimeNetworkPlayer player in network.Players) {
                charIDs.Add(player.CharacterID);

                foreach (RealtimeNetworkInteraction inter in player.Interactions) {
                    charIDs.Add(inter.OtherID);
                }
            }

            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(charIDs.ToList());
            //Dictionary<string, PsCharacter> chars

            foreach (RealtimeNetworkPlayer player in network.Players) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == player.CharacterID);

                player.Display = c?.GetDisplayName() ?? $"<missing {player.CharacterID}>";
                player.FactionID = c?.FactionID ?? 0;

                foreach (RealtimeNetworkInteraction inter in player.Interactions) {
                    PsCharacter? other = chars.FirstOrDefault(iter => iter.ID == inter.OtherID);
                    inter.OtherName = other?.GetDisplayName() ?? $"<missing {inter.OtherID}";
                    inter.FactionID = other?.FactionID ?? 0;
                }

                //player.Interactions = player.Interactions.OrderBy(iter => iter.Strength).Take(10).ToList();
            }

            long buildMs = timer.ElapsedMilliseconds; timer.Stop();

            _Logger.LogDebug($"Took {overall.ElapsedMilliseconds}ms to build realtime network for {worldID} :: "
                + $"Exp DB: {expEvents.Count} events/{expMs}ms, Kill DB: {killEvents.Count} events/{killMs}ms, "
                + $"Exp process: {processExpMs}, Kill process: {processKillMs}, build: {buildMs} ");

            _Cache.Set(string.Format(CACHE_KEY, worldID), network, new MemoryCacheEntryOptions() {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3)
            });

            return network;
        }

    }
}
