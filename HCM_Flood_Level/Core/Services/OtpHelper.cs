using System.Security.Cryptography;
using System.Text;

namespace Core.Services
{
    public static class OtpHelper
    {
        public static string GenerateOtp6()
        {
            var n = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return n.ToString("D6");
        }

        public static string HashOtpSha256(string otp)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToHexString(bytes);
        }
    }
}
