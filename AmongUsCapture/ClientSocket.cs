using System;
using System.Text.Json;
using SocketIOClient;
using System.Drawing;
using System.Net.Sockets;
using MetroFramework;
using TextColorLibrary;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        public event EventHandler OnConnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Init()
        {
            //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connecting to §1{url}§f...");
            socket = new SocketIO();

            IPCadapter.getInstance().OnToken += OnTokenHandler;
            socket.OnConnected += (sender, e) =>
            {
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connected successfully!");
                Settings.form.setColor(MetroColorStyle.Green);
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, "Connected successfully!");
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
                var OnOnConnected = this.OnConnected;
                if (OnOnConnected != null) OnOnConnected(this, new EventArgs());
            };

            socket.OnDisconnected += (sender, e) =>
            {
                Settings.form.setColor(MetroColorStyle.Red);
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"{Color.Red.ToTextColor()}Connection lost!");
                GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            };
        }

        private void OnTokenHandler(object sender, StartToken token)
        {
            
            this.Connect(token.Host, token.ConnectCode);
        }

        public void Connect(string url, string connectCode)
        {
            try
            {
                socket.ServerUri = new Uri(url);
                if (socket.Connected)
                {
                    SendConnectCode(connectCode);
                }

                if (socket.Disconnected)
                {
                    socket.ConnectAsync().ContinueWith(t =>
                    {
                        SendConnectCode(connectCode);
                    });
                }
                
            } catch (ArgumentNullException) {
                Console.WriteLine("Invalid bot host, not connecting");
            } catch (UriFormatException) {
                Console.WriteLine("Invalid bot host, not connecting");
            }
            
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
                GameMemReader.getInstance().ForceTransmitState();
                GameMemReader.getInstance().ForceTransmitLobby();
                GameMemReader.getInstance().ForceUpdatePlayers();
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
            GameMemReader.getInstance().ForceUpdatePlayers();
            socket.EmitAsync("lobby", JsonSerializer.Serialize(e));
        }

    }
}
