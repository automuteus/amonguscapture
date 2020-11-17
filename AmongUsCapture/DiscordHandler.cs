using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using AmongUsCapture.TextColorLibrary;
using DSharpPlus;

namespace AmongUsCapture
{
    public class DiscordHandler
    {
        public DiscordClient DClient;
        public event EventHandler<ReadyEventArgs> OnReady;
        public async void Init(string DiscordToken)
        {
            Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                $"{Color.LawnGreen.ToTextColor()}Trying to connect to discord");
            try
            {
                DClient = new DiscordClient(new DiscordConfiguration
                {
                    AutoReconnect = true,
                    Token = DiscordToken,
                    TokenType = TokenType.Bot
                });
                DClient.Ready += DClientOnReady;
                await DClient.ConnectAsync();
            }
            catch (Exception e)
            {
                Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                    $"{Color.Red.ToTextColor()}Error: {e}");
            }
            
        }

        private Task DClientOnReady(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                $"{Color.Aqua.ToTextColor()}Connection successful! ID: {e.Client.CurrentUser.Id}, name: {e.Client.CurrentUser.Username}");
            var args = new ReadyEventArgs {BotID = e.Client.CurrentUser.Id};
            OnReady?.Invoke(this, args);
            return Task.CompletedTask;
        }

        public async Task<bool> UpdateUser(ulong GuildID, ulong UserID, bool? mute, bool? deafen)
        {
            try
            {
                var guild = await DClient.GetGuildAsync(GuildID);
                var member = await guild.GetMemberAsync(UserID);
                await member.ModifyAsync(null, null, mute, deafen, null, null);
                return true;
            }
            catch (Exception e)
            {
                Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                    $"{Color.Red.ToTextColor()} Error: {e}");
                return false;
            }
            
        }

        public class ReadyEventArgs : EventArgs
        {
            public ulong BotID { get; set; }
        }
    }
}
