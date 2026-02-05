using System.Security.Cryptography;

namespace Business.Utils
{
    public static class TokenGenerator
    {
        public static string GenerateSecureToken()
        {
            int length = RandomNumberGenerator.GetInt32(50, 101);

            int numBytes = (int)Math.Ceiling(length * 0.75);

            byte[] randomBytes = new byte[numBytes];
            RandomNumberGenerator.Fill(randomBytes);

            string base64 = Convert.ToBase64String(randomBytes);

            string base64Url = base64.Replace("+", "-").Replace("/", "_").Replace("=", "");

            return base64Url.Length >= length
                ? base64Url.Substring(0, length)
                : base64Url.PadRight(length, '0');
        }
    }
}
