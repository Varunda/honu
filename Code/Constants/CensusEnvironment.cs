using System;

namespace watchtower.Code.Constants {

    /// <summary>
    ///     Constants about the different Census environments
    /// </summary>
    public enum CensusEnvironment {

        /// <summary>
        /// Default value
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// PC stream, corresponds to the "ps2" namespace
        /// </summary>
        PC,

        /// <summary>
        /// Playstation 4 US, corresponds to the "ps2ps4us" namespace
        /// </summary>
        PS4_US,

        /// <summary>
        /// Playstation 4 EU, corresponds to the "ps2ps4eu" namespace
        /// </summary>
        PS4_EU

    }

    public sealed class CensusEnvironmentHelper {

        /// <summary>
        ///     Get the enviorment for the corresponding world ID, or <c>null</c> if unknown
        /// </summary>
        /// <param name="worldID">ID of the world</param>
        public static CensusEnvironment? FromWorldID(short worldID) {
            if (World.PcStreams.Contains(worldID)) {
                return CensusEnvironment.PC;
            } else if (World.Ps4UsStreams.Contains(worldID)) {
                return CensusEnvironment.PS4_US;
            } else if (World.Ps4EuStreams.Contains(worldID)) {
                return CensusEnvironment.PS4_EU;
            }
            return null;
        }

        /// <summary>
        ///     Get a string representation of a <see cref="CensusEnvironment"/>
        /// </summary>
        public static string ToNamespace(CensusEnvironment env) {
            switch (env) {
                case CensusEnvironment.PC: return "ps2";
                case CensusEnvironment.PS4_US: return "ps2ps4us";
                case CensusEnvironment.PS4_EU: return "ps2ps4eu";
                case CensusEnvironment.UNKNOWN:
                    break;
            }
            throw new ArgumentException($"Unchecked value of {nameof(env)}: {env}");
        }

    }

}
