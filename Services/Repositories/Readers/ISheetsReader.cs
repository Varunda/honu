using System;
using System.Collections.Generic;

namespace watchtower.Services.Repositories.Readers {

    /// <summary>
    ///     Parses the results of a sheets API query into typed objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ISheetsReader<T> where T : class {

        /// <summary>
        ///     Abstract method that when implemented takes a list of strings
        ///     turning it into <typeparamref name="T"/>
        /// </summary>
        /// <param name="values">List of strings. Will always have the same number of rows</param>
        public abstract T ReadEntry(List<string?> values);

        /// <summary>
        ///     Read a list of values
        /// </summary>
        /// <param name="values">Values</param>
        /// <returns></returns>
        public List<T> ReadList(IEnumerable<IList<object>> values) {

            // Check max number of columns
            int colCount = 0;
            foreach (IList<object> row in values) {
                if (row.Count > colCount) {
                    colCount = row.Count;
                }
            }

            List<T> entries = new();

            int rowIndex = 0;
            foreach (IList<object> row in values) {
                List<string?> columns = new List<string?>(colCount);

                try {
                    for (int i = 0; i < colCount; ++i) {
                        // in sheets, if a column is empty and the last column, there will be a different
                        //      number of columns, instead of an empty one.
                        //
                        // For example:
                        //      ID,Name,Desc
                        //      1,a,b
                        //      2,c
                        // Will return:
                        //      [
                        //          [1, a, b],
                        //          [2, c]
                        //      ]
                        // 
                        // so we need to account for possibly one of the columns being empty
                        if (i + 1 > row.Count) {
                            columns.Add(null);
                        } else {
                            columns.Add(row[i].ToString());
                        }
                    }
                } catch (Exception) {
                    throw;
                }

                try {
                    T entry = ReadEntry(columns);
                    entries.Add(entry);
                } catch (Exception ex) {
                    throw new Exception($"error parsing row {rowIndex}: '{string.Join(", ", columns)}'. Exception message: {ex.Message}", ex);
                }

                ++rowIndex;
            }

            return entries;
        }

    }

    public static class ISheetsReaderExtensionMethods {

        public static string? GetNullableString(this List<string?> cols, int index) {
            if (index < 0 || index > cols.Count) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return cols[index];
        }

        public static string GetRequiredString(this List<string?> cols, int index) {
            string? col = cols.GetNullableString(index);
            if (col == null) {
                throw new ArgumentNullException($"Column {index} cannot be null");
            }

            return col;
        }

        public static ulong? GetNullableUInt64(this List<string?> cols, int index) {
            string? col = cols.GetNullableString(index);
            if (col == null) {
                return null;
            }

            if (ulong.TryParse(col, out ulong r) == false) {
                throw new FormatException($"'{col}' is not a valid ulong");
            }

            return r;
        }

        public static ulong GetRequiredUInt64(this List<string?> cols, int index) {
            string value = cols.GetRequiredString(index);

            if (ulong.TryParse(value, out ulong r) == false) {
                throw new FormatException($"'{value}' is not a valid ulong");
            }

            return r;
        }

        public static int GetRequiredInt32(this List<string?> cols, int index) {
            string value = cols.GetRequiredString(index);

            if (int.TryParse(value, out int r) == false) {
                throw new FormatException($"'{value}' is not a valid int");
            }

            return r;
        }

    }

}
