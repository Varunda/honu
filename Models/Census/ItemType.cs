namespace watchtower.Models.Census {

    public class ItemType : IKeyedObject {

        /// <summary>
        ///     Unique ID
        /// </summary>
        public int ID { get; set; }

        public string Name { get; set; } = "";

        public string Code { get; set; } = "";

    }
}
