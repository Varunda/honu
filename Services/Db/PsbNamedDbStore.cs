using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.PSB;

namespace watchtower.Services.Db {

    public class PsbNamedDbStore {

        private readonly ILogger<PsbNamedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<PsbNamedAccount> _Reader;

        public PsbNamedDbStore(ILogger<PsbNamedDbStore> logger,
            IDbHelper helper, IDataReader<PsbNamedAccount> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<PsbNamedAccount>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named;
            ");

            List<PsbNamedAccount> accs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return accs;
        }

        public async Task<PsbNamedAccount?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            PsbNamedAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        public async Task<PsbNamedAccount?> GetByTagAndName(string? tag, string name) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM psb_named
                    WHERE tag = @Tag
                        AND name = @Name;
            ");

            cmd.AddParameter("Tag", tag);
            cmd.AddParameter("Name", name);

            PsbNamedAccount? acc = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return acc;
        }

        public async Task Upsert(PsbNamedAccount acc) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO psb_named (
                    tag, name, vs_id, nc_id, tr_id, ns_id, notes
                ) VALUES (
                    @Tag, @Name, @VsID, @NcID, @TrID, @NsID, @Notes
                ) ON CONFLICT (tag, name) DO 
                    UPDATE SET vs_id = @VsID,
                        nc_id = @NcID,
                        tr_id = @TrID,
                        ns_id = @NsID,
                        notes = @Notes;
            ");

            cmd.AddParameter("Tag", acc.Tag);
            cmd.AddParameter("Name", acc.Name);
            cmd.AddParameter("VsID", acc.VsID);
            cmd.AddParameter("NcID", acc.NcID);
            cmd.AddParameter("TrID", acc.TrID);
            cmd.AddParameter("NsID", acc.NsID);
            cmd.AddParameter("Notes", acc.Notes);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }


    }
}
