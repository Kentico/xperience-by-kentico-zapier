using System.Security.Cryptography;
using System.Text;

namespace Kentico.Xperience.Zapier.Admin
{
    internal static class ApiKeyHelper
    {
        internal static string GenerateApiKey()
        {
            byte[] key = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(key);
        }

        internal static string GetHash(string input)
        {
            byte[] data;
            using (var sha256Hash = SHA256.Create())
            {
                data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
            // Convert the input string to a byte array and compute the hash.

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }


        internal static bool VerifyHash(string input, string hash)
        {
            string hashOfInput = GetHash(input);

            var comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }
    }
}
