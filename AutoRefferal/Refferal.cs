using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoRefferal
{
    /// <summary>
    /// Реферальный код с количеством активаций
    /// </summary>
    public class Refferal
    {
        /// <summary>
        /// Реферальный код
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Количество активаций
        /// </summary>
        public int ActivatedAccounts { get; set; }

        /// <summary>
        /// Базовый конструктор класса
        /// </summary>
        public Refferal()
        {

        }

        /// <summary>
        /// Создание нового реферального кода
        /// </summary>
        /// <param name="code">Реферальный код</param>
        /// <param name="activatedAccounts">Количество активаций</param>
        public Refferal(string code, int activatedAccounts)
        {
            Code = code;
            ActivatedAccounts = activatedAccounts;
        }

        /// <summary>
        /// Сохранение реферальных кодов в файл
        /// </summary>
        /// <param name="refferals">Коды для сохранения</param>
        public static void SaveRefferals(List<Refferal> refferals)
        {
            using (StreamWriter sw = new StreamWriter("Refferals.dat"))
            {
                sw.Write(SerializeHelper.Serialize(refferals));
            }
        }

        /// <summary>
        /// Загрука реферальных кодов из файла
        /// </summary>
        /// <returns>Загруженные коды</returns>
        public static List<Refferal> LoadRefferals()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Refferals.dat"))
                {
                    return SerializeHelper.Desirialize<List<Refferal>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                return new List<Refferal>();
            }
        }

        /// <summary>
        /// Добавление новых реферальных кодов в файл
        /// </summary>
        /// <param name="refferals">Текущие реферальные коды</param>
        /// <returns>Новый список реферальных кодов</returns>
        public static List<Refferal> GetNewRefferals(List<Refferal> refferals)
        {
            try
            {
                using (StreamReader sr = new StreamReader("Refferals.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine();
                        if (refferals.Where(x => x.Code == str).FirstOrDefault() == null)
                            refferals.Add(new Refferal(str, 0));
                    }
                    SaveRefferals(refferals);
                    File.Delete("Refferals.txt");
                    return refferals;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
