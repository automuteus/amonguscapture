using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AmongUsCapture
{
    public static class GameVerifier
    {
        public static bool VerifySteamHash(string executablePath)
        {
            //Get Steam_api.dll from (parent)filepath\Among Us_Data\Plugins\x86\steam_api.dll and steam_api64.dll
            var baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName, "\\Among Us_Data\\Plugins\\x86\\");
            var steam_apiCert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api.dll"));
            var steam_api64Cert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api64.dll"));
            //Settings.conInterface.WriteModuleTextColored("GameVerifier",Color.Yellow,$"steam_apiCert: {steam_apiCert}");
            //Settings.conInterface.WriteModuleTextColored("GameVerifier",Color.Yellow,$"steam_api64Cert: {steam_api64Cert}");
            return (steam_apiCert) && (steam_api64Cert);
        }
    }
}