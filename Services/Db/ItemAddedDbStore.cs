using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class ItemAddedDbStore {

        private readonly ILogger<ItemAddedDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        private readonly IDataReader<ItemAddedEvent> _Reader;

        public ItemAddedDbStore(ILogger<ItemAddedDbStore> logger,
            IDbHelper dbHelper, IDataReader<ItemAddedEvent> reader) {

            _Logger = logger;

            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Insert a new <see cref="ItemAddedEvent"/>
        /// </summary>
        /// <param name="ev">Event to insert</param>
        /// <returns>
        ///     The <see cref="ItemAddedEvent.ID"/> that was created
        /// </returns>
        public async Task<long> Insert(ItemAddedEvent ev) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO item_added (
                    character_id, item_id, context, item_count, timestamp, zone_id, world_id
                ) VALUES (
                    @CharacterID, @ItemID, @Context, @ItemCount, @Timestamp, @ZoneID, @WorldID
                ) RETURNING id;
            ");

            cmd.AddParameter("CharacterID", ev.CharacterID);
            cmd.AddParameter("ItemID", ev.ItemID);
            cmd.AddParameter("Context", ev.Context);
            cmd.AddParameter("ItemCount", ev.ItemCount);
            cmd.AddParameter("Timestamp", ev.Timestamp);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("WorldID", ev.WorldID);

            long ID = await cmd.ExecuteInt64(CancellationToken.None);

            return ID;
        }

        public async Task<List<ItemAddedEvent>> GetByCharacterAndPeriod(string charID, DateTime start, DateTime end) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM item_added
                    WHERE character_id = @CharID
                        AND timestamp BETWEEN @Start and @End;
            ");

            cmd.AddParameter("CharID", charID);
            cmd.AddParameter("Start", start);
            cmd.AddParameter("End", end);

            List<ItemAddedEvent> events = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return events;
        }

    }
}
