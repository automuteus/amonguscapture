using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;
using NLog.Fluent;
using Color = System.Drawing.Color;

namespace AmongUsCapture
{
    public class DiscordHandler
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public DiscordSocketClient DClient;
        public event EventHandler<ReadyEventArgs> OnReady;
        
        public async void Init(string DiscordToken)
        {
            Logger.Info("Trying to connect to discord");
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
                Logger.Error(e);
            }
            
        }
        public async void Close()
        {
            if (DClient is not null && (DClient.ConnectionState == ConnectionState.Connected || DClient.ConnectionState == ConnectionState.Connecting))
            {
                Logger.Info("Disconnecting from discord, This may cause undesired behaviour if already connected to a server.");
                
                try
                {
                    DClient.Log -= DClient_Log;
                    DClient.Ready -= DClient_Ready;
                    await DClient.StopAsync();
                }
                catch (Exception e) {
                    Logger.Error(e);
                }

            }
            
            
        }

        private Task DClient_Ready() {
            Logger.Info("Discord connection successful. ID: {ID}, Name: {name}", DClient.CurrentUser.Id, DClient.CurrentUser.Username);
            var args = new ReadyEventArgs {BotID = DClient.CurrentUser.Id};
            OnReady?.Invoke(this, args);
            return Task.CompletedTask;
        }

        private Task DClient_Log(LogMessage arg) {
            Logger.Info("{$arg}", arg);
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
            catch (Exception e) {
                Logger.Error(e);
                return false;
            }
            
        }

        public class ReadyEventArgs : EventArgs
        {
            public ulong BotID { get; set; }
        }
    }
}
