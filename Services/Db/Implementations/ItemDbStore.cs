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

    public class ItemDbStore : IDataReader<PsItem>, IItemDbStore {

        private readonly ILogger<ItemDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public ItemDbStore(ILogger<ItemDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<PsItem?> GetByID(string itemID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_item
                    WHERE id = @ID
            ");

            cmd.AddParameter("ID", itemID);

            return await ReadSingle(cmd);
        }

        public async Task Upsert(PsItem item) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_item (
                    id, category_id, type_id, name
                ) VALUES (
                    @ID, @CategoryID, @TypeID, @Name
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        category_id = @CategoryID,
                        type_id = @TypeID
            ");

            cmd.AddParameter("ID", item.ID);
            cmd.AddParameter("CategoryID", item.CategoryID);
            cmd.AddParameter("TypeID", item.TypeID);
            cmd.AddParameter("Name", item.Name);

            await cmd.ExecuteNonQueryAsync();
        }

        public override PsItem ReadEntry(NpgsqlDataReader reader) {
            PsItem item = new PsItem();

            item.ID = reader.GetString("id");
            item.Name = reader.GetString("name");
            item.CategoryID = reader.GetInt32("category_id");
            item.TypeID = reader.GetInt32("type_id");

            return item;
        }

    }
}
