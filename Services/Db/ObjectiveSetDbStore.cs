using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ObjectiveSetDbStore : BaseStaticDbStore<ObjectiveSet> {

        public ObjectiveSetDbStore(ILoggerFactory loggerFactory,
            IDataReader<ObjectiveSet> reader, IDbHelper helper)
            : base("objective_set", loggerFactory, reader, helper) { }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ObjectiveSet param) {
            cmd.CommandText = @"
                INSERT INTO objective_set (
                    set_id, group_id
                ) VALUES (
                    @SetID, @GroupID
                ) ON CONFLICT (set_id) DO 
                    UPDATE SET group_id = @GroupID;
            ";

            cmd.AddParameter("SetID", param.ID);
            cmd.AddParameter("GroupID", param.GroupID);
        }

    }

}