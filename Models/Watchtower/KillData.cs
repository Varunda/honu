﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models {

    public class KillBlock {

        public List<KillData> Entries { get; set; } = new List<KillData>();

    }

    /// <summary>
    /// Represents the kill data for a single character
    /// </summary>
    public class KillData {

        /// <summary>
        /// ID of the character
        /// </summary>
        public string ID { get; set; } = "";

        /// <summary>
        /// Name of the character. Includes the tag if they are in an outfit
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Is the player currently online
        /// </summary>
        public bool Online { get; set; } = true;

        /// <summary>
        /// How many seconds online the character has been
        /// </summary>
        public int SecondsOnline { get; set; } = 0;

        /// <summary>
        /// What faction the character is. Used to allow NSO characters to show up in VS/NC/TR lists
        /// </summary>
        public short FactionID { get; set; }

        /// <summary>
        /// How many kills they got
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// How many deaths (revived of course) they have
        /// </summary>
        public int Deaths { get; set; }

        /// <summary>
        /// How many assists the player got
        /// </summary>
        public int Assists { get; set; }

    }
}
