using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AUCapture_Console;
using AUCapture_Console.IPC;
using Castle.Core.Internal;
using Mono.Unix;
using Tmds.DBus;
using Color = System.Drawing.Color;
using Task = System.Threading.Tasks.Task;


namespace AmongUsCapture.DBus
{
    class IPCadapterDBus : IPCAdapter
    {
        private CancellationTokenSource _cancellation = new CancellationTokenSource();
        private bool _serverIsStarted;
        private bool _isHostInstance;
        private Task _serverTask;
        
        public override URIStartResult HandleURIStart(string[] args)
        {
            var myProcessId = Process.GetCurrentProcess().Id;
            //Process[] processes = Process.GetProcessesByName("AmongUsCapture");
            //Process[] dotnetprocesses = Process.GetProcessesByName("dotnet");
            //foreach (Process p in processes)
            //{
            //if (p.Id != myProcessId)
            //    {
            //        p.Kill();
            //    }
            // }
            Console.WriteLine(Program.GetExecutablePath());

            //mutex = new Mutex(true, appName, out var createdNew);
            _isHostInstance = false;
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!File.Exists(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
            {
                _isHostInstance = true;
            }
            else
            {
                // Open our PID file.
                using (var pidfile = File.OpenText(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
                {
                    var pid = pidfile.ReadLine();
                    if (pid != null)
                    {
                        var pidint = Int32.Parse(pid);

                        try
                        {
                            var capproc = Process.GetProcessById(pidint);
                            var iscapture = false;
                            
                            foreach (ProcessModule mod in capproc.Modules)
                            {
                                // If we find amonguscapturedll in the modules, we can be certain
                                // that the located pid is, in fact, an AmongUsCapture process.
                                if (mod.ModuleName == Process.GetCurrentProcess().MainModule.ModuleName)
                                {
                                    iscapture = true;
                                    break;
                                }
                            }

                            if (!iscapture || capproc.HasExited)
                                throw new ArgumentException();
                        }
                        catch (ArgumentException e)
                        {
                            // Process doesn't exist. Clear the file.
                            Console.WriteLine($"Found stale PID file containing {pid}.");
                            File.Delete(Path.Join(Settings.StorageLocation, ".amonguscapture.pid"));
                            _isHostInstance = true;
                        }
                    }
                }

            }

            if (_isHostInstance)
            {
                using (var pidwriter = File.CreateText(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
                {
                    pidwriter.Write(myProcessId);
                }
            }
            

            if (!_isHostInstance) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart) SendToken(args[0]).Wait();

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                result = URIStartResult.PARSE;
            }
            RegisterProtocol();

            return result;
        }


        private static void RegisterProtocol()
        {
            // we really should query the user for this, but since Dialogs appear to be completely fucked, we're going
            // to just install it right now.
            var xdg_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "applications");
            var xdg_file = Path.Join(xdg_path, "aucapture-opener.desktop");

            if (!File.Exists(xdg_file))
            {
                var executingassmb = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Console.WriteLine(executingassmb);
                if (Path.HasExtension("dll"))
                {
                    executingassmb = "dotnet " + executingassmb;
                }
                else
                {
                    executingassmb = Process.GetCurrentProcess().MainModule.FileName;
                    Console.WriteLine(executingassmb);
                }

                var xdg_file_write = new string[]
                {
                    "[Desktop Entry]",
                    "Type=Application",
                    "Name=aucapture URI Handler",
                    $"Exec={executingassmb} %u",
                    "StartupNotify=false",
                    "MimeType=x-scheme-handler/aucapture;"
                };

                using (var file = File.CreateText(xdg_file))
                {
                    foreach (string str in xdg_file_write)
                    {
                        file.WriteLine(str);
                    }
                }

                var xdg_posix = new UnixFileInfo(xdg_file);

                xdg_posix.FileAccessPermissions = FileAccessPermissions.UserReadWriteExecute
                                                  | FileAccessPermissions.GroupRead
                                                  | FileAccessPermissions.GroupExecute
                                                  | FileAccessPermissions.OtherRead
                                                  | FileAccessPermissions.OtherExecute;

                // Finally, register with xdg-mime.

                var xdgproc = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/usr/bin/xdg-mime",
                        Arguments = $"default aucapture-opener.desktop x-scheme-handler/aucapture",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                xdgproc.Start();
                string result = xdgproc.StandardOutput.ReadToEnd();
                xdgproc.WaitForExit();
            }
        }

        public void RemoveHandler()
        {
            var xdg_path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "applications");
            var xdg_file = Path.Join(xdg_path, "aucapture-opener.desktop");
            
            if(File.Exists(xdg_file))
            {
                File.Delete(xdg_file);
            }
        }

        public override async Task<bool> SendToken(string jsonText)
        {
            // Delay the send until we know for certain the server is started.
            // If this isn't a host instance, we don't care.
            while (!_serverIsStarted && _isHostInstance)
                await Task.Delay(500);

            // Send the token via DBus.
            using (Connection conn = new Connection(Address.Session))
            {
                await conn.ConnectAsync();
                
                var _ipclink = conn.CreateProxy<IConnectLink>("org.AmongUsCapture.ConnectLink",
                    "/org/AmongUsCapture/ConnectLink");

                await _ipclink.SendConnectUriAsync(jsonText);
            }

            return true;
        }

        public override Task SendToken(string host, string connectCode)
        {
            var st = new StartToken {ConnectCode = connectCode, Host = host};
            OnTokenEvent(st);
            return Task.CompletedTask;
        }

        public override Task RegisterMinion()
        {
            _serverTask = Task.Factory.StartNew(async () =>
            {
                try
                {
                    using (Connection conn = new Connection(Address.Session))
                    {
                        await conn.ConnectAsync();

                        var obj = new IPCLink();
                        await conn.RegisterObjectAsync(obj);
                        await conn.RegisterServiceAsync("org.AmongUsCapture.ConnectLink",
                            ServiceRegistrationOptions.None);

                        obj.SentLink += RespondToDbus;

                        _serverIsStarted = true;
                        
                        while (!_cancellation.IsCancellationRequested)
                        {
                            _cancellation.Token.ThrowIfCancellationRequested();
                            await Task.Delay(int.MaxValue);
                        }
                    }
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine("Cancel called - terminating DBus loop.");
                }
            });
            return Task.CompletedTask;
        }



        public override Task StartWithToken(string uri)
        {
            OnTokenEvent(StartToken.FromString(uri));
            return Task.CompletedTask;
        }

        public override async Task<bool> Cancel()
        {
            if (!_cancellation.IsCancellationRequested)
            {
                _cancellation.Cancel();
                await CleanPid();
                await _serverTask;
                return true;
            }

            return false;
        }

        private Task CleanPid()
        {
            // Make sure the pidfile is cleaned up if we have one.
            var pidfile = Path.Join(Settings.StorageLocation, ".amonguscapture.pid");

            if (File.Exists(pidfile))
            {
                int pid;
                bool fileread;
                using (var pidread = File.OpenText(Path.Join(Settings.StorageLocation, ".amonguscapture.pid")))
                {
                    fileread = Int32.TryParse(pidread.ReadLine(), out pid);
                }

                if (!fileread)
                {
                    // Bad read, file must be corrupt. Clear pidfile.
                    File.Delete(pidfile);
                }

                if (pid == Process.GetCurrentProcess().Id)
                {
                    // This is our process. Delete file.
                    File.Delete(pidfile);
                }
            }

            return Task.CompletedTask;
        }

        private void RespondToDbus(string signalresponse)
        {
            Settings.conInterface.WriteModuleTextColored("DBus", Color.Silver,
                $"Received DBus Method Call: \"{signalresponse}\"");
            
            var token = StartToken.FromString(signalresponse);

            OnTokenEvent(token);
        }
    }
}