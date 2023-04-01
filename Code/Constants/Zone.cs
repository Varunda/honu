using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    /// <summary>
    ///     Zone constants
    /// </summary>
    public static class Zone {

        public const uint Indar = 2;

        public const uint Hossin = 4;

        public const uint Amerish = 6;

        public const uint Esamir = 8;

        public const uint Koltyr = 14;

        public const uint VrTrainingNc = 96;

        public const uint VrTrainingTr = 97;

        public const uint VrTrainingVs = 98;

        public const uint Oshur = 344;

        public const uint Desolation = 361;

        public const uint Sancutary = 362;

        public const uint Tutorial2 = 364;

        /// <summary>
        ///     List of all static zones
        /// </summary>
        public static readonly List<uint> StaticZones = new() {
            Indar, Hossin, Amerish, Esamir, Oshur
        };

        /// <summary>
        ///     Get the name of a zone based on the zone ID. Includes the instance if one exists
        /// </summary>
        /// <param name="zoneID">ID of the zone</param>
        public static string GetName(uint zoneID) {
            uint defID = zoneID & 0xFFFF;
            uint instanceID = (zoneID & 0xFFFF0000) >> 16;

            switch (defID) {
                case Indar: return "Indar";
                case Hossin: return "Hossin";
                case Amerish: return "Amerish";
                case Esamir: return "Esamir";
                case Oshur: return "Oshur";
                case Koltyr: return "Koltyr";
                case Desolation: return (instanceID > 0) ? $"Desolation (instance {instanceID})" : "Desolation";
                case Sancutary: return "Sancutary";
                case Tutorial2: return (instanceID > 0) ? $"Tutorial 2 (instance {instanceID})" : "Tutorial 2";
                case VrTrainingNc: return "VR training (NC)";
                case VrTrainingTr: return "VR training (TR)";
                case VrTrainingVs: return "VR training (VS)";
                default: break;
            }

            return $"Unchecked {zoneID}";
        }

    }
}
