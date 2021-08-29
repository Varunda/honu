using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Constants;

namespace watchtower.Models {

    /// <summary>
    /// Count of how many players are on each continent
    /// </summary>
    public class ContinentCount {

        public FactionCount Indar { get; set; } = new FactionCount();

        public FactionCount Amerish { get; set; } = new FactionCount();

        public FactionCount Hossin { get; set; } = new FactionCount();

        public FactionCount Esamir { get; set; } = new FactionCount();

        public FactionCount Other { get; set; } = new FactionCount();

        /// <summary>
        /// These methods are gross
        /// </summary>
        /// <param name="zoneID"></param>
        public void AddToVS(uint zoneID) {
            if (zoneID == Zone.Indar) {
                ++Indar.VS;
            } else if (zoneID == Zone.Hossin) {
                ++Hossin.VS;
            } else if (zoneID == Zone.Amerish) {
                ++Amerish.VS;
            } else if (zoneID == Zone.Esamir) {
                ++Esamir.VS;
            } else {
                ++Other.VS;
            }
        }

        public void AddToNC(uint zoneID) {
            if (zoneID == Zone.Indar) {
                ++Indar.NC;
            } else if (zoneID == Zone.Hossin) {
                ++Hossin.NC;
            } else if (zoneID == Zone.Amerish) {
                ++Amerish.NC;
            } else if (zoneID == Zone.Esamir) {
                ++Esamir.NC;
            } else {
                ++Other.NC;
            }
        }

        public void AddToTR(uint zoneID) {
            if (zoneID == Zone.Indar) {
                ++Indar.TR;
            } else if (zoneID == Zone.Hossin) {
                ++Hossin.TR;
            } else if (zoneID == Zone.Amerish) {
                ++Amerish.TR;
            } else if (zoneID == Zone.Esamir) {
                ++Esamir.TR;
            } else {
                ++Other.TR;
            }
        }

        public void AddToNS(uint zoneID) {
            if (zoneID == Zone.Indar) {
                ++Indar.NS;
            } else if (zoneID == Zone.Hossin) {
                ++Hossin.NS;
            } else if (zoneID == Zone.Amerish) {
                ++Amerish.NS;
            } else if (zoneID == Zone.Esamir) {
                ++Esamir.NS;
            } else {
                ++Other.NS;
            }
        }

    }

    /// <summary>
    /// Generic count of something for each faction
    /// </summary>
    public class FactionCount {

        /// <summary>
        /// How many are on VS, including NSO
        /// </summary>
        public int VS { get; set; } = 0;

        /// <summary>
        /// How many are on NC, including NSO
        /// </summary>
        public int NC { get; set; } = 0;

        /// <summary>
        /// How many are on TR, including NSO
        /// </summary>
        public int TR { get; set; } = 0;

        /// <summary>
        /// How many NSO robots are online in total
        /// </summary>
        public int NS { get; set; } = 0;

        /// <summary>
        /// How many are not known
        /// </summary>
        public int Other { get; set; } = 0;

        /// <summary>
        /// Metadata about the zone, such as alerts and locked/unlocked
        /// </summary>
        public ZoneState? Metadata { get; set; } = null;

    }

}
