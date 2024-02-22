using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ExperienceAwardTypeDbStore : BaseStaticDbStore<ExperienceAwardType> {

        public ExperienceAwardTypeDbStore(ILoggerFactory loggerFactory,
                IDataReader<ExperienceAwardType> reader, IDbHelper helper)
            : base("experience_award_type", loggerFactory, reader, helper) {
        }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ExperienceAwardType param) {
            cmd.CommandText = @"
                INSERT INTO experience_award_type (
                    id, name
                ) VALUES (
                    @ID, @Name
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Name", param.Name);
        }

    }
}
