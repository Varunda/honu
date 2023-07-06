using DaybreakGames.Census;
using DaybreakGames.Census.Operators;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Code.Tracking;
using watchtower.Models.Census;

namespace watchtower.Services.Census {

    public class CharacterStatCollection {

        private readonly ILogger<CharacterStatCollection> _Logger;
        private readonly ICensusQueryFactory _Census;

        private readonly ICensusReader<PsCharacterStat> _Reader;
        private readonly ICensusReader<PsCharacterStatByFaction> _FactionReader;

        public CharacterStatCollection(ILogger<CharacterStatCollection> logger,
            ICensusQueryFactory census, ICensusReader<PsCharacterStat> reader,
            ICensusReader<PsCharacterStatByFaction> factionReader) {

            _Logger = logger;
            _Census = census;
            _Reader = reader;
            _FactionReader = factionReader;
        }

        /// <summary>
        ///     Get the <see cref="PsCharacterStat"/>s of a character
        /// </summary>
        /// <param name="charID">Character ID</param>
        public async Task<List<PsCharacterStat>> GetByID(string charID) {
            using Activity? trace = HonuActivitySource.Root.StartActivity("character stats - get by character id");
            trace?.AddTag("honu.characterID", charID);

            List<PsCharacterStat> stats = await GetStats(charID);

            List<PsCharacterStatByFaction> faction = await GetStatsByFaction(charID);

            foreach (PsCharacterStatByFaction fac in faction) {
                PsCharacterStat stat = new();
                stat.CharacterID = fac.CharacterID;
                stat.ProfileID = fac.ProfileID;
                stat.StatName = fac.StatName;
                stat.Timestamp = fac.Timestamp;

                stat.ValueForever = fac.ValueForeverNC + fac.ValueForeverTR + fac.ValueForeverVS;
                stat.ValueMonthly = fac.ValueMonthlyNC + fac.ValueMonthlyTR + fac.ValueMonthlyVS;
                stat.ValueWeekly = fac.ValueWeeklyNC + fac.ValueWeeklyTR + fac.ValueWeeklyVS;
                stat.ValueDaily = fac.ValueDailyNC + fac.ValueDailyTR + fac.ValueDailyVS;
                stat.ValueMaxOneLife = fac.ValueOneLifeMaxNC + fac.ValueOneLifeMaxTR + fac.ValueOneLifeMaxVS;

                stats.Add(stat);
            }

            return stats;
        }

        private async Task<List<PsCharacterStatByFaction>> GetStatsByFaction(string charID) {
            CensusQuery query = _Census.Create("characters_stat_by_faction");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            return await _FactionReader.ReadList(query);
        }

        private async Task<List<PsCharacterStat>> GetStats(string charID) {
            CensusQuery query = _Census.Create("characters_stat");
            query.Where("character_id").Equals(charID);
            query.SetLimit(1000);

            return await _Reader.ReadList(query);
        }

    }
}
