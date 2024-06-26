using System;

namespace watchtower.Models.Census {

    public class PsItem : IKeyedObject {

        public int ID { get; set; }

        public int TypeID { get; set; }

        public int CategoryID { get; set; }

        public bool IsVehicleWeapon { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public short FactionID { get; set; }

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

        public static PsItem NoItem = new PsItem() {
            ID = 0,
            TypeID = 0,
            CategoryID = 0,
            Name = "No weapon"
        };

    }

}