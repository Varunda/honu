using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.Constants {

    public class MetagameEvent {

        /// <summary>
        /// Get how long a metagame event will last
        /// </summary>
        /// <param name="metagameEventID">ID of the metagame event</param>
        /// <returns>
        ///     A <c>TimeSpan</c> representing how long the metagame event (alert) will last,
        ///     or <c>null</c> if it's unknown
        /// </returns>
        public static TimeSpan? GetDuration(int metagameEventID) {
            return metagameEventID switch {
                147 or 148 or 149 or 150 or 151 or 152 or 153 or 154 or 155 or 156 or 157 or 158 or 211 or 212 or 213 or 214 => TimeSpan.FromMinutes(90),
                176 or 177 or 178 or 179 or 186 or 187 or 188 or 189 or 190 or 191 or 192 or 193 => TimeSpan.FromMinutes(45),
                208 or 209 or 210 => TimeSpan.FromMinutes(1),
                _ => null,
            };
        }

    }
}
