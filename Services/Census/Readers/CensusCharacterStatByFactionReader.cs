using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterStatByFactionReader : ICensusReader<PsCharacterStatByFaction> {

        public override PsCharacterStatByFaction? ReadEntry(JsonElement token) {
            PsCharacterStatByFaction stat = new();

            stat.CharacterID = token.GetRequiredString("character_id");
            stat.StatName = token.GetRequiredString("stat_name");
            stat.ProfileID = token.GetInt32("profile_id", 0);
            stat.Timestamp = token.CensusTimestamp("last_save");

            stat.ValueForeverVS = token.GetInt32("value_forever_vs", 0);
            stat.ValueMonthlyVS = token.GetInt32("value_monthly_vs", 0);
            stat.ValueWeeklyVS = token.GetInt32("value_weekly_vs", 0);
            stat.ValueDailyVS = token.GetInt32("value_daily_vs", 0);
            stat.ValueOneLifeMaxVS = token.GetInt32("value_one_life_max_vs", 0);

            stat.ValueForeverNC = token.GetInt32("value_forever_nc", 0);
            stat.ValueMonthlyNC = token.GetInt32("value_monthly_nc", 0);
            stat.ValueWeeklyNC = token.GetInt32("value_weekly_nc", 0);
            stat.ValueDailyNC = token.GetInt32("value_daily_nc", 0);
            stat.ValueOneLifeMaxNC = token.GetInt32("value_one_life_max_nc", 0);

            stat.ValueForeverTR = token.GetInt32("value_forever_tr", 0);
            stat.ValueMonthlyTR = token.GetInt32("value_monthly_tr", 0);
            stat.ValueWeeklyTR = token.GetInt32("value_weekly_tr", 0);
            stat.ValueDailyTR = token.GetInt32("value_daily_tr", 0);
            stat.ValueOneLifeMaxTR = token.GetInt32("value_one_life_max_tr", 0);

            return stat;
        }

    }
}
