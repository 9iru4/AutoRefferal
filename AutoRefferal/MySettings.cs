using System;
using System.IO;

namespace AutoRefferal
{
    /// <summary>
    /// Класс описывающий настройки
    /// </summary>
    [Serializable]
    public class MySettings
    {
        /// <summary>
        /// Путь к опере в система
        /// </summary>
        public string OperaPath { get; set; }
        /// <summary>
        /// Апи ключ для смс сервиса
        /// </summary>
        public string SmsApiKey { get; set; }
        /// <summary>
        /// Апи ключ для прокси сервиса
        /// </summary>
        public string ProxyApiKey { get; set; }
        /// <summary>
        /// Выбранный браузер
        /// </summary>
        public string SelectedBrowser { get; set; }

        public bool HiddenMode { get; set; }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public MySettings()
        {

        }

        /// <summary>
        /// конструктор класса
        /// </summary>
        /// <param name="operaPath">Путь к опере</param>
        /// <param name="smsApiKey">Апи смс</param>
        /// <param name="proxyApiKey">Апи прокси</param>
        /// <param name="Browser">Браузер</param>
        public MySettings(string operaPath, string smsApiKey, string proxyApiKey, string Browser, bool hidden)
        {
            OperaPath = operaPath;
            SmsApiKey = smsApiKey;
            ProxyApiKey = proxyApiKey;
            SelectedBrowser = Browser;
            HiddenMode = hidden;
        }

        /// <summary>
        /// Загрузить настройки из файла
        /// </summary>
        /// <returns></returns>
        public void LoadSettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin/Settings.dat"))
                {
                    var settings = SerializeHelper.Desirialize<MySettings>(sr.ReadToEnd());
                    OperaPath = settings.OperaPath;
                    SmsApiKey = settings.SmsApiKey;
                    ProxyApiKey = settings.ProxyApiKey;
                    SelectedBrowser = settings.SelectedBrowser;
                    HiddenMode = settings.HiddenMode;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Сохранение настроек в файл
        /// </summary>
        public void SaveSettings(MySettings settings)
        {
            using (StreamWriter sw = new StreamWriter("bin/Settings.dat"))
            {
                sw.Write(SerializeHelper.Serialize(settings));
            }
        }
    }
}
