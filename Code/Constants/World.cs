using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.Constants {

    public class World {

        public const short Connery = 1;
        public const short Miller = 10;
        public const short Cobalt = 13;
        public const short Emerald = 17;
        public const short Jaeger = 19;
        public const short SolTech = 40;

        /// <summary>
        /// Check if a world ID is a valid world
        /// </summary>
        /// <param name="worldID"></param>
        /// <returns></returns>
        public static bool IsValidWorld(short worldID) {
            return worldID == Connery || worldID == Miller || worldID == Cobalt
                || worldID == Emerald || worldID == Jaeger || worldID == SolTech;
        }

        public static List<short> All = new() {
            Connery, Miller, Cobalt, Emerald, Jaeger, SolTech
        };

        /// <summary>
        /// Get a display friendly name of a world
        /// </summary>
        /// <param name="worldID">World ID to get the name of</param>
        /// <returns></returns>
        public static string GetName(short worldID) {
            switch (worldID) {
                case Connery: return "Connery";
                case Miller: return "Miller";
                case Cobalt: return "Cobalt";
                case Emerald: return "Emerald";
                case Jaeger: return "Jaeger";
                case SolTech: return "SolTech";
                default: return $"Unknown {worldID}";
            }
        }

    }
}
