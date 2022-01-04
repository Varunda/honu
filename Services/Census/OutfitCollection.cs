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

namespace watchtower.Services.Census {

    /// <summary>
    ///     Service to get data from the /outfit collection
    /// </summary>
    public class OutfitCollection {

        private readonly ILogger<OutfitCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        private readonly ICensusReader<PsOutfit> _Reader;
        private readonly ICensusReader<OutfitMember> _MemberReader;

        public OutfitCollection(ILogger<OutfitCollection> logger,
            ICensusQueryFactory census, ICensusReader<OutfitMember> memberReader,
            ICensusReader<PsOutfit> reader) {

            _Logger = logger;
            _Census = census;

            _MemberReader = memberReader ?? throw new ArgumentNullException(nameof(memberReader));
            _Reader = reader;
        }

        /// <summary>
        ///     Get an outfit by it's ID
        /// </summary>
        /// <param name="outfitID">ID of the outfit to get</param>
        /// <returns>
        ///     The <see cref="PsOutfit"/> with <see cref="PsOutfit.ID"/> of <paramref name="outfitID"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public Task<PsOutfit?> GetByID(string outfitID) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("outfit_id").Equals(outfitID);
            query.AddResolve("leader");

            return _Reader.ReadSingle(query);
        }

        /// <summary>
        ///     Get an outfit by it's tag (case-insensitive)
        /// </summary>
        /// <param name="tag">Tag of the outfit to get</param>
        /// <returns>
        ///     The <see cref="PsOutfit"/> with tag of <paramref name="tag"/>,
        ///     or <c>null</c> if it doesn't exist
        /// </returns>
        public Task<PsOutfit?> GetByTag(string tag) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("alias_lower").Equals(tag.ToLower());
            query.AddResolve("leader");

            return _Reader.ReadSingle(query);
        }

        /// <summary>
        ///     Search the outfit collection for outfits that match the name
        /// </summary>
        /// <param name="name">Name to search by</param>
        /// <returns></returns>
        public Task<List<PsOutfit>> SearchByName(string name) {
            CensusQuery query = _Census.Create("outfit");
            query.Where("name_lower").Contains(name);
            query.AddResolve("leader");
            query.SetLimit(100);

            return _Reader.ReadList(query);
        }

        /// <summary>
        ///     Get the members of an outfit
        /// </summary>
        /// <param name="outfitID">ID of the outfit to get the members of</param>
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

    }
}
