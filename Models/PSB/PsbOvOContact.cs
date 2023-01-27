namespace watchtower.Models.PSB {

    public class PsbOvOContact : PsbContact {

        public enum RepresentativeType {
            DEFAULT,

            OVO,

            COMMUNITY,

            OBSERVER
        }

        /// <summary>
        ///     What group this contact is a rep for. Can be an outfit tag or a community
        /// </summary>
        public string Group { get; set; } = "";

        /// <summary>
        ///     What type of representation this contact is for
        /// </summary>
        public string RepType { get; set; } = "";

        /// <summary>
        ///     How many accounts this contact is allow to request
        /// </summary>
        public int AccountLimit { get; set; }

        public bool AccountPings { get; set; }

        public bool BasePings { get; set; }

        public string Notes { get; set; } = "";

    }
}
