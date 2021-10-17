using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Implementations {

    public class WeaponStatPercentileCacheDbStore : IDataReader<WeaponStatPercentileCache>, IWeaponStatPercentileCacheDbStore {

        private readonly ILogger<WeaponStatPercentileCacheDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public WeaponStatPercentileCacheDbStore(ILogger<WeaponStatPercentileCacheDbStore> logger,
            IDbHelper dbHelper) {

            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<WeaponStatPercentileCache?> GetByItemID(string itemID, short typeID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM percentile_cache
                    WHERE item_id = @ItemID
                        AND type = @TypeID;
            ");

            cmd.AddParameter("ItemID", itemID);
            cmd.AddParameter("TypeID", typeID);

            WeaponStatPercentileCache? entry = await ReadSingle(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public async Task Upsert(string itemID, WeaponStatPercentileCache entry) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO percentile_cache (
                    item_id, type, timestamp, q0, q5, q10, q15, q20, q25, q30, q35, q40, q45, q50, q55, q60, q65, q70, q75, q80, q85, q90, q95, q100
                ) VALUES (
                    @ItemID, @Type, NOW() at time zone 'utc', @Q0, @Q5, @Q10, @Q15, @Q20, @Q25, @Q30, @Q35, @Q40, @Q45, @Q50, @Q55, @Q60, @Q65, @Q70, @Q75, @Q80, @Q85, @Q90, @Q95, @Q100
                ) ON CONFLICT (item_id, type) DO
                    UPDATE SET q0 = @Q0,
                        q5 = @Q5,
                        q10 = @Q10,
                        q15 = @Q15,
                        q20 = @Q20,
                        q25 = @Q25,
                        q30 = @Q30,
                        q35 = @Q35,
                        q40 = @Q40,
                        q45 = @Q45,
                        q50 = @Q50,
                        q55 = @Q55,
                        q60 = @Q60,
                        q65 = @Q65,
                        q70 = @Q70,
                        q75 = @Q75,
                        q80 = @Q80,
                        q85 = @Q85,
                        q90 = @Q90,
                        q95 = @Q95,
                        q100 = @Q100;
            ");

            cmd.AddParameter("ItemID", itemID);
            cmd.AddParameter("Type", entry.TypeID);
            cmd.AddParameter("Q0", entry.Q0);
            cmd.AddParameter("Q5", entry.Q5);
            cmd.AddParameter("Q10", entry.Q10);
            cmd.AddParameter("Q15", entry.Q15);
            cmd.AddParameter("Q20", entry.Q20);
            cmd.AddParameter("Q25", entry.Q25);
            cmd.AddParameter("Q30", entry.Q30);
            cmd.AddParameter("Q35", entry.Q35);
            cmd.AddParameter("Q40", entry.Q40);
            cmd.AddParameter("Q45", entry.Q45);
            cmd.AddParameter("Q50", entry.Q50);
            cmd.AddParameter("Q55", entry.Q55);
            cmd.AddParameter("Q60", entry.Q60);
            cmd.AddParameter("Q65", entry.Q65);
            cmd.AddParameter("Q70", entry.Q70);
            cmd.AddParameter("Q75", entry.Q75);
            cmd.AddParameter("Q80", entry.Q80);
            cmd.AddParameter("Q85", entry.Q85);
            cmd.AddParameter("Q90", entry.Q90);
            cmd.AddParameter("Q95", entry.Q95);
            cmd.AddParameter("Q100", entry.Q100);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

        public async Task<WeaponStatPercentileCache?> Generate(string itemID, string columnName) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @$"
                SELECT
                    @ItemID AS item_id,
                    0 AS type,
                    NOW() at time zone 'utc' AS timestamp,
	                percentile_disc(0.00) within group (order by s.{columnName}) AS q0,
	                percentile_disc(0.05) within group (order by s.{columnName}) AS q5,
	                percentile_disc(0.10) within group (order by s.{columnName}) AS q10,
	                percentile_disc(0.15) within group (order by s.{columnName}) AS q15,
	                percentile_disc(0.20) within group (order by s.{columnName}) AS q20,
	                percentile_disc(0.25) within group (order by s.{columnName}) AS q25,
	                percentile_disc(0.30) within group (order by s.{columnName}) AS q30,
	                percentile_disc(0.35) within group (order by s.{columnName}) AS q35,
	                percentile_disc(0.40) within group (order by s.{columnName}) AS q40,
	                percentile_disc(0.45) within group (order by s.{columnName}) AS q45,
	                percentile_disc(0.50) within group (order by s.{columnName}) AS q50,
	                percentile_disc(0.55) within group (order by s.{columnName}) AS q55,
	                percentile_disc(0.60) within group (order by s.{columnName}) AS q60,
	                percentile_disc(0.65) within group (order by s.{columnName}) AS q65,
	                percentile_disc(0.70) within group (order by s.{columnName}) AS q70,
	                percentile_disc(0.75) within group (order by s.{columnName}) AS q75,
	                percentile_disc(0.80) within group (order by s.{columnName}) AS q80,
	                percentile_disc(0.85) within group (order by s.{columnName}) AS q85,
	                percentile_disc(0.90) within group (order by s.{columnName}) AS q90,
	                percentile_disc(0.95) within group (order by s.{columnName}) AS q95,
	                percentile_disc(1.00) within group (order by s.{columnName}) AS q100
                FROM 
                    weapon_stats s
	            WHERE 
                    item_id = @ItemID
	            AND 
                    kills > 1159;
            ");

            cmd.AddParameter("ItemID", itemID);

            WeaponStatPercentileCache? entry = await ReadSingle(cmd);
            await conn.CloseAsync();

            return entry;
        }

        public override WeaponStatPercentileCache ReadEntry(NpgsqlDataReader reader) {
            WeaponStatPercentileCache entry = new WeaponStatPercentileCache();

            entry.Loaded = true;
            entry.ItemID = reader.GetString("item_id");
            entry.TypeID = reader.GetInt16("type");
            entry.Timestamp = reader.GetDateTime("timestamp");
            entry.Q0 = reader.GetDouble("q0");
            entry.Q5 = reader.GetDouble("q5");
            entry.Q10 = reader.GetDouble("q10");
            entry.Q15 = reader.GetDouble("q15");
            entry.Q20 = reader.GetDouble("q20");
            entry.Q25 = reader.GetDouble("q25");
            entry.Q30 = reader.GetDouble("q30");
            entry.Q35 = reader.GetDouble("q35");
            entry.Q40 = reader.GetDouble("q40");
            entry.Q45 = reader.GetDouble("q45");
            entry.Q50 = reader.GetDouble("q50");
            entry.Q55 = reader.GetDouble("q55");
            entry.Q60 = reader.GetDouble("q60");
            entry.Q65 = reader.GetDouble("q65");
            entry.Q70 = reader.GetDouble("q70");
            entry.Q75 = reader.GetDouble("q75");
            entry.Q80 = reader.GetDouble("q80");
            entry.Q85 = reader.GetDouble("q85");
            entry.Q90 = reader.GetDouble("q90");
            entry.Q95 = reader.GetDouble("q95");
            entry.Q100 = reader.GetDouble("q100");

            return entry;
        }

    }
}
