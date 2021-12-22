using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;
using watchtower.Models.Db;

namespace watchtower.Services.Db.Readers {

    public class CharacterFriendReader : IDataReader<CharacterFriend> {

        public override CharacterFriend? ReadEntry(NpgsqlDataReader reader) {
            CharacterFriend friend = new CharacterFriend();
            
            friend.CharacterID = reader.GetString("character_id");
            friend.FriendID = reader.GetString("friend_id");

            return friend;
        }

    }
}
