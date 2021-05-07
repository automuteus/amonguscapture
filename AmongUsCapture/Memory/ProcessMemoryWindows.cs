using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture
{
 public class ProcessMemoryWindows : ProcessMemory
    {

        public override bool HookProcess(string name)
        {
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

                        LoadModules();
                    }
                }
            }

            IsHooked = process != null && !process.HasExited;
            return IsHooked;
        }

        public override void LoadModules()
        {
            modules = new List<Module>();
            IntPtr[] buffer = new IntPtr[1024];
            uint cb = (uint)(IntPtr.Size * buffer.Length);
            if (WinAPI.EnumProcessModulesEx(process.Handle, buffer, cb, out uint totalModules, 3u))
            {
                uint moduleSize = totalModules / (uint)IntPtr.Size;
                StringBuilder stringBuilder = new StringBuilder(260);
                for (uint count = 0; count < moduleSize; count++)
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
        }

        public override T Read<T>(IntPtr address, params int[] offsets)
        {
            return ReadWithDefault<T>(address, default, offsets);
        }

        public override T ReadWithDefault<T>(IntPtr address, T defaultParam, params int[] offsets)
        {
            if (process == null || address == IntPtr.Zero)
                return defaultParam;

            int last = OffsetAddress(ref address, offsets);
            if (address == IntPtr.Zero)
                return defaultParam;

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

        public override string ReadString(IntPtr address, int lengthOffset = 0x8, int rawOffset = 0xC)
        {
            if (process == null || address == IntPtr.Zero)
                return default;
            int stringLength = Read<int>(address + lengthOffset);
            byte[] rawString = Read(address + rawOffset, stringLength << 1);
            return System.Text.Encoding.Unicode.GetString(rawString);
        }

        public override IntPtr[] ReadArray(IntPtr address, int size)
        {
            byte[] bytes = Read(address, size * 4);
            IntPtr[] ints = new IntPtr[size];
            for (int i = 0; i < size; i++)
            {
                ints[i] = (IntPtr) BitConverter.ToUInt32(bytes, i * 4);
            }
            return ints;
        }

        public override byte[] Read(IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];
            if (process == null || address == IntPtr.Zero)
                return buffer;

            WinAPI.ReadProcessMemory(process.Handle, address, buffer, numBytes, out int bytesRead);
            return buffer;
        }
        public override int OffsetAddress(ref IntPtr address, params int[] offsets)
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
    }
}