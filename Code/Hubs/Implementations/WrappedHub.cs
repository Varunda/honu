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
using watchtower.Services.Repositories;

namespace watchtower.Code.Hubs.Implementations {

    public class WrappedHub : Hub<IWrappedHub> {

        private readonly ILogger<WrappedHub> _Logger;
        private readonly WrappedSavedCharacterDataFileRepository _WrappedDataRepository;
        private readonly WrappedDbStore _WrappedDb;
        private readonly HostedWrappedGenerationProcess _WrappedProcessor;

        public WrappedHub(ILogger<WrappedHub> logger,
            WrappedSavedCharacterDataFileRepository wrappedDataRepository, WrappedDbStore wrappedDb,
            HostedWrappedGenerationProcess wrappedProcessor) {

            _Logger = logger;
            _WrappedDataRepository = wrappedDataRepository;
            _WrappedDb = wrappedDb;
            _WrappedProcessor = wrappedProcessor;
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
                return;
            }

            // IN PROGRESS
            if (entry.Status == WrappedEntryStatus.IN_PROGRESS) {
                return;
            }

            // DONE
            if (entry.Status == WrappedEntryStatus.DONE) {
                _Logger.LogDebug($"Loading {entry.InputCharacterIDs.Count} characters in report {entry.ID}");
                foreach (string charID in entry.InputCharacterIDs) {
                    // timestamp is when the entry was created, which would be for the previous year
                    // so if it was generated in 2023, then we want the data from 2022
                    WrappedSavedCharacterData? data = await _WrappedDataRepository.Get(entry.Timestamp.AddYears(-1), charID);
                    if (data == null) {
                        _Logger.LogError($"Missing character {charID} from {nameof(WrappedEntry)} {entry.ID}");
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

    }
}
