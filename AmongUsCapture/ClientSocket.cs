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
        private string guildID;

        public void Connect(string url, string paramGuildID)
        {
            guildID = paramGuildID;
            socket = new SocketIO(url);
            /*socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });*/
            socket.OnConnected += async (sender, e) =>
            {
                await socket.EmitAsync("guildID", guildID);
            };
            socket.ConnectAsync().GetAwaiter().GetResult();

            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;

            while(true)
            {
                string[] command = Console.ReadLine().Split();
                if (command.Length > 1 && command[0] == "setid")
                {
                    guildID = command[1];
                    File.WriteAllText("guildid.txt", guildID);
                    socket.EmitAsync("guildID", guildID);
                }
            }
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
