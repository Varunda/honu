namespace watchtower.Models.Census {

    public class OutfitWarsOutfit {

        public string OutfitID { get; set; } = "";

        public short FactionID { get; set; }

        public short WorldID { get; set; }

        public int OutfitWarID { get; set; }

        public int RegistrationOrder { get; set; }

        public string Status { get; set; }

        public int SignupCount { get; set; }

    }
}
