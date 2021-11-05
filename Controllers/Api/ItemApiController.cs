using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Constants;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.CharacterViewer.WeaponStats;
using watchtower.Models.Db;
using watchtower.Services.Db;
using watchtower.Services.Repositories;

namespace watchtower.Controllers.Api {

    [ApiController]
    [Route("/api/item")]
    public class ItemApiController : ControllerBase {

        private readonly ILogger<ItemApiController> _Logger;

        private readonly IItemRepository _ItemRepository;
        private readonly IWeaponStatPercentileCacheDbStore _PercentileDb;
        private readonly ICharacterWeaponStatDbStore _StatDb;

        public ItemApiController(ILogger<ItemApiController> logger,
            IItemRepository itemRepo, IWeaponStatPercentileCacheDbStore percDb,
            ICharacterWeaponStatDbStore statDb) {

            _Logger = logger;

            _ItemRepository = itemRepo;
            _PercentileDb = percDb ?? throw new ArgumentNullException(nameof(percDb));
            _StatDb = statDb ?? throw new ArgumentNullException(nameof(statDb));
        }

        [HttpGet("{itemID}")]
        public async Task<ActionResult<PsItem>> GetByID(string itemID) {
            PsItem? item = await _ItemRepository.GetByID(itemID);
            if (item == null) {
                return NoContent();
            }
            return Ok(item);
        }

        [HttpGet]
        public async Task<ActionResult<List<PsItem>>> GetMultiple([FromQuery] List<string> IDs) {
            List<PsItem> items = new List<PsItem>();

            foreach (string ID in IDs) {
                PsItem? item = await _ItemRepository.GetByID(ID);
                if (item != null) {
                    items.Add(item);
                }
            }

            return Ok(items);
        }

        [HttpGet("{itemID}/percentile_stats")]
        public async Task<ActionResult<WeaponStatPercentileAll>> GetPercentileStats(string itemID) {
            List<WeaponStatEntry> entries = await _StatDb.GetByItemID(itemID, 1159);

            WeaponStatPercentileAll all = new WeaponStatPercentileAll();
            all.ItemID = itemID;
            all.KD = GetBuckets(entries.Select(iter => iter.KillDeathRatio).ToList(), 100);
            all.KPM = GetBuckets(entries.Select(iter => iter.KillsPerMinute).ToList(), 100);
            all.Accuracy = GetBuckets(entries.Select(iter => iter.Accuracy * 100).ToList(), 100);
            all.HeadshotRatio = GetBuckets(entries.Select(iter => iter.HeadshotRatio * 100).ToList(), 100);

            return Ok(all);
        }

        private List<Bucket> GetBuckets(List<double> values, int bucketCount) {
            List<double> sorted = values.OrderBy(iter => iter).ToList();

            int mi = MedianIndex(0, values.Count);
            double median = Median(sorted, 0, values.Count);

            //int q1i = MedianIndex(0, m2i);
            double q1 = Median(sorted, 0, mi);
            //int q3i = m2i + MedianIndex(m2i, values.Count);
            double q3 = Median(sorted, mi, values.Count);

            double iqr = q3 - q1;

            sorted = sorted.Where(iter => iter < q3 + (iqr * 8)).ToList();

            double max = sorted.Last();
            double min = sorted.First();

            //_Logger.LogInformation($"{min}/0 {q1}/{q1i} {median2}/{m2i} {q3}/{q3i} {max}/{values.Count - 1}, iqr = {iqr}");

            double bucketWidth = (max - min) / bucketCount;

            List<Bucket> buckets = new List<Bucket>();

            Bucket iter = new Bucket() {
                Start = min,
                Width = bucketWidth,
                Count = 0
            };

            for (double i = min; i <= max; i += bucketWidth) {
                int count = sorted.Where(iter => (iter >= i && iter < i + bucketWidth)).Count();

                Bucket b = new Bucket() {
                    Start = i,
                    Width = bucketWidth,
                    Count = count
                };

                buckets.Add(b);

                //_Logger.LogInformation($"Bucket {i} - {i + bucketWidth} = {count}");
            }

            /*
            for (int i = 0; i < sorted.Count; ++i) {
                double entry = sorted[i];

                if (entry > iqr * 8) {
                    if (iter.Count > 0) {
                        buckets.Add(iter);
                    }
                    break;
                }

                if (entry > (iter.Start + iter.Width)) {
                    buckets.Add(iter);
                    iter = new Bucket() {
                        Start = entry,
                        Width = bucketWidth,
                        Count = 0
                    };
                }

                ++iter.Count;
            }
            */

            return buckets;
        }

        private static double Median(List<double> list, int lowerBound, int upperBound) {
            int median = MedianIndex(lowerBound, upperBound) + lowerBound;
            if (median % 2 == 0) {
                return list[median];
            }
            return (list[median + 1] + list[median]) / 2;
        }

        private static int MedianIndex(int lowerBound, int upperBound) {
            int n = upperBound - lowerBound + 1;
            n = (n + 1) / 2 - 1;
            return n;
        }

    }
}
