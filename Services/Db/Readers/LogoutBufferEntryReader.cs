using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Queues;

namespace watchtower.Services.Db.Readers {

    public class LogoutBufferEntryReader : IDataReader<LogoutBufferEntry> {

        public override LogoutBufferEntry? ReadEntry(NpgsqlDataReader reader) {
            LogoutBufferEntry entry = new LogoutBufferEntry();

            entry.CharacterID = reader.GetString("character_id");
            entry.LoginTime = reader.GetDateTime("login_time");
            entry.Timestamp = reader.GetDateTime("timestamp");

            return entry;
        }

    }
}
