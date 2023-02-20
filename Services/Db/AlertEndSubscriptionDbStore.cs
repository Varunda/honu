using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Db;

namespace watchtower.Services.Db {

    /// <summary>
    ///     DB service to interact with the table that stores <see cref="AlertEndSubscription"/>s
    /// </summary>
    public class AlertEndSubscriptionDbStore {

        private readonly ILogger<AlertEndSubscriptionDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<AlertEndSubscription> _Reader;

        public AlertEndSubscriptionDbStore(ILogger<AlertEndSubscriptionDbStore> logger,
            IDbHelper dbHelper, IDataReader<AlertEndSubscription> reader) {
            _Logger = logger;

            _DbHelper = dbHelper;
            _Reader = reader;
        }

        /// <summary>
        ///     Get all <see cref="AlertEndSubscription"/>s that exist in the DB
        /// </summary>
        public async Task<List<AlertEndSubscription>> GetAll() {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_end_subscription;
            ");

            List<AlertEndSubscription> subs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return subs;
        }

        /// <summary>
        ///     Get a specific <see cref="AlertEndSubscription"/> by it's ID
        /// </summary>
        /// <param name="ID">ID of the <see cref="AlertEndSubscription"/> to get</param>
        /// <returns>
        ///     The <see cref="AlertEndSubscription"/> with <see cref="AlertEndSubscription.ID"/> of <paramref name="ID"/>,
        ///     or <c>null</c> if it does not exist
        /// </returns>
        public async Task<AlertEndSubscription?> GetByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_end_subscription
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            AlertEndSubscription? sub = await _Reader.ReadSingle(cmd);
            await conn.CloseAsync();

            return sub;
        }

        /// <summary>
        ///     Get the <see cref="AlertEndSubscription"/>s created by a specific user
        /// </summary>
        /// <param name="createdID">ID of the Discord user that created the subscription</param>
        /// <returns>
        ///     A list of <see cref="AlertEndSubscription"/> with <see cref="AlertEndSubscription.CreatedByID"/>
        ///     of <paramref name="createdID"/>
        /// </returns>
        public async Task<List<AlertEndSubscription>> GetByCreatedID(ulong createdID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_end_subscription
                    WHERE created_by_id = @CreatedByID;
            ");

            cmd.AddParameter("CreatedByID", createdID);

            List<AlertEndSubscription> subs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return subs;
        }

        /// <summary>
        ///     Get all <see cref="AlertEndSubscription"/> that will be posted to a specific channel
        /// </summary>
        /// <param name="channelID">Discord ID of the channel</param>
        /// <returns>
        ///     A list of <see cref="AlertEndSubscription"/> with <see cref="AlertEndSubscription.ChannelID"/>
        ///     of <paramref name="channelID"/>
        /// </returns>
        public async Task<List<AlertEndSubscription>> GetByChannelID(ulong channelID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM alert_end_subscription
                    WHERE channel_id = @ChannelID;
            ");

            cmd.AddParameter("ChannelID", channelID);

            List<AlertEndSubscription> subs = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return subs;
        }

        /// <summary>
        ///     Insert a new <see cref="AlertEndSubscription"/> to the DB
        /// </summary>
        /// <param name="sub">Subscription to insert</param>
        /// <returns>
        ///     The row ID in the DB of the <see cref="AlertEndSubscription"/> that was just inserted
        /// </returns>
        public async Task<long> Insert(AlertEndSubscription sub) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO alert_end_subscription (
                    created_by_id, timestamp,
                    guild_id, channel_id, 
                    outfit_id, character_id, world_id,
                    world_character_minimum, outfit_character_minimum, character_minimum_seconds
                ) VALUES (
                    @CreatedByID, NOW() AT TIME ZONE 'utc',
                    @GuildID, @ChannelID,
                    @OutfitID, @CharacterID, @WorldID,
                    @WorldCharacterMinimum, @OutfitCharacterMinimum, @CharacterMinimumSeconds
                ) RETURNING id;
            ");

            cmd.AddParameter("CreatedByID", sub.CreatedByID);
            cmd.AddParameter("GuildID", sub.GuildID);
            cmd.AddParameter("ChannelID", sub.ChannelID);
            cmd.AddParameter("OutfitID", sub.OutfitID);
            cmd.AddParameter("CharacterID", sub.CharacterID);
            cmd.AddParameter("WorldID", sub.WorldID);
            cmd.AddParameter("WorldCharacterMinimum", sub.WorldCharacterMinimum);
            cmd.AddParameter("OutfitCharacterMinimum", sub.OutfitCharacterMinimum);
            cmd.AddParameter("CharacterMinimumSeconds", sub.CharacterMinimumSeconds);

            long id = await cmd.ExecuteInt64(CancellationToken.None);
            return id;
        }

        /// <summary>
        ///     Delete a specific <see cref="AlertEndSubscription"/> from the DB by it's ID
        /// </summary>
        /// <param name="ID">ID to delete</param>
        public async Task DeleteByID(long ID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                DELETE FROM alert_end_subscription
                    WHERE id = @ID;
            ");

            cmd.AddParameter("ID", ID);

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }
}
