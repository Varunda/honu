namespace watchtower.Models.Census {

    public class PsMetagameEvent : IKeyedObject {

        /// <summary>
        ///     Unique ID of the metagame event
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     Name of the metagame event
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     Description of the metagame event
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        ///     What type of metagame event this is, there is no collection for what is what
        /// </summary>
        public int TypeID { get; set; }

        /// <summary>
        ///     How many minutes this alert lasts for
        /// </summary>
        public int DurationMinutes { get; set; }

    }
}
