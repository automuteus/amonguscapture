using System;
using System.Drawing;
using Newtonsoft.Json;
using NLog.Fluent;
using SocketIOClient;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AmongUsCapture
{
    public class ClientSocket
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public event EventHandler<ConnectedEventArgs> OnConnected;
        public event EventHandler OnDisconnected;

        private SocketIO socket;
        private string ConnectCode;
        private DiscordHandler handler;
        public void Init()
        {
            // Initialize a socket.io connection.
            socket = new SocketIO();
            // Handle tokens from protocol links.

            // Register handlers for game-state change events.
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
            GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
            GameMemReader.getInstance().GameOver += GameOverHandler;



            // Handle socket connection events.
            socket.OnConnected += (sender, e) =>
            {
                // Report the connection
                //Settings.form.setConnectionStatus(true);
                Logger.Info("Connected successfully");
                
                // Alert any listeners that the connection has occurred.
                OnConnected?.Invoke(this, new ConnectedEventArgs() {Uri = socket.ServerUri.ToString()});

                // On each (re)connection, send the connect code and then force-update everything.
                socket.EmitAsync("connectCode", ConnectCode).ContinueWith((_) => {
                    Logger.Debug("Connect code {ConnectCode} sent to server", ConnectCode);
                    GameMemReader.getInstance().ForceUpdatePlayers();
                    GameMemReader.getInstance().ForceTransmitState();
                    GameMemReader.getInstance().ForceTransmitLobby();
                });
                if (!(this.handler is null))
                {
                    socket.EmitAsync("botID", handler.DClient.CurrentUser.Id);
                    Logger.Info("Sent BotID: {BotID} to server", handler.DClient.CurrentUser.Id);
                }
            };
            socket.On("modify", response =>
            {
                string jsonRequest = response.GetValue(0).ToString();
                UpdateRequest update = JsonConvert.DeserializeObject<UpdateRequest>(jsonRequest);
                Console.Write(JsonConvert.SerializeObject(update, Formatting.Indented));
                if (handler is null)
                {
                    socket.EmitAsync("taskFailed", update.TaskId);
                    return;
                };
                var paramString = "";
                Logger.Debug("Recieved task: {task}", update);
                handler.UpdateUser(update.GuildId, update.UserId, update.Parameters.Mute, update.Parameters.Deaf).ContinueWith(x => {
                    Logger.Debug("Task {TaskID} {$result}", update.TaskId, x.Result ? "completed successfully" : "failed");
                    socket.EmitAsync(x.Result ? "taskComplete" : "taskFailed", update.TaskId);
                }).Wait();

            });
            socket.On("killself", response => // O: Suicide!! o no!
            {
                Environment.Exit(0);
            });

            // Handle socket disconnection events.
            socket.OnDisconnected += (sender, e) =>
            {
                //Settings.form.setConnectionStatus(false);
                //Settings.conInterface.WriteTextFormatted($"[§bClientSocket§f] Lost connection!");
                Logger.Info("Connection lost");
                
                // Alert any listeners that the disconnection has occured.
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            };

            socket.On("requestdata", OnRequestData);
        }

        private void OnRequestData(SocketIOResponse resp)
        {
            int requestedData = resp.GetValue<int>();
            if ((requestedData & (int) GameDataType.GameState) > 0)
            {
                GameMemReader.getInstance().ForceTransmitState();
            }
            if ((requestedData & (int)GameDataType.LobbyCode) > 0)
            {
                GameMemReader.getInstance().ForceTransmitLobby();
            }
            if ((requestedData & (int)GameDataType.Players) > 0)
            {
                GameMemReader.getInstance().ForceUpdatePlayers();
            }
        }

        public void AddHandler(DiscordHandler handler)
        {
            this.handler = handler;
            if (socket.Connected)
            {
                try
                {
                    socket.EmitAsync("botID", handler.DClient.CurrentUser.Id);
                    Logger.Info("Sent BotID: {BotID}", handler.DClient.CurrentUser.Id);
                    
                }
                catch
                {
                    this.handler = null;
                }
            }
            
        }
        private void OnConnectionFailure(AggregateException e = null)
        {
            var message = e != null ? e.Message : "A generic connection error occured.";
            Logger.Error(e.Message);
        }

        public void Connect(string url, string connectCode)
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
            Logger.Debug("Room code {roomCode} sent to server", e.LobbyCode);
        }

        private void GameOverHandler(object sender, GameOverEventArgs e)
        {
            if (!socket.Connected) return;
            socket.EmitAsync("gameover",
                JsonSerializer
                    .Serialize(e)); // could possibly use continueWith() w/ callback if result is needed
        }

        public class ConnectedEventArgs : EventArgs
        {
            public string Uri { get; set; }
        }
    }

    enum GameDataType
    {
        GameState = 1,
        Players = 2,
        LobbyCode = 4
    }
}