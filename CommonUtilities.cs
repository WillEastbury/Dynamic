using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Core
{
    public static class CommonUtilities
    {
        public static byte[] GetSHA1Hash(string input, byte[] salt)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] combined = new byte[inputBytes.Length + salt.Length];
            Buffer.BlockCopy(inputBytes, 0, combined, 0, inputBytes.Length);
            Buffer.BlockCopy(salt, 0, combined, inputBytes.Length, salt.Length);
            return SHA1.HashData(combined);
        }

        public static byte[] GetSHA1Hash(byte[] input, byte[] salt)
        {
            byte[] combined = new byte[input.Length + salt.Length];
            Buffer.BlockCopy(input, 0, combined, 0, input.Length);
            Buffer.BlockCopy(salt, 0, combined, input.Length, salt.Length);
            return SHA256.HashData(combined);
        }
    }
}
