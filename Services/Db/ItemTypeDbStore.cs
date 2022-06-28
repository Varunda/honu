using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ItemTypeDbStore : BaseStaticDbStore<ItemType> {

        public ItemTypeDbStore(ILoggerFactory loggerFactory,
                IDataReader<ItemType> reader, IDbHelper helper)
            : base("item_type", loggerFactory, reader, helper) {
        }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ItemType param) {
            cmd.CommandText = @"
                INSERT INTO item_type (
                    id, name, code
                ) VALUES (
                    @ID, @Name, @Code
                ) ON CONFLICT(id) DO
                    UPDATE SET
                        name = @Name,
                        code = @Code;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Code", param.Code);
        }

    }
}
