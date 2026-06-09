using System.Security.Cryptography;
using System.Text;

namespace Farmhollow
{
    // Vorläufiges Login mit EINEM festen Account. Passwort nur als Hash gespeichert
    // (Klartext steht nirgends). Wird später durch Server+DB / Steam ersetzt.
    public static class Auth
    {
        public const string FixedEmail = "tobiasprang@icloud.com";
        // SHA-256 des festen Passworts (kein Klartext im Code)
        private const string PassHash = "a5c6ea04cb3950a29114203ec2334668b9b352753dcee0d503aa16e7eb23f856";

        public static bool LoggedIn { get; private set; }
        public static string PlayerName = "nya";   // vorerst hardcoded

        public static bool TryLogin(string email, string password)
        {
            if (email == null || password == null) return false;
            if (email.Trim().ToLowerInvariant() != FixedEmail) return false;
            if (Hash(password) != PassHash) return false;
            LoggedIn = true;
            return true;
        }

        private static string Hash(string s)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
