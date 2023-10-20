using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Code.Tracking;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterFriendCollection {

        private readonly ILogger<CharacterFriendCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        public CharacterFriendCollection(ILogger<CharacterFriendCollection> logger,
            ICensusQueryFactory census) {

            _Logger = logger;
            _Census = census;
        }

        /// <summary>
        ///     Get the friends of a character
        /// </summary>
        /// <param name="charID">ID of the character to get the friends of</param>
        public async Task<List<CharacterFriend>> GetByCharacterID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character friends - get by character id");
            trace?.AddTag("honu.characterID", charID);

            CensusQuery query = _Census.Create("characters_friend");
            query.Where("character_id").Equals(charID);

            JsonElement? token = await query.GetAsync();
            if (token == null || token.Value.ValueKind == JsonValueKind.Undefined) {
                return new List<CharacterFriend>();
            }

            JsonElement? friendToken = token.Value.GetChild("friend_list");
            if (friendToken == null || friendToken.Value.ValueKind == JsonValueKind.Undefined) {
                throw new FormatException($"Failed to get token 'friend_list' for {charID} from {token}");
            }

            List<CharacterFriend> res = new List<CharacterFriend>();

            foreach (JsonElement friend in friendToken.Value.EnumerateArray()) {
                CharacterFriend f = _Parse(friend, charID);
                res.Add(f);
            }

            return res;
        }

        private CharacterFriend _Parse(JsonElement token, string charID) {
            return new CharacterFriend() {
                CharacterID = charID,
                FriendID = token.GetRequiredString("character_id")
            };
        }

    }
}
