namespace watchtower.Models.PSB {

    /// <summary>
    ///     Status of a <see cref="PsbNamedAccount"/>
    /// </summary>
    public class PsbCharacterStatus {

        public const int OK = 1;

        public const int DOES_NOT_EXIST = 2;

        public const int DELETED = 3;

        public const int REMADE = 4;

        public static string GetName(int ID) {
            if (ID == OK) {
                return "OK";
            } else if (ID == DOES_NOT_EXIST) {
                return "Does not exist";
            } else if (ID == DELETED) {
                return "Deleted";
            } else if (ID == REMADE) {
                return "Remade";
            } else {
                return $"Unknown {ID}";
            }
        }

    }
}
