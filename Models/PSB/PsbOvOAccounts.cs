using System;
using System.Collections.Generic;

namespace watchtower.Models.PSB {

    /// <summary>
    ///     represents a single month of accounts
    /// </summary>
    public class PsbOvOAccountUsageBlock {

        /// <summary>
        ///     ID of the gdrive file these accounts come from
        /// </summary>
        public string FileID { get; set; } = "";

        /// <summary>
        ///     What accounts are in this block
        /// </summary>
        public List<PsbOvOAccount> Accounts { get; set; } = new();

        /// <summary>
        ///     What accounts are being used on what days. The Key is YYYY-MM-DD, and the Value is a list of all accounts
        ///     that can be given out on a day, regardless of if they are available
        /// </summary>
        public Dictionary<string, List<PsbOvOAccountEntry>> Usage = new();

        /// <summary>
        ///     Check if a DateTime is a valid entry into this account block
        /// </summary>
        public bool IsValidDay(DateTime when) {
            string key = $"{when:yyyy}-{when:MM}-{when:dd}";
            return Usage.ContainsKey(key);
        }

        /// <summary>
        ///     Get the usages of a single day
        /// </summary>
        /// <param name="when">Date to check. The time (hour, minutes, etc.) are not used</param>
        /// <returns>
        ///     A list of account usage entries
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     If <paramref name="when"/> does not refer to a day within this block.
        ///     Use <see cref="IsValidDay(DateTime)"/> to check before you call this method
        /// </exception>
        public List<PsbOvOAccountEntry> GetDayUsage(DateTime when) {
            string key = $"{when:yyyy}-{when:MM}-{when:dd}";

            if (Usage.TryGetValue(key, out List<PsbOvOAccountEntry>? uses) == false) {
                throw new ArgumentException($"{key} is not a valid day in this block. From {when:u}");
            }

            return uses;
        }

    }

    public class PsbOvOAccount {

        /// <summary>
        ///     The PSBx####. These are (supposed to be) unique
        /// </summary>
        public int Number { get; set; }

        public int Index { get; set; } = 0;

        /// <summary>
        ///     Username of the account
        /// </summary>
        public string Username { get; set; } = "";

        /// <summary>
        ///     Password of the account
        /// </summary>
        public string Password { get; set; } = "";

    }

    public class PsbOvOAccountEntry {


        /// <summary>
        ///     ID of the <see cref="PsbOvOAccount"/>
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        ///     What day this account was used on
        /// </summary>
        public DateTime When { get; set; } 

        /// <summary>
        ///     What outfit used this account 
        /// </summary>
        public string? UsedBy { get; set; }

    }

}
