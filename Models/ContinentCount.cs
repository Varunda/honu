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
        public void AddToVS(string zoneID) {
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

        public void AddToNC(string zoneID) {
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

        public void AddToTR(string zoneID) {
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

    }

    /// <summary>
    /// Generic count of something for each faction
    /// </summary>
    public class FactionCount {

        public int VS { get; set; } = 0;

        public int NC { get; set; } = 0;

        public int TR { get; set; } = 0;

        public int NS { get; set; } = 0;

        public int Other { get; set; } = 0;

    }

}
