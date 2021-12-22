using DaybreakGames.Census;
using DaybreakGames.Census.Exceptions;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
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
            CensusQuery query = _Census.Create("characters_friend");
            query.Where("character_id").Equals(charID);

            JToken token = await query.GetAsync();
            JToken? friendToken = token.SelectToken("friend_list");

            if (friendToken == null) {
                throw new FormatException($"Failed to get token 'friend_list' for {charID}");
            }

            List<CharacterFriend> res = new List<CharacterFriend>();

            if (friendToken is JArray friends) {
                foreach (JToken friend in friends) {
                    CharacterFriend f = _Parse(friend, charID);
                    res.Add(f);
                }
            } else {
                throw new FormatException($"Failed to convert 'friend_list' into an array");
            }

            return res;
        }

        private CharacterFriend _Parse(JToken token, string charID) {
            return new CharacterFriend() {
                CharacterID = charID,
                FriendID = token.GetRequiredString("character_id")
            };
        }

    }
}
