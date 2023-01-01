using watchtower.Models.PSB;

namespace watchtower.Code.Constants {

    /// <summary>
    ///     The different types of <see cref="PsbAccount"/>s
    /// </summary>
    public class PsbAccountType {

        /// <summary>
        ///     A named account, given to a single person
        /// </summary>
        public const short NAMED = 1;

        /// <summary>
        ///     A practice account, give to an outfit
        /// </summary>
        public const short PRACTICE = 2;

        /// <summary>
        ///     Get the name representation of an account type ID
        /// </summary>
        public static string GetName(short typeID) {
            if (typeID == NAMED) {
                return "Named";
            } else if (typeID == PRACTICE) {
                return "Practice";
            }

            return $"Unknown {typeID}";
        }

    }
}
