using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db.Implementations {

    public class OutfitDbStore : IDataReader<PsOutfit>, IOutfitDbStore {

        private readonly ILogger<OutfitDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public OutfitDbStore(ILogger<OutfitDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<PsOutfit?> GetByID(string outfitID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE id = @ID
            ");

            cmd.AddParameter("ID", outfitID);

            return await ReadSingle(cmd);
        }

        public async Task Upsert(PsOutfit outfit) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_outfit (
                    id, name, tag, faction_id, last_updated_on
                ) VALUES (
                    @ID, @Name, @Tag, @FactionID, @LastUpdatedOn
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        tag = @Tag,
                        faction_id = @FactionID,   
                        last_updated_on = @LastUpdatedOn
            ");

            cmd.AddParameter("ID", outfit.ID);
            cmd.AddParameter("Name", outfit.Name);
            cmd.AddParameter("Tag", outfit.Tag);
            cmd.AddParameter("FactionID", outfit.FactionID);
            cmd.AddParameter("LastUpdatedOn", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }

        public override PsOutfit ReadEntry(NpgsqlDataReader reader) {
            PsOutfit outfit = new PsOutfit();

            outfit.ID = reader.GetString("id");
            outfit.Name = reader.GetString("name");
            outfit.FactionID = reader.GetInt16("faction_id");
            outfit.LastUpdated = reader.GetDateTime("last_updated_on");

            if (reader.IsDBNull("tag") == true) {
                outfit.Tag = null;
            } else {
                outfit.Tag = reader.GetString("tag");
            }

            return outfit;
        }

    }
}
