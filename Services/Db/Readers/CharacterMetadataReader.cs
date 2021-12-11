using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class CharacterMetadataReader : IDataReader<CharacterMetadata> {

        public override CharacterMetadata? ReadEntry(NpgsqlDataReader reader) {
            CharacterMetadata md = new CharacterMetadata();

            md.ID = reader.GetString("id");
            md.LastUpdated = reader.GetDateTime("last_updated");
            md.NotFoundCount = reader.GetInt32("not_found_count");

            return md;
        }

    }
}
