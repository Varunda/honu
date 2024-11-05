using Newtonsoft.Json.Linq;
using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterReader : ICensusReader<PsCharacter> {
        public CensusCharacterReader(CensusMetric metrics) : base(metrics) {
        }

        public override PsCharacter? ReadEntry(JsonElement token) {
            PsCharacter player = new() {
                ID = token.GetString("character_id", "0"),
                FactionID = token.GetInt16("faction_id", -1),
                Prestige = token.GetInt32("prestige_level", 0),
                WorldID = token.GetWorldID()
            };

            player.Name = token.GetChild("name")?.GetString("first", "<missing name>") ?? "<missing name>";

            JsonElement? times = token.GetChild("times");
            if (times != null) {
                player.DateCreated = times.Value.CensusTimestamp("creation");
                player.DateLastLogin = times.Value.CensusTimestamp("last_login");
                player.DateLastSave = times.Value.CensusTimestamp("last_save");
            }

            JsonElement? outfit = token.GetChild("outfit");
            if (outfit != null) {
                player.OutfitID = outfit.Value.GetRequiredString("outfit_id");
                player.OutfitName = outfit.Value.GetString("name", "<missing name>");
                player.OutfitTag = outfit.Value.GetValue<string?>("alias");
            }

            player.BattleRank = token.GetChild("battle_rank")?.GetInt16("value", 0) ?? 0;
            player.MinutesPlayed = token.GetChild("times")?.GetInt64("minutes_played", 0) ?? 0;

            return player;
        }

    }
}
