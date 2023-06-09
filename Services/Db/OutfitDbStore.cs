using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Api;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    /// <summary>
    ///     Service to interact with the outfit table
    /// </summary>
    public class OutfitDbStore : IDataReader<PsOutfit> {

        private readonly ILogger<OutfitDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<OutfitPopulation> _PopulationReader;
        private readonly IDataReader<OutfitActivityDbEntry> _ActivityReader;

        public OutfitDbStore(ILogger<OutfitDbStore> logger,
            IDbHelper dbHelper, IDataReader<OutfitPopulation> popReader,
            IDataReader<OutfitActivityDbEntry> activityReader) {

            _Logger = logger;
            _DbHelper = dbHelper;
            _PopulationReader = popReader;
            _ActivityReader = activityReader;
        }

        /// <summary>
        ///     Get an outfit by it's ID
        /// </summary>
        /// <param name="outfitID">ID of the outfit</param>
        /// <returns>
        ///     <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<PsOutfit?> GetByID(string outfitID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE id = @ID
            ");

            cmd.AddParameter("ID", outfitID);
            await cmd.PrepareAsync();

            PsOutfit? outfit = await ReadSingle(cmd);
            await conn.CloseAsync();

            return outfit;
        }

        /// <summary>
        ///     Get a block of <see cref="PsOutfit"/>s by IDs
        /// </summary>
        /// <param name="IDs">IDs of the outfits to get</param>
        public async Task<List<PsOutfit>> GetByIDs(List<string> IDs) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE id = ANY(@IDs)
            ");

            cmd.AddParameter("IDs", IDs);

            List<PsOutfit> outfits = await ReadList(cmd);
            await conn.CloseAsync();

            return outfits;
        }

        /// <summary>
        ///     Get the outfits that exactly matches a tag, case insensitive
        /// </summary>
        /// <remarks>
        ///     A list is returned, as deleted outfits can have the same tag
        /// </remarks>
        /// <param name="tag">Tag to search by</param>
        /// <returns>
        ///     A list of all outfits with <see cref="PsOutfit.Tag"/> equal to <paramref name="tag"/>
        /// </returns>
        public async Task<List<PsOutfit>> GetByTag(string tag) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE LOWER(tag) = @Tag;
            ");

            cmd.AddParameter("Tag", tag.ToLower());

            List<PsOutfit> outfits = await ReadList(cmd);
            await conn.CloseAsync();

            return outfits;
        }

        /// <summary>
        ///     Get all outfits stored in the database
        /// </summary>
        /// <returns></returns>
        public async Task<List<PsOutfit>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit;
            ");

            List<PsOutfit> outfits = await ReadList(cmd);
            await conn.CloseAsync();

            return outfits;
        }

        /// <summary>
        ///     Search for outfit by name, does not need to match exactly. Case insensitive
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns>
        ///     A list of all <see cref="PsOutfit"/>s that have <paramref name="name"/> contain <see cref="PsOutfit.Name"/>
        /// </returns>
        public async Task<List<PsOutfit>> SearchByName(string name) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM wt_outfit
                    WHERE LOWER(NAME) LIKE @Name;
            ");

            cmd.AddParameter("Name", $"%{name.ToLower()}%");

            List<PsOutfit> outfits = await ReadList(cmd);
            await conn.CloseAsync();

            return outfits;
        }

        /// <summary>
        ///     Update/Insert an outfit
        /// </summary>
        /// <param name="outfit">Parameters used to update/insert the entry</param>
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

        /// <summary>
        ///     Get the population of each outfit on a world at a specific time
        /// </summary>
        /// <param name="time">Time to search for</param>
        /// <param name="worldID">ID of the world</param>
        /// <returns>
        ///     A list of <see cref="OutfitPopulation"/>s, each containing the outfit and how many members
        ///     they had online at a given time
        /// </returns>
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

        /// <summary>
        ///     Get how many unique characters had session starts within a time period, broken into intervals of hours or days
        /// </summary>
        /// <param name="outfitID">ID of the outfit</param>
        /// <param name="start">Start period</param>
        /// <param name="end">End period</param>
        /// <param name="bucketWidthSec">How wide to make each bucket</param>
        /// <returns>
        ///     A list of <see cref="OutfitActivityDbEntry"/>s for the time range given.
        ///     If there are 0 characters online during an interval within the period,
        ///         instead of an entry with a count of 0 being returned, no row will be included.
        /// </returns>
        public async Task<List<OutfitActivityDbEntry>> GetActivity(string outfitID, DateTime start, DateTime end, int bucketWidthSec = 3600) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            await using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                SELECT
                    gs as timestamp,
                    count(distinct(character_id)) as count
                FROM
                    wt_session s
                INNER JOIN
                    generate_series(@Start, @End, @BucketWidth * '1 second'::INTERVAL) gs
                        ON (
                            (s.start BETWEEN gs AND gs + @BucketWidth * '1 second'::INTERVAL)
                            OR (s.finish BETWEEN gs AND gs + @BucketWidth * '1 second'::INTERVAL)
                            OR (start <= gs AND finish >= gs + @BucketWidth * '1 second'::INTERVAL)
                            OR (start >= gs AND finish <= gs + @BucketWidth * '1 second'::INTERVAL)
                        )
                WHERE
                    s.outfit_id = @OutfitID
                GROUP BY 
                    1
                ;
            ");

            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);
            cmd.AddParameter("OutfitID", outfitID);
            cmd.AddParameter("BucketWidth", bucketWidthSec);

            cmd.CommandTimeout = 60;

            List<OutfitActivityDbEntry> activity = await _ActivityReader.ReadList(cmd);
            await conn.CloseAsync();

            return activity;
        }

        public override PsOutfit ReadEntry(NpgsqlDataReader reader) {
            PsOutfit outfit = new PsOutfit();

            outfit.ID = reader.GetString("id");
            outfit.Name = reader.GetString("name");
            outfit.Tag = reader.GetNullableString("tag");
            outfit.FactionID = reader.GetInt16("faction_id");
            outfit.LastUpdated = reader.GetDateTime("last_updated_on");
            outfit.LeaderID = reader.GetString("leader_id");
            outfit.MemberCount = reader.GetInt32("member_count");
            outfit.LastUpdated = reader.GetDateTime("timestamp");
            outfit.DateCreated = reader.GetNullableDateTime("time_create") ?? DateTime.MinValue;
            outfit.Timestamp = reader.GetDateTime("timestamp");

            return outfit;
        }

    }
}
