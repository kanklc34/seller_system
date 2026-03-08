using System.Security.Cryptography;
using System.Text;

namespace Saller_System.Services
{
    public static class GuvenlikServisi
    {
        public static string Hashle(string sifre)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sifre));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}