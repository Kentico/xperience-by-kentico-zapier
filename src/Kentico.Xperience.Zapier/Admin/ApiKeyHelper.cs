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

        internal static string GetToken(string input)
        {
            byte[] data = Encoding.UTF8.GetBytes(input);

            return Convert.ToBase64String(SHA256.HashData(data));
        }


        internal static bool VerifyToken(string input, string hash)
        {
            string hashOfInput = GetToken(input);
            return hashOfInput == hash;
        }
    }
}
