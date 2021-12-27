using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Queues;

namespace watchtower.Services.Db.Readers {

    public class ObjectiveTypeReader : IDataReader<ObjectiveType> {

        public override ObjectiveType? ReadEntry(NpgsqlDataReader reader) {
            ObjectiveType entry = new ObjectiveType();

            entry.ID = reader.GetInt32("id");
            entry.Description = reader.GetString("description");
            entry.Param1 = reader.GetNullableString("param1");
            entry.Param2 = reader.GetNullableString("param2");
            entry.Param3 = reader.GetNullableString("param3");
            entry.Param4 = reader.GetNullableString("param4");
            entry.Param5 = reader.GetNullableString("param5");
            entry.Param6 = reader.GetNullableString("param6");
            entry.Param7 = reader.GetNullableString("param7");
            entry.Param8 = reader.GetNullableString("param8");
            entry.Param9 = reader.GetNullableString("param9");
            entry.Param10 = reader.GetNullableString("param10");

            return entry;
        }

    }
}
