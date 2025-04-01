using Microsoft.Extensions.Logging;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Events;

namespace watchtower.Services.Db {

    public class FishCaughtDbStore {

        private readonly ILogger<FishCaughtDbStore> _Logger;
        private readonly IDbHelper _DbHelper;

        public FishCaughtDbStore(ILogger<FishCaughtDbStore> logger, IDbHelper dbHelper) {
            _Logger = logger;
            _DbHelper = dbHelper;
        }

        public async Task<ulong> Insert(FishCaughtEvent ev, CancellationToken cancel) {
            using NpgsqlConnection conn = _DbHelper.Connection(Dbs.EVENTS);
            using NpgsqlCommand cmd = await _DbHelper.Command(conn, @"
                INSERT INTO fish_caught (
                    world_id, zone_id, 
                    timestamp,
                    character_id, 
                    fish_id,
                    team_id, loadout_id
                ) VALUES (
                    @WorldID, @ZoneID,
                    @Timestamp,
                    @CharacterID,
                    @FishID,
                    @TeamID, @LoadoutID
                ) RETURNING id;
            ", cancel);

            cmd.AddParameter("WorldID", ev.WorldID);
            cmd.AddParameter("ZoneID", ev.ZoneID);
            cmd.AddParameter("Timestamp", ev.Timestamp);
            cmd.AddParameter("CharacterID", ev.CharacterID);
            cmd.AddParameter("FishID", ev.FishID);
            cmd.AddParameter("TeamID", ev.TeamID);
            cmd.AddParameter("LoadoutID", ev.LoadoutID);
            await cmd.PrepareAsync(cancel);

            ulong ID = await cmd.ExecuteUInt64(cancel);
            return ID;
        }

    }
}
