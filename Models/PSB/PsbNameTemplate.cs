using System.Collections.Generic;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     Get the names of an account given a tag and name
    /// </summary>
    public static class PsbNameTemplate {

        /// <summary>
        ///     VS name of the name
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string VS(string? tag, string name) => $"{tag}x{name}VS";

        /// <summary>
        ///     NC name of the name
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string NC(string? tag, string name) => $"{tag}x{name}NC";

        /// <summary>
        ///     TR name of the name
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string TR(string? tag, string name) => $"{tag}x{name}TR";

        /// <summary>
        ///     All the variations of the NS names used
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<string> NS(string? tag, string name) {
            List<string> names = new List<string>() {
                $"{tag}x{name}NS",
                $"{tag}x{name}DONOTUSE",
                $"DONOTUSEx{name}",
                $"DONTUSEx{name}"
            };

            if (name.StartsWith("Practice") || name.StartsWith("practice")) {
                string number = name.Replace("Practice", "");
                names.Add($"DONOTUSEx{tag}{number}");
            }

            return names;
        }

    }
}
