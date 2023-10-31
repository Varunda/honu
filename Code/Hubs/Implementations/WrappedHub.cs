using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Census;
using watchtower.Models.Events;
using watchtower.Models.Wrapped;
using watchtower.Services.Db;
using watchtower.Services.Hosted;
using watchtower.Services.Queues;
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class WrappedHub : Hub<IWrappedHub> {

        private readonly ILogger<WrappedHub> _Logger;
        private readonly WrappedSavedCharacterDataFileRepository _WrappedDataRepository;
        private readonly WrappedDbStore _WrappedDb;
        private readonly HostedWrappedGenerationProcess _WrappedProcessor;
        private readonly WrappedGenerationQueue _WrappedQueue;

        public WrappedHub(ILogger<WrappedHub> logger,
            WrappedSavedCharacterDataFileRepository wrappedDataRepository, WrappedDbStore wrappedDb,
            HostedWrappedGenerationProcess wrappedProcessor, WrappedGenerationQueue wrappedQueue) {

            _Logger = logger;
            _WrappedDataRepository = wrappedDataRepository;
            _WrappedDb = wrappedDb;
            _WrappedProcessor = wrappedProcessor;
            _WrappedQueue = wrappedQueue;
        }

        public async Task JoinGroup(Guid ID) {
            string connId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connId, $"wrapped-{ID}");

            WrappedEntry? entry = await _WrappedDb.GetByID(ID);

            if (entry == null) {
                await Clients.Caller.SendError($"{nameof(WrappedEntry)} {ID} does not exist");
                return;
            }

            IWrappedHub group = Clients.Group($"wrapped-{entry.ID}");

            await group.SendWrappedEntry(entry);

            _Logger.LogDebug($"ConnectionId {connId} joined group {ID}, which has status {entry.Status}");

            // NOT STARTED
            if (entry.Status == WrappedEntryStatus.NOT_STARTED) {
                _Logger.LogDebug($"putting {entry.ID} into queue");
                _WrappedQueue.Queue(entry);

                await group.UpdateStatus(WrappedStatus.PENDING_CREATION);

                Dictionary<Guid, int> poses = _WrappedQueue.GetQueuePositions();
                if (poses.TryGetValue(entry.ID, out int position) == true) {
                    await group.SendQueuePosition(position, poses.Count);
                } else {
                    _Logger.LogWarning($"missing position for {entry.ID}");
                }

                return;
            }

            // IN PROGRESS
            if (entry.Status == WrappedEntryStatus.IN_PROGRESS) {
                Dictionary<Guid, int> poses = _WrappedQueue.GetQueuePositions();
                if (poses.TryGetValue(entry.ID, out int position) == true) {
                    await group.SendQueuePosition(position, poses.Count);
                } else {
                    _Logger.LogWarning($"missing position for {entry.ID}");
                }

                await group.UpdateStatus(WrappedStatus.STARTED);

                return;
            }

            // DONE
            if (entry.Status == WrappedEntryStatus.DONE) {
                _Logger.LogDebug($"Loading {entry.InputCharacterIDs.Count} characters in report {entry.ID}");
                await Clients.Group($"wrapped-{entry.ID}").UpdateStatus(WrappedStatus.LOADING_EVENT_DATA);

                foreach (string charID in entry.InputCharacterIDs) {
                    // timestamp is when the entry was created, which would be for the previous year
                    // so if it was generated in 2023, then we want the data from 2022
                    WrappedSavedCharacterData? data = await _WrappedDataRepository.Get(entry.Timestamp.AddYears(-1), charID);
                    if (data == null) {
                        string err = $"Missing character {charID} from {nameof(WrappedEntry)} {entry.ID}! this wrapped is DONE, but missing character data";
                        _Logger.LogError(err);
                        await Clients.Group($"wrapped-{entry.ID}").SendError(err);
                        continue;
                    }

                    entry.Kills.AddRange(data.Kills);
                    entry.Deaths.AddRange(data.Deaths);
                    entry.Experience.AddRange(data.Experience);
                    entry.VehicleDestroy.AddRange(data.VehicleDestroy);
                    entry.ControlEvents.AddRange(data.ControlEvents);
                    entry.ItemAddedEvents.AddRange(data.ItemAddedEvents);
                    entry.AchievementEarned.AddRange(data.AchievementEarned);
                    entry.Sessions.AddRange(data.Sessions);
                }

                await _WrappedProcessor.SendEventData(entry);
                await _WrappedProcessor.SendStaticData(entry);

                await Clients.Group($"wrapped-{entry.ID}").UpdateStatus(WrappedStatus.DONE);

                return;
            }

            throw new Exception($"Unchecked status of {nameof(WrappedEntry)} {ID}: {entry.Status}");
        }

        public async Task GetQueuePositions() {
            Dictionary<Guid, int> pos = _WrappedQueue.GetQueuePositions();

            string s = "";

            foreach (KeyValuePair<Guid, int> iter in pos) {
                s += $"<{iter.Key}, {iter.Value}> ";
            }

            await Clients.Caller.SendMessage(s);
        }

    }
}
