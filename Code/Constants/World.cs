using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.Constants {

    public class World {

        public const short Connery = 1;
        public const short Osprey = 1;
        public const short Helios = 3; // Likely
        public const short Miller = 10;
        public const short Wainwright = 10;
        public const short Cobalt = 13;
        public const short Emerald = 17;
        public const short Jaeger = 19;
        public const short Apex = 24;
        public const short Briggs = 25;
        public const short SolTech = 40;
        public const short Genudine = 1000;
        public const short Ceres = 2000;

        /// <summary>
        ///     Check if a world ID is a world that is saved to the DB
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        /// <returns>
        ///     If the world is one that is saved to the DB
        /// </returns>
        public static bool IsTrackedWorld(short worldID) {
            return worldID == Connery || worldID == Miller || worldID == Cobalt
                || worldID == Emerald || worldID == Jaeger || worldID == SolTech
                || worldID == Apex;
        }

        /// <summary>
        ///     List of worlds that pc realtime streams are made for
        /// </summary>
        public static readonly List<short> PcStreams = new() {
            Connery, Miller,
            // Cobalt, // 2024-10-29: merged into Miller, RIP
            // Emerald, // 2025-04-08: merged into Connery, RIP
            Jaeger, SolTech, Apex
        };

        /// <summary>
        ///     List of worlds that ps4us streams are made for
        /// </summary>
        public static readonly List<short> Ps4UsStreams = new() { Genudine };

        /// <summary>
        ///     List of worlds that ps4eu streams are made for
        /// </summary>
        public static readonly List<short> Ps4EuStreams = new() { Ceres };

        /// <summary>
        ///     All servers
        /// </summary>
        public static readonly List<short> All = new() {
            Connery, Helios, Miller, Cobalt, Emerald, Jaeger, SolTech, Apex, Briggs, Genudine, Ceres
        };

        /// <summary>
        ///     Get a display friendly name of a world
        /// </summary>
        /// <param name="worldID">World ID to get the name of</param>
        /// <returns></returns>
        public static string GetName(short worldID) {
            switch (worldID) {
                case Osprey: return "Osprey (US)";
                case Helios: return "Helios";
                case Wainwright: return "Wainwright (EU)";
                case Cobalt: return "Cobalt";
                case Emerald: return "Emerald";
                case Jaeger: return "Jaeger";
                case Apex: return "Apex";
                case Briggs: return "Briggs";
                case SolTech: return "SolTech";
                case Genudine: return "Genudine";
                case Ceres: return "Ceres";
                default: return $"Unknown {worldID}";
            }
        }

        /// <summary>
        ///     Check if an ID is a valid world ID
        /// </summary>
        public static bool IsValidID(short worldID) {
            return All.Contains(worldID);
        }

    }
}
