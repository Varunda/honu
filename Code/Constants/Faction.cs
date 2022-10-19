using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public sealed class Faction {

        public const short UNKNOWN = -1;

        public const short VS = 1;

        public const short NC = 2;

        public const short TR = 3;

        public const short NS = 4;

        public readonly static List<short> All = new() { VS, NC, TR, NS };

        public static string GetName(short factionID) {
            switch (factionID) {
                case UNKNOWN: return "unknown";
                case VS: return "VS";
                case NC: return "NC";
                case TR: return "TR";
                case NS: return "NS";
                default: return $"Unchecked {factionID}";
            }
        }

    }
}
