using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;

namespace AmongUsCapture
{
    class ClientSocket
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

            socket.ConnectAsync().GetAwaiter().GetResult(); // Make code wait for socket to fully connect before trying to send stuff.

            while(true)
            {
                string[] command = Console.ReadLine().Split();
                if (command.Length > 1 && command[0] == "connect")
                {
                    SendConnectCode(command[1]);
                }
            }
        }

        private void SendConnectCode(string connectCode)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connect", ConnectCode).ContinueWith((t) => {
                GameMemReader.getInstance().ForceUpdate();
                GameMemReader.getInstance().ForceTransmitState();
            });
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            socket.EmitAsync("state", JsonSerializer.Serialize(e.NewState)).GetAwaiter().GetResult(); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            socket.EmitAsync("player", JsonSerializer.Serialize(e)).GetAwaiter().GetResult(); //Makes code wait for socket to emit before closing thread.
        }
    }
}
