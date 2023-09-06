using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Census;

namespace watchtower.Services.Census {
    public class KillboardCollection {

        private readonly ILogger<KillboardCollection> _Logger;

        private readonly ICensusQueryFactory _Census;
        private readonly ICensusReader<CharacterEventGrouped> _Reader;

        public KillboardCollection(ILogger<KillboardCollection> logger,
            ICensusQueryFactory census, ICensusReader<CharacterEventGrouped> reader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader;
        }

        public async Task<List<KillboardEntry>> GetByCharacterID(string charID) {
            CensusQuery query = _Census.Create("characters_event_grouped");
            query.Where("character_id").Equals(charID);

            List<CharacterEventGrouped> groups = await _Reader.ReadList(query);

            Dictionary<string, KillboardEntry> map = new();

            foreach (CharacterEventGrouped group in groups) {
                if (group.CharacterID == charID) {
                    continue;
                }

                if (map.TryGetValue(group.CharacterID, out KillboardEntry? entry) == false) {
                    entry = new KillboardEntry();
                    entry.SourceCharacterID = charID;
                    entry.OtherCharacterID = group.CharacterID;
                }

                if (group.TableType == "DEATH") {
                    entry.Deaths += group.Count;
                } else if (group.TableType == "KILL") {
                    entry.Kills += group.Count;
                } else {
                    _Logger.LogWarning($"unchecked TableType: {group.TableType}");
                }

                map[group.CharacterID] = entry;
            }

            return map.Values
                .OrderByDescending(iter => iter.Kills + iter.Deaths)
                .Take(200)
                .ToList();
        }

    }
}
