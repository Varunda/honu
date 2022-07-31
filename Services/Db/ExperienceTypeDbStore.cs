using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class ExperienceTypeDbStore : BaseStaticDbStore<ExperienceType> {

        public ExperienceTypeDbStore(ILoggerFactory loggerFactory,
            IDataReader<ExperienceType> reader, IDbHelper helper)
            : base("experience_type", loggerFactory, reader, helper) { }

        internal override void SetupUpsertCommand(NpgsqlCommand cmd, ExperienceType param) {
            cmd.CommandText = @"
                INSERT INTO experience_type (
                    id, name, amount
                ) VALUES (
                    @ID, @Name, @Amount
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        amount = @Amount;
            ";

            cmd.AddParameter("ID", param.ID);
            cmd.AddParameter("Name", param.Name);
            cmd.AddParameter("Amount", param.Amount);
        }

    }
}
