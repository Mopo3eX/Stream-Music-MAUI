using NAudio.Wave;
using Newtonsoft.Json;
using Stream_Music_MAUI.Classes;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using TwitchLib.Communication.Interfaces;
using static System.Formats.Asn1.AsnWriter;
using static System.Reflection.Metadata.BlobBuilder;

namespace Stream_Music_MAUI
{
    public partial class MainPage : ContentPage
    {

        private static System.Timers.Timer UpdInfo;
        public static AudioDeviceManager AudioDeviceManager;
        public static Log LogSystem;
        public static Classes.Settings? Settings;
        public static MainPage Instance;
        public OBS? OBS;
        public Twitch? Twitch;
        public yt_dlp yt_Dlp;
        public NAudioClass audioClass;
        public MainPage()
        {
            InitializeComponent();
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            LogSystem = new Log(this);
            SetTimer();
            Instance = this;
            AudioDeviceManager = new AudioDeviceManager(LogSystem);
            if (File.Exists(".\\config.json"))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<Classes.Settings>(File.ReadAllText(".\\config.json"));
                }
                catch(Exception error)
                {
                    LogSystem.ErrorLog(error.Message,error.StackTrace);
                    Settings = new Classes.Settings();
                }
            }
            else
                Settings = new Classes.Settings();
            if (!String.IsNullOrEmpty(Settings.ObsWebSocketURL))
                OBS = new OBS(LogSystem, Settings.ObsWebSocketURL, Settings.ObsWebSocketPassword);
            if (!String.IsNullOrEmpty(Settings.BotUserName) && !String.IsNullOrEmpty(Settings.BotToken))
                Twitch = new Twitch(LogSystem, Settings.ChannelName, Settings.BotUserName, Settings.BotToken);
            yt_Dlp = new yt_dlp(LogSystem);
            audioClass = new NAudioClass(LogSystem);
            

