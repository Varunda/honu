using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Census;
using watchtower.Models;

namespace watchtower.Services {

    public class FileEventLoader : BackgroundService {

        private const string _EventsFile = "PreviousEvents.json";

        private readonly ILogger<FileEventLoader> _Logger;
        private readonly ICharacterCollection _Characters;

        public FileEventLoader(ILogger<FileEventLoader> logger,
            ICharacterCollection charCollection) {

            _Logger = logger;

            _Characters = charCollection;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                string json = await File.ReadAllTextAsync(_EventsFile);

                JToken root = JToken.Parse(json);

                EventStore store = new EventStore();

                // Dunno why I need a force here
                try {
                    store = root.ToObject<EventStore>()!;
                } catch (Exception ex) {
                    _Logger.LogError(ex, "Failed to turn JSON to EventStore");
                    return;
                }

                EventStore.Get().TrKills.AddRange(store.TrKills);
                EventStore.Get().TrHeals.AddRange(store.TrHeals);
                EventStore.Get().TrRevives.AddRange(store.TrRevives);
                EventStore.Get().TrResupplies.AddRange(store.TrResupplies);
                EventStore.Get().TrRepairs.AddRange(store.TrRepairs);

                EventStore.Get().NcKills.AddRange(store.NcKills);
                EventStore.Get().NcHeals.AddRange(store.NcHeals);
                EventStore.Get().NcRevives.AddRange(store.NcRevives);
                EventStore.Get().NcResupplies.AddRange(store.NcResupplies);
                EventStore.Get().NcRepairs.AddRange(store.NcRepairs);

                EventStore.Get().VsKills.AddRange(store.VsKills);
                EventStore.Get().VsHeals.AddRange(store.VsHeals);
                EventStore.Get().VsRevives.AddRange(store.VsRevives);
                EventStore.Get().VsResupplies.AddRange(store.VsResupplies);
                EventStore.Get().VsRepairs.AddRange(store.VsRepairs);

                List<string> charIDs = store.TrKills.Select(i => i.CharacterID).ToList();
                charIDs.AddRange(store.TrKills.Select(i => i.CharacterID));
                charIDs.AddRange(store.TrHeals.Select(i => i.CharacterID));
                charIDs.AddRange(store.TrRevives.Select(i => i.CharacterID));
                charIDs.AddRange(store.TrResupplies.Select(i => i.CharacterID));
                charIDs.AddRange(store.TrRepairs.Select(i => i.CharacterID));
                charIDs.AddRange(store.NcKills.Select(i => i.CharacterID));
                charIDs.AddRange(store.NcHeals.Select(i => i.CharacterID));
                charIDs.AddRange(store.NcRevives.Select(i => i.CharacterID));
                charIDs.AddRange(store.NcResupplies.Select(i => i.CharacterID));
                charIDs.AddRange(store.NcRepairs.Select(i => i.CharacterID));
                charIDs.AddRange(store.VsKills.Select(i => i.CharacterID));
                charIDs.AddRange(store.VsHeals.Select(i => i.CharacterID));
                charIDs.AddRange(store.VsRevives.Select(i => i.CharacterID));
                charIDs.AddRange(store.VsResupplies.Select(i => i.CharacterID));
                charIDs.AddRange(store.VsRepairs.Select(i => i.CharacterID));
                charIDs = charIDs.Distinct().ToList();

                _ = _Characters.CacheBlock(charIDs);
            } catch (FileNotFoundException) {
                _Logger.LogInformation("No previous events found in '{File}'", _EventsFile);
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to perform FileEventLoader");
            }
        }

    }
}
