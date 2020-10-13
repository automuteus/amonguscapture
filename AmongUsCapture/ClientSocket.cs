using System;
using System.Drawing;
using System.Text.Json;
using AmongUsCapture.TextColorLibrary;
using CaptureGUI;
using SocketIOClient;

namespace AmongUsCapture
{
    public class ClientSocket
    {
        public event EventHandler<ConnectedEventArgs> OnConnected;
        public event EventHandler OnDisconnected;

        private SocketIO socket;
        private string ConnectCode;

        public void Init()
        {
            // Initialize a socket.io connection.
            socket = new SocketIO();

            // Handle tokens from protocol links.
            IPCadapter.getInstance().OnToken += OnTokenHandler;

            // Register handlers for game-state change events.
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
            GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;

            // Handle socket connection events.
            socket.OnConnected += (sender, e) =>
            {
                // Report the connection
                //Settings.form.setConnectionStatus(true);
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan, "Connected successfully!");


                // Alert any listeners that the connection has occurred.
                OnConnected?.Invoke(this, new ConnectedEventArgs() {Uri = socket.ServerUri.ToString()});

                // On each (re)connection, send the connect code and then force-update everything.
                socket.EmitAsync("connectCode", ConnectCode).ContinueWith((_) =>
                {
                    Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                        $"Connection code ({Color.Red.ToTextColor()}{ConnectCode}{MainWindow.NormalTextColor.ToTextColor()}) sent to server.");
                    GameMemReader.getInstance().ForceUpdatePlayers();
                    GameMemReader.getInstance().ForceTransmitState();
                    GameMemReader.getInstance().ForceTransmitLobby();
                });
            };

            // Handle socket disconnection events.
            socket.OnDisconnected += (sender, e) =>
            {
                //Settings.form.setConnectionStatus(false);
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                    $"{Color.Red.ToTextColor()}Connection lost!");

                // Alert any listeners that the disconnection has occured.
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            };
        }

        public void OnTokenHandler(object sender, StartToken token)
        {
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Attempting to connect to host {Color.LimeGreen.ToTextColor()}{token.Host}{Color.White.ToTextColor()} with connect code {Color.Red.ToTextColor()}{token.ConnectCode}{Color.White.ToTextColor()}");
            if (socket.Connected)
                // Disconnect from the existing host...
                socket.DisconnectAsync().ContinueWith((t) =>
                {
                    // ...then connect to the new one.
                    Connect(token.Host, token.ConnectCode);
                });
            else
                // Connect using the host and connect code specified by the token.
                Connect(token.Host, token.ConnectCode);
        }

        private void OnConnectionFailure(AggregateException e = null)
        {
            var message = e != null ? e.Message : "A generic connection error occured.";
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"{Color.Red.ToTextColor()}{message}");
        }

        private void Connect(string url, string connectCode)
        {
            try
            {
                ConnectCode = connectCode;
                socket.ServerUri = new Uri(url);
                if (socket.Connected) socket.DisconnectAsync().Wait();
                socket.ConnectAsync().ContinueWith(t =>
                {
                    if (!t.IsCompletedSuccessfully)
                    {
                        OnConnectionFailure(t.Exception);
                        return;
                    }
                });
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Invalid bot host, not connecting");
            }
            catch (UriFormatException)
            {
                Console.WriteLine("Invalid bot host, not connecting");
            }
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("state",
                JsonSerializer
                    .Serialize(e.NewState)); // could possibly use continueWith() w/ callback if result is needed
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("player",
                JsonSerializer.Serialize(e)); //Makes code wait for socket to emit before closing thread.
        }

        private void JoinedLobbyHandler(object sender, LobbyEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("lobby", JsonSerializer.Serialize(e));
            Settings.conInterface.WriteModuleTextColored("ClientSocket", Color.Cyan,
                $"Room code ({Color.Yellow.ToTextColor()}{e.LobbyCode}{MainWindow.NormalTextColor.ToTextColor()}) sent to server.");
        }

        public class ConnectedEventArgs : EventArgs
        {
            public string Uri { get; set; }
        }
    }
}