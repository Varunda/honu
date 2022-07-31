using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch61ItemAddedAndAchievement : IDbPatch {
        public int MinVersion => 61;

        public string Name => "item_added and achievement_earned";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS item_added (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    character_id varchar NOT NULL,
                    item_id int NOT NULL,
                    context varchar NOT NULL,
                    item_count int NOT NULL,
                    timestamp timestamptz NOT NULL,
                    zone_id int NOT NULL,
                    world_id smallint NOT NULL
                );

                CREATE INDEX IF NOT EXISTS item_added_character_id ON item_added(character_id);

                CREATE TABLE IF NOT EXISTS achievement_earned (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    character_id varchar NOT NULL,
                    achievement_id int NOT NULL,
                    timestamp timestamptz NOT NULL,
                    zone_id int NOT NULL,
                    world_id smallint NOT NULL
                );
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
