using Newtonsoft.Json;
using Stream_Music_MAUI.Classes;

namespace Stream_Music_MAUI;

public partial class Settings : ContentPage
{
	public Settings(Classes.Settings settings)
	{
		InitializeComponent();
		List<string> deviceArray = new List<string>();
        deviceArray.Add($"-1:По умолчанию");
        foreach (var device in MainPage.AudioDeviceManager._activeOutputDevices)
		{
			deviceArray.Add($"{device.Key}:{device.Value.FriendlyName}");

        }
		Devices.ItemsSource = deviceArray;
        if(Devices.Items.Count >= settings.AudioDevice + 1)
            Devices.SelectedIndex = settings.AudioDevice + 1;
        BotUserName.Text = settings.BotUserName;
        BotToken.Text = settings.BotToken;
        ChannelName.Text = settings.ChannelName;
        ObsWebSocketURL.Text = settings.ObsWebSocketURL;
        ObsWebSocketPassword.Text = settings.ObsWebSocketPassword;
        GDITextName.Text = settings.GDITextName;
        GDITextNameNext.Text = settings.GDITextNameNext;
        OBSDir.Text = settings.OBSDir;
        VTMDir.Text = settings.VTMDir;
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions();
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".exe" } }, // file extension
                });
        options.FileTypes = customFileType;
        SelectOBSFile(options);

    }
    public async Task SelectOBSFile(PickOptions options)
    {
        var result = await FilePicker.Default.PickAsync(options);
        OBSDir.Text = result.FullPath;

    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        PickOptions options = new PickOptions();
        var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".exe" } }, // file extension
                });
        options.FileTypes = customFileType;
        SelectVTMFile(options);

    }
    public async Task SelectVTMFile(PickOptions options)
    {
        var result = await FilePicker.Default.PickAsync(options);
        VTMDir.Text = result.FullPath;

    }

    private void SaveClicked(object sender, EventArgs e)
    {
        var settings = new Classes.Settings();
        settings.BotUserName = BotUserName.Text;
        settings.BotToken = BotToken.Text;
        settings.ChannelName = ChannelName.Text;
        settings.ObsWebSocketURL = ObsWebSocketURL.Text;
        settings.ObsWebSocketPassword = ObsWebSocketPassword.Text;
        settings.GDITextName = GDITextName.Text;
        settings.GDITextNameNext = GDITextNameNext.Text;
        if (Devices.SelectedIndex == 0 || Devices.SelectedIndex == -1)
            settings.AudioDevice = -1;
        else
        {
            string deviceName = (string)Devices.SelectedItem;
            int deviceID = int.Parse(deviceName.Split(':')[0]);
            settings.AudioDevice = deviceID;
        }
        settings.OBSDir = OBSDir.Text;
        settings.VTMDir = VTMDir.Text;
        string jsonSettings = JsonConvert.SerializeObject(settings);
        File.WriteAllText(".\\config.json",jsonSettings);
        MainPage.Settings = settings;
        MainPage.Instance.OnReloadSettings();
        Navigation.PopAsync();
    }
}