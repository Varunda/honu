using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using watchtower.Models.Db;

namespace watchtower.Models.Api {

    public class WeaponStatPercentileAll {

        public string ItemID { get; set; } = "";

        public List<Bucket> Accuracy { get; set; } = new List<Bucket>();

        public List<Bucket> HeadshotRatio { get; set; } = new List<Bucket>();

        public List<Bucket> KD { get; set; } = new List<Bucket>();

        public List<Bucket> KPM { get; set; } = new List<Bucket>();

    }
}
