using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public sealed class Loadout {

        public const short NC_INFILTRATOR = 1;
        public const short NC_LIGHT_ASSAULT = 3;
        public const short NC_MEDIC = 4;
        public const short NC_ENGINEER = 5;
        public const short NC_HEAVY_ASSAULT = 6;
        public const short NC_MAX = 7;

        public const short TR_INFILTRATOR = 8;
        public const short TR_LIGHT_ASSAULT = 10;
        public const short TR_MEDIC = 11;
        public const short TR_ENGINEER = 12;
        public const short TR_HEAVY_ASSAULT = 13;
        public const short TR_MAX = 14;

        public const short VS_INFILTRATOR = 15;
        public const short VS_LIGHT_ASSAULT = 17;
        public const short VS_MEDIC = 18;
        public const short VS_ENGINEER = 19;
        public const short VS_HEAVY_ASSAULT = 20;
        public const short VS_MAX = 21;

        public const short NS_INFILTRATOR = 28;
        public const short NS_LIGHT_ASSAULT = 29;
        public const short NS_MEDIC = 30;
        public const short NS_ENGINEER = 31;
        public const short NS_HEAVY_ASSAULT = 32;
        public const short NS_MAX = 45;

        public static short GetFaction(short loadoutID) {
            if (loadoutID == NC_INFILTRATOR
                    || loadoutID == NC_LIGHT_ASSAULT
                    || loadoutID == NC_MEDIC 
                    || loadoutID == NC_ENGINEER 
                    || loadoutID == NC_HEAVY_ASSAULT
                    || loadoutID == NC_MAX) {
                return Faction.NC;
            } else if (loadoutID == VS_INFILTRATOR
                    || loadoutID == VS_LIGHT_ASSAULT
                    || loadoutID == VS_MEDIC 
                    || loadoutID == VS_ENGINEER 
                    || loadoutID == VS_HEAVY_ASSAULT
                    || loadoutID == VS_MAX) {
                return Faction.VS;
            } else if (loadoutID == TR_INFILTRATOR
                    || loadoutID == TR_LIGHT_ASSAULT
                    || loadoutID == TR_MEDIC 
                    || loadoutID == TR_ENGINEER 
                    || loadoutID == TR_HEAVY_ASSAULT
                    || loadoutID == TR_MAX) {
                return Faction.TR;
            } else if (loadoutID == NS_INFILTRATOR
                    || loadoutID == NS_LIGHT_ASSAULT
                    || loadoutID == NS_MEDIC 
                    || loadoutID == NS_ENGINEER 
                    || loadoutID == NS_HEAVY_ASSAULT
                    || loadoutID == NS_MAX) {
                return Faction.NS;
            }

            return Faction.UNKNOWN;
        }

    }
}
