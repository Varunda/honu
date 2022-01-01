namespace watchtower.Models.Census {

    public class Achievement : IKeyedObject {

        public int ID { get; set; }

        public int? ItemID { get; set; }

        public int ObjectiveGroupID { get; set; }

        public int? RewardID { get; set; }

        public bool Repeatable { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

    }
}
