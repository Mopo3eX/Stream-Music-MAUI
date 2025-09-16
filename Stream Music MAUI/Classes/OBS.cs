using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    public class OBS
    {
        private OBSWebsocket obs;
        private bool Connected = false;
        private bool Connecting = false;
        private bool ForceDisconnect = false;
        private Log LogSystem;
        private string Url;
        private string Password;
        public bool IsConnected
        {
            get
            {
                return obs.IsConnected;
            }
        }
        public OBS(Log logSystem,string url,string password)
        {
            Url = url;
            Password = password;
            LogSystem = logSystem;
            obs = new OBSWebsocket();
            obs.Connected += Obs_Connected;
            obs.Disconnected += Obs_Disconnected;
            Connect();
        }

        public async Task Connect()
        {
            if (ForceDisconnect || Connecting)
                return;
            Connecting=true;
            LogSystem.InfoLog("Подключаемся к OBS");
            
            obs.ConnectAsync(Url, Password);
            Connecting = false;
        }

        public static bool IsStarted()
        {
            if(MainPage.Settings== null)
                return false;
            if(String.IsNullOrEmpty(MainPage.Settings.OBSDir))
                return false;
            if(Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainPage.Settings.OBSDir)).Length > 0)
                return true;
            return false;
        }

        private void Obs_Disconnected(object? sender, OBSWebsocketDotNet.Communication.ObsDisconnectionInfo e)
        {
            if (ForceDisconnect == false)
            {
                Connected = false;
                Connect();
                LogSystem.InfoLog("OBS отключился, пробуем переподключиться...");
            }
            
        }
        public void Disconnect()
        {
            ForceDisconnect = true;
            obs.Disconnect();
        }
        public void WaitOBS()
        {
            ForceDisconnect = false;
            Connected = false;
        }
        private void Obs_Connected(object? sender, EventArgs e)
        {
            Connected=true;
            LogSystem.InfoLog("OBS подключился");
        }
        public bool UpdateGDIText(string ItemName, string Text)
        {
            if(IsConnected == false) return false;
            try
            {
                var settings = new JObject
                {
                    { "text", Text }

                };
                obs.SetInputSettings(ItemName, settings, false);
                return true;
            }
            catch (Exception er)
            {
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                LogSystem.ErrorLog(er.Message,er.StackTrace);
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                return false;
            }
        }
    }
}
