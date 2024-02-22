using System;
using System.Security.Cryptography;
using System.Text;

namespace Shared
{
    //This class contains the tools to create hashes from a variety of variables

    public static class Hasher
    {
        //Generates a hash from a given string

        public static string GetHashFromString(string input, bool noSpecialChars = true)
        {
            using SHA256 shaAlgorythm = SHA256.Create();
            byte[] code = shaAlgorythm.ComputeHash(Encoding.ASCII.GetBytes(input));

            if (noSpecialChars) return BitConverter.ToString(code).Replace("-", "");
            else return BitConverter.ToString(code);
        }

        //Generates a hash from a given byte array

        public static string GetHashFromBytes(byte[] input, bool noSpecialChars = true)
        {
            using SHA256 shaAlgorythm = SHA256.Create();
            if (noSpecialChars) return BitConverter.ToString(input).Replace("-", "");
            else return BitConverter.ToString(input);
        }
    }
}
