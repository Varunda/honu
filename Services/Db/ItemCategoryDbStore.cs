using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ItemCategoryDbStore : BaseStaticDbStore<ItemCategory> {
        public ItemCategoryDbStore(ILoggerFactory loggerFactory,
                IDataReader<ItemCategory> reader, IDbHelper helper)
            : base("item_category", loggerFactory, reader, helper) {
        }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ItemCategory param) {
            cmd.CommandText = @"
                INSERT INTO item_category (
                    id, name
                ) VALUES (
                    @ID, @Name
                ) ON CONFLICT (id) DO
                    UPDATE SET
                        name = @Name;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Name", param.Name);
        }

    }
}
