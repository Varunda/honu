using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Hubs;
using watchtower.Code.Hubs.Implementations;
using watchtower.Code.Tracking;
using watchtower.Models;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Wrapped;
using watchtower.Services.Db;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Services.Hosted {

    public class HostedWrappedGenerationProcess : BackgroundService {

        private const string SERVICE_NAME = "wrapped_generation";

        private readonly ILogger<HostedWrappedGenerationProcess> _Logger;
        private readonly WrappedGenerationQueue _Queue;
        private readonly IServiceHealthMonitor _ServiceHealthMonitor;

        private readonly IHubContext<WrappedHub, IWrappedHub> _Hub;

        private readonly CharacterRepository _CharacterRepository;
        private readonly OutfitRepository _OutfitRepository;
        private readonly ItemRepository _ItemRepository;
        private readonly AchievementRepository _AchievementRepository;
        private readonly WrappedSavedCharacterDataFileRepository _WrappedCharacterDataRepository;
        private readonly FacilityRepository _FacilityRepository;
        private readonly ExperienceTypeRepository _ExpTypeRepository;
        private readonly FireGroupToFireModeRepository _FireGroupXrefRepository;
        private readonly VehicleRepository _VehicleRespository;

        private readonly KillEventDbStore _KillDb;
        private readonly ExpEventDbStore _ExpDb;
        private readonly VehicleDestroyDbStore _VehicleDestroyDb;
        private readonly FacilityControlDbStore _FacilityControlDb;
        private readonly FacilityPlayerControlDbStore _FacilityPlayerControlDb;
        private readonly AchievementEarnedDbStore _AchievementEarnedDb;
        private readonly ItemAddedDbStore _ItemAddedDb;
        private readonly SessionDbStore _SessionDb;
        private readonly WrappedDbStore _WrappedDb;

        public HostedWrappedGenerationProcess(ILogger<HostedWrappedGenerationProcess> logger,
            WrappedGenerationQueue queue, IServiceHealthMonitor serviceHealthMonitor,
            IHubContext<WrappedHub, IWrappedHub> hub, CharacterRepository characterRepository,
            OutfitRepository outfitRepository, ItemRepository itemRepository,
            AchievementRepository achievementRepository, KillEventDbStore killDb,
            ExpEventDbStore expDb, VehicleDestroyDbStore vehicleDestroyDb,
            FacilityControlDbStore facilityControlDb, FacilityPlayerControlDbStore facilityPlayerControlDb,
            AchievementEarnedDbStore achievementEarnedDb, ItemAddedDbStore itemAddedDb,
            SessionDbStore sessionDb, WrappedSavedCharacterDataFileRepository wrappedCharacterDataRepository,
            WrappedDbStore wrappedDb, FacilityRepository facilityRepository,
            ExperienceTypeRepository expTypeRepository, FireGroupToFireModeRepository fireGroupXrefRepository,
            VehicleRepository vehicleRespository) {

            _Logger = logger;
            _Queue = queue;
            _ServiceHealthMonitor = serviceHealthMonitor;
            _Hub = hub;

            _CharacterRepository = characterRepository;
            _OutfitRepository = outfitRepository;
            _ItemRepository = itemRepository;
            _AchievementRepository = achievementRepository;
            _WrappedCharacterDataRepository = wrappedCharacterDataRepository;

            _KillDb = killDb;
            _ExpDb = expDb;
            _VehicleDestroyDb = vehicleDestroyDb;
            _FacilityControlDb = facilityControlDb;
            _FacilityPlayerControlDb = facilityPlayerControlDb;
            _AchievementEarnedDb = achievementEarnedDb;
            _ItemAddedDb = itemAddedDb;
            _SessionDb = sessionDb;
            _WrappedDb = wrappedDb;
            _FacilityRepository = facilityRepository;
            _ExpTypeRepository = expTypeRepository;
            _FireGroupXrefRepository = fireGroupXrefRepository;
            _VehicleRespository = vehicleRespository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            _Logger.LogInformation($"started");

            while (stoppingToken.IsCancellationRequested == false) {
                WrappedEntry? entry = null;

                try {
                    ServiceHealthEntry serviceHealthEntry = _ServiceHealthMonitor.Get(SERVICE_NAME) ?? new() { Name = SERVICE_NAME };

                    if (serviceHealthEntry.Enabled == false) {
                        await Task.Delay(1000, stoppingToken);
                        continue;
                    }

                    entry = await _Queue.Dequeue(stoppingToken);
                } catch (Exception ex) {
                    _Logger.LogError(ex, "failed to fetch queue entry");
                }

                if (entry == null) {
                    _Logger.LogDebug($"entry is null, continuing");
                    continue;
                }

                try {
                    _Logger.LogDebug($"Starting entry {entry.ID} with {entry.InputCharacterIDs.Count} input characters: [{string.Join(", ", entry.InputCharacterIDs)}]");

                    Stopwatch timer = Stopwatch.StartNew();
                    await ProcessEntry(entry);
                    _Logger.LogInformation($"Processed wrapped entry {entry.ID} in {timer.ElapsedMilliseconds}ms");
                    _Queue.AddProcessTime(timer.ElapsedMilliseconds);
                } catch (Exception ex) {
                    _Logger.LogDebug(ex, $"unhandled exception while processing entry {entry.ID}");
                }

                try {
                    Dictionary<Guid, int> poses = _Queue.GetQueuePositions();

                    List<Task> updates = new(poses.Count);
                    foreach (KeyValuePair<Guid, int> iter in poses) {
                        updates.Add(_Hub.Clients.Group($"wrapped-{iter.Key}").SendQueuePosition(iter.Value, poses.Count));
                    }
                    await Task.WhenAll(updates);
                } catch (Exception ex) {
                    _Logger.LogError($"failed to update queue positions after de-queueing {entry.ID}", ex);
                }
            }
        }

        private async Task HubUpdateStatus(WrappedEntry entry, string status) {
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateStatus(status);
        }

        /// <summary>
        ///     process a single wrapped entry
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        protected async Task ProcessEntry(WrappedEntry entry) {
            entry.Status = WrappedEntryStatus.IN_PROGRESS;
            await _WrappedDb.UpdateStatus(entry.ID, entry.Status);

            // STARTED
            await HubUpdateStatus(entry, WrappedStatus.STARTED);

            if (entry.ID == Guid.Empty) {
                _Logger.LogError($"Got an empty ID when processing entry {entry.ID}");
                return;
            }

            if (entry.InputCharacterIDs.Count == 0) {
                await _Hub.Clients.Group($"{entry.ID}").SendError($"This wrapped entry had 0 characters input");
                _Logger.LogWarning($"No characters for entry {entry.ID}");
                return;
            }

            // LOADING_INPUT_CHARACTERS
            await HubUpdateStatus(entry, WrappedStatus.LOADING_INPUT_CHARACTERS);
            List<PsCharacter> inputCharacters = await _CharacterRepository.GetByIDs(entry.InputCharacterIDs, CensusEnvironment.PC, fast: false);
            _Logger.LogDebug($"Loaded {inputCharacters.Count} of {entry.InputCharacterIDs.Count} requested");
            AddCharactersToEntry(inputCharacters, entry);

            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateInputCharacters(inputCharacters);

            foreach (PsCharacter c in inputCharacters) {
                try {
                    await Task.Delay(10000);

                    WrappedSavedCharacterData data = await ProcessCharacter(entry, c);
                    entry.Kills.AddRange(data.Kills);
                    entry.Deaths.AddRange(data.Deaths);
                    entry.Experience.AddRange(data.Experience);
                    entry.VehicleDestroy.AddRange(data.VehicleDestroy);
                    entry.ControlEvents.AddRange(data.ControlEvents);
                    entry.ItemAddedEvents.AddRange(data.ItemAddedEvents);
                    entry.AchievementEarned.AddRange(data.AchievementEarned);
                    entry.Sessions.AddRange(data.Sessions);
                } catch (Exception ex) {
                    _Logger.LogError($"failed to process character {c.Name}/{c.ID}", ex);
                    entry.Status = WrappedEntryStatus.NOT_STARTED;
                    await _WrappedDb.UpdateStatus(entry.ID, entry.Status);
                }

            }

            await SendEventData(entry);

            await SendStaticData(entry);

            entry.Status = WrappedEntryStatus.DONE;
            await _WrappedDb.UpdateStatus(entry.ID, entry.Status);
        }

        public async Task SendEventData(WrappedEntry entry) {
            _Logger.LogDebug($"sending event data for {entry.ID}");
            IWrappedHub group = _Hub.Clients.Group($"wrapped-{entry.ID}");

            await group.UpdateStatus(WrappedStatus.LOADING_EVENT_DATA);

            await group.SendSessions(entry.Sessions);
            await group.SendKills(entry.Kills);
            await group.SendDeaths(entry.Deaths);
            await group.SendExp(entry.Experience);
            await group.SendFacilityControl(entry.ControlEvents);
            await group.SendItemAdded(entry.ItemAddedEvents);
            await group.SendAchievementEarned(entry.AchievementEarned);
            await group.SendVehicleDestroy(entry.VehicleDestroy);
        }

        /// <summary>
        ///     Send the static data (characters, items, etc.)
        /// </summary>
        /// <param name="entry">WrappedEntry</param>
        /// <returns>
        ///     A task when the static data has been sent
        /// </returns>
        public async Task SendStaticData(WrappedEntry entry) {
            _Logger.LogDebug($"sending static data for {entry.ID}");
            WrappedEntryIdSet idSet = WrappedEntryIdSet.FromEntry(entry);

            await HubUpdateStatus(entry, WrappedStatus.LOADING_STATIC_DATA);

            // LOADING CHARACTERS
            List<PsCharacter> chars = await _CharacterRepository.GetByIDs(idSet.Characters, CensusEnvironment.PC, fast: true);
            entry.Characters = chars.ToDictionary(iter => iter.ID);

            // LOADING OUTFITS
            HashSet<string> outfitIDs = new(chars.Where(iter => iter.OutfitID != null).Select(iter => iter.OutfitID!));
            List<PsOutfit> outfits = await _OutfitRepository.GetByIDs(outfitIDs.ToList());
            entry.Outfits = outfits.ToDictionary(iter => iter.ID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateOutfits(outfits);

            // also load the leaders of the outfits, as it's how the world and faction of an outfit is loaded
            HashSet<string> leaderOutfitIDs = new(outfits.Select(iter => iter.LeaderID));
            List<PsCharacter> outfitLeaders = await _CharacterRepository.GetByIDs(leaderOutfitIDs, CensusEnvironment.PC);
            foreach (PsCharacter leader in outfitLeaders) {
                if (entry.Characters.ContainsKey(leader.ID) == false) {
                    entry.Characters.Add(leader.ID, leader);
                }
            }
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateCharacters(entry.Characters.Values.ToList());

            // LOADING ITEMS
            List<PsItem> items = await _ItemRepository.GetByIDs(idSet.Items);
            entry.Items = items.ToDictionary(iter => iter.ID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateItems(items);

            // LOADING VEHICLES
            List<PsVehicle> vehicles = await _VehicleRespository.GetAll();
            entry.Vehicles = vehicles.ToDictionary(iter => iter.ID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateVehicles(vehicles);

            // LOADING FACILITIES
            List<PsFacility> facilities = await _FacilityRepository.GetByIDs(idSet.Facilities);
            entry.Facilities = facilities.ToDictionary(iter => iter.FacilityID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateFacilities(facilities);

            // LOADING ACHIEVEMENTS
            List<Achievement> achs = await _AchievementRepository.GetByIDs(idSet.Achievements);
            entry.Achievements = achs.ToDictionary(iter => iter.ID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateAchievements(achs);

            // LOADING EXP TYPES
            List<ExperienceType> types = await _ExpTypeRepository.GetByIDs(idSet.ExperienceTypes);
            entry.ExperienceTypes = types.ToDictionary(iter => iter.ID);
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateExpTypes(types);

            // LOADING FIRE GROUP XREF
            List<FireGroupToFireMode> fireGroupXrefs = new();
            foreach (int fireModeID in idSet.FireGroupXrefs) {
                List<FireGroupToFireMode> fireGroups = await _FireGroupXrefRepository.GetByFireModeID(fireModeID);
                fireGroupXrefs.AddRange(fireGroups);
            }
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateFireGroupToFireMode(fireGroupXrefs);

            // DONE
            await _Hub.Clients.Group($"wrapped-{entry.ID}").UpdateStatus(WrappedStatus.DONE);
        }

        /// <summary>
        ///     Load the data of a single character
        /// </summary>
        /// <remarks>
        ///     First, a json file containing the data is checked for. If it exists and can be parsed correctly, it is assumed
        ///     it contains all the events of the character.
        ///     Otherwise, DB lookups are performed for each character
        /// </remarks>
        /// <param name="entry">Wrapped entry that includes this character</param>
        /// <param name="character">The character the data is being loaded for</param>
        /// <returns>
        ///     The data of a single character
        /// </returns>
        protected async Task<WrappedSavedCharacterData> ProcessCharacter(WrappedEntry entry, PsCharacter character) {
            DateTime yearStart = new(DateTime.UtcNow.Year - 1, 1, 1);
            DateTime yearEnd = new(DateTime.UtcNow.Year, 1, 1);

            using Activity? rootTrace = HonuActivitySource.Root.StartActivity("Wrapped - Character");
            rootTrace?.AddTag("honu.wrapped.character_id", character.ID);
            rootTrace?.AddTag("honu.wrapped.character_name", character.Name);
            rootTrace?.AddTag("honu.wrapped.year_start", $"{yearStart:u}");
            rootTrace?.AddTag("honu.wrapped.year_end", $"{yearEnd:u}");

            _Logger.LogDebug($"Character {character.ID}/{character.Name} will go from {yearStart:u} to {yearEnd:u}");

            WrappedSavedCharacterData? data = await _WrappedCharacterDataRepository.Get(yearStart, character.ID);

            if (data == null) {
                data = new WrappedSavedCharacterData();

                data.CharacterID = character.ID;

                // CHARACTER SESSIONS
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - session")) {
                    data.Sessions = await Retry(() => _SessionDb.GetByRangeAndCharacterID(character.ID, yearStart, yearEnd));
                    trace?.AddTag("honu.count", data.Sessions.Count);
                }

                // CHARACTER KILLS
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - kills")) {
                    data.Kills = await Retry(() => _KillDb.LoadWrappedKills(character.ID, yearStart));
                    trace?.AddTag("honu.count", data.Kills.Count);
                }

                // CHARACTER DEATHS
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - deaths")) {
                    data.Deaths = await Retry(() => _KillDb.LoadWrappedDeaths(character.ID, yearStart));
                    trace?.AddTag("honu.count", data.Deaths.Count);
                }

                // CHARACTER EXPERIENCE
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - exp")) {
                    data.Experience = await Retry(() => _ExpDb.LoadWrapped(character.ID, yearStart));
                    trace?.AddTag("honu.count", data.Experience.Count);
                }

                // CHARACTER VEHICLE DESTROY
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - vehicle destroy")) {
                    data.VehicleDestroy = await Retry(() => _VehicleDestroyDb.LoadWrappedKills(character.ID, yearStart));
                    data.VehicleDestroy.AddRange(await Retry(() => _VehicleDestroyDb.LoadWrappedDeaths(character.ID, yearStart)));
                    trace?.AddTag("honu.count", data.VehicleDestroy.Count);
                }

                // CHARACTER CONTROL
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - control")) {
                    List<PlayerControlEvent> playerControlEvents = await Retry(() => _FacilityPlayerControlDb.LoadWrapped(character.ID, yearStart));
                    HashSet<long> playerControlEventIDs = new(playerControlEvents.Select(iter => iter.ControlID));
                    data.ControlEvents = await Retry(() => _FacilityControlDb.GetByIDs(playerControlEventIDs.ToList()));
                    trace?.AddTag("honu.count", data.ControlEvents.Count);
                }

                // CHARACTER ACHIEVEMENT EARNED
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - achievement earned")) {
                    data.AchievementEarned = await Retry(() => _AchievementEarnedDb.LoadWrapped(character.ID, yearStart));
                    trace?.AddTag("honu.count", data.AchievementEarned.Count);
                }

                // CHARACTER ITEM ADDED
                using (Activity? trace = HonuActivitySource.Root.StartActivity("Wrapped - item added")) {
                    data.ItemAddedEvents = await Retry(() => _ItemAddedDb.LoadWrapped(character.ID, yearStart));
                    trace?.AddTag("honu.count", data.ItemAddedEvents.Count);
                }

                await _WrappedCharacterDataRepository.Save(character.ID, yearStart, data);
            }

            return data;
        }

        private void AddCharacterToEntry(PsCharacter c, WrappedEntry entry) {
            if (entry.Characters.ContainsKey(c.ID) == false) {
                entry.Characters.Add(c.ID, c);
            }
        }

        private void AddCharactersToEntry(List<PsCharacter> chars, WrappedEntry entry) {
            foreach (PsCharacter c in chars) {
                AddCharacterToEntry(c, entry);
            }
        }

        /// <summary>
        ///     retry a call that may fail a number of times. Pass a lambda in
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="action">Lambda to be called</param>
        /// <param name="retryCount">How many times to retry, defaults to 3 tries</param>
        /// <returns>The called function, retried if necessary</returns>
        /// <exception cref="Exception"></exception>
        private async Task<T> Retry<T>(Func<Task<T>> action, int retryCount = 3) {
            for (int i = 0; i <= retryCount; ++i) {
                try {
                    T datum = await action();
                    return datum;
                } catch (Exception ex) {
                    if (i >= retryCount) {
                        throw;
                    }

                    _Logger.LogError(ex, $"error on try {i} of retry");
                }
            }

            throw new Exception($"how did we get here");
        }

    }
}
