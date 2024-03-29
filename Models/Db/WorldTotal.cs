﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Db {

    public class WorldTotal {

        public const string TOTAL_VS_KILLS = "vs_kills";
        public const string TOTAL_VS_DEATHS = "vs_deaths";
        public const string TOTAL_VS_ASSISTS = "vs_assists";
        public const string TOTAL_VS_HEALS = "vs_heals";
        public const string TOTAL_VS_REVIVES = "vs_revives";
        public const string TOTAL_VS_RESUPPLIES = "vs_resupplies";
        public const string TOTAL_VS_SPAWNS = "vs_spawns";
        public const string TOTAL_VS_VEHICLE_KILLS = "vs_vehicle_kills";
        public const string TOTAL_VS_SHIELD_REPAIR = "vs_shield_repair";
        public const string TOTAL_VS_KILLS_NC = "vs_kills_nc";
        public const string TOTAL_VS_KILLS_TR = "vs_kills_tr";

        public const string TOTAL_NC_KILLS = "nc_kills";
        public const string TOTAL_NC_DEATHS = "nc_deaths";
        public const string TOTAL_NC_ASSISTS = "nc_assists";
        public const string TOTAL_NC_HEALS = "nc_heals";
        public const string TOTAL_NC_REVIVES = "nc_revives";
        public const string TOTAL_NC_RESUPPLIES = "nc_resupplies";
        public const string TOTAL_NC_SPAWNS = "nc_spawns";
        public const string TOTAL_NC_VEHICLE_KILLS = "nc_vehicle_kills";
        public const string TOTAL_NC_SHIELD_REPAIR = "nc_shield_repair";
        public const string TOTAL_NC_KILLS_VS = "nc_kills_vs";
        public const string TOTAL_NC_KILLS_TR = "nc_kills_tr";

        public const string TOTAL_TR_KILLS = "tr_kills";
        public const string TOTAL_TR_DEATHS = "tr_deaths";
        public const string TOTAL_TR_ASSISTS = "tr_assists";
        public const string TOTAL_TR_HEALS = "tr_heals";
        public const string TOTAL_TR_REVIVES = "tr_revives";
        public const string TOTAL_TR_RESUPPLIES = "tr_resupplies";
        public const string TOTAL_TR_SPAWNS = "tr_spawns";
        public const string TOTAL_TR_VEHICLE_KILLS = "tr_vehicle_kills";
        public const string TOTAL_TR_SHIELD_REPAIR = "tr_shield_repair";
        public const string TOTAL_TR_KILLS_VS = "tr_kills_vs";
        public const string TOTAL_TR_KILLS_NC = "tr_kills_nc";

        public List<WorldTotalEntry> Entries { get; set; } = new List<WorldTotalEntry>();

        public int GetValue(string key) {
            return Entries.FirstOrDefault(iter => iter.Key == key)?.Value ?? 0;
        }

    }

    public class WorldTotalEntry {

        public string Key { get; set; } = "";

        public int Value { get; set; }

    }

}
