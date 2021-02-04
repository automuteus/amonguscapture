using System;
using System.Drawing;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace AmongUsCapture
{
    public enum EventName
    {
        state,
        player,
        lobby,
        gameover
    }
    public class WebsocketEvent
    {
        public EventName EventID { get; set; }
        public string EventData { get; set; }

        public WebsocketEvent(EventName eventId, object eventData)
        {
            EventID = eventId;
            EventData = JsonConvert.SerializeObject(eventData);
        }
    }
    
    public class Regrets : WebSocketBehavior
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override void OnOpen ()
        {
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
            GameMemReader.getInstance().JoinedLobby += JoinedLobbyHandler;
            GameMemReader.getInstance().GameOver += GameOverHandler;
            Logger.Debug("New connection to API server");
        }

        private void GameOverHandler(object? sender, GameOverEventArgs e)
        {
            Send(JsonConvert.SerializeObject(new WebsocketEvent(EventName.gameover, e)));
        }

        private void JoinedLobbyHandler(object? sender, LobbyEventArgs e)
        {
            Send(JsonConvert.SerializeObject(new WebsocketEvent(EventName.lobby, e)));
        }

        private void PlayerChangedHandler(object? sender, PlayerChangedEventArgs e)
        {
            Send(JsonConvert.SerializeObject(new WebsocketEvent(EventName.player, e)));
        }

        private void GameStateChangedHandler(object? sender, GameStateChangedEventArgs e)
        {
            Send(JsonConvert.SerializeObject(new WebsocketEvent(EventName.state, e)));
        }

        protected override void OnClose(CloseEventArgs e)
        {
            GameMemReader.getInstance().GameStateChanged -= GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged -= PlayerChangedHandler;
            GameMemReader.getInstance().JoinedLobby -= JoinedLobbyHandler;
            GameMemReader.getInstance().GameOver -= GameOverHandler;
            Logger.Info("Connection terminated on API server");
        }
    }
    public class ServerSocket
    {
        public static ServerSocket instance = new ServerSocket();
        private WebSocketServer _wssv;


        public void Start()
        {
            _wssv ??= new WebSocketServer(42069);
            _wssv.AddWebSocketService<Regrets>("/api");
            _wssv.Start();
        }

        public void Stop()
        {
            _wssv ??= new WebSocketServer(42069);
            _wssv.RemoveWebSocketService("/api");
            _wssv.Stop();
        }
    }
}