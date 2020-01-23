using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FotoShoutApi.Security {
    public class Cryptography {
        public static string GenerateMD5Hash(string str) {
            // Calculate MD5 hash
            MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            byte[] hash = md5.ComputeHash(bytes);

            // Convert to HEX string
            StringBuilder sb = new StringBuilder();
            for (int idx = 0; idx < hash.Length; idx++)
                sb.Append(hash[idx].ToString("X2"));

            return sb.ToString();
        }
    }
}