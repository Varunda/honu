using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Api;

namespace watchtower.Services.Db.Readers {

    public class VehicleUsageDataReader : IDataReader<VehicleUsageData> {

        public override VehicleUsageData? ReadEntry(NpgsqlDataReader reader) {
            VehicleUsageData data = new();

            data.ID = reader.GetUInt64("id");
            data.WorldID = reader.GetInt16("world_id");
            data.ZoneID = reader.GetUInt32("zone_id");
            data.Timestamp = reader.GetDateTime("timestamp");
            data.Total = reader.GetInt32("total_players");

            _CreateUsage(reader, "usage_vs", data.Vs);
            _CreateUsage(reader, "usage_nc", data.Nc);
            _CreateUsage(reader, "usage_tr", data.Tr);
            _CreateUsage(reader, "usage_other", data.Other);

            return data;
        }

        /// <summary>
        ///     the vehicle usage data is stored as a map where keys are the vehicle ID, and the value is the count.
        ///     this function converts the {id,count} pairs into {id,<see cref="VehicleUsageEntry"/>} data
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldName"></param>
        /// <param name="faction"></param>
        private void _CreateUsage(NpgsqlDataReader reader, string fieldName, VehicleUsageFaction faction) {
            Dictionary<int, int>? usageVs = JsonSerializer.Deserialize<Dictionary<int, int>>(reader.GetString(fieldName));
            if (usageVs != null) {
                foreach (KeyValuePair<int, int> kvp in usageVs) {
                    faction.Usage.Add(kvp.Key, new VehicleUsageEntry() {
                        VehicleID = kvp.Key,
                        Count = kvp.Value,
                        Vehicle = null,
                        VehicleName = ""
                    });
                }
                faction.Total = faction.Usage.Aggregate(0, (acc, iter) => acc += iter.Value.Count);
                faction.TotalVehicles = faction.Usage.Where(iter => iter.Key > 0)
                    .Aggregate(0, (acc, iter) => acc += iter.Value.Count);
            }
        }


    }
}
