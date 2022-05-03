using watchtower.Constants;

namespace watchtower.Code.Constants {

    /// <summary>
    ///     Static collection of profile IDs
    /// </summary>
    public class Profile {

        public const short INFILTRATOR = 1;

        public const short LIGHT_ASSAULT = 3;

        public const short MEDIC = 4;

        public const short ENGINEER = 5;

        public const short HEAVY = 6;

        public const short MAX = 7;

        /// <summary>
        ///     Get the profile ID of a loadout ID
        /// </summary>
        /// <param name="loadoutID">Loadout ID to get the profile of</param>
        /// <returns>
        ///     The profile ID of the loadout, or <c>null</c> if it does not exist
        /// </returns>
        public static short? GetProfileID(short loadoutID) {
            return loadoutID switch {
                Loadout.VS_INFILTRATOR or Loadout.NC_INFILTRATOR or Loadout.TR_INFILTRATOR or Loadout.NS_INFILTRATOR => INFILTRATOR,
                Loadout.VS_LIGHT_ASSAULT or Loadout.NC_LIGHT_ASSAULT or Loadout.TR_LIGHT_ASSAULT or Loadout.NS_LIGHT_ASSAULT => LIGHT_ASSAULT,
                Loadout.VS_MEDIC or Loadout.NC_MEDIC or Loadout.TR_MEDIC or Loadout.NS_MEDIC => MEDIC,
                Loadout.VS_ENGINEER or Loadout.NC_ENGINEER or Loadout.TR_ENGINEER or Loadout.NS_ENGINEER => ENGINEER,
                Loadout.VS_HEAVY_ASSAULT or Loadout.NC_HEAVY_ASSAULT or Loadout.TR_HEAVY_ASSAULT or Loadout.NS_HEAVY_ASSAULT => HEAVY,
                Loadout.VS_MAX or Loadout.NC_MAX or Loadout.TR_MAX or Loadout.NS_MAX => MAX,
                _ => null
            };
        }

    }
}
