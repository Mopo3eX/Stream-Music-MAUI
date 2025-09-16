using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    class VTM
    {
        public static bool IsStarted()
        {
            if (MainPage.Settings == null)
                return false;
            if (String.IsNullOrEmpty(MainPage.Settings.VTMDir))
                return false;
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(MainPage.Settings.VTMDir)).Length > 0)
                return true;
            return false;
        }
    }
}
