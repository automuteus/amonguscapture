using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Web;
using AmongUsCaptureConsole.IPC.RpcBuffer;

namespace AmongUsCaptureConsole.IPC
{
    abstract class IpcAdapter
    {
        public const string appName = "AmongUsCapture";
        protected const string UriScheme = "aucapture";
        protected const string FriendlyName = "AmongUsCapture";
        protected Mutex mutex;
        private static IpcAdapter instance;

        public static IpcAdapter getInstance()
        {
            return instance ??= new IpcAdapterRpcBuffer();
        }
        public event EventHandler<StartTokenEventArgs> OnToken;
        protected virtual void OnTokenEvent(StartTokenEventArgs e)
        {
            // Safely raise the event for all subscribers
            OnToken?.Invoke(this, e);
        }
        
        public abstract URIStartResult HandleURIStart(string[] args);
        
        public abstract void SendToken(string jsonText);
        public abstract void SendToken(string host, string connectCode);

        public abstract void RegisterMinion();
    }
    
    
    public enum URIStartResult
    {
        CLOSE,
        PARSE,
        CONTINUE
    }

    public class StartTokenEventArgs : EventArgs
    {
        public string Host { get; set; }
        public string ConnectCode { get; set; }

        public static StartTokenEventArgs FromString(string rawToken)
        {
            try
            {
                rawToken = new string(rawToken.Where(c => !char.IsControl(c)).ToArray());
                Uri uri = new Uri(rawToken);
                NameValueCollection nvc = HttpUtility.ParseQueryString(uri.Query);
                bool insecure = (nvc["insecure"] != null && nvc["insecure"] != "false") || uri.Query == "?insecure";
                return new StartTokenEventArgs() { Host = (insecure ? "http://" : "https://") + uri.Authority, ConnectCode = uri.AbsolutePath.Substring(1) };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new StartTokenEventArgs();
            }
        }
    }
}
