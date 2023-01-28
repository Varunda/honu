using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using watchtower.Models.PSB;

namespace watchtower.Services.Repositories.Readers {

    public class PsbCalendarEntryReader : ISheetsReader<PsbCalendarEntry> {

        public override PsbCalendarEntry ReadEntry(List<string?> values) {

            PsbCalendarEntry entry = new();
            entry.Valid = false;
            entry.Input = values;

            // Check if this is a header line or not
            string status = values.GetNullableString(1) ?? "";
            if (status == "Status") { // is a day header row
                return entry;
            }

            if (values.GetNullableString(2) == null) { // not filled in
                return entry;
            }

            string? groups = values.GetNullableString(2);
            if (groups == null) {
                entry.Error = $"Column 3 (groups) was empty";
                return entry;
            }
            entry.Groups = groups.Split(new char[]{ ',', '/', '\\' }).Select(iter => iter.Trim()).ToList();

            string? bases = values.GetNullableString(3);
            if (bases == null) {
                entry.Error = $"Column 4 (bases) was empty";
                return entry;
            }
            entry.BaseNames = bases.Split(",").Select(iter => iter.Trim()).ToList();

            string? start = values.GetNullableString(8);
            if (start == null) {
                entry.Error = $"Column 9 (start date) is empty";
                return entry;
            }
            if (DateTime.TryParse(start, null, DateTimeStyles.AssumeLocal, out DateTime startDate) == false) {
                entry.Error = $"Column 9 (start date) is an invalid DateTime";
                return entry;
            }
            entry.Start = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            string? end = values.GetNullableString(11);
            if (end == null) {
                entry.Error = $"Column 12 (end date) is empty";
                return entry;
            }
            if (DateTime.TryParse(end, out DateTime endDate) == false) {
                entry.Error = $"Column 12 (end date) is an invalid DateTime";
                return entry;
            }
            entry.End = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            entry.Notes = values.GetNullableString(8) ?? "";
            entry.Valid = true;

            return entry;
        }

    }
}
