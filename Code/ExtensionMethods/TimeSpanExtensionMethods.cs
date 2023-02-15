using System;

namespace watchtower.Code.ExtensionMethods {

    public static class TimeSpanExtensionMethods {

        public static string GetRelativeFormat(this TimeSpan span) {
            if (span.TotalMilliseconds < 1000) {
                return $"{span.TotalMilliseconds:n0}ms";
            }
            if (span.TotalSeconds < 60) {
                return $"{span.TotalSeconds:n0}s";
            }
            if (span.TotalMinutes < 60) {
                return $"{span.TotalMinutes:n0}m {span.Seconds}s";
            }

            if (span.TotalHours < 24) {
                return $"{span.TotalHours:n0}h {span.Minutes}m";
            }

            return $"{span.TotalDays:n0}d {span.Hours}h";
        }

    }
}
