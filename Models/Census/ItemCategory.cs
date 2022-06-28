namespace watchtower.Models.Census {

    public class ItemCategory : IKeyedObject {

        /// <summary>
        ///     Unique ID of the item category
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     Name of the item category
        /// </summary>
        public string Name { get; set; } = "";

    }
}
