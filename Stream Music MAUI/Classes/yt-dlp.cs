using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stream_Music_MAUI.Classes
{
    public class yt_dlp
    {
        private Log LogSystem;
        public yt_dlp(Log logSystem)
        {
            LogSystem = logSystem;
        }

        async Task<List<YouTubeVideo>> GetPlaylistVideos(string playlistUrl)
        {
            // Запуск yt-dlp для получения списка треков из плейлиста
            // Возвращает список YouTubeVideo с Title и Url
            var videos = new List<YouTubeVideo>();

            var psi = new ProcessStartInfo
            {
                FileName = ".\\yt-dlp.exe",
                Arguments = $"--no-cookies-from-browser -J --flat-playlist \"{playlistUrl}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);

            string output = await process.StandardOutput.ReadToEndAsync();
            string errorOutput = await process.StandardError.ReadToEndAsync();

            process.WaitForExit();
            LogSystem.DebugLog($"yt-dlp Exit Code: {process.ExitCode}");
            //Console.WriteLine($"yt-dlp Exit Code: {process.ExitCode}");
            LogSystem.DebugLog($"STDOUT length: {output.Length}");
            //Console.WriteLine($"STDOUT length: {output.Length}");
            if (!string.IsNullOrEmpty(errorOutput))
            {
                LogSystem.DebugLog("STDERR:");
                //Console.WriteLine("STDERR:");
                LogSystem.DebugLog(errorOutput);
                //Console.WriteLine(errorOutput);
            }

            if (string.IsNullOrWhiteSpace(output))
            {
                LogSystem.ErrorLog("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
                return null;
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
                              //throw new Exception("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
            }
            PlayerList playerList = JsonConvert.DeserializeObject<PlayerList>(output);

            foreach (var entr in playerList.entries)
            {
                videos.Add(new YouTubeVideo
                {
                    Title = entr.title,
                    Url = $"https://www.youtube.com/watch?v={entr.id}",
                    Duration = entr.duration
                });
            }

            return videos;
        }
        public async Task<string> GetAudioStreamUrl(string videoUrl)
        {
            try
            {


                // Получаем прямой URL аудиопотока с помощью yt-dlp
                var psi = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--cookies \"cookies.txt\" --no-cookies-from-browser -f bestaudio --get-url \"{videoUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                string url = await process.StandardOutput.ReadLineAsync();
                string errorOutput = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                LogSystem.DebugLog($"yt-dlp Exit Code: {process.ExitCode}");
                //Console.WriteLine($"yt-dlp Exit Code: {process.ExitCode}");
                LogSystem.DebugLog($"STDOUT length: {(url != null ? url.Length.ToString() : "null")}");
                //Console.WriteLine($"STDOUT length: {(url != null ? url.Length.ToString() : "null")}");
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    LogSystem.ErrorLog("STDOUT:");
                    //Console.WriteLine("STDOUT:");
                    LogSystem.ErrorLog(url);
                    //Console.WriteLine(url);
                    LogSystem.ErrorLog("STDERR:");
                    //Console.WriteLine("STDERR:");
                    LogSystem.ErrorLog(errorOutput);
                    //Console.WriteLine(errorOutput);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(url))
                {
                    LogSystem.ErrorLog("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
                    //Console.WriteLine("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
                    return null;
                }
                return url;
            }
            catch (Exception err)
            {
                LogSystem.ErrorLog(err.Message, err.StackTrace);
                //Console.WriteLine($"{err.Message}\r\n{err.StackTrace}");
                return null;
            }
        }
        public async Task<VideoInfo> GetInfo(string videoUrl)
        {
            // Получаем информацию о видео (JSON) через yt-dlp
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "yt-dlp",
                    Arguments = $"--cookies \"cookies.txt\" --no-cookies-from-browser --no-warnings --skip-download --dump-single-json  \"{videoUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                string json = process.StandardOutput.ReadLine();
                string errorOutput = process.StandardError.ReadToEnd();
                process.WaitForExit();
                LogSystem.DebugLog($"yt-dlp Exit Code: {process.ExitCode}");
                //Console.WriteLine($"yt-dlp Exit Code: {process.ExitCode}");
                LogSystem.DebugLog($"STDOUT length: {json.Length}");
                //Console.WriteLine($"STDOUT length: {json.Length}");
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    LogSystem.DebugLog("STDERR:");
                    //Console.WriteLine("STDERR:");
                    LogSystem.DebugLog(errorOutput);
                    //Console.WriteLine(errorOutput);
                }

                if (string.IsNullOrWhiteSpace(json))
                {
                    LogSystem.ErrorLog("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
                    //Console.WriteLine("yt-dlp вернул пустой результат. Проверь STDOUT и STDERR.");
                    return null;
                }
                var info = JsonConvert.DeserializeObject<VideoInfo>(json);
                return info;
            }
            catch (Exception err)
            {
                LogSystem.ErrorLog(err.Message, err.StackTrace);
                //Console.WriteLine($"{err.Message}\r\n{err.StackTrace}");
                return null;
            }
        }

    }
}
