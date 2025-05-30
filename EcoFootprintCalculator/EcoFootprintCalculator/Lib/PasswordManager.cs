using System.Security.Cryptography;

namespace EcoFootprintCalculator.Lib
{
    public static class PasswordManager
    {
        // It's good practice to use constants for magical values!
        private const int SaltSize = 16;        // 16 bytes
        private const int HashSize = 32;        // 32 bytes for SHA256
        private const int Iterations = 100_000; // 100,000 iterations

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            string combined = Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);

            // Debug line: print the generated hash to console so you can check its length in your terminal/database
            System.Console.WriteLine("DEBUG GENERATED HASH: " + combined);

            return combined;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] hash = Convert.FromBase64String(parts[1]);
                byte[] testHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
            catch (FormatException)
            {
                // One of the Base64 strings is invalid
                return false;
            }
        }
    }
}