namespace watchtower.Models {

    public class FactionGeneric<T> where T : unmanaged {

        public T VS { get; set; } = default;

        public T NC { get; set; } = default;

        public T TR { get; set; } = default;

        public T NS { get; set; } = default;

    }
}
