using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {

            if (hash != null && (hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2x$") || hash.StartsWith("$2y$")))
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hash);
                }
                catch
                {
                    return password == hash;
                }
            }

            return password == hash;
        }
    }
}
