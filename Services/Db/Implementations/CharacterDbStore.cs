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

    public class CharacterDbStore : IDataReader<PsCharacter>, ICharacterDbStore {

        private readonly ILogger<CharacterDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterDbStore(ILogger<CharacterDbStore> logger,
                IDbHelper helper) {

            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DbHelper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public async Task<PsCharacter?> GetByID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT c.*, o.id AS outfit_id, o.tag AS outfit_tag, o.name AS outfit_name
                    FROM wt_character c
                        LEFT JOIN wt_outfit o ON c.outfit_id = o.id
                    WHERE c.id = @ID
            ");

            cmd.AddParameter("@ID", charID);

            PsCharacter? c = await ReadSingle(cmd);
            await conn.CloseAsync();

            return c;
        }

        public async Task<List<PsCharacter>> GetByName(string name) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT c.*, o.id AS outfit_id, o.tag AS outfit_tag, o.name AS outfit_name
                    FROM wt_character c
                        LEFT JOIN wt_outfit o ON c.outfit_id = o.id
                    WHERE c.name_lower = @Name
            ");

            cmd.AddParameter("Name", name.ToLower());

            List<PsCharacter> c = await ReadList(cmd);
            await conn.CloseAsync();

            return c;
        }

        public async Task Upsert(PsCharacter character) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_character (
                    id, name, world_id, faction_id, outfit_id, battle_rank, prestige, last_updated_on
                ) VALUES (
                    @ID, @Name, @WorldID, @FactionID, @outfitID, @BattleRank, @Prestige, @LastUpdatedOn
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        world_id = @WorldID,
                        faction_id = @FactionID,
                        outfit_id = @OutfitID,
                        battle_rank = @BattleRank,
                        prestige = @Prestige,
                        last_updated_on = @LastUpdatedOn
            ");

            cmd.AddParameter("ID", character.ID);
            cmd.AddParameter("Name", character.Name);
            cmd.AddParameter("WorldID", character.WorldID);
            cmd.AddParameter("FactionID", character.FactionID);
            cmd.AddParameter("OutfitID", character.OutfitID);
            cmd.AddParameter("BattleRank", character.BattleRank);
            cmd.AddParameter("Prestige", character.Prestige);
            cmd.AddParameter("LastUpdatedOn", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override PsCharacter ReadEntry(NpgsqlDataReader reader) {
            PsCharacter c = new PsCharacter();

            // Why keep the reading separate? So if there is an error reading a column,
            // the exception contains the line, which contains the bad column
            c.ID = reader.GetString("id");
            c.Name = reader.GetString("name");
            c.FactionID = reader.GetInt16("faction_id");
            c.WorldID = reader.GetInt16("world_id");
            c.LastUpdated = reader.GetDateTime("last_updated_on");
            c.BattleRank = reader.GetInt16("battle_rank");
            c.Prestige = reader.GetBoolean("prestige");

            c.OutfitID = reader.GetNullableString("outfit_id");
            c.OutfitTag = reader.GetNullableString("outfit_tag");
            c.OutfitName = reader.GetNullableString("outfit_name");

            return c;
        }

    }
}
