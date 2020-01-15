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
            accounts = Account.LoadAccounts();
            refferals = Refferal.LoadRefferals();
        }

        /// <summary>
        /// Сообщаем об отмене использования номера
        /// </summary>
        /// <param name="number">Телефон</param>
        public void DeclinePhone()
        {
            if (phone != null)
            {
                phone.DeclinePhone();
            }
        }

        /// <summary>
        /// Получение кода регистрации по смс
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
                phone.MessageSend();
                Thread.Sleep(15000);
                phone.GetCode();
                if (phone.StatusCode.Contains("STATUS_OK"))
                {
                    driver.FindElement(By.Name("RegistrationForm[sms]")).SendKeys(phone.Code.ToString());
                    return true;
                }
                else
                {
                    Thread.Sleep(15000);
                    phone.GetCode();
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
                phone.GetCode();
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
            phone.GetPhoneNumber();
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
            phone = new PhoneNumber();
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
                                                phone.NumberConformation();
                                                Thread.Sleep(4000);
                                                driver.Navigate().GoToUrl("https://pgbonus.ru/lk#");
                                                Thread.Sleep(5000);
                                                if (driver.FindElements(By.ClassName("confirmed")).Count == 2)
                                                {
                                                    buffAccounts[i].SaveAccountInfo("Зарегистрирован");
                                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                    Account.SaveAccounts(accounts);
                                                    item.ActivatedAccounts++;
                                                    Refferal.SaveRefferals(refferals);
                                                }
                                                else
                                                {
                                                    if (driver.FindElements(By.ClassName("confirmed")).Count == 1)
                                                    {
                                                        buffAccounts[i].SaveAccountInfo("Использован, но не подтвержден");
                                                        accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                                        Account.SaveAccounts(accounts);
                                                    }
                                                    else
                                                    {
                                                        buffAccounts[i].SaveAccountInfo("Бабки просраны");
                                                        i--;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                DeclinePhone();
                                                i--;
                                            }

                                        }
                                        else
                                        {
                                            DeclinePhone();
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
                                    DeclinePhone();
                                    accounts[i].SaveAccountInfo("Проблема с имейлом");
                                    accounts.Remove(accounts.Where(x => x.Name == buffAccounts[i].Name).FirstOrDefault());
                                    Account.SaveAccounts(accounts);
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
                DeclinePhone();
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
            var res = Account.GetNewAccounts(accounts);
            if (res == null)
                MessageBox.Show("Аккаунты не добавлены");
            else
                accounts = res;
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
