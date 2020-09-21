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

            socket.ConnectAsync();

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
            socket.EmitAsync("state", JsonSerializer.Serialize(e.NewState)); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            socket.EmitAsync("player", JsonSerializer.Serialize(e));
        }
    }
}
