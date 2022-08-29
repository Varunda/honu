using Npgsql;
using System.Data;
using watchtower.Models;

namespace watchtower.Services.Db.Readers {

    public class HonuAccountPermissionReader : IDataReader<HonuAccountPermission> {

        public override HonuAccountPermission? ReadEntry(NpgsqlDataReader reader) {
            HonuAccountPermission perm = new();

            perm.ID = reader.GetInt64("id");
            perm.AccountID = reader.GetInt64("account_id");
            perm.Permission = reader.GetString("permission");
            perm.Timestamp = reader.GetDateTime("timestamp");
            perm.GrantedByID = reader.GetInt64("granted_by_id");

            return perm;
        }

    }
}
