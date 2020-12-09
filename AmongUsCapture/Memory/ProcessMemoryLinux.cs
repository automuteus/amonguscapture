using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AmongUsCapture
{
    public class ProcessMemoryLinux : ProcessMemory
    {
        public override bool HookProcess(string name)
        {
            // append .exe to the name of the process, since we will be hooking the exe run by Wine.
            name = name + ".exe";
            if (!IsHooked)
            {
                Process[] processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                {
                    process = processes[0];
                    if (process != null && !process.HasExited)
                    {
                        int pid = process.Id;

                        // Get PID - we will need this to calculate the /proc folder location.
                        if (Directory.Exists($"/proc/{pid}/"))
                        {
                            // Quickly run 'file -L' /proc/pid to get arch of file
                            var processval = new Process()
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "/usr/bin/file",
                                    Arguments = $"-L \"/proc/{pid}/exe\"",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                }
                            };

                            processval.Start();
                            string result = processval.StandardOutput.ReadToEnd();
                            processval.WaitForExit();

                            bool flag = result.Contains("64-bit");

                            is64Bit = flag;

                            LoadModules();

                        }

                    }
                }
            }

            return IsHooked;
        }

        public override void LoadModules()
        {
            modules = new List<Module>();
            
            // Read /proc/<pid>/maps for library mapping information.
            // Reading from /proc/<pid>/maps is negligible, since this file is a kernel pseudofile.
            // Also, it's more reliable then using C#'s native Process object.
            
            if (!File.Exists($"/proc/{process.Id}/maps"))
            {
                // We don't have the maps file yet, or we ended up in a state where it doesn't exist.
                return;
            }
            
            var proc_maps = File.ReadLines($"/proc/{process.Id}/maps")
                .Where(s => s.Contains("GameAssembly.dll"))
                .ToList();

            if (proc_maps.Count <= 0)
            {
                // If we don't have a line, the maps file hasn't been populated yet,
                // or GameAssembly.dll hasn't been loaded.
                return;
            }

            // Linux appears to make two instances of WINE/DLL modules.

            // We want the first one, since that represents the beginning
            // of the GameAssembly.dll memory space.
            
            // /proc/pid/maps legend:
            // address           perms offset  dev   inode   pathname

            string[] map_lines1 = proc_maps[0].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            string[] addr_vals1 = map_lines1[0].Split('-');
            
            // Under linux, we can confirm the actual 'start' location, but not the end. The 'end' is the
            // end of the initial instance of 'GameAssembly.dll", which is likely not the entire entry.
            // Therefore, our module will only really 'know' where the first entry ends.
            
            // We will have to change this if it becomes necessary, but for now, everything is working as intended.
            
            uint addr_start = UInt32.Parse(addr_vals1[0], System.Globalization.NumberStyles.HexNumber);
            uint addr_end = UInt32.Parse(addr_vals1[1], System.Globalization.NumberStyles.HexNumber);
            uint memsize =  addr_end - addr_start;
            

            // Ensure we have an absolute path by catting all potential additional strings after index 5.
            StringBuilder pathbuilder = new StringBuilder();
            for (int x = 5; x < map_lines1.Length; x++)
            {
                pathbuilder.Append(map_lines1[x] + " ");
            }

            string librarypath = pathbuilder.ToString();
            
            modules.Add(new Module()
            {
                Name = librarypath.Split('/').Last().Trim(), // Make sure hidden characters aren't there.
                BaseAddress = (IntPtr) addr_start,
                FileName = librarypath,
                MemorySize = memsize,
                EntryPointAddress = IntPtr.Zero
            });

        }

        public override T Read<T>(IntPtr address, params int[] offsets)
        {
            return ReadWithDefault<T>(address, default, offsets);
        }

        public override T ReadWithDefault<T>(IntPtr address, T defaultParam, params int[] offsets)
        {
            if (process == null || address == IntPtr.Zero)
            {
                return defaultParam;
            }

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
                    return *(T*) ptr;
                }
            }
        }

        public override string ReadString(IntPtr address)
        {
            if (process == null || address == IntPtr.Zero)
                return default;
            int stringLength = Read<int>(address + 0x8);
            byte[] rawString = Read(address + 0xC, stringLength << 1);
            return Encoding.Unicode.GetString(rawString);
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

        /*
         * Compared to the Windows version of the ProcessMemory module, Linux requires some extra marshalling.
         *
         * This is because to read and store, Linux uses 'iovec' C structs to provide the base pointer
         * and length of the information being read.
         * */
        
        private int OffsetAddress(ref IntPtr address, params int[] offsets)
        {
            byte[] buffer = new byte[is64Bit ? 8 : 4];
            IntPtr buffer_marshal;
            IntPtr local_ptr;
            IntPtr remote_ptr;
            
            unsafe
            {
                // We need to work unsafe here to get the size of the iovec structures, then malloc them.
                buffer_marshal = Marshal.AllocHGlobal(is64Bit ? 8 : 4);
                local_ptr = Marshal.AllocHGlobal(sizeof(iovec));
                remote_ptr = Marshal.AllocHGlobal(sizeof(iovec));
            }

            for (int i = 0; i < offsets.Length - 1; i++)
            {


                var local = new iovec()
                {
                    iov_base = buffer_marshal,
                    iov_len = is64Bit ? 8 : 4
                };
                var remote = new iovec()
                {
                    iov_base = address + offsets[i],
                    iov_len = buffer.Length
                };

                Marshal.StructureToPtr(local, local_ptr, true);
                Marshal.StructureToPtr(remote, remote_ptr, true);

                LinuxAPI.process_vm_readv(process.Id, local_ptr, 1, remote_ptr, 1, 0);

                Marshal.Copy(local.iov_base, buffer, 0, buffer.Length);
                
                if (is64Bit)
                    address = (IntPtr) BitConverter.ToUInt64(buffer, 0);
                else
                    address = (IntPtr) BitConverter.ToUInt32(buffer, 0);
                if (address == IntPtr.Zero)
                    break;
            }

            Marshal.FreeHGlobal(local_ptr);
            Marshal.FreeHGlobal(remote_ptr);
            Marshal.FreeHGlobal(buffer_marshal);

            return offsets.Length > 0 ? offsets[offsets.Length - 1] : 0;
        }

        private byte[] Read(IntPtr address, int numBytes)
        {
            byte[] buffer = new byte[numBytes];

            if (process == null || address == IntPtr.Zero)
            {
                return buffer;
            }

            IntPtr buffer_marshal = Marshal.AllocHGlobal(numBytes);
            IntPtr local_ptr;
            IntPtr remote_ptr;

            unsafe
            {
                local_ptr = Marshal.AllocHGlobal(sizeof(iovec));
                remote_ptr = Marshal.AllocHGlobal(sizeof(iovec));
            }
            
            var local = new iovec()
            {
                iov_base = buffer_marshal,
                iov_len = numBytes
            };
            var remote = new iovec()
            {
                iov_base = address,
                iov_len = numBytes
            };
            
            Marshal.StructureToPtr(local, local_ptr, true);
            Marshal.StructureToPtr(remote, remote_ptr, true);

            LinuxAPI.process_vm_readv(process.Id, local_ptr, 1, remote_ptr, 1, 0);

            Marshal.Copy(local.iov_base, buffer, 0, numBytes);

            Marshal.FreeHGlobal(local_ptr);
            Marshal.FreeHGlobal(remote_ptr);
            Marshal.FreeHGlobal(buffer_marshal);

            return buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct iovec
        {
            public IntPtr iov_base;
            public int iov_len;
        }

    }

    public static class LinuxAPI
    {
        [DllImport("libc", SetLastError = true)]
        public static extern int process_vm_readv(int pid, IntPtr local_iov, ulong liovcnt, IntPtr remote_iov,
            ulong riovcnt, ulong flags);
    }
}