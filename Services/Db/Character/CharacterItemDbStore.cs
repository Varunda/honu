using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterItemDbStore : IDataReader<CharacterItem> {

        private readonly ILogger<CharacterItemDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public CharacterItemDbStore(ILogger<CharacterItemDbStore> logger,
            IDbHelper helper) {

            _Logger = logger;
            _DbHelper = helper;
        }

        public async Task<List<CharacterItem>> GetByID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_items
                    WHERE character_id = @CharacterID
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterItem> items = await ReadList(cmd);
            await conn.CloseAsync();

            return items;
        }

        public async Task Set(string charID, List<CharacterItem> items) {
            if (items.Count == 0) {
                return;
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                BEGIN;

                DELETE FROM character_items
                    WHERE character_id = @CharacterID;

                INSERT INTO character_items(character_id, item_id, account_level, stack_count)
                    VALUES {string.Join(",\n", items.Select(iter => $"('{iter.CharacterID}', '{iter.ItemID}', {iter.AccountLevel}, {(iter.StackCount == null ? "null" : iter.StackCount)})"))};

                COMMIT;
            ");

            cmd.AddParameter("CharacterID", charID);

            //_Logger.LogDebug(cmd.CommandText);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public override CharacterItem? ReadEntry(NpgsqlDataReader reader) {
            CharacterItem item = new CharacterItem();

            item.CharacterID = reader.GetString("character_id");
            item.ItemID = reader.GetString("item_id");
            item.AccountLevel = reader.GetBoolean("account_level");
            item.StackCount = reader.GetNullableInt32("stack_count");

            return item;
        }

    }
}
