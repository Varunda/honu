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
        public const int SHIELD_REPAIR = 438;
        public const int VEHICLE_RESUPPLY = 240;

        public const int SQUAD_HEAL = 51;
        public const int SQUAD_REVIVE = 53;
        public const int SQUAD_RESUPPLY = 55;
        public const int SQUAD_MAX_REPAIR = 142;
        public const int SQUAD_SHIELD_REPAIR = 439;
        public const int SQUAD_VEHICLE_RESUPPLY = 241;

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
        public const int VKILL_SUNDY = 68;
        public const int VKILL_VANGUARD = 69;
        public const int VKILL_HARASSER = 301;
        public const int VKILL_VALKYRIE = 501;
        public const int VKILL_ANT = 651;
        public const int VKILL_COLOSSUS = 1449;
        public const int VKILL_JAVELIN = 1480;
        public const int VKILL_CHIMERA = 1565;
        public const int VKILL_DERVISH = 1635;

        public const int REPAIR_FLASH = 31;
        public const int REPAIR_ENGI_TURRET = 88;
        public const int REPAIR_PHALANX = 89;
        public const int REPAIR_DROP_POD = 90;
        public const int REPAIR_GALAXY = 91;
        public const int REPAIR_LIBERATOR = 92;
        public const int REPAIR_LIGHTNING = 93;
        public const int REPAIR_MAGRIDER = 94;
        public const int REPAIR_MOSQUITO = 95;
        public const int REPAIR_PROWLER = 96;
        public const int REPAIR_REAVER = 97;
        public const int REPAIR_SCYTHE = 98;
        public const int REPAIR_SUNDERER = 99;
        public const int REPAIR_VANGUARD = 100;
        public const int REPAIR_HARASSER = 303;
        public const int REPAIR_VALKYRIE = 503;
        public const int REPAIR_ANT = 653;
        public const int REPAIR_HARDLIGHT_BARRIER = 1375;
        public const int REPAIR_COLOSSUS = 1451;
        public const int REPAIR_JAVELIN = 1482;
        public const int REPAIR_CHIMERA = 1571;
        public const int REPAIR_DERVISH = 1638;

        public const int SQUAD_REPAIR_FLASH = 28;
        public const int SQUAD_REPAIR_ENGI_TURRET = 129;
        public const int SQUAD_REPAIR_PHALANX = 130;
        public const int SQUAD_REPAIR_DROP_POD = 131;
        public const int SQUAD_REPAIR_GALAXY = 132;
        public const int SQUAD_REPAIR_LIBERATOR = 133;
        public const int SQUAD_REPAIR_LIGHTNING = 134;
        public const int SQUAD_REPAIR_MAGRIDER = 135;
        public const int SQUAD_REPAIR_MOSQUITO = 136;
        public const int SQUAD_REPAIR_PROWLER = 137;
        public const int SQUAD_REPAIR_REAVER = 138;
        public const int SQUAD_REPAIR_SCYTHE = 139;
        public const int SQUAD_REPAIR_SUNDERER = 140;
        public const int SQUAD_REPAIR_VANGUARD = 141;
        public const int SQUAD_REPAIR_HARASSER = 302;
        public const int SQUAD_REPAIR_VALKYRIE = 505;
        public const int SQUAD_REPAIR_ANT = 656;
        public const int SQUAD_REPAIR_HARDLIGHT_BARRIER = 1378;
        public const int SQUAD_REPAIR_COLOSSUS = 1452;
        public const int SQUAD_REPAIR_JAVELIN = 1481;
        public const int SQUAD_REPAIR_CHIMERA = 1571;
        public const int SQUAD_REPAIR_DERVISH = 1638;

        public static List<int> VehicleKillEvents = new List<int>() {
            VKILL_FLASH, VKILL_GALAXY, VKILL_LIBERATOR,
            VKILL_LIGHTNING, VKILL_MAGRIDER, VKILL_MOSQUITO,
            VKILL_PROWLER, VKILL_REAVER, VKILL_SCYTHE,
            VKILL_VANGUARD, VKILL_HARASSER, VKILL_VALKYRIE,
            VKILL_ANT, VKILL_COLOSSUS, VKILL_JAVELIN,
            VKILL_CHIMERA, VKILL_DERVISH
        };

        public static List<int> VehicleRepairEvents = new List<int>() {
            REPAIR_FLASH, REPAIR_ENGI_TURRET, REPAIR_PHALANX, REPAIR_DROP_POD, REPAIR_GALAXY,
            REPAIR_LIBERATOR, REPAIR_LIGHTNING, REPAIR_MAGRIDER, REPAIR_MOSQUITO, REPAIR_PROWLER,
            REPAIR_REAVER, REPAIR_SCYTHE, REPAIR_SUNDERER, REPAIR_VANGUARD, REPAIR_HARASSER,
            REPAIR_VALKYRIE, REPAIR_ANT, REPAIR_HARDLIGHT_BARRIER, REPAIR_COLOSSUS, REPAIR_JAVELIN,
            REPAIR_CHIMERA, REPAIR_DERVISH
        };

        public static List<int> SquadVehicleRepairEvents = new List<int>() {
            SQUAD_REPAIR_FLASH, SQUAD_REPAIR_ENGI_TURRET, SQUAD_REPAIR_PHALANX, SQUAD_REPAIR_DROP_POD, SQUAD_REPAIR_GALAXY,
            SQUAD_REPAIR_LIBERATOR, SQUAD_REPAIR_LIGHTNING, SQUAD_REPAIR_MAGRIDER, SQUAD_REPAIR_MOSQUITO, SQUAD_REPAIR_PROWLER,
            SQUAD_REPAIR_REAVER, SQUAD_REPAIR_SCYTHE, SQUAD_REPAIR_SUNDERER, SQUAD_REPAIR_VANGUARD, SQUAD_REPAIR_HARASSER,
            SQUAD_REPAIR_VALKYRIE, SQUAD_REPAIR_ANT, SQUAD_REPAIR_HARDLIGHT_BARRIER, SQUAD_REPAIR_COLOSSUS, SQUAD_REPAIR_JAVELIN,
            SQUAD_REPAIR_CHIMERA, SQUAD_REPAIR_DERVISH
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

        public static bool IsMaxRepair(int expId) {
            return expId == MAX_REPAIR || expId == SQUAD_MAX_REPAIR;
        }

        public static bool IsShieldRepair(int expId) {
            return expId == SHIELD_REPAIR || expId == SQUAD_SHIELD_REPAIR;
        }

        public static bool OtherIDIsCharacterID(int expID) {
            return IsHeal(expID) || IsRevive(expID) || IsResupply(expID) || IsShieldRepair(expID);
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

        public static bool IsVehicleRepair(int expId) {
            return VehicleKillEvents.Contains(expId) || SquadVehicleRepairEvents.Contains(expId);
        }

    }
}
