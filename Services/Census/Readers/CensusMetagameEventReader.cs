﻿using System.Text.Json;
using watchtower.Code.ExtensionMethods;
using watchtower.Models.Census;
using watchtower.Services.Metrics;

namespace watchtower.Services.Census.Readers {

    public class CensusMetagameEventReader : ICensusReader<PsMetagameEvent> {
        public CensusMetagameEventReader(CensusMetric metrics) : base(metrics) {
        }

        public override PsMetagameEvent? ReadEntry(JsonElement token) {
            PsMetagameEvent ev = new();

            ev.ID = token.GetRequiredInt32("metagame_event_id");
            ev.Name = token.GetChild("name")?.GetString("en", "<missing en>") ?? "<missing name token>";
            ev.Description = token.GetChild("description")?.GetString("en", "<missing en>") ?? "<missing description token>";
            ev.TypeID = token.GetInt32("type", -1);
            ev.DurationMinutes = token.GetInt32("duration_minutes", 1);

            return ev;
        }
    }
}
