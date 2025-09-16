using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Stream_Music_MAUI.Classes
{
    public class NAudioClass
    {
        private OBS Obs;
        private Log LogSystem;
        public bool Playing = false;
        public WasapiOut Player;

        public float Volume = 1;
        
        public NAudioClass(Log logSystem)
        {
            LogSystem = logSystem;
        }
        //public static MMDeviceEnumerator GetDevices()
        //{
        //    var enumerator = new MMDeviceEnumerator();
        //    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        //    return devices;
        //}
        public async Task<bool> PlayAudioStream(string audioUrl)
        {
            // Создаем MediaFoundationReader для чтения потока
            // Создаем BufferedWaveProvider для буферизации
            // Инициализируем WasapiOut (выбранное устройство)
            // Читаем поток и добавляем в буфер
            // Обновляем OBS, если название еще не обновлено
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            MMDevice device;
            if(MainPage.Settings.AudioDevice == -1)
            {
                device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }
            else
            {
                device = devices[MainPage.Settings.AudioDevice];
            }

                MediaFoundationReader tempReader = null;
            try
            {
                tempReader = new MediaFoundationReader(audioUrl);

                //tempReader.WriteTimeout = 1300;
                var waveFormat = tempReader.WaveFormat;

                var buffered = new BufferedWaveProvider(waveFormat)
                {
                    BufferDuration = TimeSpan.FromSeconds(30),
                    DiscardOnBufferOverflow = true
                };

                using (var outDevice = new WasapiOut(device, AudioClientShareMode.Shared, false, 100))
                {
                    outDevice.Init(buffered);
                    outDevice.Volume = Volume;
                    Player = outDevice;
                    outDevice.Play();
                    bool readFinished = false;
                    var readTask = Task.Run(() =>
                    {
                        try
                        {
                            var buffer = new byte[16384];
                            int read;
                            while ((read = tempReader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                while (buffered.BufferedDuration > TimeSpan.FromSeconds(10))
                                {
                                    Thread.Sleep(50);
                                    if (outDevice.PlaybackState != PlaybackState.Playing) return;
                                }

                                buffered.AddSamples(buffer, 0, read);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                        readFinished = true;
                    });

                    while (outDevice.PlaybackState == PlaybackState.Playing)
                    {
                        if (readFinished && buffered.BufferedDuration < TimeSpan.FromMilliseconds(150))
                        {
                            outDevice.Stop();
                            break;
                        }
                        Playing = true;
                        await Task.Delay(500);
                    }

                    try { await readTask.ConfigureAwait(false); } catch { }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine($"{err.Message}\r\n{err.StackTrace}");
                Playing = false;
                return false;
            }
            finally
            {
                tempReader?.Dispose();
                
            }
            Playing = false;
            return true;
        }
    }
}
