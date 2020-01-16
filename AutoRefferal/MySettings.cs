using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutoRefferal
{
    public class MySettings
    {
        public string OperaPath { get; set; }
        public string SmsApiKey { get; set; }
        public string ProxyApiKey { get; set; }


        public MySettings()
        {

        }

        public MySettings(string operaPath, string smsApiKey, string proxyApiKey)
        {
            OperaPath = operaPath;
            SmsApiKey = smsApiKey;
            ProxyApiKey = proxyApiKey;
        }

        public bool LoadSettings()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Settings.txt"))
                {
                    var all = sr.ReadLine().Split(':');
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
