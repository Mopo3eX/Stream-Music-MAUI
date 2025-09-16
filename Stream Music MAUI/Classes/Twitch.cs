using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;

namespace Stream_Music_MAUI.Classes
{
    public class Twitch
    {
        public TwitchClient Client = new TwitchClient();

        public bool Connected = false;
        private string ChannelName;
        private string Token;
        private string UserName;
        private Log LogSystem;
        private ConnectionCredentials ConnectionCredentials;
        
        public Twitch(Log logSystem, string channelName,string userName, string token)
        {
            ChannelName = channelName;
            Token = token;
            LogSystem = logSystem;
            Client.OnLog += Client_OnLog;
            Client.OnJoinedChannel += Client_OnJoinedChannel;
            Client.OnModeratorJoined += Client_OnModeratorJoined;
            Client.OnUserJoined += Client_OnUserJoined;
            Client.OnMessageReceived += Client_OnMessageReceived;
            Client.OnMessageSent += Client_OnMessageSent;
            Client.OnUserLeft += Client_OnUserLeft;
            Client.OnModeratorLeft += Client_OnModeratorLeft;
            Client.OnConnected += Client_OnConnected;
            Client.OnDisconnected += Client_OnDisconnected;
            ConnectionCredentials connectionCredentials = new ConnectionCredentials(ChannelName, Token);
            ConnectionCredentials = connectionCredentials;
            Client.Initialize(ConnectionCredentials, ChannelName);
            Client.Connect();
            AutoMessage(ChannelName);
        }
        bool Start = true;
        private async Task AutoMessage(string Channel)
        {
            while (true)
            {
                if (MessageCounter > 5 || Start)
                {
                    Start = false;
                    MessageCounter = 0;
                    await Task.Delay(new TimeSpan(0, 10, 0));
                    if(Client != null && Connected && MainPage.Settings.MusicAddOnChat)
                        Client.SendMessage(Channel, "🎵 Теперь вы можете добавлять музыку на стрим! \r\nЧтобы заказать трек, напишите в чат: \r\n!addmusic <ссылка на YouTube>");
                }
                await Task.Delay(new TimeSpan(0, 0, 1));
            }
        }
        public List<string> OnlineUsers = new List<string>();
        public int MessageCounter = 0;
        private void Client_OnDisconnected(object? sender, TwitchLib.Communication.Events.OnDisconnectedEventArgs e)
        {
            Connected = false;
            LogSystem.DebugLog($"Disconnected");
        }

        private void Client_OnConnected(object? sender, TwitchLib.Client.Events.OnConnectedArgs e)
        {
            Connected = true;
            Client.SendRaw("CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership");
            //Client.SendRaw("CAP REQ :twitch.tv/membership");
            LogSystem.DebugLog($"{e.BotUsername} is connected to {e.AutoJoinChannel}");
        }

        private void Client_OnModeratorLeft(object? sender, TwitchLib.Client.Events.OnModeratorLeftArgs e)
        {
            LogSystem.InfoLog($"[Moderator] {e.Username} is Left");
        }

        private void Client_OnUserLeft(object? sender, TwitchLib.Client.Events.OnUserLeftArgs e)
        {
            LogSystem.InfoLog($"{e.Username} is left");
            if (OnlineUsers.Contains(e.Username))
            {
                OnlineUsers.Remove(e.Username);
                MainPage.Instance.UpdateUsers();
                
            }
        }

        private void Client_OnMessageSent(object? sender, TwitchLib.Client.Events.OnMessageSentArgs e)
        {
            LogSystem.InfoLog($"[Message Sent] {e.SentMessage.DisplayName}: {e.SentMessage.Message}");
        }

        private void Client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (e.ChatMessage.Message.ToLower().StartsWith("!addmusic "))
            {
                MainPage.Instance.OnAddMusic(sender, e);
            }
            MessageCounter++;
            LogSystem.InfoLog($"[Message Received] {e.ChatMessage.DisplayName}: {e.ChatMessage.Message}");
        }

        private void Client_OnUserJoined(object? sender, TwitchLib.Client.Events.OnUserJoinedArgs e)
        {
            LogSystem.InfoLog($"{e.Username} is joined");
            if(!OnlineUsers.Contains(e.Username))
            {
                OnlineUsers.Add(e.Username);
                
                MainPage.Instance.UpdateUsers();
            }
        }

        private void Client_OnModeratorJoined(object? sender, TwitchLib.Client.Events.OnModeratorJoinedArgs e)
        {
            LogSystem.InfoLog($"[Moderator] {e.Username} is joined");
        }

        private void Client_OnJoinedChannel(object? sender, TwitchLib.Client.Events.OnJoinedChannelArgs e)
        {
            LogSystem.InfoLog($"Bot joined to {e.Channel}");
            
        }

        private void Client_OnLog(object? sender, TwitchLib.Client.Events.OnLogArgs e)
        {
            LogSystem.DebugLog(e.Data);
        }
    }
}
