using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture
{

    public abstract class ProcessMemory
    {
        private static ProcessMemory instance;
        public static ProcessMemory getInstance()
        {
            if (instance == null)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    instance = new ProcessMemoryWindows();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    instance = new ProcessMemoryLinux();
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
            return instance;
        }
        public bool is64Bit;
        public Process process;
        public List<Module> modules;
        public bool IsHooked { get; protected set; }
        public abstract bool HookProcess(string name);
        public abstract void LoadModules();
        public abstract T Read<T>(IntPtr address, params int[] offsets) where T : unmanaged;
        public abstract byte[] Read(IntPtr address, int numBytes);
        public abstract T ReadWithDefault<T>(IntPtr address, T defaultparam, params int[] offsets) where T : unmanaged;

        public abstract string ReadString(IntPtr address, int lengthOffset = 0x8, int rawOffset = 0xC);
        public abstract IntPtr[] ReadArray(IntPtr address, int size);
        public abstract int OffsetAddress(ref IntPtr address, params int[] offsets);

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
        protected struct ModuleInfo
        {
            public IntPtr BaseAddress;
            public uint ModuleSize;
            public IntPtr EntryPoint;
        }
    }
}