using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public sealed class Loadout {

        public static string NC_INFILTRATOR = "1";
        public static string NC_LIGHT_ASSAULT = "3";
        public static string NC_MEDIC = "4";
        public static string NC_ENGINEER = "5";
        public static string NC_HEAVY_ASSAULT = "6";
        public static string NC_MAX = "7";

        public static string TR_INFILTRATOR = "8";
        public static string TR_LIGHT_ASSAULT = "10";
        public static string TR_MEDIC = "11";
        public static string TR_ENGINEER = "12";
        public static string TR_HEAVY_ASSAULT = "13";
        public static string TR_MAX = "14";

        public static string VS_INFILTRATOR = "15";
        public static string VS_LIGHT_ASSAULT = "17";
        public static string VS_MEDIC = "18";
        public static string VS_ENGINEER = "19";
        public static string VS_HEAVY_ASSAULT = "20";
        public static string VS_MAX = "21";

        public static string GetFaction(string loadoutID) {
            if (loadoutID == NC_INFILTRATOR
                    || loadoutID == NC_LIGHT_ASSAULT
                    || loadoutID == NC_MEDIC 
                    || loadoutID == NC_ENGINEER 
                    || loadoutID == NC_HEAVY_ASSAULT
                    || loadoutID == NC_MAX) {
                return Faction.NC;
            }
            if (loadoutID == VS_INFILTRATOR
                    || loadoutID == VS_LIGHT_ASSAULT
                    || loadoutID == VS_MEDIC 
                    || loadoutID == VS_ENGINEER 
                    || loadoutID == VS_HEAVY_ASSAULT
                    || loadoutID == VS_MAX) {
                return Faction.VS;
            }
            if (loadoutID == TR_INFILTRATOR
                    || loadoutID == TR_LIGHT_ASSAULT
                    || loadoutID == TR_MEDIC 
                    || loadoutID == TR_ENGINEER 
                    || loadoutID == TR_HEAVY_ASSAULT
                    || loadoutID == TR_MAX) {
                return Faction.TR;
            }

            return Faction.UNKNOWN;
        }

    }
}
