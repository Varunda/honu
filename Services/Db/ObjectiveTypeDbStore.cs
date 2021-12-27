
using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ObjectiveTypeDbStore : BaseStaticDbStore<ObjectiveType> {

        public ObjectiveTypeDbStore(ILoggerFactory loggerFactory,
            IDataReader<ObjectiveType> reader, IDbHelper helper)
            : base("objective_type", loggerFactory, reader, helper) { }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ObjectiveType param) {
            cmd.CommandText = @"
                INSERT INTO objective_type (
                    id, description, 
                    param1, param2, param3, param4, param5,
                    param6, param7, param8, param9, param10
                ) VALUES (
                    @ID, @Description,
                    @Param1, @Param2, @Param3, @Param4, @Param5,
                    @Param6, @Param7, @Param8, @Param9, @Param10
                ) ON CONFLICT (id) DO 
                    UPDATE SET description = @Description,
                        param1 = @Param1,
                        param2 = @Param2,
                        param3 = @Param3,
                        param4 = @Param4,
                        param5 = @Param5,
                        param6 = @Param6,
                        param7 = @Param7,
                        param8 = @Param8,
                        param9 = @Param9,
                        param10 = @Param10;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Description", param.Description);
            cmd.AddParameter("Param1", param.Param1);
            cmd.AddParameter("Param2", param.Param2);
            cmd.AddParameter("Param3", param.Param3);
            cmd.AddParameter("Param4", param.Param4);
            cmd.AddParameter("Param5", param.Param5);
            cmd.AddParameter("Param6", param.Param6);
            cmd.AddParameter("Param7", param.Param7);
            cmd.AddParameter("Param8", param.Param8);
            cmd.AddParameter("Param9", param.Param9);
            cmd.AddParameter("Param10", param.Param10);
        }

    }

}