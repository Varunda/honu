using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public class DirectiveCollectionsPopulator : IHostedService {

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

        public async Task StartAsync(CancellationToken cancellationToken) {
            try {
                Stopwatch timer = Stopwatch.StartNew();

                List<PsDirective> censusDirs = await _DirectiveCensus.GetAll();
                List<PsDirective> dbDirs = await _DirectiveDb.GetAll();

                _Logger.LogDebug($"Directives: got {censusDirs.Count} from Census, have {dbDirs.Count} in DB");
                if (censusDirs.Count > dbDirs.Count) {
                    foreach (PsDirective dir in censusDirs) {
                        await _DirectiveDb.Upsert(dir);
                    }
                }

                List<DirectiveTree> censusTrees = await _DirectiveTreeCensus.GetAll();
                List<DirectiveTree> dbTrees = await _DirectiveTreeDb.GetAll();

                _Logger.LogDebug($"Trees: got {censusTrees.Count} from Census, have {dbTrees.Count} in DB");
                if (censusTrees.Count > dbTrees.Count) {
                    foreach (DirectiveTree tree in censusTrees) {
                        await _DirectiveTreeDb.Upsert(tree);
                    }
                }

                List<DirectiveTier> censusTiers = await _DirectiveTierCensus.GetAll();
                List<DirectiveTier> dbTiers = await _DirectiveTierDb.GetAll();

                _Logger.LogDebug($"Tiers: got {censusTiers.Count} from Census, have {dbTiers.Count} in DB");
                if (censusTiers.Count > dbTiers.Count) {
                    foreach (DirectiveTier tree in censusTiers) {
                        await _DirectiveTierDb.Upsert(tree);
                    }
                }

                List<DirectiveTreeCategory> censusTreeCategorys = await _DirectiveTreeCategoryCensus.GetAll();
                List<DirectiveTreeCategory> dbTreeCategorys = await _DirectiveTreeCategoryDb.GetAll();

                _Logger.LogDebug($"TreeCategorys: got {censusTreeCategorys.Count} from Census, have {dbTreeCategorys.Count} in DB");
                if (censusTreeCategorys.Count > dbTreeCategorys.Count) {
                    foreach (DirectiveTreeCategory tree in censusTreeCategorys) {
                        await _DirectiveTreeCategoryDb.Upsert(tree);
                    }
                }

                _Logger.LogDebug($"Finished directive populator in {timer.ElapsedMilliseconds}ms");
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to populate directive tables");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }

    }
}
