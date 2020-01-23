using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FotoShoutUtils.Utils {
    public static class StringUtils {
        public static bool Contains(this string source, string strToCheck, StringComparison comp) {
            return (source.IndexOf(strToCheck, comp) >= 0);
        }

        public static string ToString(ICollection<string> coll, string delimiter = "|") {
            string ret = "";
            foreach (string str in coll) {
                if (!string.IsNullOrEmpty(ret))
                    ret += delimiter;
                ret += str;
            }

            return ret;
        }
    }
}