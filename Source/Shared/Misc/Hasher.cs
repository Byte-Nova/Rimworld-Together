using System;
using System.Security.Cryptography;
using System.Text;

namespace RimworldTogether.Shared.Misc
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
