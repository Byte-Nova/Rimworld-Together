using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class Hasher
    {
        public static string GetHash(string input)
        {
            using (SHA256 shaAlgorythm = SHA256.Create())
            {
                byte[] code = shaAlgorythm.ComputeHash(Encoding.ASCII.GetBytes(input));
                return BitConverter.ToString(code).Replace("-", "");
            }
        }
    }
}
