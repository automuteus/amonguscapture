using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#if BUILT_FOR_WINDOWS
using Microsoft.Win32;
#endif

namespace AmongUsCaptureConsole.IPC.RpcBuffer
{
    class IpcAdapterRpcBuffer : IpcAdapter
    {
        public override URIStartResult HandleURIStart(string[] args)
        {
            Console.WriteLine(Program.GetExecutablePath());

            mutex = new Mutex(true, appName, out var createdNew);
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!createdNew) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart)
                    SendToken(args[0]);

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                result = URIStartResult.PARSE;
            }

            RegisterProtocol();

            return result;
        }

        public override void SendToken(string host, string connectCode)
        {
            var st = new StartTokenEventArgs {ConnectCode = connectCode, Host = host};
            OnTokenEvent(st);
        }

        public override void SendToken(string jsonText)
        {
            var rpcGru = new SharedMemory.RpcBuffer(appName); //Soup told me not to but its funny
            var RPCresult = rpcGru.RemoteRequest(Encoding.UTF8.GetBytes(jsonText));
            Encoding.UTF8.GetString(RPCresult.Data, 0, RPCresult.Data.Length);
        }

        private static void RegisterProtocol()
        {
#if BUILT_FOR_WINDOWS
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            // Literally code that only works under Windows. This isn't even included with the .NET Core 3 Linux runtime.
            // Consider handling protocol registration outside of this library.
            using var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme);

            // Replace typeof(App) by the class that contains the Main method or any class located in the project that produces the exe.
            // or replace typeof(App).Assembly.Location by anything that gives the full path to the exe
            var applicationLocation = Program.GetExecutablePath();

            key.SetValue("", "URL:" + FriendlyName);
            key.SetValue("URL Protocol", "");

            using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
            {
                defaultIcon.SetValue("", applicationLocation + ",1");
            }

            using (var commandKey = key.CreateSubKey(@"shell\open\command"))
            {
                commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
            }
#endif
        }

        public override void RegisterMinion()
        {
            new SharedMemory.RpcBuffer(appName, (msgId, payload) =>
            {
                var serverResponse = "Carbon has a huge pp also this is debug messages.";
                var gotData = Encoding.UTF8.GetString(payload, 0, payload.Length);
                Console.WriteLine($"RPCMinion: Got data: {gotData}");
                OnTokenEvent(StartTokenEventArgs.FromString(gotData)); //Invoke method and return.
                return Encoding.UTF8.GetBytes(serverResponse);
            });
        }
    }
    
}
