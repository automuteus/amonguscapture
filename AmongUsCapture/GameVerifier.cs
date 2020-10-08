using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AmongUsCapture
{
    public static class GameVerifier
    {
        private static string GetChecksumFromFile(string filepath)
        {
            StringBuilder Sb = new StringBuilder();

            using var hash = SHA256.Create();
            using FileStream fileStream = File.OpenRead(filepath);
            
            Encoding enc = Encoding.UTF8;
            Byte[] result = hash.ComputeHash(fileStream);

            foreach (Byte b in result)
                Sb.Append(b.ToString("x2"));
            return Sb.ToString();
        }

        public static bool VerifyGameHash(string filepath)
        {
            var gameChecksum = GetChecksumFromFile(filepath);
            return string.Equals(Settings.GameOffsets.GameHash, gameChecksum,
                StringComparison.CurrentCultureIgnoreCase);
        }
    }
}