using Newtonsoft.Json.Linq;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterReader : ICensusReader<PsCharacter> {

        public override PsCharacter? ReadEntry(JToken token) {
            if (token == null) {
                return null;
            }

            PsCharacter player = new PsCharacter {
                ID = token.GetString("character_id", "0"),
                FactionID = token.GetInt16("faction_id", -1),
                Prestige = token.GetInt32("prestige_level", 0),
                WorldID = token.GetWorldID()
            };

            player.Name = token.SelectToken("name")?.GetString("first", "<missing name>") ?? "<missing name>";

            JToken? times = token.SelectToken("times");
            if (times != null) {
                player.DateCreated = times.CensusTimestamp("creation");
                player.DateLastLogin = times.CensusTimestamp("last_login");
                player.DateLastSave = times.CensusTimestamp("last_save");
            }

            JToken? outfit = token.SelectToken("outfit");
            if (outfit != null) {
                player.OutfitID = outfit.GetRequiredString("outfit_id");
                player.OutfitName = outfit.GetString("name", "<missing name>");
                player.OutfitTag = outfit.Value<string?>("alias");
            }

            player.BattleRank = token.SelectToken("battle_rank")?.GetInt16("value", 0) ?? 0;

            return player;
        }

    }
}
