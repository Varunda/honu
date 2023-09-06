namespace watchtower.Models.Census {

    public class KillboardEntry {

        public string SourceCharacterID { get; set; } = "";

        public string OtherCharacterID { get; set; } = "";

        public int Kills { get; set; }

        public int Deaths { get; set; }

    }
}
