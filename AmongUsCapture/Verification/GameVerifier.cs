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
    [Flags]
    public enum AmongUsValidity
    {
        OK = 0b_0000_0000,
        STEAM_VERIFICAITON_FAIL = 0b_0000_0001,
        GAME_VERIFICATION_FAIL = 0b_0000_0010
    }

    public class ValidatorEventArgs : EventArgs
    {
        public AmongUsValidity Validity;
    }
    
    public static class GameVerifier
    {
        private const string steamapi32_orig_hash = "07407c1bc2f3114042dbcfe8183b77f73e178be7";
        private const string steamapi64_orig_hash = "e77433094c56433685a68a4436e81b76c3b5e1f5";
        private const string amongusexe_orig_hash = "adb281fa5deee89800ddf68eea941a6b8cf6f38e";
        private const string gameassembly_orig_hash = "009a5b65dca2bbf48d64b6e9e13a050ef3dbf201";

        public static bool VerifySteamHash(string executablePath)
        {
           if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
           {
                //Get Steam_api.dll from (parent)filepath\Among Us_Data\Plugins\x86\steam_api.dll and steam_api64.dll
                var baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName,"/Among Us_Data/Plugins/x86/");
                if (!Directory.Exists(baseDllFolder))
                {
                    baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName, "/Among Us_Data/Plugins/x86_64/");
                }
                var steam_apiCert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api.dll"));
                var steam_api64Cert = AuthenticodeTools.IsTrusted(Path.Join(baseDllFolder, "steam_api64.dll"));
                //Settings.conInterface.WriteModuleTextColored("GameVerifier",Color.Yellow,$"steam_apiCert: {steam_apiCert}");
                //Settings.conInterface.WriteModuleTextColored("GameVerifier",Color.Yellow,$"steam_api64Cert: {steam_api64Cert}");
                return (steam_apiCert) && (steam_api64Cert);
           }
            
           if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
           {
                var baseDllFolder = Path.Join(Directory.GetParent(executablePath).FullName,
                    "/Among Us_Data/Plugins/x86/");
                
                var steam_apiPath = Path.Join(baseDllFolder, "steam_api.dll");
                var steam_api64Path = Path.Join(baseDllFolder, "steam_api64.dll");
                var steam_apiHash = String.Empty;
                var steam_api64Hash = String.Empty;
                
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    
                    using (FileStream fs = new FileStream(steam_apiPath, FileMode.Open))
                    {
                        using (var bs = new BufferedStream(fs))
                        {
                            var hash = sha1.ComputeHash(bs);
                            StringBuilder steam_apihashSb = new StringBuilder(2 * hash.Length);
                            foreach (byte byt in hash)
                            {
                                steam_apihashSb.AppendFormat("{0:X2}", byt);
                            }

                            steam_apiHash = steam_apihashSb.ToString();
                        }
                    }    
                    
                    using (FileStream fs = new FileStream(steam_api64Path, FileMode.Open))
                    {
                        using (var bs = new BufferedStream(fs))
                        {
                            var hash = sha1.ComputeHash(bs);
                            StringBuilder steam_api64hashSb = new StringBuilder(2 * hash.Length);
                            foreach (byte byt in hash)
                            {
                                steam_api64hashSb.AppendFormat("{0:X2}", byt);
                            }

                            steam_api64Hash = steam_api64hashSb.ToString();
                        }
                    }
                }

                return (String.Equals(steamapi32_orig_hash.ToUpper(), steam_apiHash) &&
                        String.Equals(steamapi64_orig_hash.ToUpper(), steam_api64Hash));

           }
            
           throw new PlatformNotSupportedException();
        }
        
        public static bool VerifyGameHash(string executablePath)
        {
            // This is for Beta detection.
            var baseDllFolder = Directory.GetParent(executablePath).FullName;

            var amongus_exePath = Path.Join(baseDllFolder, "Among Us.exe");
            var gameassembly_dllPath = Path.Join(baseDllFolder, "GameAssembly.dll");
            var amongus_exeHash = String.Empty;
            var gameassembly_dllHash = String.Empty;
            
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                using (FileStream fs = new FileStream(amongus_exePath, FileMode.Open))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_apihashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_apihashSb.AppendFormat("{0:X2}", byt);
                        }

                        amongus_exeHash = steam_apihashSb.ToString();
                    }
                }    
                    
                using (FileStream fs = new FileStream(gameassembly_dllPath, FileMode.Open))
                {
                    using (var bs = new BufferedStream(fs))
                    {
                        var hash = sha1.ComputeHash(bs);
                        StringBuilder steam_api64hashSb = new StringBuilder(2 * hash.Length);
                        foreach (byte byt in hash)
                        {
                            steam_api64hashSb.AppendFormat("{0:X2}", byt);
                        }

                        gameassembly_dllHash = steam_api64hashSb.ToString();
                    }
                }
            }
            return (String.Equals(amongusexe_orig_hash.ToUpper(), amongus_exeHash) &&
                    String.Equals(gameassembly_orig_hash.ToUpper(), gameassembly_dllHash));
        }
    }
}