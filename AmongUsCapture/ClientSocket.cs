using System;
using System.Text.Json;
using SocketIOClient;
using System.Drawing;
using MetroFramework;
using TextColorLibrary;

namespace AmongUsCapture
{
    public class ClientSocket
    { 
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Init()
        {
            //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connecting to §1{url}§f...");
            socket = new SocketIO();

            IPCadapter.getInstance().OnToken += OnTokenHandler;
            OnConnected += (sender, e) =>
            {
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Connected successfully!");
                Settings.form.setColor(MetroColorStyle.Green);
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, "Connected successfully!");
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
            };

            OnDisconnected += (sender, e) =>
            {
                Settings.form.setColor(MetroColorStyle.Red);
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"{Color.Red.ToTextColor()}Connection lost!");
                GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
                GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            };
        }

        public void OnTokenHandler(object sender, StartToken token)
        {
            if (socket.Connected)
            {
                socket.DisconnectAsync().ContinueWith((t) =>
                {
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                    this.Connect(token.Host, token.ConnectCode);
                });
            } else
            {
                this.Connect(token.Host, token.ConnectCode);
            }
        }

        private void OnConnectionFailure(AggregateException e = null)
        {
            string message = e != null ? e.Message : "A generic connection error occured.";
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, $"{Color.Red.ToTextColor()}{message}");
        }

        private void Connect(string url, string connectCode)
        {
            try
            {
                socket.ServerUri = new Uri(url);
                socket.ConnectAsync().ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        OnConnectionFailure(t.Exception);
                        return;
                    }
                    OnConnected?.Invoke(this, EventArgs.Empty);
                    SendConnectCode(connectCode);
                });
            } catch (ArgumentNullException) {
                Console.WriteLine("Invalid bot host, not connecting");
            } catch (UriFormatException) {
                Console.WriteLine("Invalid bot host, not connecting");
            }
        }

        public void SendConnectCode(string connectCode, EventHandler callback = null)
        {
            ConnectCode = connectCode;
            socket.EmitAsync("connectCode", ConnectCode).ContinueWith((_) =>
            {
                GameMemReader.getInstance().ForceUpdatePlayers();
                GameMemReader.getInstance().ForceTransmitState();
                GameMemReader.getInstance().ForceTransmitLobby();
                callback?.Invoke(this, new EventArgs());
            });
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Connection code ({Color.Red.ToTextColor()}{connectCode}{UserForm.NormalTextColor.ToTextColor()}) sent to server.");
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
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Room code ({Color.Yellow.ToTextColor()}{e.LobbyCode}{UserForm.NormalTextColor.ToTextColor()}) sent to server.");
        }
    }
}
