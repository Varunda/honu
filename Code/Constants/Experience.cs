using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public sealed class Experience {

        public const int ASSIST = 2;
        public const int SPAWN_ASSIST = 3;
        public const int PRIORITY_ASSIST = 371;
        public const int HIGH_PRIORITY_ASSIST = 372;

        public const int HEAL = 4;
        public const int MAX_REPAIR = 6;
        public const int REVIVE = 7;
        public const int RESUPPLY = 34;

        public const int SQUAD_HEAL = 51;
        public const int SQUAD_REVIVE = 53;
        public const int SQUAD_RESUPPLY = 55;
        public const int SQUAD_MAX_REPAIR = 142;

        public const int SQUAD_SPAWN = 56;
        public const int GALAXY_SPAWN_BONUS = 201;
        public const int SUNDERER_SPAWN_BONUS = 233;
        public const int SQUAD_VEHICLE_SPAWN_BONUS = 355;
        public const int GENERIC_NPC_SPAWN = 1410;

        public const int VKILL_FLASH = 24;
        public const int VKILL_GALAXY = 60;
        public const int VKILL_LIBERATOR = 61;
        public const int VKILL_LIGHTNING = 62;
        public const int VKILL_MAGRIDER = 63;
        public const int VKILL_MOSQUITO = 64;
        public const int VKILL_PROWLER = 65;
        public const int VKILL_REAVER = 66;
        public const int VKILL_SCYTHE = 67;
        // Where'd VKILL_SUNDERER go? :)
        public const int VKILL_VANGUARD = 69;
        public const int VKILL_HARASSER = 301;
        public const int VKILL_VALKYRIE = 501;
        public const int VKILL_ANT = 651;
        public const int VKILL_COLOSSUS = 1449;
        public const int VKILL_JAVELIN = 1480;

        public static List<int> VehicleKillEvents = new List<int>() {
            VKILL_FLASH, VKILL_GALAXY, VKILL_LIBERATOR,
            VKILL_LIGHTNING, VKILL_MAGRIDER, VKILL_MOSQUITO,
            VKILL_PROWLER, VKILL_REAVER, VKILL_SCYTHE,
            VKILL_VANGUARD, VKILL_HARASSER, VKILL_VALKYRIE,
            VKILL_ANT, VKILL_COLOSSUS, VKILL_JAVELIN,
        };

        public static bool IsAssist(int expId) {
            return expId == ASSIST || expId == SPAWN_ASSIST
                || expId == PRIORITY_ASSIST || expId == HIGH_PRIORITY_ASSIST;
        }

        public static bool IsHeal(int expID) {
            return expID == HEAL || expID == SQUAD_HEAL;
        }

        public static bool IsRevive(int expId) {
            return expId == REVIVE || expId == SQUAD_REVIVE;
        }

        public static bool IsResupply(int expId) {
            return expId == RESUPPLY || expId == SQUAD_RESUPPLY;
        }

        public static bool IsSpawn(int expId) {
            return expId == SQUAD_SPAWN || expId == SQUAD_VEHICLE_SPAWN_BONUS
                || expId == GALAXY_SPAWN_BONUS || expId == GENERIC_NPC_SPAWN
                || expId == SUNDERER_SPAWN_BONUS;
        }

        public static bool IsVehicleKill(int expId) {
            return expId == VKILL_FLASH || expId == VKILL_GALAXY || expId == VKILL_LIBERATOR
                || expId == VKILL_LIGHTNING || expId == VKILL_MAGRIDER || expId == VKILL_MOSQUITO
                || expId == VKILL_PROWLER || expId == VKILL_REAVER || expId == VKILL_SCYTHE
                || expId == VKILL_VANGUARD || expId == VKILL_HARASSER || expId == VKILL_ANT
                || expId == VKILL_VALKYRIE || expId == VKILL_JAVELIN;
        }

    }
}
