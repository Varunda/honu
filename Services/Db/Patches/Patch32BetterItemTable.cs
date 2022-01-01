using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch32BetterItemTable : IDbPatch {

        public int MinVersion => 32;
        public string Name => "add more to wt_item";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                DROP TABLE IF EXISTS wt_item;

                CREATE TABLE IF NOT EXISTS wt_item (
                    id int PRIMARY KEY NOT NULL,
                    category_id int NOT NULL,
                    type_id int NOT NULL,
                    is_vehicle_weapon boolean NOT NULL,
                    name varchar NOT NULL,
                    description varchar NOT NULL,
                    faction_id smallint NOT NULL,
                    image_id int NOT NULL,
                    image_set_id int NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
