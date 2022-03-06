namespace watchtower.Models.Alert {

    public class AlertPlayerProfileData {

        public long ID { get; set; }

        public long AlertID { get; set; }

        public string CharacterID { get; set; } = "";

        public short ProfileID { get; set; }

        public int Kills { get; set; }

        public int Deaths { get; set; }

        public int VehicleKills { get; set; }

        public int TimeAs { get; set; }

    }
}
