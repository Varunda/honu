using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public sealed class Experience {

        public const string ASSIST = "2";
        public const string SPAWN_ASSIST = "3";
        public const string PRIORITY_ASSIST = "371";
        public const string HIGH_PRIORITY_ASSIST = "372";
        public const string HEAL = "4";
        public const string MAX_REPAIR = "6";
        public const string REVIVE = "7";
        public const string RESUPPLY = "34";
        public const string SQUAD_HEAL = "51";
        public const string SQUAD_REVIVE = "53";
        public const string SQUAD_RESUPPLY = "55";
        public const string SQUAD_MAX_REPAIR = "142";
        public const string SQUAD_SPAWN = "56";
        public const string GALAXY_SPAWN_BONUS = "201";
        public const string SUNDERER_SPAWN_BONUS = "233";
        public const string SQUAD_VEHICLE_SPAWN_BONUS = "355";
        public const string GENERIC_NPC_SPAWN = "1410";

        public static bool IsAssist(string expId) {
            return expId == ASSIST || expId == SPAWN_ASSIST
                || expId == PRIORITY_ASSIST || expId == HIGH_PRIORITY_ASSIST;
        }

        public static bool IsSpawn(string expId) {
            return expId == SQUAD_SPAWN || expId == SQUAD_VEHICLE_SPAWN_BONUS
                || expId == GALAXY_SPAWN_BONUS || expId == GENERIC_NPC_SPAWN
                || expId == SUNDERER_SPAWN_BONUS;
        }

    }
}
