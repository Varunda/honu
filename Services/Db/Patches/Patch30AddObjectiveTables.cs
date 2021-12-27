using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch30AddObjectiveTables : IDbPatch {

        public int MinVersion => 30;
        public string Name => "create objective tables";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS objective_type (
                    id int NOT NULL PRIMARY KEY,
                    description varchar NOT NULL,
                    param1 varchar NULL,
                    param2 varchar NULL,
                    param3 varchar NULL,
                    param4 varchar NULL,
                    param5 varchar NULL,
                    param6 varchar NULL,
                    param7 varchar NULL,
                    param8 varchar NULL,
                    param9 varchar NULL,
                    param10 varchar NULL
                );
                
                CREATE TABLE IF NOT EXISTS objective_set (
                    set_id int NOT NULL PRIMARY KEY,
                    group_id int NOT NULL
                );

                CREATE TABLE IF NOT EXISTS objective (
                    id int NOT NULL PRIMARY KEY,
                    type_id int NOT NULL,
                    group_id int NOT NULL,
                    param1 varchar NULL,
                    param2 varchar NULL,
                    param3 varchar NULL,
                    param4 varchar NULL,
                    param5 varchar NULL,
                    param6 varchar NULL,
                    param7 varchar NULL,
                    param8 varchar NULL,
                    param9 varchar NULL,
                    param10 varchar NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
