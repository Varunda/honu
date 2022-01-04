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

    public class OutfitMemberFixerStartupService : BackgroundService {

        private readonly ILogger<OutfitMemberFixerStartupService> _Logger;
        private readonly OutfitDbStore _OutfitDb;
        private readonly OutfitCollection _OutfitCensus;

        public OutfitMemberFixerStartupService(ILogger<OutfitMemberFixerStartupService> logger,
            OutfitDbStore outfitDb, OutfitCollection outfitCensus) {

            _Logger = logger;
            _OutfitDb = outfitDb;
            _OutfitCensus = outfitCensus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            List<PsOutfit> outfits;

            try {
                outfits = await _OutfitDb.GetAll();
            } catch (Exception ex) {
                _Logger.LogError(ex, "failed to get all outfits");
                return;
            }

            outfits = outfits.Where(iter => iter.MemberCount == 0).ToList();

            _Logger.LogInformation($"Have {outfits.Count} outfits with 0 members");

            int found = 0;
            int missing = 0;
            int errored = 0;
            int index = 0;

            Stopwatch timer = Stopwatch.StartNew();

            foreach (PsOutfit outfit in outfits) {
                try {
                    PsOutfit? censusOutfit = await _OutfitCensus.GetByID(outfit.ID);

                    if (censusOutfit != null) {
                        ++found;
                        await _OutfitDb.Upsert(censusOutfit);
                    } else {
                        ++missing;
                    }
                } catch (Exception ex) {
                    _Logger.LogError(ex, $"failed to update outfit {outfit.ID}");
                    ++errored;
                }

                ++index;
                if (index % 100 == 0) {
                    _Logger.LogDebug($"Completed {index}/{outfits.Count} outfits. Found {found}, missing {missing}, errored {errored}");
                }
            }

            _Logger.LogInformation($"Fixed {outfits.Count} outfits in {timer.ElapsedMilliseconds}ms. Found {found}, missing {missing}, errored {errored}");
        }

    }
}
