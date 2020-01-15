using OpenQA.Selenium;
using OpenQA.Selenium.Opera;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace AutoRefferal
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IWebDriver driver;
        List<Account> accounts;
        PhoneNumber phone;
        List<Refferal> refferals;

        /// <summary>
        /// Инициализация программы и загрузка данных
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadAccounts();
            refferals = Refferal.LoadRefferals();
        }

        /// <summary>
        /// Получение телефона для отправки смс
        /// </summary>
        /// <returns>телефон и ид</returns>
        public PhoneNumber GetPhoneNumber()
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=getNumber&service=fx&operator=any&country=0");//get number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("ACCESS_NUMBER"))
                    {
                        var num = result.Split(':');
                        var code = num[0];
                        var id = num[1];
                        var tel = num[2];
                        return new PhoneNumber(code, id, tel);
                    }
                    else return new PhoneNumber(result);
                }
            }
        }

        /// <summary>
        /// Отправка уведомления об отправленном смс
        /// </summary>
        /// <param name="number">Телефон</param>
        /// <returns>Телефон</returns>
        public PhoneNumber MessageSend(PhoneNumber number)
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=1&id=" + number.Id.ToString());//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    number.StatusCode = result;
                    return number;
                }
            }
        }

        /// <summary>
        /// Получаем смс код
        /// </summary>
        /// <param name="number">Телефон</param>
        /// <returns>Телефон</returns>
        public PhoneNumber GetCode(PhoneNumber number)
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=getStatus&id=" + phone.Id);//get message
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    if (result.Contains("STATUS_OK"))
                    {
                        var res = result.Split(':');
                        number.StatusCode = res[0];
                        number.Code = int.Parse(res[1]);
                        return number;
                    }
                    else
                    {
                        number.StatusCode = result;
                        return number;
                    }
                }
            }
        }

        /// <summary>
        /// Сообщаем об успешном использовании номера
        /// </summary>
        /// <param name="number">Телефон</param>
        public void NumberConformation(PhoneNumber number)
        {
            WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=6&id=" + number.Id.ToString());//activate number
            WebResponse response = request.GetResponse();
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    number.StatusCode = result;
                }
            }
        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        /// <param name="number">Телефон</param>
        public void DeclinePhone(PhoneNumber number)
        {
            if (phone != null)
            {
                WebRequest request = WebRequest.Create("http://sms-activate.ru/stubs/handler_api.php?api_key=1b2c6c99b521dAed531d16449226396d&action=setStatus&status=-1&id=" + number.Id.ToString());//activate number
                WebResponse response = request.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var result = reader.ReadToEnd();
                        number.StatusCode = result;
                    }
                }
            }
        }

        /// <summary>
        /// Получение кода смс
        /// </summary>
        /// <returns>Получен ли код</returns>
        public bool GetCode()
        {
            driver.FindElement(By.Name("RegistrationForm[phone]")).SendKeys(phone.Number.ToString());
            Thread.Sleep(1000);
            driver.FindElement(By.Id("phone_code-button")).Click();
            Thread.Sleep(1000);
            try
            {
                driver.SwitchTo().Alert().Accept();
                phone = MessageSend(phone);
                Thread.Sleep(15000);
                phone = GetCode(phone);
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(phone.Code.ToString());
                    return true;
                }
                else
                {
                    Thread.Sleep(15000);
                    phone = GetCode(phone);
                    if (phone.StatusCode.Contains("STATUS_OK"))
                    {
                        driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(phone.Code.ToString());
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                phone = GetCode(phone);
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(phone.Code.ToString());
                    return true;
                }
                else return false;
            }
        }

        /// <summary>
        /// Получение номера телефона для регистрации
        /// </summary>
        /// <returns>Получен ли номер</returns>
        public bool GetNumberPhone()
        {
            phone = GetPhoneNumber();
            if (phone.StatusCode.Contains("ACCESS_NUMBER"))
            {
                return true;
            }
            else
            {
                WriteLog(phone.StatusCode);
                return false;
            }
        }

        /// <summary>
        /// Событие закрития окна
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                driver.Quit();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Начало работы атоматической регистрации
        /// </summary>
        private void StartReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in refferals)
                {
                    var buffAccounts = new List<Account>(accounts);
                    if (accounts.Count == 0)
                    {
                        MessageBox.Show("Аккаунты закончились");
                        break;
                    }
                    if (buffAccounts.Count > 0 && item.ActivatedAccounts < 10)
                    {
                        for (int i = 0; item.ActivatedAccounts < 10; i++)
                        {
                            if (accounts.Count == 0)
                                break;
                            OperaOptions options = new OperaOptions();
                            using (StreamReader sr = new StreamReader("Settings.txt"))
                            {
                                options.BinaryLocation = sr.ReadLine();
                            }
                            options.AddArgument("user-data-dir=" + Directory.GetCurrentDirectory() + @"\opera");
                            options.AddArgument("private");
                            driver = new OperaDriver(options);


                            driver.Navigate().GoToUrl("https://pgbonus.ru/register");
                            Thread.Sleep(3000);
                            SendName(buffAccounts[i].Name);
                            Thread.Sleep(3000);
                            SendEmail(buffAccounts[i].Email);
                            Thread.Sleep(3000);
                            SendRefferal(item.Code);
                            Thread.Sleep(4000);
                            if (!driver.FindElement(By.ClassName("field-registrationform-first_name")).GetAttribute("innerHTML").Contains("в настоящее время регистрация невозможна"))
                            {
                                if (!driver.FindElement(By.ClassName("field-registrationform-email")).GetAttribute("innerHTML").Contains("зарегистрирова"))
                                {
                                    CheckPass();
                                    Thread.Sleep(2000);
                                    if (GetNumberPhone())
                                    {
                                        if (GetCode())
                                        {
                                            Thread.Sleep(2000);
                                            CheckAgrrement();
                                            Thread.Sleep(3000);
                                            if (!driver.FindElement(By.ClassName("field-registrationform-first_name")).GetAttribute("innerHTML").Contains("в настоящее время регистрация невозможна"))
                                            {
                                                SubmitReg();
                                                NumberConformation(phone);
                                                Thread.Sleep(4000);
                                                driver.Navigate().GoToUrl("https://pgbonus.ru/lk#");
                                                Thread.Sleep(5000);
                                                if (driver.FindElements(By.ClassName("confirmed")).Count == 2)
                                                {
                                                    SaveAccountInfo(buffAccounts[i], "Зарегистрирован");
                                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                    SaveAccounts();
                                                    item.ActivatedAccounts++;
                                                    Refferal.SaveRefferals(refferals);
                                                }
                                                else
                                                {
                                                    if (driver.FindElements(By.ClassName("confirmed")).Count == 1)
                                                    {
                                                        SaveAccountInfo(buffAccounts[i], "Использован, но не подтвержден");
                                                        accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                        SaveAccounts();
                                                    }
                                                    else
                                                    {
                                                        SaveAccountInfo(buffAccounts[i], "Бабки просраны");
                                                        i--;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DeclinePhone(phone);
                                                i--;
                                            }

                                        }
                                        else
                                        {
                                            DeclinePhone(phone);
                                            i--;
                                        }
                                    }
                                    else
                                    {
                                        i--;
                                    }
                                }
                                else
                                {
                                    DeclinePhone(phone);
                                    SaveAccountInfo(accounts[i], "Проблема с имейлом");
                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                    SaveAccounts();
                                }
                            }
                            else
                            {
                                i--;
                            }
                            driver.Quit();
                            Thread.Sleep(1000);
                        }
                    }
                }
                MessageBox.Show("Рефферальные коды закончились");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошло неведанное говно, программа перешла в ручной режим.");
                DeclinePhone(phone);
                WriteLog(ex.ToString());
            }
        }

        /// <summary>
        /// Нажатие элемента подтвердить
        /// </summary>
        public void SubmitReg()
        {
            driver.FindElement(By.ClassName("submit")).Click();
        }

        /// <summary>
        /// Ввод имени для поля имя
        /// </summary>
        /// <param name="Name">Имя для ввода</param>
        public void SendName(string Name)
        {
            driver.FindElement(By.Name("RegistrationForm[first_name]")).SendKeys(Name);
        }

        /// <summary>
        /// Ввод имейла для поля имейл
        /// </summary>
        /// <param name="Email">Имейл для ввода</param>
        public void SendEmail(string Email)
        {
            driver.FindElement(By.Name("RegistrationForm[email]")).SendKeys(Email);
        }

        /// <summary>
        /// Ввод рефферального кода для поля пригласивший
        /// </summary>
        /// <param name="refferal"></param>
        public void SendRefferal(string refferal)
        {
            driver.FindElement(By.Name("RegistrationForm[rcode]")).SendKeys(refferal);
        }

        /// <summary>
        /// Нажатие на чекбокс сгенерировать пароль
        /// </summary>
        public void CheckPass()
        {
            driver.FindElement(By.XPath("//*[@id=\"random_pass\"]")).Click();
        }

        /// <summary>
        /// Нажатие на чекбокс подтвердить регистрацию
        /// </summary>
        public void CheckAgrrement()
        {
            driver.FindElement(By.XPath("//*[@id=\"registrationform-aggr\"]")).Click();
        }

        /// <summary>
        /// Добавление новых аккаунтов из файла
        /// </summary>
        private void Accounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader("Accounts.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        var str = sr.ReadLine().Split(':');
                        if (accounts.Where(x => x.Email == str[0]).FirstOrDefault() == null)
                            accounts.Add(new Account(str[0], str[1]));
                    }
                }
                File.Delete("Accounts.txt");
                MessageBox.Show($"Добавлено {accounts.Count} аккаунтов");
                SaveAccounts();

            }
            catch (Exception)
            {
                MessageBox.Show("Аккаунты не найдены");
            }
        }

        /// <summary>
        /// Сохранение информации о текущем аккаунте в файл
        /// </summary>
        /// <param name="acc">Аккаунт</param>
        /// <param name="mess">Статус</param>
        public void SaveAccountInfo(Account acc, string mess)
        {
            using (StreamWriter sw = new StreamWriter("UsedAccounts.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + ":" + acc.Name + ":" + acc.Email + ":" + mess);
            }
        }

        /// <summary>
        /// Логиование данных в файл
        /// </summary>
        /// <param name="log">Текст для логирования</param>
        public void WriteLog(string log)
        {
            using (StreamWriter sw = new StreamWriter("log.txt", true))
            {
                sw.WriteLine(DateTime.Now.ToString() + "   " + log);
            }
        }

        /// <summary>
        /// Сохранение аккаунтов в файл
        /// </summary>
        public void SaveAccounts()
        {
            using (StreamWriter sw = new StreamWriter("Accounts.dat"))
            {
                sw.Write(SerializeHelper.Serialize(accounts));
            }
        }

        /// <summary>
        /// Загрузка аккаунтов из файла
        /// </summary>
        public void LoadAccounts()
        {
            try
            {
                using (StreamReader sr = new StreamReader("Accounts.dat"))
                {
                    accounts = SerializeHelper.Desirialize<List<Account>>(sr.ReadToEnd());
                }
            }
            catch (Exception)
            {
                accounts = new List<Account>();
            }
        }

        /// <summary>
        /// Получение новых реферальных кодов из файла
        /// </summary>
        /// <returns>Результат получения кодов</returns>
        public bool GetRefferals()
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
                    Refferal.SaveRefferals(refferals);
                    File.Delete("Refferals.txt");
                    return true;
                }
            }
            catch (Exception e)
            {
                WriteLog(e.ToString());
                return false;
            }

        }

        /// <summary>
        /// Добавление новых реферальных кодов
        /// </summary>
        private void Refferals_Click(object sender, RoutedEventArgs e)
        {
            if (!GetRefferals())
            {
                MessageBox.Show("Промокоды не найдены");
            }
        }
    }
}
