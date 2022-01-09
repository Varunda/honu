namespace watchtower.Models.PSB {

    public class PsbNamedAccount {

        public long ID { get; set; }

        public string? Tag { get; set; }

        public string Name { get; set; } = "";

        public string? VsID { get; set; }

        public string? NcID { get; set; }

        public string? TrID { get; set; }

        public string? NsID { get; set; }

        public string? Notes { get; set; }

    }

    public static class PsbNameTemplates {

        public static string VS(string? tag, string name) => $"{tag}x{name}VS";

        public static string NC(string? tag, string name) => $"{tag}x{name}NC";

        public static string TR(string? tag, string name) => $"{tag}x{name}TR";

        public static string NS(string? tag, string name) => $"{tag}x{name}NS";

    }

}
