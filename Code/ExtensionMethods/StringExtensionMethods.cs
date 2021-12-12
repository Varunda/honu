using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace watchtower.Code.ExtensionMethods {

    public static class StringExtensionMethods {

        public static string Truncate(this string str, int length) {
            if (str.Length > length) {
                return str.Substring(0, length - 3) + "...";
            }

            return str;
        }

    }
}
