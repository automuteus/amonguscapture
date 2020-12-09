using System;
using System.Threading.Tasks;
using Tmds.DBus;

namespace AmongUsCapture.DBus
{
    [DBusInterface("org.AmongUsCapture.ConnectLink")]
    public interface IConnectLink : IDBusObject
    {
        Task SendConnectUriAsync(string uri);
    }
    
    public class IPCLink : IConnectLink
    {
        public ObjectPath ObjectPath => new ObjectPath("/org/AmongUsCapture/ConnectLink");
        public event Action<string> SentLink;
        
        public Task SendConnectUriAsync(string uri)
        {
            // Call event and send URI.
            SentLink?.Invoke(uri);
            return Task.CompletedTask;
        }
    }
}