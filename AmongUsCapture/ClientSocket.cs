using System;
using System.Text.Json;
using SocketIOClient;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        public event EventHandler OnConnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Connect(string url)
        {
            Program.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connecting to §1{url}§f...");
            socket = new SocketIO(url);
            /*socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });*/
            socket.OnConnected += (sender, e) =>
            {
                Program.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connected successfully!");
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
                this.OnConnected(this, new EventArgs());
            };
            
            socket.OnDisconnected += (sender, e) =>
            {
                Program.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            };

            socket.ConnectAsync();
        }

        public void SendConnectCode(string connectCode)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connect", ConnectCode).ContinueWith((_) =>
            {
                GameMemReader.getInstance().ForceUpdatePlayers();
                GameMemReader.getInstance().ForceTransmitState();
            });
            Program.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connection code (§c{connectCode}§f) sent to server.");
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
