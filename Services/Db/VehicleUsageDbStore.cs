using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Api;

namespace watchtower.Services.Db {

    public class VehicleUsageDbStore {

        private readonly ILogger<VehicleUsageDbStore> _Logger;
        private readonly IDataReader<VehicleUsageData> _Reader;
        private readonly IDbHelper _Helper;

        public VehicleUsageDbStore(ILogger<VehicleUsageDbStore> logger,
            IDataReader<VehicleUsageData> reader, IDbHelper helper) {

            _Logger = logger;
            _Reader = reader;
            _Helper = helper;
        }

        /// <summary>
        ///     insert a <see cref="VehicleUsageData"/> into the DB
        /// </summary>
        /// <param name="data">vehicle usage data that is saved to the DB</param>
        /// <param name="cancel">cancellation token</param>
        /// <returns>a task for when the async operation is complete</returns>
        public async Task Insert(VehicleUsageData data, CancellationToken cancel = default) {
            using NpgsqlConnection conn = _Helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _Helper.Command(conn, @"
                INSERT INTO vehicle_usage (
                    world_id, zone_id, timestamp, total_players, 
                    usage_vs, usage_nc, usage_tr, usage_other
                ) VALUES (
                    @WorldID, @ZoneID, @Timestamp, @TotalPlayers,
                    @UsageVs, @UsageNc, @UsageTr, @UsageOther
                ) RETURNING id;
            ");

            cmd.AddParameter("WorldID", data.WorldID);
            cmd.AddParameter("ZoneID", data.ZoneID);
            cmd.AddParameter("Timestamp", data.Timestamp);
            cmd.AddParameter("TotalPlayers", data.Total);
            cmd.AddParameter("UsageVs", _ConvertFactionDataToJson(data.Vs));
            cmd.AddParameter("UsageNc", _ConvertFactionDataToJson(data.Nc));
            cmd.AddParameter("UsageTr", _ConvertFactionDataToJson(data.Tr));
            cmd.AddParameter("UsageOther", _ConvertFactionDataToJson(data.Other));

            await cmd.PrepareAsync(cancel);

            await cmd.ExecuteInt64(cancel);
            await conn.CloseAsync();
        }

        /// <summary>
        ///     load a <see cref="VehicleUsageData"/> that was saved in the DB by
        ///     its <see cref="VehicleUsageData.ID"/>
        /// </summary>
        /// <param name="ID">ID of the <see cref="VehicleUsageData"/> to get</param>
        /// <returns>
        ///     the <see cref="VehicleUsageData"/> with <see cref="VehicleUsageData.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<VehicleUsageData?> GetByID(ulong ID) {
            using NpgsqlConnection conn = _Helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _Helper.Command(conn, @"
                SELECT *
                    FROM vehicle_usage
                    WHERE id = @ID;
            ");
            cmd.AddParameter("ID", ID);
            await cmd.PrepareAsync();

            VehicleUsageData? data = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return data;
        }

        /// <summary>
        ///     get <see cref="VehicleUsageData"/> between 2 periods
        /// </summary>
        /// <param name="start">start of the period</param>
        /// <param name="end">end of the period</param>
        /// <param name="cancel">cancellation token, default to <see cref="CancellationToken.None"/></param>
        /// <returns>
        ///     a list of <see cref="VehicleUsageData"/> with <see cref="VehicleUsageData.Timestamp"/>
        ///     between <paramref name="start"/> and <paramref name="end"/>
        /// </returns>
        public async Task<List<VehicleUsageData>> GetByTimestamp(DateTime start, DateTime end, CancellationToken cancel = default) {
            using NpgsqlConnection conn = _Helper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _Helper.Command(conn, @"
                SELECT *
                    FROM vehicle_usage
                    WHERE timestamp BETWEEN @Start AND @End;
            ");
            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);
            await cmd.PrepareAsync(cancel);

            List<VehicleUsageData> data = await _Reader.ReadList(cmd, cancel);
            await conn.CloseAsync();

            return data;
        }

        private JsonElement _ConvertFactionDataToJson(VehicleUsageFaction data) {
            Dictionary<int, int> usage = new(
                data.Usage.Where(iter => iter.Value.Count > 0)
                    .Select(iter => {
                        return new KeyValuePair<int, int>(iter.Key, iter.Value.Count);
                    })
            );

            return JsonSerializer.SerializeToElement(usage, JsonSerializerOptions.Default);
        }

    }
}
