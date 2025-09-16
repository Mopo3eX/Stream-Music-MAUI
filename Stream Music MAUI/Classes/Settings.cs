using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    public class Settings
    {
        public string BotUserName { get; set; }
        public string BotToken { get; set; }
        public string ChannelName { get; set; }
        public string ObsWebSocketURL { get; set; }
        public string GDITextName { get; set; }
        public string GDITextNameNext { get; set; }
        public string ObsWebSocketPassword { get; set; }
        public int AudioDevice { get; set; } = -1;
        public string OBSDir {  get; set; }
        public string VTMDir { get; set; }
        public double Volume { get; set; } = 0;
        public bool MusicAddOnChat { get; set; } = false;
    }
}
