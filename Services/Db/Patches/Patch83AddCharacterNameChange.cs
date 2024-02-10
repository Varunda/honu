using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch83AddCharacterNameChange : IDbPatch {
        public int MinVersion => 83;
        public string Name => "add character_name_change";

        public async Task Execute(IDbHelper helper) {

            using NpgsqlConnection conn = helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                CREATE TABLE IF NOT EXISTS character_name_change (
                    id bigint NOT NULL PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
                    character_id varchar NOT NULL,
                    old_name varchar NOT NULL,
                    new_name varchar NOT NULL,
                    lower_bound timestamptz NOT NULL,
                    upper_bound timestamptz NOT NULL,
                    timestamp timestamptz NOT NULL
                );

                CREATE INDEX idx_character_name_change_character_id ON character_name_change (character_id);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
