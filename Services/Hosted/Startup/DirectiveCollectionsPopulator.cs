using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Services.Census;
using watchtower.Services.Db;

namespace watchtower.Services.Hosted.Startup {

    /// <summary>
    ///     Runs once at startup, populating all the static directive databases
    /// </summary>
    public class DirectiveCollectionsPopulator : BackgroundService {

        private readonly ILogger<DirectiveCollectionsPopulator> _Logger;

        private readonly DirectiveCollection _DirectiveCensus;
        private readonly DirectiveTreeCollection _DirectiveTreeCensus;
        private readonly DirectiveTierCollection _DirectiveTierCensus;
        private readonly DirectiveTreeCategoryCollection _DirectiveTreeCategoryCensus;
        private readonly DirectiveDbStore _DirectiveDb;
        private readonly DirectiveTreeDbStore _DirectiveTreeDb;
        private readonly DirectiveTierDbStore _DirectiveTierDb;
        private readonly DirectiveTreeCategoryDbStore _DirectiveTreeCategoryDb;

        public DirectiveCollectionsPopulator(ILogger<DirectiveCollectionsPopulator> logger,
            DirectiveCollection directive, DirectiveTreeCollection dirTree,
            DirectiveTierCollection dirTier, DirectiveTreeCategoryCollection dirTreeCat,
            DirectiveDbStore dirDb, DirectiveTreeDbStore treeDb,
            DirectiveTierDbStore tierDb, DirectiveTreeCategoryDbStore catDb) {

            _Logger = logger;

            _DirectiveCensus = directive ?? throw new ArgumentNullException(nameof(directive));
            _DirectiveTreeCensus = dirTree ?? throw new ArgumentNullException(nameof(dirTree));
            _DirectiveTierCensus = dirTier ?? throw new ArgumentNullException(nameof(dirTier));
            _DirectiveTreeCategoryCensus = dirTreeCat ?? throw new ArgumentNullException(nameof(dirTreeCat));
            _DirectiveDb = dirDb ?? throw new ArgumentNullException(nameof(dirDb));
            _DirectiveTreeDb = treeDb ?? throw new ArgumentNullException(nameof(treeDb));
            _DirectiveTierDb = tierDb ?? throw new ArgumentNullException(nameof(tierDb));
            _DirectiveTreeCategoryDb = catDb ?? throw new ArgumentNullException(nameof(catDb));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            Stopwatch timer = Stopwatch.StartNew();
            Stopwatch stepTimer = Stopwatch.StartNew();

            await UpdateStaticCollection(_DirectiveCensus, _DirectiveDb, (entry) => $"{entry.ID}", true, stoppingToken);
            if (stoppingToken.IsCancellationRequested == true) { return; }
            long directiveMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            await UpdateStaticCollection(_DirectiveTreeCategoryCensus, _DirectiveTreeCategoryDb, (entry) => $"{entry.ID}", true, stoppingToken);
            if (stoppingToken.IsCancellationRequested == true) { return; }
            long categoryMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            await UpdateStaticCollection(_DirectiveTreeCensus, _DirectiveTreeDb, (entry) => $"{entry.ID}", true, stoppingToken);
            if (stoppingToken.IsCancellationRequested) { return; }
            long treeMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            await UpdateStaticCollection(_DirectiveTierCensus, _DirectiveTierDb, (entry) => $"{entry.TreeID}:{entry.TierID}", true, stoppingToken);
            long tierMs = stepTimer.ElapsedMilliseconds; stepTimer.Restart();

            _Logger.LogDebug($"Finished directive populator in {timer.ElapsedMilliseconds}ms, directive {directiveMs}ms, category {categoryMs}ms, tree {treeMs}ms, tier {tierMs}ms");
        }

        protected async Task UpdateStaticCollection<T>(IStaticCollection<T> collection, IStaticDbStore<T> dbStore, Func<T, string> keyFunc, bool log, CancellationToken cancel) {
            List<T> census = await collection.GetAll();
            if (cancel.IsCancellationRequested == true) { return; }

            List<T> db = await dbStore.GetAll();
            if (cancel.IsCancellationRequested == true) { return; }

            _Logger.LogDebug($"{typeof(T).Name}: Census has {census.Count} entries, DB has {db.Count}");

            foreach (T entry in census) {
                string entryKey = keyFunc(entry);
                T? dbEntry = db.FirstOrDefault(iter => entryKey == keyFunc(iter));
                if (dbEntry == null) {
                    await dbStore.Upsert(entry);
                    if (log == true) {
                        _Logger.LogTrace($"New entry from Census {entryKey}");
                    }
                } else if (dbEntry.Equals(entry) == false) {
                    await dbStore.Upsert(entry);
                    if (log == true) {
                        _Logger.LogTrace($"Updated entry from Census {entryKey}\n{JToken.FromObject(entry!)}\n{JToken.FromObject(dbEntry!)}");
                    }
                }
            }
        }

    }
}
