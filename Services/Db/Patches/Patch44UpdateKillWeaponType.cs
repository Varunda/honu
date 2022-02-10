using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch44UpdateKillWeaponType : IDbPatch {
        public int MinVersion => 44;
        public string Name => "update wt_kill.weapon_id to be an int";

        public async Task Execute(IDbHelper helper) {
            using NpgsqlConnection conn = helper.Connection();
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_kills
                    ALTER COLUMN weapon_id TYPE INT
                    USING weapon_id::integer;
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }
    }
}
