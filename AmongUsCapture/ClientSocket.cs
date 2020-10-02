using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;
using System.Drawing;
using TextColorLibrary;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        private SocketIO socket;
        private string ConnectCode;

        public void Init()
        {
            socket = new SocketIO();

            IPCadapter.getInstance().OnToken += OnTokenHandler;

            socket.OnConnected += (sender, e) =>
            {
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
            };

            socket.OnDisconnected += (sender, e) =>
            {
                GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            };
        }

        private void OnTokenHandler(object sender, StartToken token)
        {
            if (socket.Connected)
            {
                socket.DisconnectAsync().ContinueWith((t) =>
                {
                    this.Connect(token.Host, token.ConnectCode);
                });
            } else
            {
                this.Connect(token.Host, token.ConnectCode);
            }
        }

        public void Connect(string url, string connectCode)
        {
            try
            {
                socket.ServerUri = new Uri(url);
                socket.ConnectAsync().ContinueWith(t =>
                {
                    SendConnectCode(connectCode);
                });
            } catch (ArgumentNullException) {
                Console.WriteLine("Invalid bot host, not connecting");
            } catch (UriFormatException) {
                Console.WriteLine("Invalid bot host, not connecting");
            }
        }

        public void SendConnectCode(string connectCode)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connect", ConnectCode).ContinueWith((t) => {
                GameMemReader.getInstance().ForceUpdatePlayers();
                GameMemReader.getInstance().ForceTransmitState();
            });
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"Connection code ({Color.Red.ToTextColor()}{connectCode}{UserForm.NormalTextColor.ToTextColor()}) sent to server.");
            //Program.conInterface.WriteModuleTextColored("GameMemReader", System.Drawing.Color.Aqua, $"Connection code ({connectCode}) sent to server.");
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
