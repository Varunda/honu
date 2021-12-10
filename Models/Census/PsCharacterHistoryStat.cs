using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Models.Census {

    /// <summary>
    ///     Represents data from the /characters_stat_history collection
    /// </summary>
    public class PsCharacterHistoryStat {

        /// <summary>
        ///     ID of the character
        /// </summary>
        public string CharacterID { get; set; } = "";

        /// <summary>
        ///     What type this entry is for
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        ///     When in UTC this was last updated
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        ///     How many in total has this character gotten
        /// </summary>
        public int AllTime { get; set; }

        /// <summary>
        ///     Max gotten in a single life
        /// </summary>
        public int OneLifeMax { get; set; }

        // I hate this

        public int Day1 { get; set; }
        public int Day2 { get; set; }
        public int Day3 { get; set; }
        public int Day4 { get; set; }
        public int Day5 { get; set; }
        public int Day6 { get; set; }
        public int Day7 { get; set; }
        public int Day8 { get; set; }
        public int Day9 { get; set; }
        public int Day10 { get; set; }
        public int Day11 { get; set; }
        public int Day12 { get; set; }
        public int Day13 { get; set; }
        public int Day14 { get; set; }
        public int Day15 { get; set; }
        public int Day16 { get; set; }
        public int Day17 { get; set; }
        public int Day18 { get; set; }
        public int Day19 { get; set; }
        public int Day20 { get; set; }
        public int Day21 { get; set; }
        public int Day22 { get; set; }
        public int Day23 { get; set; }
        public int Day24 { get; set; }
        public int Day25 { get; set; }
        public int Day26 { get; set; }
        public int Day27 { get; set; }
        public int Day28 { get; set; }
        public int Day29 { get; set; }
        public int Day30 { get; set; }
        public int Day31 { get; set; }

        public List<int> Days {
            get => new List<int>() {
                Day1, Day2, Day3, Day4, Day5, Day6, Day7, Day8, Day9, Day10, Day11,
                Day12, Day13, Day14, Day15, Day16, Day17, Day18, Day19, Day20, Day21,
                Day22, Day23, Day24, Day25, Day26, Day27, Day28, Day29, Day30, Day31
            };
        }

        public int Month1 { get; set; }
        public int Month2 { get; set; }
        public int Month3 { get; set; }
        public int Month4 { get; set; }
        public int Month5 { get; set; }
        public int Month6 { get; set; }
        public int Month7 { get; set; }
        public int Month8 { get; set; }
        public int Month9 { get; set; }
        public int Month10 { get; set; }
        public int Month11 { get; set; }
        public int Month12 { get; set; }

        public List<int> Months {
            get => new List<int>() {
                Month1, Month2, Month3, Month4, Month5, Month6,
                Month7, Month8, Month9, Month10, Month11, Month12,
            };
        }

        public int Week1 { get; set; }
        public int Week2 { get; set; }
        public int Week3 { get; set; }
        public int Week4 { get; set; }
        public int Week5 { get; set; }
        public int Week6 { get; set; }
        public int Week7 { get; set; }
        public int Week8 { get; set; }
        public int Week9 { get; set; }
        public int Week10 { get; set; }
        public int Week11 { get; set; }
        public int Week12 { get; set; }
        public int Week13 { get; set; }

        public List<int> Weeks {
            get => new List<int>() {
                Week1, Week2, Week3, Week4, Week5, Week6, Week7,
                Week8, Week9, Week10, Week11, Week12, Week13
            };
        }

    }
}
