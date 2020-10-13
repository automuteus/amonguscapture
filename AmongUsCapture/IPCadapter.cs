using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using Newtonsoft.Json;
using SharedMemory;

namespace AmongUsCapture
{
    class IPCadapter
    {
        public const string appName = "AmongUsCapture";
        private const string UriScheme = "aucapture";
        private const string FriendlyName = "AmongUs Capture";
        private Mutex mutex;
        private static IPCadapter instance = new IPCadapter();
        public event EventHandler<StartToken> OnToken;
        public static IPCadapter getInstance()
        {
            return instance;
        }

        public URIStartResult HandleURIStart(string[] args)
        {
            var myProcessId = Process.GetCurrentProcess().Id;
            //Process[] processes = Process.GetProcessesByName("AmongUsCapture");
            //foreach (Process p in processes)
            //{
            //if (p.Id != myProcessId)
            //    {
            //        p.Kill();
            //    }
            // }
            Console.WriteLine(Program.GetExecutablePath());

            mutex = new Mutex(true, appName, out var createdNew);
            var wasURIStart = args.Length > 0 && args[0].StartsWith(UriScheme + "://");
            var result = URIStartResult.CONTINUE;

            if (!createdNew) // send it to already existing instance if applicable, then close
            {
                if (wasURIStart) SendToken(args[0]);

                return URIStartResult.CLOSE;
            }
            else if (wasURIStart) // URI start on new instance, continue as normal but also handle current argument
            {
                result = URIStartResult.PARSE;
            }

            RegisterProtocol();

            return result;
        }

        public void SendToken(string host, string connectCode)
        {
            var st = new StartToken {ConnectCode = connectCode, Host = host};
            OnToken?.Invoke(this, st);
        }

        public bool SendToken(string jsonText)
        {
            var rpcGru = new RpcBuffer(appName); //Soup told me not to but its funny
            var RPCresult = rpcGru.RemoteRequest(Encoding.UTF8.GetBytes(jsonText));
            var MinionResponse = Encoding.UTF8.GetString(RPCresult.Data, 0, RPCresult.Data.Length);
            return RPCresult.Success;
        }

        private static void RegisterProtocol()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
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
            }
        }

        public void RegisterMinion()
        {
            var rpcMinion = new RpcBuffer(appName, (msgId, payload) =>
            {
                var serverResponse = "Carbon has a huge pp also this is debug messages.";
                var gotData = Encoding.UTF8.GetString(payload, 0, payload.Length);
                Console.WriteLine($"RPCMinion: Got data: {gotData}");
                OnToken?.Invoke(this, StartToken.FromString(gotData)); //Invoke method and return.
                return Encoding.UTF8.GetBytes(serverResponse);
            });
        }

        public void startWithToken(string uri)
        {
            OnToken?.Invoke(this, StartToken.FromString(uri));
        }
    }

    public enum URIStartResult
    {
        CLOSE,
        PARSE,
        CONTINUE
    }

    public class StartToken : EventArgs
    {
        public string Host { get; set; }
        public string ConnectCode { get; set; }

        public static StartToken FromString(string rawToken)
        {
            try
            {
                Uri uri = new Uri(rawToken);
                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
                bool insecure = (nvc["insecure"] != null && nvc["insecure"] != "false") || uri.Query == "?insecure";
                return new StartToken() { Host = (insecure ? "http://" : "https://") + uri.Authority, ConnectCode = uri.AbsolutePath.Substring(1) };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StartToken();
            }
        }
    }

}
