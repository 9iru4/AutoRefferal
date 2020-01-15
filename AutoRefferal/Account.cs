using System;
using System.Collections.Generic;
using System.IO;

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
            using (StreamWriter sw = new StreamWriter("Accounts.dat"))
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
                using (StreamReader sr = new StreamReader("Accounts.dat"))
                {
                    return SerializeHelper.Desirialize<List<Account>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new List<Account>();
            }
        }
    }
}
