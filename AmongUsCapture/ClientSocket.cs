using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        private SocketIO socket;
        private string ConnectCode;

        public void Connect(string url)
        {
            socket = new SocketIO(url);
            /*socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });*/
            socket.OnConnected += (sender, e) =>
            {
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
            };


            socket.ConnectAsync();
        }


        public void SendConnectCode(string connectCode)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connect", ConnectCode).ContinueWith((t) => {
                GameMemReader.getInstance().ForceUpdate();
                GameMemReader.getInstance().ForceTransmitState();
            });
            Program.conInterface.WriteLine($"Connection code ({connectCode}) sent to server.");
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            socket.EmitAsync("state", JsonSerializer.Serialize(e.NewState)); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            socket.EmitAsync("player", JsonSerializer.Serialize(e)); //Makes code wait for socket to emit before closing thread.
        }
    }
}
