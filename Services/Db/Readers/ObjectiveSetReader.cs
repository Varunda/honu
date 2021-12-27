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

    public class ObjectiveSetReader : IDataReader<ObjectiveSet> {

        public override ObjectiveSet? ReadEntry(NpgsqlDataReader reader) {
            ObjectiveSet entry = new ObjectiveSet();

            entry.ID = reader.GetInt32("set_id");
            entry.GroupID = reader.GetInt32("group_id");

            return entry;
        }

    }
}
