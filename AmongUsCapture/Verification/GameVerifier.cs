using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace AmongUsCapture
{
    [Flags]
    public enum AmongUsValidity
    {
        OK = 0b_0000_0000,
        STEAM_VERIFICAITON_FAIL = 0b_0000_0001,
        GAME_VERIFICATION_FAIL = 0b_0000_0010
    }

    public class ValidatorEventArgs : EventArgs
    {
        public AmongUsValidity Validity { get; set; }
    }

    public static class GameVerifier
    {
        private const string steamapi32_orig_hash = "07407c1bc2f3114042dbcfe8183b77f73e178be7";
        private const string steamapi64_orig_hash = "e77433094c56433685a68a4436e81b76c3b5e1f5";

        public static bool VerifySteamHash(string executablePath)
        {
            //Get Steam_api.dll from (parent)filepath\Among Us_Data\Plugins\x86\steam_api.dll and steam_api64.dll
            var baseDllFolder = Path.Join(
                    Directory.GetParent(executablePath).FullName,
                    "Among Us_Data",
                    "Plugins",
                    "x86");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return
                    WinAuthTools.IsTrusted(Path.Join(baseDllFolder, "steam_api.dll")) &&
                    WinAuthTools.IsTrusted(Path.Join(baseDllFolder, "steam_api64.dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return
                    string.Equals(steamapi32_orig_hash.ToUpper(), ComputeSHA1(Path.Join(baseDllFolder, "steam_api.dll"))) &&
                    string.Equals(steamapi64_orig_hash.ToUpper(), ComputeSHA1(Path.Join(baseDllFolder, "steam_api64.dll")));
            }

            throw new PlatformNotSupportedException();
        }

        private static string ComputeSHA1(string filePath)
        {
            using SHA1Managed sha1 = new SHA1Managed();

            using FileStream fs = new FileStream(filePath, FileMode.Open);

            using var bs = new BufferedStream(fs);

            var hashByte = sha1.ComputeHash(bs);
            StringBuilder hashSb = new StringBuilder(2 * hashByte.Length);
            foreach (byte byt in hashByte)
                hashSb.AppendFormat("{0:X2}", byt);

            return hashSb.ToString();
        }
    }
}
