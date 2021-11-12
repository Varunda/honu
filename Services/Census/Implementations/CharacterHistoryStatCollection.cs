using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class CharacterHistoryStatCollection : ICharacterHistoryStatCollection {

        private readonly ILogger<CharacterHistoryStatCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public CharacterHistoryStatCollection(ILogger<CharacterHistoryStatCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        public async Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID) {
            CensusQuery query = _Census.Create("characters_stat_history");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            List<PsCharacterHistoryStat> stats = new List<PsCharacterHistoryStat>();

            try {
                IEnumerable<JToken> results = await query.GetListAsync();

                foreach (JToken token in results) {
                    PsCharacterHistoryStat stat = _Parse(token);
                    stats.Add(stat);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get all");
            }

            return stats;
        }

        private PsCharacterHistoryStat _Parse(JToken token) {
            PsCharacterHistoryStat stat = new PsCharacterHistoryStat();

            stat.CharacterID = token.GetString("character_id", "0");
            stat.Type = token.GetString("stat_name", "<missing>");
            stat.AllTime = token.GetInt32("all_time", 0);
            stat.OneLifeMax = token.GetInt32("one_life_max", 0);
            stat.LastUpdated = token.CensusTimestamp("last_save");

            JToken? days = token.SelectToken("day");
            if (days != null) {
                stat.Day1 = days.GetInt32("d01", 0);
                stat.Day2 = days.GetInt32("d02", 0);
                stat.Day3 = days.GetInt32("d03", 0);
                stat.Day4 = days.GetInt32("d04", 0);
                stat.Day5 = days.GetInt32("d05", 0);
                stat.Day6 = days.GetInt32("d06", 0);
                stat.Day7 = days.GetInt32("d07", 0);
                stat.Day8 = days.GetInt32("d08", 0);
                stat.Day9 = days.GetInt32("d09", 0);
                stat.Day10 = days.GetInt32("d10", 0);
                stat.Day11 = days.GetInt32("d11", 0);
                stat.Day12 = days.GetInt32("d12", 0);
                stat.Day13 = days.GetInt32("d13", 0);
                stat.Day14 = days.GetInt32("d14", 0);
                stat.Day15 = days.GetInt32("d15", 0);
                stat.Day16 = days.GetInt32("d16", 0);
                stat.Day17 = days.GetInt32("d17", 0);
                stat.Day18 = days.GetInt32("d18", 0);
                stat.Day19 = days.GetInt32("d19", 0);
                stat.Day20 = days.GetInt32("d20", 0);
                stat.Day21 = days.GetInt32("d21", 0);
                stat.Day22 = days.GetInt32("d22", 0);
                stat.Day23 = days.GetInt32("d23", 0);
                stat.Day24 = days.GetInt32("d24", 0);
                stat.Day25 = days.GetInt32("d25", 0);
                stat.Day26 = days.GetInt32("d26", 0);
                stat.Day27 = days.GetInt32("d27", 0);
                stat.Day28 = days.GetInt32("d28", 0);
                stat.Day29 = days.GetInt32("d29", 0);
                stat.Day30 = days.GetInt32("d30", 0);
                stat.Day31 = days.GetInt32("d31", 0);
            }

            JToken? months = token.SelectToken("month");
            if (months != null) {
                stat.Month1 = months.GetInt32("m01", 0);
                stat.Month2 = months.GetInt32("m02", 0);
                stat.Month3 = months.GetInt32("m03", 0);
                stat.Month4 = months.GetInt32("m04", 0);
                stat.Month5 = months.GetInt32("m05", 0);
                stat.Month6 = months.GetInt32("m06", 0);
                stat.Month7 = months.GetInt32("m07", 0);
                stat.Month8 = months.GetInt32("m08", 0);
                stat.Month9 = months.GetInt32("m09", 0);
                stat.Month10 = months.GetInt32("m10", 0);
                stat.Month11 = months.GetInt32("m11", 0);
                stat.Month12 = months.GetInt32("m12", 0);
            }

            JToken? weeks = token.SelectToken("week");
            if (weeks != null) {
                stat.Week1 = weeks.GetInt32("w01", 0);
                stat.Week2 = weeks.GetInt32("w02", 0);
                stat.Week3 = weeks.GetInt32("w03", 0);
                stat.Week4 = weeks.GetInt32("w04", 0);
                stat.Week5 = weeks.GetInt32("w05", 0);
                stat.Week6 = weeks.GetInt32("w06", 0);
                stat.Week7 = weeks.GetInt32("w07", 0);
                stat.Week8 = weeks.GetInt32("w08", 0);
                stat.Week9 = weeks.GetInt32("w09", 0);
                stat.Week10 = weeks.GetInt32("w10", 0);
                stat.Week11 = weeks.GetInt32("w11", 0);
                stat.Week12 = weeks.GetInt32("w12", 0);
                stat.Week13 = weeks.GetInt32("w13", 0);
            }

            return stat;
        }

    }
}
