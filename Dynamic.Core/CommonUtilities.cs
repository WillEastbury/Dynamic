using System.Security.Cryptography;
using System.Text;

namespace Dynamic.Core
{
    public static class CommonUtilities
    {
        public static byte[] GetSHA256Hash(string input, byte[] hash)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(input).Concat(hash).ToArray());
        }
        public static byte[] GetSHA256Hash(byte[] input, byte[] hash)
        {
            return SHA256.HashData(input.Concat(hash).ToArray());
        }

        public static bool IsStepsOfChangeAway(string e, string newPassword, int distance = 2)
        {
            // Levenshtein distance calculation
            int m = e.Length;
            int n = newPassword.Length;

            // Create a matrix to store results of subproblems
            int[,] dp = new int[m + 1, n + 1];

            // Fill the matrix
            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0)
                        dp[i, j] = j; // Insert all characters of newPassword
                    else if (j == 0)
                        dp[i, j] = i; // Remove all characters of e
                    else
                    {
                        int cost = (e[i - 1] == newPassword[j - 1]) ? 0 : 1; // No cost if characters are equal, else 1
                        dp[i, j] = Math.Min(
                            Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), // Deletion or Insertion
                            dp[i - 1, j - 1] + cost // Substitution
                        );
                    }
                }
            }

            // The final distance is the bottom-right value in the matrix
            int editDistance = dp[m, n];

            // If the edit distance is less than or equal to the degree, return true
            return editDistance <= distance;
        }

        public static Tuple<byte[], byte[]> VernamCipherText(byte[] secretKey, byte[] encryptionKey)
        {
            if (encryptionKey == null) encryptionKey = RandomNumberGenerator.GetBytes(secretKey.Length);

            byte[] xoredkey = new byte[secretKey.Length];

            for (int i = 0; i < secretKey.Length; i++)
            {
                xoredkey[i] = (byte)(secretKey[i] ^ encryptionKey[i % encryptionKey.Length]);
            }

            return new Tuple<byte[], byte[]>(xoredkey, encryptionKey);
        }
    
        public static string Base64UrlEncode(byte[] input)
        {
            // Base64Url encoding (standard Base64 without padding)
            var base64 = Convert.ToBase64String(input);
            base64 = base64.Split('=')[0]; // Remove padding
            base64 = base64.Replace('+', '-'); // Replace Base64 chars with URL-safe chars
            base64 = base64.Replace('/', '_');
            return base64;
        }

        public static byte[] Base64UrlDecode(string encodedValue)
        {
            // Step 1: Replace URL-safe characters with standard Base64 characters
            string base64 = encodedValue
                            .Replace('-', '+')  // Replace '-' with '+'
                            .Replace('_', '/');  // Replace '_' with '/'

            // Step 2: Add padding if necessary
            int paddingLength = base64.Length % 4;
            if (paddingLength > 0)
            {
                base64 = base64.PadRight(base64.Length + (4 - paddingLength), '=');
            }

            // Step 3: Decode the Base64 string into bytes
            return Convert.FromBase64String(base64);
        }
    }
}
