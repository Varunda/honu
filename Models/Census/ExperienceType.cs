namespace watchtower.Models.Census {

    public class ExperienceType : IKeyedObject {

        /// <summary>
        ///     unique ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        ///     name of the experience event
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        ///     how much XP you get base from the experience event
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        ///     a category(?) of experience types. see <see cref="ExperienceAwardType"/>
        /// </summary>
        public int AwardTypeID { get; set; }

    }
}
