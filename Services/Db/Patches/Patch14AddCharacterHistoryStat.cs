using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch14AddCharacterHistoryStat : IDbPatch {

        public int MinVersion => 14;

        public string Name => "Add character history stat table";

        public async Task Execute(IDbHelper helper) {
            using (NpgsqlConnection conn = helper.Connection()) {
                // Fun fact, I grrr when I see this
                using NpgsqlCommand cmd = await helper.Command(conn, @"
                    CREATE TABLE IF NOT EXISTS character_history_stat (
                        character_id varchar NOT NULL,
                        type varchar NOT NULL,
                        timestamp timestamptz NOT NULL DEFAULT (NOW() at time zone 'utc'),
                        all_time int NOT NULL,
                        one_life_max int NOT NULL,
                        
                        day1 int NOT NULL,
                        day2 int NOT NULL,
                        day3 int NOT NULL,
                        day4 int NOT NULL,
                        day5 int NOT NULL,
                        day6 int NOT NULL,
                        day7 int NOT NULL,
                        day8 int NOT NULL,
                        day9 int NOT NULL,
                        day10 int NOT NULL,
                        day11 int NOT NULL,
                        day12 int NOT NULL,
                        day13 int NOT NULL,
                        day14 int NOT NULL,
                        day15 int NOT NULL,
                        day16 int NOT NULL,
                        day17 int NOT NULL,
                        day18 int NOT NULL,
                        day19 int NOT NULL,
                        day20 int NOT NULL,
                        day21 int NOT NULL,
                        day22 int NOT NULL,
                        day23 int NOT NULL,
                        day24 int NOT NULL,
                        day25 int NOT NULL,
                        day26 int NOT NULL,
                        day27 int NOT NULL,
                        day28 int NOT NULL,
                        day29 int NOT NULL,
                        day30 int NOT NULL,
                        day31 int NOT NULL,

                        week1 int NOT NULL,
                        week2 int NOT NULL,
                        week3 int NOT NULL,
                        week4 int NOT NULL,
                        week5 int NOT NULL,
                        week6 int NOT NULL,
                        week7 int NOT NULL,
                        week8 int NOT NULL,
                        week9 int NOT NULL,
                        week10 int NOT NULL,
                        week11 int NOT NULL,
                        week12 int NOT NULL,
                        week13 int NOT NULL,

                        month1 int NOT NULL,
                        month2 int NOT NULL,
                        month3 int NOT NULL,
                        month4 int NOT NULL,
                        month5 int NOT NULL,
                        month6 int NOT NULL,
                        month7 int NOT NULL,
                        month8 int NOT NULL,
                        month9 int NOT NULL,
                        month10 int NOT NULL,
                        month11 int NOT NULL,
                        month12 int NOT NULL,

                        PRIMARY KEY (character_id, type)
                    );
                ");

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
