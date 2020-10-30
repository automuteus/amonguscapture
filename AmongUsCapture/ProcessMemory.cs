﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture
{
    public static class ProcessMemory
    {
        private static bool Is64Bit { get; set; }
        public static Process Process { get; private set; }
        public static List<Module> Modules { get; private set; }

        public static bool IsHooked => Process != null && !Process.HasExited;

        public static bool HookProcess(string name)
        {
            if (IsHooked)
                return IsHooked;
            
            Process[] processes = Process.GetProcessesByName(name);
            if (processes.Length < 1)
                return IsHooked;

            
            Process = processes[0];
            if (IsHooked)
            {
                WinApi.IsWow64Process(Process.Handle, out var flag);
                Is64Bit = Environment.Is64BitOperatingSystem && !flag;

                LoadModules();
            }
            
            return IsHooked;
        }

        public static void LoadModules()
        {
            Modules = new List<Module>();
            IntPtr[] buffer = new IntPtr[1024];
            uint cb = (uint)(IntPtr.Size * buffer.Length);
            if (!WinApi.EnumProcessModulesEx(Process.Handle, buffer, cb, out uint totalModules, 3u))
                return;
            
            uint moduleSize = totalModules / (uint)IntPtr.Size;
            StringBuilder stringBuilder = new StringBuilder(260);
            for (uint count = 0; count < moduleSize; count++)
            {
                stringBuilder.Clear();
                if (WinApi.GetModuleFileNameEx(Process.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u)
                    break;

                string fileName = stringBuilder.ToString();
                stringBuilder.Clear();
                if (WinApi.GetModuleBaseName(Process.Handle, buffer[count], stringBuilder, (uint)stringBuilder.Capacity) == 0u)
                    break;

                string moduleName = stringBuilder.ToString();
                ModuleInfo moduleInfo = default;
                if (!WinApi.GetModuleInformation(Process.Handle, buffer[count], out moduleInfo, (uint)Marshal.SizeOf(moduleInfo)))
                    break;

                Modules.Add(new Module
                {
                    FileName = fileName,
                    BaseAddress = moduleInfo.BaseAddress,
                    EntryPointAddress = moduleInfo.EntryPoint,
                    MemorySize = moduleInfo.ModuleSize,
                    Name = moduleName
                });
            }
        }

        public static T Read<T>(IntPtr address, params int[] offsets) where T : unmanaged
        {
            return ReadWithDefault<T>(address, default, offsets);
        }

        public static T ReadWithDefault<T>(IntPtr address, T defaultParam, params int[] offsets) where T : unmanaged
        {
            if (Process == null || address == IntPtr.Zero)
                return defaultParam;

            int last = OffsetAddress(ref address, offsets);
            if (address == IntPtr.Zero)
                return defaultParam;

            unsafe
            {
                int size = sizeof(T);
                if (typeof(T) == typeof(IntPtr)) size = Is64Bit ? 8 : 4;
                byte[] buffer = Read(address + last, size);
                fixed (byte* ptr = buffer)
                {
                    return *(T*)ptr;
                }
            }
        }

        public static string ReadString(IntPtr address)
        {
            if (Process == null || address == IntPtr.Zero)
                return default;
            int stringLength = Read<int>(address + 0x8);
            byte[] rawString = Read(address + 0xC, stringLength << 1);
            return Encoding.Unicode.GetString(rawString);
        }

        public static IntPtr[] ReadArray(IntPtr address, int size)
        {
            byte[] bytes = Read(address, size * 4);
            IntPtr[] ints = new IntPtr[size];
            for (int i = 0; i < size; i++)
            {
                ints[i] = (IntPtr) BitConverter.ToUInt32(bytes, i * 4);
            }
            return ints;
        }

        private static byte[] Read(IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];
            if (Process == null || address == IntPtr.Zero)
                return buffer;

            WinApi.ReadProcessMemory(Process.Handle, address, buffer, numBytes, out int bytesRead);
            return buffer;
        }

        private static int OffsetAddress(ref IntPtr address, params int[] offsets)
        {
            byte[] buffer = new byte[Is64Bit ? 8 : 4];
            for (int i = 0; i < offsets.Length - 1; i++)
            {
                WinApi.ReadProcessMemory(Process.Handle, address + offsets[i], buffer, buffer.Length, out int bytesRead);
                if (Is64Bit)
                    address = (IntPtr)BitConverter.ToUInt64(buffer, 0);
                else
                    address = (IntPtr)BitConverter.ToUInt32(buffer, 0);
                if (address == IntPtr.Zero)
                    break;
            }
            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }

        private static class WinApi
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
