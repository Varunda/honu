using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class OutfitDbStore : IDataReader<PsOutfit>, IOutfitDbStore {

        private readonly ILogger<OutfitDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<OutfitPopulation> _PopulationReader;

        public OutfitDbStore(ILogger<OutfitDbStore> logger,
            IDbHelper dbHelper, IDataReader<OutfitPopulation> popReader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _PopulationReader = popReader ?? throw new ArgumentNullException(nameof(popReader));
        }

        public async Task<PsOutfit?> GetByID(string outfitID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE id = @ID
            ");

            cmd.AddParameter("ID", outfitID);

            PsOutfit? outfit = await ReadSingle(cmd);
            await conn.CloseAsync();

            return outfit;
        }

        public async Task Upsert(PsOutfit outfit) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO wt_outfit (
                    id, name, tag, faction_id, last_updated_on, time_create, leader_id, member_count
                ) VALUES (
                    @ID, @Name, @Tag, @FactionID, @LastUpdatedOn, @DateCreated, @LeaderID, @MemberCount
                ) ON CONFLICT (id) DO
                    UPDATE SET name = @Name,
                        tag = @Tag,
                        faction_id = @FactionID,   
                        last_updated_on = @LastUpdatedOn,
                        time_create = @DateCreated,
                        leader_id = @LeaderID,
                        member_count = @MemberCount
            ");

            cmd.AddParameter("ID", outfit.ID);
            cmd.AddParameter("Name", outfit.Name);
            cmd.AddParameter("Tag", outfit.Tag);
            cmd.AddParameter("FactionID", outfit.FactionID);
            cmd.AddParameter("LastUpdatedOn", DateTime.UtcNow);
            cmd.AddParameter("DateCreated", outfit.DateCreated);
            cmd.AddParameter("LeaderID", outfit.LeaderID);
            cmd.AddParameter("MemberCount", outfit.MemberCount);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public async Task<List<OutfitPopulation>> GetPopulation(DateTime time, short worldID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                WITH outfits AS (
                SELECT wt_session.outfit_id, COUNT(wt_session.outfit_id)
                    FROM wt_session
                        INNER JOIN wt_character c on c.id = wt_session.character_id
                    WHERE (
                            (start <= @Time AND finish >= @Time)
                            OR (start <= @Time AND finish IS NULL)
                        )
                        AND world_id = @WorldID
                    GROUP BY wt_session.outfit_id
                )
                SELECT *
                    FROM outfits os
                    INNER JOIN wt_outfit o ON o.id = os.outfit_id
            ");

            cmd.AddParameter("Time", time);
            cmd.AddParameter("WorldID", worldID);

            List<OutfitPopulation> pop = await _PopulationReader.ReadList(cmd);
            await conn.CloseAsync();

            return pop;
        }

        public override PsOutfit ReadEntry(NpgsqlDataReader reader) {
            PsOutfit outfit = new PsOutfit();

            outfit.ID = reader.GetString("id");
            outfit.Name = reader.GetString("name");
            outfit.FactionID = reader.GetInt16("faction_id");
            outfit.LastUpdated = reader.GetDateTime("last_updated_on");
            outfit.LeaderID = reader.GetString("leader_id");
            outfit.MemberCount = reader.GetInt32("member_count");
            outfit.LastUpdated = reader.GetNullableDateTime("time_create") ?? DateTime.MinValue;
            outfit.Tag = reader.GetNullableString("tag");

            return outfit;
        }
    }
}
