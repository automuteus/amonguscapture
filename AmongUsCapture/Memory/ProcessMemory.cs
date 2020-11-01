using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AmongUsCapture
{
    public abstract class ProcessMemory
    {
        private static readonly object instanceLock = new object();

        private static ProcessMemory instance;
        public static ProcessMemory getInstance()
        {
            if (instance != null)
                return instance;

            lock (instanceLock)
            {
                if (instance != null)
                    return instance;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    instance = new ProcessMemoryWindows();
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    instance = new ProcessMemoryLinux();
                else
                    throw new PlatformNotSupportedException();

                return instance;
            }
        }

        protected bool Is64Bit { get; set; }
        public Process Process { get; protected set; }
        public List<Module> Modules { get; protected set; }
        public bool IsHooked { get; protected set; }

        public abstract bool HookProcess(string name);
        public abstract void LoadModules();
        public abstract T Read<T>(IntPtr address, params int[] offsets) where T : unmanaged;
        public abstract T ReadWithDefault<T>(IntPtr address, T defaultparam, params int[] offsets) where T : unmanaged;

        public abstract string ReadString(IntPtr address);
        public abstract IntPtr[] ReadArray(IntPtr address, int size);

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
