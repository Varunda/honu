using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Constants {

    public static class Zone {

        public const uint Indar = 2;

        public const uint Hossin = 4;

        public const uint Amerish = 6;

        public const uint Esamir = 8;

        public static List<uint> All = new List<uint>() {
            Indar, Hossin, Amerish, Esamir
        };

    }
}
