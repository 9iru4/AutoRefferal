﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoRefferal
{
    public class MyProxy
    {
        public string ApiKey { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int UsedActivation { get; set; }

        public MyProxy()
        {

        }

        public MyProxy(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
            UsedActivation = 0;
        }

        public void SetApi(string key)
        {
            ApiKey = key;
        }

        /// <summary>
        /// Сохранение прокси в файл
        /// </summary>
        /// <param name="refferals">Прокси для сохранения</param>
        public static void SaveProxies(List<MyProxy> proxies)
        {
            using (StreamWriter sw = new StreamWriter("Proxy.dat"))
            {
                sw.Write(SerializeHelper.Serialize(proxies));
            }
        }

        /// <summary>
        /// Загрука прокси из файла
        /// </summary>
        /// <returns>Загруженные прокси</returns>
        public static List<MyProxy> LoadProxies()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Proxy.dat"))
                {
                    return SerializeHelper.Desirialize<List<MyProxy>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new List<MyProxy>();
            }
        }

        /// <summary>
        /// Добавление новых прокси из файла
        /// </summary>
        /// <param name="refferals">Текущие прокси</param>
        /// <returns>Новый список прокси</returns>
        public static List<MyProxy> GetNewProxies(List<MyProxy> proxies)
        {
            try
            {
                using (StreamReader sr = new StreamReader("Proxy.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine().Split(':');
                        if (proxies.Where(x => x.IpAddress == str[0]).FirstOrDefault() == null)
                            proxies.Add(new MyProxy(str[0], str[1]));
                    }
                    SaveProxies(proxies);
                    File.Delete("Proxy.txt");
                    return proxies;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}