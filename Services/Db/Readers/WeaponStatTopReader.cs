using Npgsql;
using System;
using System.Data;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class WeaponStatTopReader : IDataReader<WeaponStatTop> {

        public override WeaponStatTop? ReadEntry(NpgsqlDataReader reader) {
            WeaponStatTop top = new();

            top.ID = reader.GetInt64("id");

            top.TypeID = reader.GetInt16("type_id");
            top.WorldID = reader.GetInt16("world_id");
            top.FactionID = reader.GetInt16("faction_id");

            top.ItemID = reader.GetInt32("item_id");
            top.CharacterID = reader.GetString("character_id");
            top.VehicleID = reader.GetInt32("vehicle_id");

            top.Timestamp = reader.GetDateTime("timestamp");

            top.Kills = reader.GetInt32("kills");
            top.Deaths = reader.GetInt32("deaths");
            top.Shots = reader.GetInt32("shots");
            top.ShotsHit = reader.GetInt32("shots_hit");
            top.Headshots = reader.GetInt32("headshots");
            top.VehicleKills = reader.GetInt32("vehicle_kills");
            top.SecondsWith = reader.GetInt32("seconds_with");

            top.KillDeathRatio = top.Kills / Math.Max(1d, top.Deaths);
            top.KillsPerMinute = top.Kills / (Math.Max(1d, top.SecondsWith) / 60d);
            top.Accuracy = top.ShotsHit / Math.Max(1d, top.Shots);
            top.HeadshotRatio = top.Headshots / Math.Max(1d, top.Kills);
            top.VehicleKillsPerMinute = top.VehicleKills / (Math.Max(1d, top.SecondsWith) / 60d);

            return top;
        }

    }
}
