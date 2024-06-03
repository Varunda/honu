using Npgsql;
using System.Threading.Tasks;

namespace watchtower.Services.Db.Patches {

    [Patch]
    public class Patch87AddAggregateSessionStats : IDbPatch {
        public int MinVersion => 87;
        public string Name => "add aggregate session stats";

        public async Task Execute(IDbHelper helper) {

            using NpgsqlConnection conn = helper.Connection(Dbs.CHARACTER);
            using NpgsqlCommand cmd = await helper.Command(conn, @"
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS summary_calculated timestamptz NULL;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS kills int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS deaths int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS vehicle_kills int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS experience_gained bigint NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS heals int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS revives int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS shield_repairs int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS resupplies int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS spawns int NOT NULL DEFAULT -1;
                ALTER TABLE wt_session ADD COLUMN IF NOT EXISTS repairs int NOT NULL DEFAULT -1;

                CREATE INDEX IF NOT EXISTS idx_wt_session_summary_calculated
                    ON wt_session (summary_calculated);
            ");

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
