namespace watchtower.Models.Census {
    
    public class PsVehicle : IKeyedObject {

        public int ID { get; set; }

        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public int TypeID { get; set; }

        public int CostResourceID { get; set; }

        public int ImageSetID { get; set; }

        public int ImageID { get; set; }

    }
}
