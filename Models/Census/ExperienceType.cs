namespace watchtower.Models.Census {

    public class ExperienceType : IKeyedObject {

        public int ID { get; set; }

        public string Name { get; set; } = "";

        public double Amount { get; set; }

    }
}
