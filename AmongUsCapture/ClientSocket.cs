using System.Text.Json;
using System.Threading.Tasks;
using SocketIOClient;

namespace AmongUsCapture
{
    class ClientSocket
    { 
        private SocketIO socket;

        public async Task Run(string url, string guildID)
        {
            GameMemReader.getInstance().GameStateChanged += GameStateChangedHandler;
            GameMemReader.getInstance().PlayerChanged += PlayerChangedHandler;

            this.socket = new SocketIO(url);
            socket.On("hi", response =>
            {
                string text = response.GetValue<string>();
            });
            socket.OnConnected += async (sender, e) =>
            {
                await socket.EmitAsync("guildID", guildID);
            };
            await socket.ConnectAsync();
        }

        private void GameStateChangedHandler(object sender, GameStateChangedEventArgs e)
        {
            socket.EmitAsync("state", JsonSerializer.Serialize(e.NewState)).GetAwaiter().GetResult();
        }

        private void PlayerChangedHandler(object sender, PlayerChangedEventArgs e)
        {
            socket.EmitAsync("player", JsonSerializer.Serialize(e)).GetAwaiter().GetResult();
        }
    }
}
