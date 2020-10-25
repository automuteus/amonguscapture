using System.IO;

namespace AmongUsCapture
{
    public static class GameVerifier
    {
        public static bool VerifySteamHash(string executablePath)
        {
            // Get Steam_api.dll from (parent)filepath\Among Us_Data\Plugins\x86\steam_api.dll and steam_api64.dll

            var baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName, "Among Us_Data", "Plugins", "x86");
            var steam_apiCert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api.dll"));
            var steam_api64Cert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api64.dll"));

            return steam_apiCert && steam_api64Cert;
        }
    }
}
