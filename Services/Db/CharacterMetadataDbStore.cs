using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    public class CharacterMetadataDbStore {

        private readonly ILogger<CharacterMetadataDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterMetadata> _Reader;
        private readonly IMemoryCache _Cache;

        private const string CACHE_KEY = "CharacterMetadata.{0}"; // {0} => character ID

        public CharacterMetadataDbStore(ILogger<CharacterMetadataDbStore> logger,
            IDbHelper helper, IDataReader<CharacterMetadata> reader,
            IMemoryCache cache) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
            _Cache = cache;
        }

        /// <summary>
        ///     Get the <see cref="CharacterMetadata"/> of a character
        /// </summary>
        /// <param name="charID">ID of the character</param>
        /// <returns>
        ///     The <see cref="CharacterMetadata"/> with <see cref="CharacterMetadata.ID"/>
        ///     of <paramref name="charID"/>, or <c>null</c> if it does not exist
        /// </returns>
        public async Task<CharacterMetadata?> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_metadata
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", charID);

            CharacterMetadata? md = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return md;
        }

        /// <summary>
        ///     Update/Insert a <see cref="CharacterMetadata"/>
        /// </summary>
        /// <param name="charID">ID of the character the <paramref name="metadata"/> is for</param>
        /// <param name="metadata">Metadata, used for the parameters</param>
        public async Task Upsert(string charID, CharacterMetadata metadata) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO character_metadata (
                    id, last_updated, not_found_count
                ) VALUES (
                    @ID, @LastUpdated, @NotFoundCount
                ) ON CONFLICT (id) DO
                    UPDATE set last_updated = @LastUpdated,
                        not_found_count = @NotFoundCount;
            ");

            cmd.AddParameter("ID", charID);
            cmd.AddParameter("LastUpdated", metadata.LastUpdated);
            cmd.AddParameter("NotFoundCount", metadata.NotFoundCount);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
