

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Npgsql;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Db {

    public class CharacterFriendDbStore {

        private readonly ILogger<CharacterFriendDbStore> _Logger;
        private readonly IDbHelper _DbHelper;
        private readonly IDataReader<CharacterFriend> _Reader;

        public CharacterFriendDbStore(ILogger<CharacterFriendDbStore> logger,
            IDbHelper helper, IDataReader<CharacterFriend> reader) {

            _Logger = logger;
            _DbHelper = helper;
            _Reader = reader;
        }

        public async Task<List<CharacterFriend>> GetByCharacterID(string charID) {
            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                SELECT *
                    FROM character_friends
                    WHERE character_id = @CharacterID;
            ");

            cmd.AddParameter("CharacterID", charID);

            List<CharacterFriend> friends = await _Reader.ReadList(cmd);
            await conn.CloseAsync();

            return friends;
        }

        /// <summary>
        ///     Set the friends of a character. This is destructive, and will delete all previous entries
        ///     and insert new entries from what's passed in <paramref name="friends"/>
        /// </summary>
        /// <remarks>
        ///     If 0 friends are passed in <paramref name="friends"/>, an exception is thrown to prevent
        ///     a potentially destructive set operation. Because all friends are deleted, then inserted,
        ///     it is possible to accidentally remove friends from a character if the Census call failed,
        ///     or the character was deleted
        /// </remarks>
        /// <param name="charID">ID of the character to set the friends of</param>
        /// <param name="friends">List of friends the character will be set to</param>
        public async Task Set(string charID, List<CharacterFriend> friends) {
            if (friends.Count == 0) {
                throw new Exception($"Refusing to set 0 friends for {charID}, this is potentially destructive");
            }

            using NpgsqlConnection conn = _DbHelper.Connection();
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, $@"
                BEGIN;

                DELETE FROM character_friends
                    WHERE character_id = @CharacterID;

                INSERT INTO character_friends (
                    character_id, friend_id, timestamp
                ) VALUES
                    {string.Join(",\n", friends.Select((_, index) => $"(@CharacterID, @FriendID_{index}, NOW() AT TIME ZONE 'utc')"))}
                ;

                COMMIT;
            ");

            cmd.AddParameter("CharacterID", charID);

            for (int i = 0; i < friends.Count; ++i) {
                cmd.AddParameter($"FriendID_{i}", friends[i].FriendID);
            }

            await cmd.ExecuteNonQueryAsync();
            await conn.CloseAsync();
        }

    }

}