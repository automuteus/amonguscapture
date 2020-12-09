using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AmongUsCapture.TextColorLibrary;
using Discord;
using Discord.WebSocket;
using Color = System.Drawing.Color;

namespace AmongUsCapture
{
    public class DiscordHandler
    {
        public DiscordSocketClient DClient;
        public event EventHandler<ReadyEventArgs> OnReady;
        public async void Init(string DiscordToken)
        {
            Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                $"{Color.LawnGreen.ToTextColor()}Trying to connect to discord");
            try
            {
                DClient = new DiscordSocketClient();
                DClient.Log += DClient_Log;
                DClient.Ready += DClient_Ready;
                await DClient.LoginAsync(TokenType.Bot, DiscordToken, true);
                await DClient.StartAsync();
            }
            catch (Exception e)
            {
                Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                    $"{Color.Red.ToTextColor()}Error: {e}");
            }
            
        }

        private Task DClient_Ready()
        {
            Settings.conInterface.WriteModuleTextColored("DiscordHandler", Color.Red,
                $"{Color.Aqua.ToTextColor()}Connection successful! ID: {DClient.CurrentUser.Id}, name: {DClient.CurrentUser.Username}");
            var args = new ReadyEventArgs {BotID = DClient.CurrentUser.Id};
            OnReady?.Invoke(this, args);
            return Task.CompletedTask;
        }

        private Task DClient_Log(LogMessage arg)
        {
            Settings.conInterface.WriteModuleTextColored("Discord.Net", Color.Red, $"{arg.ToString()}");
            return Task.CompletedTask;
        }

        public async Task<bool> UpdateUser(ulong GuildID, ulong UserID, bool mute, bool deafen)
        {
            try
            {
                var guild = DClient.GetGuild(GuildID);
                var member = guild.GetUser(UserID);
                TaskStatus s = TaskStatus.WaitingToRun;
                return await member.ModifyAsync(x =>
                {
                    x.Deaf = deafen;
                    x.Mute = mute;
                }, new RequestOptions
                {
                    RetryMode = RetryMode.Retry502,
                    Timeout = 200
                }).ContinueWith(x =>
                {
                    Console.WriteLine($"Status: {x.Status}, Exception: {x.Exception}, Faulted: {x.IsFaulted}");
                    return !x.IsFaulted;
                });
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
