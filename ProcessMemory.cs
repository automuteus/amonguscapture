using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongcordClient
{
    public static class ProcessMemory
    {
        private static bool is64Bit;
        public static Process process;
        public static List<Module> modules;
        public static bool IsHooked { get; private set; } = false;
        public static bool HookProcess(string name)
        {
            IsHooked = process != null && !process.HasExited;
            if (!IsHooked)
            {
                Process[] processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    process = processes[0];
                    if (process != null && !process.HasExited)
                    {
                        bool flag;
                        WinAPI.IsWow64Process(process.Handle, out flag);
                        is64Bit = Environment.Is64BitOperatingSystem && !flag;

                        modules = new List<Module>();
                        IntPtr[] buffer = new IntPtr[1024];
                        uint cb = (uint)(IntPtr.Size * buffer.Length);
                        if (WinAPI.EnumProcessModulesEx(process.Handle, buffer, cb, out uint totalModules, 3u))
                        {
                            uint moduleSize = totalModules / (uint)IntPtr.Size;
                            StringBuilder stringBuilder = new StringBuilder(260);
                            for (uint count = 0;  count < moduleSize; count++)
                            {
                                stringBuilder.Clear();
                                if (WinAPI.GetModuleFileNameEx(process.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u)
                                    break;
                                string fileName = stringBuilder.ToString();
                                stringBuilder.Clear();
                                if (WinAPI.GetModuleBaseName(process.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u)
                                    break;
                                string moduleName = stringBuilder.ToString();
                                ModuleInfo moduleInfo = default;
                                if (!WinAPI.GetModuleInformation(process.Handle, buffer[count], out moduleInfo, (uint)Marshal.SizeOf(moduleInfo)))
                                    break;
                                modules.Add(new Module
                                {
                                    FileName = fileName,
                                    BaseAddress = moduleInfo.BaseAddress,
                                    EntryPointAddress = moduleInfo.EntryPoint,
                                    MemorySize = moduleInfo.ModuleSize,
                                    Name = moduleName
                                });
                            }
                        }

                        IsHooked = true;
                    }
                }
            }
            return IsHooked;
        }
        public static T Read<T>(IntPtr address, params int[] offsets) where T : unmanaged
        {
            if (process == null || address == IntPtr.Zero)
                return default;

            int last = OffsetAddress(ref address, offsets);
            if (address == IntPtr.Zero)
                return default;

            unsafe
            {
                int size = sizeof(T);
                if (typeof(T) == typeof(IntPtr)) size = is64Bit ? 8 : 4;
                byte[] buffer = Read(address + last, size);
                fixed (byte* ptr = buffer)
                {
                    return *(T*)ptr;
                }
            }
        }

        public static string ReadString(IntPtr address)
        {
            if (process == null || address == IntPtr.Zero)
                return default;
            int stringLength = Read<int>(address + 0x8);
            byte[] rawString = Read(address + 0xC, stringLength << 1);
            return System.Text.Encoding.Unicode.GetString(rawString);
        }

        private static byte[] Read(IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];
            if (process == null || address == IntPtr.Zero)
                return buffer;

            WinAPI.ReadProcessMemory(process.Handle, address, buffer, numBytes, out int bytesRead);
            return buffer;
        }
        private static int OffsetAddress(ref IntPtr address, params int[] offsets)
        {
            byte[] buffer = new byte[is64Bit ? 8 : 4];
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                WinAPI.ReadProcessMemory(process.Handle, address + offsets[i], buffer, buffer.Length, out int bytesRead);
                if (is64Bit)
                    address = (IntPtr)BitConverter.ToUInt64(buffer, 0);
                else
                    address = (IntPtr)BitConverter.ToUInt32(buffer, 0);
                if (address == IntPtr.Zero)
                    break;
            }
            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }
        private static class WinAPI
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool IsWow64Process(IntPtr hProcess, [MarshalAs(UnmanagedType.Bool)] out bool wow64Process);
            [DllImport("psapi.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool EnumProcessModulesEx(IntPtr hProcess, [Out] IntPtr[] lphModule, uint cb, out uint lpcbNeeded, uint dwFilterFlag);
            [DllImport("psapi.dll", SetLastError = true)]
            public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
            [DllImport("psapi.dll")]
            public static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, uint nSize);
            [DllImport("psapi.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetModuleInformation(IntPtr hProcess, IntPtr hModule, out ModuleInfo lpmodinfo, uint cb);
        }
        public class Module
        {
            public IntPtr BaseAddress { get; set; }
            public IntPtr EntryPointAddress { get; set; }
            public string FileName { get; set; }
            public uint MemorySize { get; set; }
            public string Name { get; set; }
            public FileVersionInfo FileVersionInfo { get { return FileVersionInfo.GetVersionInfo(FileName); } }
            public override string ToString()
            {
                return Name ?? base.ToString();
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct ModuleInfo
        {
            public IntPtr BaseAddress;
            public uint ModuleSize;
            public IntPtr EntryPoint;
        }
    }
}
