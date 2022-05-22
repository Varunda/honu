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

        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;

        private readonly CharacterRepository _CharacterRepository;

        private const int MINUTES_BACK = 5;

        public RealtimeNetworkBuilder(ILogger<RealtimeNetworkBuilder> logger,
            KillEventDbStore killDb, ExpEventDbStore expDb,
            CharacterRepository characterRepository) {

            _Logger = logger;
            _KillDb = killDb;
            _ExpDb = expDb;
            _CharacterRepository = characterRepository;
        }

        public async Task<RealtimeNetwork> Build(short worldID) {
            DateTime startPeriod = DateTime.UtcNow - TimeSpan.FromMinutes(MINUTES_BACK);

            Stopwatch timer = Stopwatch.StartNew();
            Stopwatch overall = Stopwatch.StartNew();

            List<ExpEvent> expEvents = await _ExpDb.GetByRange(startPeriod, DateTime.UtcNow, zoneID: null, worldID: worldID);
            long expMs = timer.ElapsedMilliseconds; timer.Restart();

            List<KillEvent> killEvents = await _KillDb.GetByRange(startPeriod, DateTime.UtcNow, zoneID: null, worldID: worldID);
            long killMs = timer.ElapsedMilliseconds; timer.Restart();

            Dictionary<string, Dictionary<string, decimal>> data = new(); // <char id, <other id, strength>>

            void IncreaseStrength(string charID, string otherID, DateTime when, decimal amount) {
                // Scale based on how long ago the event took place
                decimal secondsAgo = (decimal) (DateTime.UtcNow - when).TotalSeconds;
                decimal scaleFactor = 1m - (secondsAgo / (MINUTES_BACK * 60m));
                amount = scaleFactor * amount;

                if (data.ContainsKey(charID) == false) {
                    data.Add(charID, new Dictionary<string, decimal>());
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

                decimal str = 0;

                if (Experience.IsHeal(exp.ExperienceID)) {
                    str = 0.1m;
                } else if (Experience.IsResupply(exp.ExperienceID)) {
                    str = 0.05m;
                } else if (Experience.IsRevive(exp.ExperienceID)) {
                    str = 0.2m;
                } else if (Experience.IsShieldRepair(exp.ExperienceID)) {
                    str = 0.05m;
                } else if (Experience.IsMaxRepair(exp.ExperienceID)) {
                    str = 0.1m;
                } else if (Experience.IsAssist(exp.ExperienceID)) {
                    str = 0.25m;
                }

                if (str > 0) {
                    IncreaseStrength(exp.SourceID, exp.OtherID, exp.Timestamp, str);
                }
            }

            long processExpMs = timer.ElapsedMilliseconds; timer.Restart();

            foreach (KillEvent kill in killEvents) {
                if (kill.AttackerCharacterID != kill.KilledCharacterID) {
                    IncreaseStrength(kill.AttackerCharacterID, kill.KilledCharacterID, kill.Timestamp, 0.5m);
                }
            }

            long processKillMs = timer.ElapsedMilliseconds; timer.Restart();

            RealtimeNetwork network = new RealtimeNetwork();
            network.WorldID = worldID;
            network.Timestamp = DateTime.UtcNow;

            foreach (KeyValuePair<string, Dictionary<string, decimal>> iter in data) {
                //_Logger.LogDebug($"{iter.Key} has interacted with {iter.Value.Count} characters");

                // Skip characters who aren't interacting with lots of people
                if (iter.Value.Count < 2) {
                    continue;
                }

                List<RealtimeNetworkInteraction> interactions = new List<RealtimeNetworkInteraction>();

                foreach (KeyValuePair<string, decimal> playerIter in iter.Value) {
                    //_Logger.LogTrace($"{iter.Key} has interacted with {playerIter.Key} with a strength of {playerIter.Value}");

                    if (playerIter.Value < 0.1m) {
                        continue;
                    }

                    interactions.Add(new RealtimeNetworkInteraction() {
                        OtherID = playerIter.Key,
                        Strength = playerIter.Value
                    });
                }

                // Not enough interesting interactions
                if (interactions.Count < 2) {
                    continue;
                }

                network.Players.Add(new RealtimeNetworkPlayer() {
                    CharacterID = iter.Key,
                    Interactions = interactions
                });
            }

            List<string> charIDs = network.Players.Select(iter => iter.CharacterID).ToList();
            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(charIDs);

            foreach (RealtimeNetworkPlayer player in network.Players) {
                PsCharacter? c = chars.FirstOrDefault(iter => iter.ID == player.CharacterID);

                if (c == null) {
                    player.Display = $"<missing {player.CharacterID}>";
                } else {
                    player.Display = $"{c.Name}";
                    if (c.OutfitID != null) {
                        player.Display = $"[{c.OutfitTag}] " + player.Display;
                    }

                    player.FactionID = c.FactionID;
                }

                player.Interactions = player.Interactions.OrderBy(iter => iter.Strength).Take(10).ToList();
            }

            long buildMs = timer.ElapsedMilliseconds; timer.Stop();

            _Logger.LogDebug($"Took {overall.ElapsedMilliseconds}ms to build realtime network for {worldID} :: "
                + $"Exp DB: {expEvents.Count} events/{expMs}ms, Kill DB: {killEvents.Count} events/{killMs}ms, "
                + $"Exp process: {processExpMs}, Kill process: {processKillMs}, build: {buildMs} ");

            return network;
        }

    }
}
