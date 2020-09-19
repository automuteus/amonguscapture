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
        private string GuildID;

        public void Connect(string url, string paramGuildID)
        {
            GuildID = paramGuildID;
            socket = new SocketIO(url);
            /*socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });*/
            socket.OnConnected += (sender, e) =>
            {
                GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
                GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;
                UpdateGuildID(paramGuildID, false);
            };

            socket.ConnectAsync();

            while(true)
            {
                string[] command = Console.ReadLine().Split();
                if (command.Length > 1 && command[0] == "setid")
                {
                    UpdateGuildID(command[1]);
                }
            }
        }

        private void UpdateGuildID(string newGuildId, bool toFile = true)
        {
            GuildID = newGuildId;
            if (toFile) File.WriteAllText("guildid.txt", GuildID);
            socket.EmitAsync("guildID", GuildID).ContinueWith((t) => {
                GameMemReader.getInstance().ForceUpdate();
            }); ;
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
