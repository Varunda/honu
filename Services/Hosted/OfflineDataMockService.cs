using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using watchtower.Constants;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class OfflineDataMockService : BackgroundService {

        private readonly ILogger<OfflineDataMockService> _Logger;

        private readonly ICharacterRepository _CharacterRepository;
        private readonly IExpEventDbStore _ExpDb;
        private readonly IKillEventDbStore _KillDb;

        private readonly IBackgroundTaskQueue _EventQueue;

        private readonly Random _Random;

        private List<PsCharacter> _RandomCharacters = new List<PsCharacter>();

        private List<string> _RandomCharacterNames = new List<string>() {
            "slatter1", "Asc3nder", "Meaningofbread", "ganidiot",
            "Slaeter", "Vilehydra", "ganidiotTR", "CloneKanoTR"
        };

        public OfflineDataMockService(ILogger<OfflineDataMockService> logger,
            ICharacterRepository charRepo, IExpEventDbStore expDb,
            IKillEventDbStore killDb, IBackgroundTaskQueue eventQueue) {

            _Logger = logger;

            _CharacterRepository = charRepo;
            _ExpDb = expDb;
            _KillDb = killDb;

            _EventQueue = eventQueue ?? throw new ArgumentNullException(nameof(eventQueue));

            _Random = new Random();
        }

        public override async Task StartAsync(CancellationToken cancellationToken) {
            foreach (string charName in _RandomCharacterNames) {
                PsCharacter? c = await _CharacterRepository.GetFirstByName(charName);

                if (c != null) {
                    _Logger.LogDebug($"Loaded character: {c.Name}/{c.ID}, on faction {c.FactionID}");
                    _RandomCharacters.Add(c);
                } else {
                    _Logger.LogWarning($"Missing character: {charName}");
                }
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            while (stoppingToken.IsCancellationRequested == false) {
                try {
                    int eventAmount = _Random.Next(10);

                    for (int i = 0; i < eventAmount; ++i) {
                        int eventType = _Random.Next(3);

                        if (eventType == 0) {
                            JToken killEvent = _MakeRandomKill();
                            _EventQueue.Queue(killEvent);
                        } else if (eventType == 1 || eventType == 2) {
                            JToken expEvent = _MakeRandomExp();
                            _EventQueue.Queue(expEvent);
                        } else {
                            _Logger.LogWarning($"Unhandled random event type {eventType}");
                        }
                    }

                    int secondDelay = _Random.Next(5);
                    await Task.Delay(secondDelay * 1000, stoppingToken);
                } catch (Exception ex) when (stoppingToken.IsCancellationRequested == false) {
                    _Logger.LogError(ex, "Error in offline mock data builder");
                } catch (Exception) when (stoppingToken.IsCancellationRequested == true) {
                    _Logger.LogInformation($"Stopping offline data mocker");
                }
            }
        }

        private PsCharacter _GetRandomCharacter() => _RandomCharacters[_Random.Next(_RandomCharacters.Count - 1)];

        private string _TokenBase(string payload) {
            return string.Format(@"
                {{
                    ""type"": ""serviceMessage"",
                    ""payload"": {{
                        {0},
                        ""zone_id"": ""1"",
                        ""world_id"": ""1"",
                        ""timestamp"": ""{1}""
                    }}
                }} 
            ", payload, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        private short _GetDefaultLoadoutID(short factionID) {
            if (factionID == Faction.VS) {
                return Loadout.VS_INFILTRATOR;
            } else if (factionID == Faction.NC) {
                return Loadout.NC_INFILTRATOR;
            } else if (factionID == Faction.TR) {
                return Loadout.TR_INFILTRATOR;
            } else if (factionID == Faction.NS) {
                return Loadout.NS_INFILTRATOR;
            }
            return -1;
        }

        private JToken _MakeRandomExp() {
            PsCharacter source = _GetRandomCharacter();

            int expType = _Random.Next(3);

            string otherID = "";
            List<int> eventTypes = new List<int>();

            if (expType == 0) { // Support
                eventTypes = new List<int>() {
                    Experience.HEAL, Experience.SQUAD_HEAL,
                    Experience.REVIVE, Experience.SQUAD_REVIVE,
                    Experience.RESUPPLY, Experience.SQUAD_RESUPPLY
                };

                PsCharacter? supported = null;
                foreach (PsCharacter c in _RandomCharacters) {
                    if (c.FactionID == source.FactionID && c.ID != source.ID) {
                        supported = c;
                        break;
                    }
                }

                if (supported != null) {
                    otherID = supported.ID;
                }
            } else if (expType == 1) { // Assist
                eventTypes = new List<int>() {
                    Experience.ASSIST, Experience.SPAWN_ASSIST,
                    Experience.PRIORITY_ASSIST, Experience.HIGH_PRIORITY_ASSIST
                };
            } else if (expType == 2) { // Vehicle kill
                eventTypes = Experience.VehicleKillEvents;
            }

            int experienceID = eventTypes[_Random.Next(eventTypes.Count - 1)];
            short loadoutID = _GetDefaultLoadoutID(source.FactionID);

            string payload = $@"
                ""event_name"": ""GainExperience"",
                ""character_id"": ""{source.ID}"",
                ""experience_id"": ""{experienceID}"",
                ""other_id"": ""{otherID}"",
                ""loadout_id"": ""{loadoutID}"",
                ""amount"": ""0""
            ";
            string token = _TokenBase(payload);

            JToken expEvent = JToken.Parse(token);
            return expEvent;
        }

        private JToken _MakeRandomKill() {
            PsCharacter attacker = _GetRandomCharacter();
            PsCharacter killed = _GetRandomCharacter();

            short attackerLoadoutID = _GetDefaultLoadoutID(attacker.FactionID);
            short killedLoadoutID = _GetDefaultLoadoutID(killed.FactionID);

            string isHeadshot = (_Random.Next(2) == 0) ? "1" : "0";

            string payload = $@"
                ""event_name"": ""Death"",
                ""character_id"": ""{killed.ID}"",
                ""attacker_character_id"": ""{attacker.ID}"",
                ""attacker_loadout_id"": ""{attackerLoadoutID}"",
                ""character_loadout_id"": ""{killedLoadoutID}"",
                ""attacker_weapon_id"": ""0"",
                ""attacker_fire_mode_id"": ""0"",
                ""attacker_vehicle_id"": ""0"",
                ""is_headshot"": ""{isHeadshot}""
            ";
            string token = _TokenBase(payload);

            JToken killEvent = JToken.Parse(token);
            return killEvent;
        }

    }

}