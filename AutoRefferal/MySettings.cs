using System;
using System.IO;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Класс описывающий настройки
    /// </summary>
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
        public MySettings(string operaPath, string smsApiKey, string proxyApiKey)
        {
            OperaPath = operaPath;
            SmsApiKey = smsApiKey;
            ProxyApiKey = proxyApiKey;
        }

        /// <summary>
        /// Загрузить настройки из файла
        /// </summary>
        /// <returns></returns>
        public bool LoadSettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Settings.txt"))
                {
                    var all = sr.ReadLine().Split('|');
                    OperaPath = all[0];
                    SmsApiKey = all[1];
                    ProxyApiKey = all[2];
                    return true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Пожалуйста укажите настройки.");
                return false;
            }
        }
    }
}
