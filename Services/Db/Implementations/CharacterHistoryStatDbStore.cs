using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Implementations {

    public class CharacterHistoryStatDbStore : IDataReader<PsCharacterHistoryStat>, ICharacterHistoryStatDbStore {

        private readonly ILogger<CharacterHistoryStatDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterHistoryStatDbStore(ILogger<CharacterHistoryStatDbStore> logger,
            IDbHelper helper) {

            _Logger = logger;
            _DbHelper = helper;
        }

        public async Task<List<PsCharacterHistoryStat>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_history_stat
                    WHERE character_id = @CharID;
            ");

            cmd.AddParameter("CharID", charID);

            List<PsCharacterHistoryStat> stats = await ReadList(cmd);
            await conn.CloseAsync();

            return stats;
        }

        public async Task<List<PsCharacterHistoryStat>> GetByCharacterIDs(List<string> IDs) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_history_stat
                    WHERE character_id = ANY(@IDs)
            ");

            cmd.AddParameter("IDs", IDs);

            List<PsCharacterHistoryStat> stats = await ReadList(cmd);
            await conn.CloseAsync();

            return stats;
        }

        public async Task Upsert(string charID, string type, PsCharacterHistoryStat stat) {
            // grrrr.....
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_history_stat (
                    character_id, type, timestamp, all_time, one_life_max,
                    day1,
                    day2,
                    day3,
                    day4,
                    day5,
                    day6,
                    day7,
                    day8,
                    day9,
                    day10,
                    day11,
                    day12,
                    day13,
                    day14,
                    day15,
                    day16,
                    day17,
                    day18,
                    day19,
                    day20,
                    day21,
                    day22,
                    day23,
                    day24,
                    day25,
                    day26,
                    day27,
                    day28,
                    day29,
                    day30,
                    day31,

                    week1,
                    week2,
                    week3,
                    week4,
                    week5,
                    week6,
                    week7,
                    week8,
                    week9,
                    week10,
                    week11,
                    week12,
                    week13,

                    month1,
                    month2,
                    month3,
                    month4,
                    month5,
                    month6,
                    month7,
                    month8,
                    month9,
                    month10,
                    month11,
                    month12
                ) VALUES (
                    @CharacterID, @Type, @Timestamp, @AllTime, @OneLifeMax,

                    @Day1,
                    @Day2,
                    @Day3,
                    @Day4,
                    @Day5,
                    @Day6,
                    @Day7,
                    @Day8,
                    @Day9,
                    @Day10,
                    @Day11,
                    @Day12,
                    @Day13,
                    @Day14,
                    @Day15,
                    @Day16,
                    @Day17,
                    @Day18,
                    @Day19,
                    @Day20,
                    @Day21,
                    @Day22,
                    @Day23,
                    @Day24,
                    @Day25,
                    @Day26,
                    @Day27,
                    @Day28,
                    @Day29,
                    @Day30,
                    @Day31,

                    @Week1,
                    @Week2,
                    @Week3,
                    @Week4,
                    @Week5,
                    @Week6,
                    @Week7,
                    @Week8,
                    @Week9,
                    @Week10,
                    @Week11,
                    @Week12,
                    @Week13,

                    @Month1,
                    @Month2,
                    @Month3,
                    @Month4,
                    @Month5,
                    @Month6,
                    @Month7,
                    @Month8,
                    @Month9,
                    @Month10,
                    @Month11,
                    @Month12
                ) ON CONFLICT (character_id, type) DO 
                    UPDATE SET timestamp = @Timestamp,
                        all_time = @AllTime,
                        one_life_max = @OneLifeMax,
                        day1 = @Day1,
                        day2 = @Day2,
                        day3 = @Day3,
                        day4 = @Day4,
                        day5 = @Day5,
                        day6 = @Day6,
                        day7 = @Day7,
                        day8 = @Day8,
                        day9 = @Day9,
                        day10 = @Day10,
                        day11 = @Day11,
                        day12 = @Day12,
                        day13 = @Day13,
                        day14 = @Day14,
                        day15 = @Day15,
                        day16 = @Day16,
                        day17 = @Day17,
                        day18 = @Day18,
                        day19 = @Day19,
                        day20 = @Day20,
                        day21 = @Day21,
                        day22 = @Day22,
                        day23 = @Day23,
                        day24 = @Day24,
                        day25 = @Day25,
                        day26 = @Day26,
                        day27 = @Day27,
                        day28 = @Day28,
                        day29 = @Day29,
                        day30 = @Day30,
                        day31 = @Day31,

                        week1 = @Week1,
                        week2 = @Week2,
                        week3 = @Week3,
                        week4 = @Week4,
                        week5 = @Week5,
                        week6 = @Week6,
                        week7 = @Week7,
                        week8 = @Week8,
                        week9 = @Week9,
                        week10 = @Week10,
                        week11 = @Week11,
                        week12 = @Week12,
                        week13 = @Week13,

                        month1 = @Month1,
                        month2 = @Month2,
                        month3 = @Month3,
                        month4 = @Month4,
                        month5 = @Month5,
                        month6 = @Month6,
                        month7 = @Month7,
                        month8 = @Month8,
                        month9 = @Month9,
                        month10 = @Month10,
                        month11 = @Month11,
                        month12 = @Month12;
            ");

            cmd.AddParameter("CharacterID", charID);
            cmd.AddParameter("Type", type);
            cmd.AddParameter("Timestamp", stat.LastUpdated);
            cmd.AddParameter("AllTime", stat.AllTime);
            cmd.AddParameter("OneLifeMax", stat.OneLifeMax);

            cmd.AddParameter("Day1", stat.Day1);
            cmd.AddParameter("Day2", stat.Day2);
            cmd.AddParameter("Day3", stat.Day3);
            cmd.AddParameter("Day4", stat.Day4);
            cmd.AddParameter("Day5", stat.Day5);
            cmd.AddParameter("Day6", stat.Day6);
            cmd.AddParameter("Day7", stat.Day7);
            cmd.AddParameter("Day8", stat.Day8);
            cmd.AddParameter("Day9", stat.Day9);
            cmd.AddParameter("Day10", stat.Day10);
            cmd.AddParameter("Day11", stat.Day11);
            cmd.AddParameter("Day12", stat.Day12);
            cmd.AddParameter("Day13", stat.Day13);
            cmd.AddParameter("Day14", stat.Day14);
            cmd.AddParameter("Day15", stat.Day15);
            cmd.AddParameter("Day16", stat.Day16);
            cmd.AddParameter("Day17", stat.Day17);
            cmd.AddParameter("Day18", stat.Day18);
            cmd.AddParameter("Day19", stat.Day19);
            cmd.AddParameter("Day20", stat.Day20);
            cmd.AddParameter("Day21", stat.Day21);
            cmd.AddParameter("Day22", stat.Day22);
            cmd.AddParameter("Day23", stat.Day23);
            cmd.AddParameter("Day24", stat.Day24);
            cmd.AddParameter("Day25", stat.Day25);
            cmd.AddParameter("Day26", stat.Day26);
            cmd.AddParameter("Day27", stat.Day27);
            cmd.AddParameter("Day28", stat.Day28);
            cmd.AddParameter("Day29", stat.Day29);
            cmd.AddParameter("Day30", stat.Day30);
            cmd.AddParameter("Day31", stat.Day31);

            cmd.AddParameter("Week1", stat.Week1);
            cmd.AddParameter("Week2", stat.Week2);
            cmd.AddParameter("Week3", stat.Week3);
            cmd.AddParameter("Week4", stat.Week4);
            cmd.AddParameter("Week5", stat.Week5);
            cmd.AddParameter("Week6", stat.Week6);
            cmd.AddParameter("Week7", stat.Week7);
            cmd.AddParameter("Week8", stat.Week8);
            cmd.AddParameter("Week9", stat.Week9);
            cmd.AddParameter("Week10", stat.Week10);
            cmd.AddParameter("Week11", stat.Week11);
            cmd.AddParameter("Week12", stat.Week12);
            cmd.AddParameter("Week13", stat.Week13);

            cmd.AddParameter("Month1", stat.Month1);
            cmd.AddParameter("Month2", stat.Month2);
            cmd.AddParameter("Month3", stat.Month3);
            cmd.AddParameter("Month4", stat.Month4);
            cmd.AddParameter("Month5", stat.Month5);
            cmd.AddParameter("Month6", stat.Month6);
            cmd.AddParameter("Month7", stat.Month7);
            cmd.AddParameter("Month8", stat.Month8);
            cmd.AddParameter("Month9", stat.Month9);
            cmd.AddParameter("Month10", stat.Month10);
            cmd.AddParameter("Month11", stat.Month11);
            cmd.AddParameter("Month12", stat.Month12);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override PsCharacterHistoryStat ReadEntry(NpgsqlDataReader reader) {
            PsCharacterHistoryStat stat = new PsCharacterHistoryStat();

            stat.CharacterID = reader.GetString("character_id");
            stat.Type = reader.GetString("type");
            stat.LastUpdated = reader.GetDateTime("timestamp");
            stat.AllTime = reader.GetInt32("all_time");
            stat.OneLifeMax = reader.GetInt32("one_life_max");

            stat.Day1 =  reader.GetInt32("day1");
            stat.Day2 =  reader.GetInt32("day2");
            stat.Day3 =  reader.GetInt32("day3");
            stat.Day4 =  reader.GetInt32("day4");
            stat.Day5 =  reader.GetInt32("day5");
            stat.Day6 =  reader.GetInt32("day6");
            stat.Day7 =  reader.GetInt32("day7");
            stat.Day8 =  reader.GetInt32("day8");
            stat.Day9 =  reader.GetInt32("day9");
            stat.Day10 = reader.GetInt32("day10");
            stat.Day11 = reader.GetInt32("day11");
            stat.Day12 = reader.GetInt32("day12");
            stat.Day13 = reader.GetInt32("day13");
            stat.Day14 = reader.GetInt32("day14");
            stat.Day15 = reader.GetInt32("day15");
            stat.Day16 = reader.GetInt32("day16");
            stat.Day17 = reader.GetInt32("day17");
            stat.Day18 = reader.GetInt32("day18");
            stat.Day19 = reader.GetInt32("day19");
            stat.Day20 = reader.GetInt32("day20");
            stat.Day21 = reader.GetInt32("day21");
            stat.Day22 = reader.GetInt32("day22");
            stat.Day23 = reader.GetInt32("day23");
            stat.Day24 = reader.GetInt32("day24");
            stat.Day25 = reader.GetInt32("day25");
            stat.Day26 = reader.GetInt32("day26");
            stat.Day27 = reader.GetInt32("day27");
            stat.Day28 = reader.GetInt32("day28");
            stat.Day29 = reader.GetInt32("day29");
            stat.Day30 = reader.GetInt32("day30");
            stat.Day31 = reader.GetInt32("day31");

            stat.Week1 =  reader.GetInt32("week1");
            stat.Week2 =  reader.GetInt32("week2");
            stat.Week3 =  reader.GetInt32("week3");
            stat.Week4 =  reader.GetInt32("week4");
            stat.Week5 =  reader.GetInt32("week5");
            stat.Week6 =  reader.GetInt32("week6");
            stat.Week7 =  reader.GetInt32("week7");
            stat.Week8 =  reader.GetInt32("week8");
            stat.Week9 =  reader.GetInt32("week9");
            stat.Week10 = reader.GetInt32("week10");
            stat.Week11 = reader.GetInt32("week11");
            stat.Week12 = reader.GetInt32("week12");
            stat.Week13 = reader.GetInt32("week13");

            stat.Month1 =  reader.GetInt32("month1");
            stat.Month2 =  reader.GetInt32("month2");
            stat.Month3 =  reader.GetInt32("month3");
            stat.Month4 =  reader.GetInt32("month4");
            stat.Month5 =  reader.GetInt32("month5");
            stat.Month6 =  reader.GetInt32("month6");
            stat.Month7 =  reader.GetInt32("month7");
            stat.Month8 =  reader.GetInt32("month8");
            stat.Month9 =  reader.GetInt32("month9");
            stat.Month10 = reader.GetInt32("month10");
            stat.Month11 = reader.GetInt32("month11");
            stat.Month12 = reader.GetInt32("month12");

            return stat;
        }
    }
}
