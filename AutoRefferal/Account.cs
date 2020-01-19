using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoRefferal
{
    /// <summary>
    /// Аккаунт для регистрации
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Имейл
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Базовый конструктор класса
        /// </summary>
        public Account()
        {

        }

        /// <summary>
        /// Добавление нового аккаунта
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        public Account(string name, string email)
        {
            Name = name;
            Email = email;
        }

        /// <summary>
        /// Сохранение аккаунтов в файл
        /// </summary>

        public static void SaveAccounts(List<Account> accounts)
        {
            using (StreamWriter sw = new StreamWriter("bin/Accounts.dat", false, Encoding.Unicode))
            {
                sw.Write(SerializeHelper.Serialize(accounts));
            }
        }

        /// <summary>
        /// Загрузка аккаунтов из файла
        /// </summary>
        public static List<Account> LoadAccounts()
        {
            try
            {
                using (StreamReader sr = new StreamReader("bin/Accounts.dat", Encoding.Unicode))
                {
                    return SerializeHelper.Desirialize<List<Account>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new List<Account>();
            }
        }

        /// <summary>
        /// Добавление новых аккаунтов
        /// </summary>
        /// <param name="accounts">Текущие аккаунты</param>
        /// <param name="pathToFile">Путь к файлу</param>
        /// <returns>Список аккаунтов</returns>
        public static List<Account> AddNewAccounts(List<Account> accounts, string pathToFile)
        {
            try
            {
                using (StreamReader sr = new StreamReader(pathToFile, Encoding.Default))
                {
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine().Split(':');
                        if (accounts.Where(x => x.Email == str[0]).FirstOrDefault() == null)
                            accounts.Add(new Account(str[0], str[1]));
                    }
                }
                SaveAccounts(accounts);
                return accounts;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Сохранение информации о текущем аккаунте в файл
        /// </summary>
        /// <param name="mess">Статус</param>
        public void SaveAccountInfo(string mess)
        {
            using (StreamWriter sw = new StreamWriter("LOG/UsedAccounts.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + ":" + Name + ":" + Email + ":" + mess);
            }
        }
    }
}
