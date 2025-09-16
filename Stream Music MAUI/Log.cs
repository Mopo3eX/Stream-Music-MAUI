using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Stream_Music_MAUI
{
    public class Log
    {
        public static MainPage MainPage { get; set; }
        public Log(MainPage Form)
        {
            MainPage = Form;
        }
        string FormatDateTime = "yyyy-MM-dd HH:mm:ss";
        public void ErrorLog(string message,string StackTrace=null)
        {
            string forFull = "";
            if (StackTrace != null)
                forFull = $"[ERROR][{DateTime.Now.ToString(FormatDateTime)}] {message}\r\nStack Trace:\r\n{StackTrace}\r\n";
            else
                forFull = $"[ERROR][{DateTime.Now.ToString(FormatDateTime)}] {message}\r\n";
            MainPage.OnLog(MainPage.LogType.Error, message);
            File.AppendAllText("error.log", forFull);
            FullLog(forFull);
        }
        public void WarningLog(string message)
        {
            string forFull = $"[WARN][{DateTime.Now.ToString(FormatDateTime)}] {message}\r\n";
            MainPage.OnLog(MainPage.LogType.Warning, message);
            File.AppendAllText("warn.log", forFull);
            FullLog(forFull);
        }
        public void InfoLog(string message)
        {
            string forFull = $"[INFO][{DateTime.Now.ToString(FormatDateTime)}] {message}\r\n";
            MainPage.OnLog(MainPage.LogType.Info, message);
            File.AppendAllText("info.log", forFull);
            FullLog(forFull);
        }
        public void DebugLog(string message)
        {
            string forFull = $"[DEBUG][{DateTime.Now.ToString(FormatDateTime)}] {message}\r\n";
            MainPage.OnLog(MainPage.LogType.Debug, message);
            File.AppendAllText("debug.log", forFull);
            FullLog(forFull);
        }
        public void FullLog(string message)
        {
            File.AppendAllText("full.log",message);
            //MainThread.InvokeOnMainThreadAsync(() =>
            //{
            //    MainPage.LogBox.Text += message + "\r\n";
            //});
            //MainPage.LogBox.Text= message+"\r\n";
        }
    }
}
