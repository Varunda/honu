using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch38UpdatePsbNamedTable : IDbPatch {
        public int MinVersion => 38;
        public string Name => "update psb_named";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS timestamp timestamptz DEFAULT (NOW() at time zone 'utc');

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS player_name varchar NULL;

                UPDATE psb_named
                    SET player_name = name;

                ALTER TABLE psb_named
                    ALTER COLUMN player_name TYPE varchar;

                CREATE TABLE IF NOT EXISTS psb_character_status (
                    id int NOT NULL PRIMARY KEY,
                    name varchar NOT NULL
                );

                INSERT INTO psb_character_status (
                    id, name
                ) VALUES 
                    (1, 'ok'),
                    (2, 'doesnotexist'),
                    (3, 'deleted'),
                    (4, 'remade')   
                ON CONFLICT DO NOTHING;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS vs_status int NULL;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS nc_status int NULL;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS tr_status int NULL;

                ALTER TABLE psb_named
                    ADD COLUMN IF NOT EXISTS ns_status int NULL;

                UPDATE psb_named
                    SET vs_status = 2,
                        nc_status = 2,
                        tr_status = 2,
                        ns_status = 2;
    
                ALTER TABLE psb_named
                    ALTER COLUMN vs_status TYPE int;

                ALTER TABLE psb_named
                    ALTER COLUMN nc_status TYPE int;

                ALTER TABLE psb_named
                    ALTER COLUMN tr_status TYPE int;

                ALTER TABLE psb_named
                    ALTER COLUMN ns_status TYPE int;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
