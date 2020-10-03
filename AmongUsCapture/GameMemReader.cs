using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TextColorLibrary;

namespace AmongUsCapture
{
    public enum GameState
    {
        LOBBY,
        TASKS,
        DISCUSSION,
        MENU,
        UNKNOWN
    }
    class GameMemReader
    {
        private static GameMemReader instance = new GameMemReader();
        private bool shouldForceUpdatePlayers = false;
        private bool shouldForceTransmitState = false;
        private bool shouldTransmitLobby = false;
        private IGameOffsets _gameOffsets = Settings.GameOffsets;
        public static GameMemReader getInstance()
        {
            return instance;
        }
        public event EventHandler<GameStateChangedEventArgs> GameStateChanged;

        public event EventHandler<PlayerChangedEventArgs> PlayerChanged;

        public event EventHandler<ChatMessageEventArgs> ChatMessageAdded;

        public event EventHandler<LobbyEventArgs> JoinedLobby;


        public Dictionary<string, PlayerInfo> oldPlayerInfos = new Dictionary<string, PlayerInfo>(10); // Important: this is making the assumption that player names are unique. They are, but for better tracking of players and to eliminate any ambiguity the keys of this probably need to be the players' network IDs instead
        public Dictionary<string, PlayerInfo> newPlayerInfos = new Dictionary<string, PlayerInfo>(10); // container for new player infos. Also has capacity 10 already assigned so no internal resizing of the data structure is needed

        private IntPtr GameAssemblyPtr = IntPtr.Zero;
        private GameState oldState = GameState.UNKNOWN;
        private bool exileCausesEnd = false;

        private int prevChatBubsVersion;

        public void RunLoop()
        {
            while (true)
            {
                Settings.conInterface.WriteModuleTextColored("GameMemReader", Color.Red, UserForm.getRainbowText("We don't support cracked versions. Pay the $5 to the sloth!"));
                Thread.Sleep(1000);
            }
        }

        public void ForceUpdatePlayers()
        {
            this.shouldForceUpdatePlayers = true;
        }

        public void ForceTransmitState()
        {
            this.shouldForceTransmitState = true;
        }
    }

    public class GameStateChangedEventArgs : EventArgs
    {
        public GameState NewState { get; set; }
    }

    public enum PlayerAction
    {
        Joined,
        Left,
        Died,
        ChangedColor,
        ForceUpdated,
        Disconnected,
        Exiled
    }

    public enum PlayerColor
    {
        Red = 0,
        Blue = 1,
        Green = 2,
        Pink = 3,
        Orange = 4,
        Yellow = 5,
        Black = 6,
        White = 7,
        Purple = 8,
        Brown = 9,
        Cyan = 10,
        Lime = 11
    }

    public enum PlayRegion
    {
        NorthAmerica = 0,
        Asia = 1,
        Europe = 2
    }

    public class PlayerChangedEventArgs : EventArgs
    {
        public PlayerAction Action { get; set; }
        public string Name { get; set; }
        public bool IsDead { get; set; }
        public bool Disconnected { get; set; }
        public PlayerColor Color { get; set; }
    }

    public class ChatMessageEventArgs : EventArgs
    {
        public string Sender { get; set; }
        public PlayerColor Color {get; set;}
        public string Message { get; set; }
    }

    public class LobbyEventArgs : EventArgs
    {
        public string LobbyCode { get; set; }
        public PlayRegion Region { get; set; }
    }
}
