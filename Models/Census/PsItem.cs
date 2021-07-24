using System;

namespace watchtower.Models.Census {

    public class PsItem {

        public string ID { get; set; } = "";

        public int TypeID { get; set; }

        public int CategoryID { get; set; }

        public string Name { get; set; } = "";

        public static PsItem NoItem = new PsItem() {
            ID = "0",
            TypeID = 0,
            CategoryID = 0,
            Name = "No weapon"
        };

    }

}