using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Implementations {

    public class OutfitCollection : IOutfitCollection {

        private readonly ILogger<OutfitCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        private readonly ICensusReader<OutfitMember> _MemberReader;

        public OutfitCollection(ILogger<OutfitCollection> logger,
            ICensusQueryFactory census, ICensusReader<OutfitMember> memberReader) {

            _Logger = logger;
            _Census = census;

            _MemberReader = memberReader ?? throw new ArgumentNullException(nameof(memberReader));
        }

        public Task<PsOutfit?> GetByID(string outfitID) {
            return GetFromCensusByID(outfitID, true);
        }

        public Task<PsOutfit?> GetByTag(string tag) {
            return GetFromCensusByTag(tag, true);
        }

        public async Task<List<OutfitMember>> GetMembers(string outfitID) {
            CensusQuery query = _Census.Create("outfit_member");
            query.Where("outfit_id").Equals(outfitID);
            query.SetLimit(5000);

            // c:join=characters_world^to:character_id^on:character_id^inject_at:world_id
            CensusJoin join = query.JoinService("characters_world");
            join.ToField("character_id");
            join.OnField("character_id");
            join.WithInjectAt("world_id");

            Stopwatch timer = Stopwatch.StartNew();

            List<OutfitMember> members = new List<OutfitMember>();

            int iter = 0;

            while (iter < 10) {
                query.SetStart(iter * 5000);

                List<OutfitMember> page = await _MemberReader.ReadList(query);

                members.AddRange(page);
                ++iter;

                if (iter >= 10) {
                    _Logger.LogError($"Failed to get all members for outfit ID {outfitID} in 10 iterations. Currently have {members.Count} found, and got {page.Count} in current iteration");
                }

                if (page.Count < 5000) {
                    break;
                }
            }

            _Logger.LogDebug($"Took {timer.ElapsedMilliseconds}ms and {iter} iterations to load members for {outfitID}");

            return members;
        }

        private async Task<PsOutfit?> GetFromCensusByID(string outfitID, bool retry) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("outfit_id").Equals(outfitID);
            query.AddResolve("leader");

            //_Logger.LogDebug($"Getting outfit {outfitID} from census");

            PsOutfit? outfit = null;

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    outfit = Parse(result);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get outfit {outfitID}", outfitID);
                return await GetFromCensusByID(outfitID, false);
            }

            return outfit;
        }

        private async Task<PsOutfit?> GetFromCensusByTag(string tag, bool retry) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("alias_lower").Equals(tag.ToLower());

            query.AddResolve("leader");

            PsOutfit? outfit = null;

            try {
                JToken? result = await query.GetAsync();

                if (result != null) {
                    outfit = Parse(result);
                }
            } catch (Exception ex) {
                _Logger.LogError(ex, "Failed to get outfit {outfitID}", tag);
                if (retry == true) {
                    return await GetFromCensusByTag(tag, false);
                }
                throw;
            }

            return outfit;
        }

        private PsOutfit Parse(JToken result) {
            PsOutfit outfit = new PsOutfit() {
                ID = result.GetString("outfit_id", "0"),
                Name = result.GetString("name", "<MISSING NAME>"),
                Tag = result.NullableString("alias"),
                MemberCount = result.GetInt32("member_count", 0),
                LeaderID = result.GetString("leader_character_id", ""),
                DateCreated = result.CensusTimestamp("time_created")
            };

            JToken? leaderToken = result.SelectToken("leader");
            if (leaderToken == null) {
                _Logger.LogWarning($"Missing outfit leader for {outfit.ID}/{outfit.Name}");
            } else {
                outfit.FactionID = leaderToken.GetInt16("faction_id", -1);
            }

            return outfit;
        }

    }
}
