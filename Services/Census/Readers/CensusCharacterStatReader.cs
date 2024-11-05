using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusCharacterStatReader : ICensusReader<PsCharacterStat> {
        public CensusCharacterStatReader(CensusMetric metrics) : base(metrics) {
        }

        public override PsCharacterStat? ReadEntry(JsonElement token) {
            PsCharacterStat stat = new PsCharacterStat();

            stat.CharacterID = token.GetRequiredString("character_id");
            stat.StatName = token.GetRequiredString("stat_name");
            stat.ProfileID = token.GetInt32("profile_id", 0);
            stat.ValueForever = token.GetInt32("value_forever", 0);
            stat.ValueMonthly = token.GetInt32("value_monthly", 0);
            stat.ValueWeekly = token.GetInt32("value_weekly", 0);
            stat.ValueDaily = token.GetInt32("value_daily", 0);
            stat.ValueMaxOneLife = token.GetInt32("value_one_life_max", 0);
            stat.Timestamp = token.CensusTimestamp("last_save");

            return stat;
        }

    }
}
