using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stream_Music_MAUI.Classes
{
    public class AudioDeviceManager
    {
        private int _deviceIndex = -1; // Храним индекс устройства. -1 означает устройство по умолчанию.

        // Поле для хранения MMDeviceEnumerator, чтобы не создавать его каждый раз
        private readonly MMDeviceEnumerator _enumerator = new MMDeviceEnumerator();

        // Словарь для хранения пар: индекс устройства -> MMDevice
        public Dictionary<int, MMDevice> _activeOutputDevices;
        private Log LogSystem;
        public AudioDeviceManager(Log logSystem)
        {
            LogSystem = logSystem;
            RefreshActiveOutputDevices();
        }

        // Публичное свойство для получения текущего выбранного устройства.
        // Его можно использовать для установки устройства.
        public int CurrentDeviceIndex
        {
            get { return _deviceIndex; }
            set
            {
                // Проверяем, валиден ли переданный индекс
                if (_activeOutputDevices.ContainsKey(value))
                {
                    _deviceIndex = value;
                    LogSystem.DebugLog($"Выбрано устройство: {_activeOutputDevices[value].FriendlyName}");
                }
                else if (value == -1 && _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console) != null)
                {
                    // Разрешаем установку -1 для устройства по умолчанию
                    _deviceIndex = -1;
                    LogSystem.DebugLog("Выбрано устройство вывода по умолчанию.");
                }
                else
                {
                    // Если устройство не найдено или невалидный индекс
                    LogSystem.ErrorLog($"Ошибка: Недопустимый индекс устройства '{value}'.");
                    //Console.WriteLine($"Ошибка: Недопустимый индекс устройства '{value}'.");
                    // Оставляем старое значение _deviceIndex, если попытка установки была некорректной
                }
            }
        }

        /// <summary>
        /// Обновляет список активных звуковых устройств вывода.
        /// </summary>
        public void RefreshActiveOutputDevices()
        {
            // Получаем все активные устройства вывода
            var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            // Создаем словарь с индексами и объектами MMDevice
            _activeOutputDevices = devices.Select((device, index) => new { Device = device, Index = index })
                                          .ToDictionary(x => x.Index, x => x.Device);

            LogSystem.DebugLog($"Обнаружено {_activeOutputDevices.Count} активных звуковых устройств вывода.");
        }

        /// <summary>
        /// Выводит список всех активных звуковых устройств вывода с их названиями и индексами.
        /// </summary>
        public void ListActiveAudioOutputDevices()
        {
            LogSystem.DebugLog("\n--- Список активных звуковых устройств вывода ---");

            // Проверяем, есть ли устройство по умолчанию
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            if (defaultDevice != null)
            {
                LogSystem.DebugLog($"  -1: {defaultDevice.FriendlyName} (Устройство по умолчанию)");
            }
            else
            {
                LogSystem.DebugLog("  -1: Устройство вывода по умолчанию не найдено.");
            }

            if (_activeOutputDevices.Count == 0)
            {
                LogSystem.ErrorLog("  Активные устройства вывода не найдены.");
            }
            else
            {
                foreach (var kvp in _activeOutputDevices)
                {
                    LogSystem.DebugLog($"  {kvp.Key}: {kvp.Value.FriendlyName}");
                }
            }
            LogSystem.DebugLog("-------------------------------------------------");
        }

        /// <summary>
        /// Получает объект MMDevice по его индексу.
        /// </summary>
        /// <param name="index">Индекс устройства.</param>
        /// <returns>Объект MMDevice или null, если устройство не найдено.</returns>
        public MMDevice GetDeviceByIndex(int index)
        {
            if (_activeOutputDevices.ContainsKey(index))
            {
                return _activeOutputDevices[index];
            }
            // Если запрашивается устройство по умолчанию (-1)
            if (index == -1)
            {
                return _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }
            return null;
        }

        // Пример использования:
        // public void SomeMethodThatUsesDevice()
        // {
        //     // Например, если вы хотите воспроизвести звук на выбранном устройстве
        //     using (var waveOut = new WaveOutEvent())
        //     using (var audioFile = new AudioFileReader("путь_к_файлу.mp3")) // Или другая реализация IWaveProvider
        //     {
        //         var device = GetDeviceByIndex(CurrentDeviceIndex);
        //         if (device != null)
        //         {
        //             waveOut.DeviceNumber = _enumerator.GetDeviceIndex(device); // WaveOutEvent требует индекс устройства
        //             waveOut.Init(audioFile);
        //             waveOut.Play();
        //             // ... дальнейшая логика
        //         }
        //         else
        //         {
        //             LogSystem.ErrorLog("Не удалось инициализировать воспроизведение: не выбрано или не найдено устройство.");
        //         }
        //     }
        // }
    }
}

    // Пример использования:
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        var audioManager = new AudioDeviceManager();

    //        // Выводим список доступных устройств
    //        audioManager.ListActiveAudioOutputDevices();

    //        // Пример установки устройства (например, первое в списке)
    //        // Если есть устройства, индекс 0 будет доступен
    //        if (audioManager._activeOutputDevices.Count > 0)
    //        {
    //            audioManager.CurrentDeviceIndex = 0; // Выбираем первое устройство
    //        }
    //        else
    //        {
    //            Console.WriteLine("Нет активных устройств для выбора, используется устройство по умолчанию.");
    //            audioManager.CurrentDeviceIndex = -1; // Устройство по умолчанию
    //        }

    //        // Вы можете также выбрать устройство по умолчанию, установив -1
    //        // audioManager.CurrentDeviceIndex = -1;

    //        // Теперь вы можете использовать CurrentDeviceIndex для инициализации WaveOutEvent
    //        // Например:
    //        // var deviceToUse = audioManager.GetDeviceByIndex(audioManager.CurrentDeviceIndex);
    //        // if (deviceToUse != null)
    //        // {
    //        //     using (var outputDevice = new WaveOutEvent())
    //        //     {
    //        //         outputDevice.DeviceNumber = audioManager._enumerator.GetDeviceIndex(deviceToUse);
    //        //         // ... остальная часть кода для воспроизведения
    //        //     }
    //        // }

    //        Console.WriteLine("\nДля выхода нажмите Enter.");
    //        Console.ReadLine();
    //    }
    //}

    //// Предполагаемый класс для логирования (для примера)
    //public static class LogSystem
    //{
    //    public static void ErrorLog(string message)
    //    {
    //        Console.ForegroundColor = ConsoleColor.Red;
    //        Console.Error.WriteLine($"[ERROR] {message}");
    //        Console.ResetColor();
    //    }
    //}
