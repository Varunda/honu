using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusOutfitMemberReader : ICensusReader<OutfitMember> {

        public override OutfitMember? ReadEntry(JToken token) {
            OutfitMember member = new OutfitMember();

            member.OutfitID = token.GetRequiredString("outfit_id");
            member.CharacterID = token.GetRequiredString("character_id");
            member.MemberSince = token.CensusTimestamp("member_since");
            member.RankOrder = token.GetInt32("rank_ordinal", 0);
            member.Rank = token.GetString("rank", "");
            member.WorldID = token.SelectToken("world_id")?.GetWorldID();

            return member;
        }

    }
}