            //PlayListView.ItemsSource = AddedList;
            //AddedList.Add(new YouTubeVideo { Title = "Title", Url = "Url", Duration = 100 });
            VolumeSlider.Value = Settings.Volume;
            AddOnChatToggle.IsToggled = Settings.MusicAddOnChat;
            UsersListView.Header = "Список пользователей (0)";
            Twitch.Client.OnMessageReceived += Client_OnMessageReceived;
        }
        public enum LogType
        {
            Error,
            Warning,
            Info,
            Debug
        }
        public void OnLog(LogType type,string Message)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                AddColoredLog(type, Message);
                LogScroll.ScrollToAsync(0, int.MaxValue, true);
            });

        }
        private void AddColoredLog(LogType type, string Message)
        {
            var formattedString = LogLabel.FormattedText;
            if (formattedString == null)
                formattedString = new FormattedString();
            Color colorByType;
            string logTypeText;
            switch(type)
            {
                case LogType.Debug:
                    colorByType = new Color(200, 200, 255);
                    logTypeText = "[DEBUG]";
                    break;
                case LogType.Warning:
                    colorByType = new Color(255, 255, 0);
                    logTypeText = "[WARN]";
                    break;
                case LogType.Error:
                    colorByType = new Color(255, 0, 0);
                    logTypeText = "[ERROR]";
                    break;
                default:
                    colorByType = new Color(255, 255, 255);
                    logTypeText = "[INFO]";
                    break;
            }
            // Добавляем разноцветные текстовые отрезки
            formattedString.Spans.Add(new Span
            {
                Text = logTypeText,
                TextColor = colorByType,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            });

            formattedString.Spans.Add(new Span
            {
                Text = $": {Message}\n",
                TextColor = Colors.White,
                FontSize = 14
            });


            // Устанавливаем форматированный текст для Label
            LogLabel.FormattedText = formattedString;
        }
        private void Client_OnMessageReceived(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                AddColoredMessage(e.ChatMessage.DisplayName,e.ChatMessage.Message);
                ChatScroll.ScrollToAsync(0, int.MaxValue, true);
            });
            
        }
        public static string GenerateColorFromUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return "#FFFFFF"; // Белый цвет по умолчанию

            // Генерируем хэш от ника
            byte[] hash;
            using (MD5 md5 = MD5.Create())
            {
                hash = md5.ComputeHash(Encoding.UTF8.GetBytes(username));
            }

            // Преобразуем хэш в HSL цвет с контролируемой насыщенностью и яркостью
            double hue = (double)BitConverter.ToUInt32(hash, 0) / UInt32.MaxValue * 360; // 0-360 градусов
            double saturation = 0.7 + (BitConverter.ToUInt32(hash, 4) % 300) / 1000.0; // 70-100%
            double lightness = 0.4 + (BitConverter.ToUInt32(hash, 8) % 300) / 1000.0; // 40-70%

            // Конвертируем HSL в RGB
            (int r, int g, int b) = HslToRgb(hue, saturation, lightness);

            return $"#{r:X2}{g:X2}{b:X2}";
        }
        private static (int, int, int) HslToRgb(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // Оттенки серого
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h + 120);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 120);
            }

            return ((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 360;
            if (t > 360) t -= 360;

            if (t < 60) return p + (q - p) * t / 60;
            if (t < 180) return q;
            if (t < 240) return p + (q - p) * (240 - t) / 60;
            return p;
        }
        private void AddColoredMessage(string Nick, string Message)
        {
            var formattedString = formattedLabel.FormattedText;
            if (formattedString == null)
                formattedString = new FormattedString();
            
            // Добавляем разноцветные текстовые отрезки
            formattedString.Spans.Add(new Span
            {
                Text = Nick,
                TextColor = Color.FromArgb(GenerateColorFromUsername(Nick)),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold
            });

            formattedString.Spans.Add(new Span
            {
                Text = $": {Message}\n",
                TextColor = Colors.White,
                FontSize = 14
            });

            
            // Устанавливаем форматированный текст для Label
            formattedLabel.FormattedText = formattedString;
        }
        private void SetTimer()
        {
            UpdInfo = new System.Timers.Timer(1000);
            UpdInfo.Elapsed += OnUpdate;
            UpdInfo.AutoReset = true;
            UpdInfo.Enabled = true;
        }
        public void UpdateUsers()
        {
            MainThread.InvokeOnMainThreadAsync(() =>
            {
                UsersListView.ItemsSource = Twitch.OnlineUsers;
                UsersListView.Header = $"Список пользователей ({Twitch.OnlineUsers.Count})";
                UsersListView.ItemTemplate = new DataTemplate(() =>
                {
                    var personLbl = new Label { FontSize = 16, TextColor = Color.FromArgb("#FFFFFF") };
                    personLbl.SetBinding(Label.TextProperty, new Binding("BindingContext", source: RelativeBindingSource.Self));
                    return personLbl;
                });
            });
            //UsersListView.ItemsSource = Twitch.OnlineUsers;
        }
        public void OnReloadSettings()
        {
            if (!String.IsNullOrEmpty(Settings.ObsWebSocketURL))
            {
                if(OBS != null && OBS.IsConnected)
                {
                    OBS.Disconnect();
                }
                OBS = new OBS(LogSystem, Settings.ObsWebSocketURL, Settings.ObsWebSocketPassword);
            }
        }
        public List<string> Users { 
            get
            {
                if(Twitch == null && !Twitch.Connected)
                {
                    return new List<string> { };
                }
                else
                    return Twitch.OnlineUsers;
            }
            set; }
        private void OnUpdate(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (OBS.IsStarted())
            {
                MainThread.InvokeOnMainThreadAsync(() =>
            {
                btnOBSStart.BackgroundColor = new Color(0, 255, 0, 255);
            });
                if(OBS != null && !OBS.IsConnected)
                {
                    OBS.Connect();
                }
            }
            else
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    btnOBSStart.BackgroundColor = new Color(255, 0, 0, 255);
                });
            if (VTM.IsStarted())
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    btnVTMStart.BackgroundColor = new Color(0, 255, 0, 255);
                });
            else
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    btnVTMStart.BackgroundColor = new Color(255, 0, 0, 255);
                });
            if (OBS != null && OBS.IsConnected)
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OBSWebSocketStatus.Text = "Вкл.";
                });
            else
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    OBSWebSocketStatus.Text = "Выкл.";
                });
            if (Twitch != null && Twitch.Connected)
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TwitchStatus.Text = "Вкл.";
                    AddOnChatToggle.IsEnabled = true;
                });
                
            }
            else
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TwitchStatus.Text = "Выкл.";
                    AddOnChatToggle.IsEnabled = false;
                });
            if (audioClass != null && audioClass.Player != null && audioClass.Player.PlaybackState == PlaybackState.Playing)
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PlayerStatus.Text = "Вкл.";
                    //AddOnChatToggle.IsEnabled = true;
                });
            else
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    PlayerStatus.Text = "Выкл.";
                    //AddOnChatToggle.IsEnabled = false;
                });
            if(!PlayListPicker.IsFocused)
            {
                PlayListPicker.ItemsSource = AddedList;
            }
        }

        private void Settings_Clicked(object sender, EventArgs e)
        {

            Navigation.PushAsync(new Settings(Settings));

        }

        private void btnOBSStart_Clicked(object sender, EventArgs e)
        {
            ChangeStateOBS();
        }
        private async Task ChangeStateOBS()
        {
            if (!OBS.IsStarted())
            {
                if (String.IsNullOrEmpty(Settings.OBSDir))
                {
                    await DisplayAlert("Ошибка", "Вы не можете запустить OBS, пока не укажите к нему путь в настройках", "OK");
                }
                else
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = Settings.OBSDir,
                            WorkingDirectory = Settings.OBSDir.Replace(Path.GetFileName(Settings.OBSDir), ""),
                            UseShellExecute = false,
                        };

                        var process = Process.Start(psi);
                        process.PriorityClass = ProcessPriorityClass.High;
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ErrorLog(ex.Message, ex.StackTrace);
                    }
                }
            }
            else
            {
                bool answer = await DisplayAlert("Закрыть OBS?", "Вы действительно хотите закрыть OBS?", "Да", "Нет");
                if (answer)
                {
                    if(OBS != null && OBS.IsConnected)
                    {
                        OBS.Disconnect();
                    }
                    await Task.Delay(1000);
                    Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Settings.OBSDir))[0].Kill();
                    if(OBS != null)
                    {
                        OBS.WaitOBS();
                    }
                }
            }
        }

        private void btnVTMStart_Clicked(object sender, EventArgs e)
        {
            ChangeStateVTM();
        }
        private async Task ChangeStateVTM()
        {
            if (!VTM.IsStarted())
            {
                if (String.IsNullOrEmpty(Settings.VTMDir))
                {
                    await DisplayAlert("Ошибка", "Вы не можете запустить VTM, пока не укажите к нему путь в настройках", "OK");
                }
                else
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = Settings.VTMDir,
                            WorkingDirectory = Settings.VTMDir.Replace(Path.GetFileName(Settings.VTMDir), ""),
                            UseShellExecute = false,
                        };

                        var process = Process.Start(psi);
                        process.PriorityClass = ProcessPriorityClass.High;
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ErrorLog(ex.Message, ex.StackTrace);
                    }
                }
            }
            else
            {
                bool answer = await DisplayAlert("Закрыть VTM?", "Вы действительно хотите закрыть VTM?", "Да", "Нет");
                if (answer)
                    Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Settings.VTMDir))[0].Kill();
            }
        }

        private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            SelectedVolume.Text = $"{Math.Round(VolumeSlider.Value * 100, 2)}%";
            Settings.Volume = VolumeSlider.Value;
            if (audioClass != null)
            {
                audioClass.Volume = (float)VolumeSlider.Value;
                if (audioClass.Player != null)
                {
                    audioClass.Player.Volume = (float)VolumeSlider.Value;
                }
            }
        }
        public void SaveSettings()
        {
            string jsonSettings = JsonConvert.SerializeObject(Settings);
            File.WriteAllText(".\\config.json", jsonSettings);
        }

        private void VolumeSlider_DragCompleted(object sender, EventArgs e)
        {
            SaveSettings();
            //AddedList.Add(new YouTubeVideo { Title = SelectedVolume.Text, Url = "Url", Duration = 100 });
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            Settings.MusicAddOnChat = AddOnChatToggle.IsToggled;
            SaveSettings();
        }

        List<string> CheckingURLs = new List<string>();
        public async Task OnAddMusic(object? sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            if (!Settings.MusicAddOnChat)
                return;
            if (AddedList.Count >= 5)
            {
                Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Плейлист уже заполнен, как только осовбодится местечко, попробуйте ещё раз");
                return;
            }
            if (CheckingURLs.Count >= 5)
            {
                Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Извините, бот сейчас перегружен проверкой треков, подождите немного и попробуйте ещё раз");
                return;
            }
            string url = e.ChatMessage.Message.Replace("!addmusic ", "");

            if (!url.StartsWith("https://www.youtube.com/watch?v=") && !url.StartsWith("https://youtu.be/"))
            {
                Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "К сожадению можно добавлять только музыку с YouTube");
                return;
                //e.ChatMessage.
            }
            if (CheckingURLs.Contains(url))
                return;
            CheckingURLs.Add(url);
            var info = await yt_Dlp.GetInfo(url);
            if (info == null)
                return;
            if (info.duration > 360 || info.duration < 30)
            {
                Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Данный трэк не подходит, попробуйте что то другое");
                if (CheckingURLs.Contains(url))
                    CheckingURLs.Remove(url);
                return;
            }
            if (AddedList.Where(x => x.Url == $"https://www.youtube.com/watch?v={info.id}").Count() != 0)
            {
                Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, "Данный трэк уже есть в плейлисте.");
                if (CheckingURLs.Contains(url))
                    CheckingURLs.Remove(url);
                return;
            }
            if (CheckingURLs.Contains(url))
                CheckingURLs.Remove(url);
            AddedList.Add(new YouTubeVideo { Title = $"[{e.ChatMessage.DisplayName}]{info.title}", Url = $"https://www.youtube.com/watch?v={info.id}" });
            Twitch.Client.SendReply(e.ChatMessage.Channel, e.ChatMessage.Id, $"{info.title} добавлен в список проигрывания");
        }


        public List<YouTubeVideo> StandartList = new List<YouTubeVideo>();
        public List<YouTubeVideo> AddedList = new List<YouTubeVideo>();
        public bool OnStart = true;
        public bool OnError = false;
        public bool Stoped = false;
        public Random Random = new Random();
        private async Task StartPlaying()
        {
            try
            {
                // Загружаем стандартный плейлист
                //StandartList = await GetPlaylistVideos(PlayerList);
                StandartList = JsonConvert.DeserializeObject<List<YouTubeVideo>>(File.ReadAllText("ParcedMusic.json"));

                //YouTubeVideo selectedVideo = null;
                while (true)
                {
                    if(OBS != null)
                        OBS.UpdateGDIText(Settings.GDITextNameNext, "▶ Далее: Идёт подбор...");
                    NextMusicName.Text = "Идёт подбор...";
                    if (!OnStart && !OnError)
                    {
                        for (int i = 0; i < 30; i++)
                            if (!OnError)
                            {
                                await Task.Delay(500);
                            }
                            else
                                break;

                    }
                    else
                    {
                        OnStart = false;
                        OnError = false;
                    }
                    YouTubeVideo selectedVideo = null;
                    // Выбираем трек: либо из очереди AddedList, либо случайный из StandartList
                    if (Twitch != null && Twitch.Connected && Settings.MusicAddOnChat)
                    {
                        selectedVideo = AddedList.Count == 0
                        ? StandartList[Random.Next(0, StandartList.Count - 1)]
                        : AddedList.First();
                        AddedList.Remove(selectedVideo);
                    }
                    else
                    {
                        selectedVideo = StandartList[Random.Next(0, StandartList.Count - 1)];
                    }

                        // Получаем прямую ссылку на аудиопоток
                        string audioUrl = await yt_Dlp.GetAudioStreamUrl(selectedVideo.Url);
                    if (audioUrl != null)
                    {
                        // Если уже что-то играет, показываем "Следующий трек"
                        if (audioClass.Player != null && audioClass.Player.PlaybackState == PlaybackState.Playing)
                        {
                            string PlayingNext = $"▶ Далее: {selectedVideo.Title}";
                            NextMusicName.Text = selectedVideo.Title;
                            OBS.UpdateGDIText(Settings.GDITextNameNext, PlayingNext);
                            LogSystem.InfoLog(PlayingNext);

                        }
                        // Ждем окончания предыдущего трека
                        while (audioClass.Player != null && audioClass.Player.PlaybackState == PlaybackState.Playing)
                        {
                            await Task.Delay(500);
                        }
                        // Проверка на команду Stop
                        if (Stoped)
                        {
                            Stoped = false;
                            StandartList.Clear();
                            AddedList.Clear();
                            OBS.UpdateGDIText(Settings.GDITextNameNext, "");
                            OBS.UpdateGDIText(Settings.GDITextName, "");
                            CurrentMusicName.Text = "Ничего";
                            NextMusicName.Text = "Ничего";
                            if (audioClass.Player == null)
                                audioClass.Player.Stop();
                            return;
                        }
                        // Обновляем название в OBS
                        string Playing = $"▶ Сейчас играет: {selectedVideo.Title}";
                        OBS.UpdateGDIText(Settings.GDITextName, Playing);
                        CurrentMusicName.Text = selectedVideo.Title;
                        LogSystem.InfoLog(Playing);
                        // Запускаем проигрывание
                        audioClass.PlayAudioStream(audioUrl);
                    }
                    else
                    {
                        OnError = true;
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                LogSystem.ErrorLog(e.Message,e.StackTrace);
            }
        }

        private void btnEnable_Clicked(object sender, EventArgs e)
        {
            StartPlaying();
            btnClear.IsEnabled = true;
            btnDisable.IsEnabled = true;
            btnSkip.IsEnabled = true;
            btnEnable.IsEnabled = false;
        }

        private void btnDisable_Clicked(object sender, EventArgs e)
        {
            Stoped = true;
            if (audioClass.Player != null)
                audioClass.Player.Stop();
            btnClear.IsEnabled = false;
            btnDisable.IsEnabled = false;
            btnSkip.IsEnabled = false;
            btnEnable.IsEnabled = true;
        }

        private void btnSkip_Clicked(object sender, EventArgs e)
        {
            if (audioClass.Player != null)
                audioClass.Player.Stop();
        }

        private void btnClear_Clicked(object sender, EventArgs e)
        {
            AddedList.Clear();
        }

        private void RemoveAddedMusic_Clicked(object sender, EventArgs e)
        {
            if(PlayListPicker.SelectedItem != null)
            {
                AddedList.Remove((YouTubeVideo)PlayListPicker.SelectedItem);
            }
            
        }

        private void UsersListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }

}
