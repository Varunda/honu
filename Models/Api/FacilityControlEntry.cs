using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Api {

    public class FacilityControlEntry {

        /// <summary>
        /// ID of the facility this entry is for
        /// </summary>
        public int FacilityID { get; set; }

        /// <summary>
        /// Name of the facility
        /// </summary>
        public string FacilityName { get; set; } = "";

        /// <summary>
        /// What type the facility is
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        /// Name of the type of the facility
        /// </summary>
        public string TypeName { get; set; } = "";

        /// <summary>
        /// Zone ID the facility is in
        /// </summary>
        public uint ZoneID { get; set; }

        /// <summary>
        /// How many times this facility has been captured
        /// </summary>
        public int Captured { get; set; }

        /// <summary>
        /// How many players on average are present when the facility is captured
        /// </summary>
        public float CaptureAverage { get; set; }

        /// <summary>
        /// How many times this facility has been defended
        /// </summary>
        public int Defended { get; set; }

        /// <summary>
        /// How many players on average are present when the facility is defended
        /// </summary>
        public float DefenseAverage { get; set; }

        /// <summary>
        /// How many players are present on average whenever the facility is captured or defended
        /// </summary>
        public float TotalAverage { get; set; }


    }

}
