using System;
using System.Text.Json;
using SocketIOClient;
using System.Drawing;
using TextColorLibrary;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        public event EventHandler OnConnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Connect(string url)
        {
            //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connecting to §1{url}§f...");
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"Connecting to {url}...");
            socket = new SocketIO(url);
            /*socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });*/
            socket.OnConnected += (sender, e) =>
            {
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connected successfully!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, "Connected successfully!");
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
                this.OnConnected(this, new EventArgs());
            };
            
            socket.OnDisconnected += (sender, e) =>
            {
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"{Color.Red.ToTextColor()}Connection lost!");
                GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            };

            socket.ConnectAsync();
        }

        public void SendConnectCode(string connectCode)
        {
            SendConnectCode(connectCode, null);
        }

        public void SendConnectCode(string connectCode, EventHandler callback)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connect", ConnectCode).ContinueWith((_) =>
            {
                GameMemReader.getInstance().ForceUpdatePlayers();
                GameMemReader.getInstance().ForceTransmitState();
                if (callback != null)
                {
                    callback.Invoke(this, new EventArgs());
                }
            });
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"Connection code ({Color.Red.ToTextColor()}{connectCode}{UserForm.NormalTextColor.ToTextColor()}) sent to server.");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", System.Drawing.Color.Aqua, $"Connection code ({connectCode}) sent to server.");
        }

        public void SendRoomCode(LobbyEventArgs args)
        {
            socket.EmitAsync("lobby", JsonSerializer.Serialize(args));
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"Room code ({Color.Yellow.ToTextColor()}{args.LobbyCode}{UserForm.NormalTextColor.ToTextColor()}) sent to server.");
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            socket.EmitAsync("state", JsonSerializer.Serialize(e.NewState)); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            socket.EmitAsync("player", JsonSerializer.Serialize(e)); //Makes code wait for socket to emit before closing thread.
        }

        private void JoinedLobbyHandler(object sender, LobbyEventArgs e)
        {
            socket.EmitAsync("lobby", JsonSerializer.Serialize(e));
        }

    }
}
