namespace watchtower.Models.Census {

    public class ExperienceAwardType : IKeyedObject {

        /// <summary>
        ///     unique ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     name
        /// </summary>
        public string Name { get; set; } = "";

    }
}
